using System;
using System.Collections.Generic;
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
                    try
                    {
                        idx = await Http.WaitForChanges(
                            _client,
                            _options.Prefix,
                            _configurationReceived,
                            _dispose.Token,
                            _options.Timeout,
                            idx,
                            _options.Converters.KeyParser);
                    }
                    catch
                    {
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