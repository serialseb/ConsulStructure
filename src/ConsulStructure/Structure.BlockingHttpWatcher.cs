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

            async Task Run()
            {
                var idx = 0;
                while (_dispose.IsCancellationRequested == false)
                {
                    HttpRequestMessage request = null;
                    HttpResponseMessage response = null;
                    var timings = TimeSpan.Zero;
                    try
                    {
                        idx = await Http.WaitForChanges(
                            async r =>
                            {
                                Stopwatch watch = new Stopwatch();
                                request = r;
                                watch.Start();
                                response = await _client.SendAsync(request, _dispose.Token);
                                watch.Stop();
                                timings = watch.Elapsed;
                                return response;
                            },
                            _options.Prefix,
                            _configurationReceived,
                            _options.Timeout,
                            idx,
                            _options.Converters.KeyParser);
                        _options.Events.HttpSuccess(request, response, timings);
                    }
                    catch (OperationCanceledException)
                    {
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