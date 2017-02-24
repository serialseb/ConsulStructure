using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsulStructure
{
    internal partial class Structure
    {

        internal static Structure Start(Action<KeyValuePair<string,byte[]>> instance, Options options = null)
        {
            options = options ?? new Options();

            return new Structure(new LambdaStructureWatcher(instance, options).Dispose);
        }
        internal static Structure Start<T>(T instance, Options options = null)
        {
            options = options ?? new Options();

            return new Structure(new StructureWatcher<T>(instance, options).Dispose);
        }

        readonly Func<Task> _stopper;

        Structure(Func<Task> stopper)
        {
            _stopper = stopper;
        }

        public Task Stop()
        {
            return _stopper();
        }
    }
}