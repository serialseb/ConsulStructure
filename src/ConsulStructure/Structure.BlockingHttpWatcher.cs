using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            public BlockingHttpWatcher(
                Action<IEnumerable<KeyValuePair<string, byte[]>>> configurationReceived,
                Options options)
            {
                _configurationReceived = configurationReceived;
                _options = options;
                _client = _options.Factories.HttpClient(_options);
                _loop = Run();
            }

            static Func<Http.Invoker,Http.Invoker> Timings(Action<TimeSpan> timings)
            {
                return next => async request =>
                {
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    var response = await next(request);
                    stopWatch.Stop();
                    timings(stopWatch.Elapsed);
                    return response;
                };
            }

            static Func<Http.Invoker, Http.Invoker> Capture(Action<HttpRequestMessage, HttpResponseMessage> capture)
            {
                return next => async request =>
                {
                    var response = await next(request);
                    capture(request, response);
                    return response;
                };
            }

            static Http.Invoker Send(HttpClient client, CancellationToken cancellationToken)
            {
                return request => client.SendAsync(request, cancellationToken);
            }
            async Task Run()
            {
                var idx = 0;
                while (_dispose.IsCancellationRequested == false)
                {
                    HttpRequestMessage request = null;
                    HttpResponseMessage response = null;
                    var timings = TimeSpan.Zero;

                    var invoker =
                        Capture((req, res) => {request = req; response = res;})
                            (Timings(t => timings = t)
                            (Send(_client, _dispose.Token)));
                    try
                    {
                        idx = await Http.WaitForChanges(
                            invoker,
                            _options.Prefix,
                            _configurationReceived,
                            _options.Timeout,
                            idx,
                            _options.Converters.KeyParser);
                        _options.Events.HttpSuccess(request, response, timings);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("canceled");
                    }
                    catch (Exception e)
                    {
                        _options.Events.HttpError(e);
                    }
                }
            }

            public async Task Stop()
            {
                if (_dispose.IsCancellationRequested == false)
                    _dispose.Cancel();
                await _loop;
                _client.Dispose();
            }
        }
    }
}