using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ConsulStructure
{
    partial class Structure
    {
        class BlockingHttpWatcher
        {
            readonly Action<IEnumerable<KeyValuePair<string, byte[]>>> _configurationReceived;
            readonly Options _options;
            readonly HttpClient _client;
            readonly CancellationTokenSource _dispose = new CancellationTokenSource();
            readonly Task _loop;
            readonly Func<TimeSpan, TimeSpan> _backoff;

            public BlockingHttpWatcher(
                Action<IEnumerable<KeyValuePair<string, byte[]>>> configurationReceived,
                Options options)
            {
                _configurationReceived = configurationReceived;
                _options = options;
                _client = _options.Factories.HttpClient(_options);
                _backoff = existing => options.HttpBackoff(options, existing);
                _loop = Run();
            }

            static Func<Http.Invoker, Http.Invoker> Capture(
                Action<HttpRequestMessage, HttpResponseMessage, TimeSpan> capture)
            {
                return next => async request =>
                {
                    var stopWatch = new Stopwatch();

                    stopWatch.Start();
                    var response = await next(request);
                    stopWatch.Stop();

                    capture(request, response, stopWatch.Elapsed);
                    return response;
                };
            }

            static Func<Http.Invoker, Http.Invoker> ExponentialBackoff(
                Func<TimeSpan,TimeSpan> backoffAlgo,
                CancellationToken disposer)
            {
                return next => async request =>
                {
                    var backoff = TimeSpan.FromSeconds(1);
                    var nullMessage = Http.NullResponse;
                    do
                    {
                        disposer.ThrowIfCancellationRequested();

                        var response = await next(request);
                        if (response != nullMessage)
                            return response;

                        await Task.Delay(backoff, disposer);
                        var previous = request;
                        request = new HttpRequestMessage(previous.Method, previous.RequestUri);

                        foreach (var h in previous.Headers)
                            request.Headers.Add(h.Key, h.Value);

                        backoff = backoffAlgo(backoff);
                    } while (true);
                };
            }

            static Func<Http.Invoker, Http.Invoker> CatchExceptions(
                Action<Exception> error,
                CancellationToken dispose)
            {
                return next => async env =>
                {
                    try
                    {
                        return await next(env);
                    }
                    catch (OperationCanceledException cancel) when (cancel.CancellationToken == dispose)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        error(e);
                        return Http.NullResponse;
                    }
                };
            }

            static Http.Invoker CheckResponseValid(Http.Invoker inner)
            {
                return async request =>
                {
                    var response = await inner(request);

                    if (!response.IsSuccessStatusCode)
                        throw new InvalidOperationException("Response code was not 200");

                    if (!response.Headers.Contains("X-Consul-Index") ||
                        response.Headers.GetValues("X-Consul-Index").Count() != 1)
                        throw new InvalidOperationException("Missing X-Consul-Index header");

                    return response;
                };
            }

            static Http.Invoker Send(HttpMessageInvoker client, CancellationToken cancellationToken)
            {
                return request => client.SendAsync(request, cancellationToken);
            }

            Http.Invoker Invoker()
            {
                return
                    ExponentialBackoff(_backoff, _dispose.Token)
                    (CatchExceptions(e => _options.Events.HttpError(e), _dispose.Token)
                    (Capture(_options.Events.HttpSuccess)
                        (CheckResponseValid(Send(_client, _dispose.Token)))));
            }

            async Task Run()
            {
                var idx = 0;
                while (!_dispose.IsCancellationRequested)
                {
                    try
                    {
                        idx = await Http.WaitForChanges(
                            Invoker(),
                            _options.Prefix,
                            _configurationReceived,
                            _options.HttpTimeout,
                            idx,
                            _options.Converters.KeyParser);
                    }
                    catch (OperationCanceledException cancelled) when (cancelled.CancellationToken == _dispose.Token)
                    {
                        return;
                    }
                }
            }

            public async Task Stop()
            {
                if (!_dispose.IsCancellationRequested)
                    _dispose.Cancel();
                await _loop;
                _client.Dispose();
            }
        }
    }
}