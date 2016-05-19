using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Orleans;
using Orleans.CodeGeneration;
using Orleans.Streams;

namespace Derivco.Orniscient.TestImplementation
{
    public class FirstGrain : Grain, IFirstGrain
    {
        private IStreamProvider _streamProvider;
        public override Task OnActivateAsync()
        {
            _streamProvider = GetStreamProvider("SMSProvider");
            RegisterTimer(p => AddGrains() ,null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(15));
            return base.OnActivateAsync();
        }

        public Task KeepAlive()
        {
            Console.WriteLine("Hi, I am your first Grain");
            return TaskDone.Done;
        }

        private async Task AddGrains()
        {
            for (var i = 0; i < 3; i++)
            {
                var temp = Guid.NewGuid();
                await _streamProvider.GetStream<Guid>(temp, "TestStream").OnNextAsync(temp);
            }
        }

        //public Task<object> Invoke(MethodInfo method, InvokeMethodRequest request, IGrainMethodInvoker invoker)
        //{
        //    Debug.WriteLine($".....called method : {method.Name}");
        //    return invoker.Invoke(this, request);
        //}
    }
}