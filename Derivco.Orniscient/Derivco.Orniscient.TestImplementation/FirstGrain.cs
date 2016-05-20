using System;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Orleans;
using Orleans.Streams;

namespace Derivco.Orniscient.TestImplementation
{
    [Proxy.Attributes.Orniscient]
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
            for (var i = 0; i < 15; i++)
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