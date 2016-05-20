using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Attributes;
using Orleans;
using Orleans.CodeGeneration;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;

namespace Derivco.Orniscient.TestImplementation
{
    public class DewaldTelementary : ITraceTelemetryConsumer, IEventTelemetryConsumer
    {
        public void Flush()
        {
            Debug.WriteLine("....TrackTrace Flush");
        }

        public void Close()
        {
            Debug.WriteLine("....TrackTrace Close");
        }

        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            var stringbui = new StringBuilder();
            foreach (var key in properties.Keys)
            {
                stringbui.AppendLine($"{key} : {properties[key]},");
            }
            Debug.WriteLine($"....TrackTrace 4 : eventName : {eventName}, properties : {stringbui.ToString()} ");
        }

        public void TrackTrace(string message)
        {
            Debug.WriteLine("....TrackTrace 1 :" + message);
        }

        public void TrackTrace(string message, Severity severity)
        {
            Debug.WriteLine("....TrackTrace 2:" + message);
        }

        public void TrackTrace(string message, Severity severity, IDictionary<string, string> properties)
        {
            var stringbui = new StringBuilder();
            foreach (var key in properties.Keys)
            {
                stringbui.AppendLine($"{key} : {properties[key]},");
            }


            Debug.WriteLine($"....TrackTrace 3 :{message}, properties : {stringbui.ToString()} ");
        }

        public void TrackTrace(string message, IDictionary<string, string> properties)
        {
            var stringbui = new StringBuilder();
            foreach (var key in properties.Keys)
            {
                stringbui.AppendLine($"{key} : {properties[key]},");
            }

            Debug.WriteLine($"....TrackTrace 4 :{message}, properties : {stringbui.ToString()} ");
        }
    }




    public class CustomLogConsumer : ILogConsumer {
        public void Log(Severity severity, TraceLogger.LoggerType loggerType, string caller, string message, IPEndPoint myIPEndPoint,
            Exception exception, int eventCode = 0)
        {
            if (message.Contains("SubGrain"))
            {
                //we can get the type and activation details from here.....need to mine from the string, not very nice.....
                //[Activation: S127.0.0.1:11111:200750136*grn/83371988/dacbb382@d32e78b1 #GrainType=Derivco.Orniscient.TestImplementation.SubGrain Placement=RandomPlacement State=Activating]"
                Debug.WriteLine(".....This is from my custom log consumer : " + message);
            }
        }
    }


    public class SomeBootstrapper : IBootstrapProvider
    {
        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            TraceLogger.AddTraceLevelOverride("Catalog",Severity.Warning);
            TraceLogger.LogConsumers.Add(new CustomLogConsumer());
            Logger.TelemetryConsumers.Add(new DewaldTelementary());

            


            //var temp = providerRuntime.GetInvokeInterceptor();


            //this is not allowed. Seems like this needs to be done per grain level....
            //providerRuntime.SetInvokeInterceptor((method, request, grain, invoker) =>
            //{
            //    var temp = new StringBuilder();
            //    if (request.Arguments != null)
            //    {
            //        foreach (var argument in request.Arguments)
            //        {
            //            temp.Append($",{argument.ToString()}");
            //        }
            //    }
            //    Debug.WriteLine("Something has happened");
            //    //Debug.WriteLine($"...........name : {name}, request : {temp.ToString()}, grain : {grain.GetPrimaryKeyString()}, invoker : {invoker.InterfaceId}");
            //    return invoker.Invoke(grain, request);
            //});
            Debug.WriteLine("The bootstrap provider has been registered and all is working well.");
            return TaskDone.Done;
        }

        public Task Close()
        {
            throw new NotImplementedException();
        }

        public string Name { get; }
    }

    public interface ISubGrain : IGrainWithGuidKey
    {
        Task SayHallo();
    }

    [ImplicitStreamSubscription("TestStream")]
    [Proxy.Attributes.Orniscient(linkFromType: typeof(FirstGrain),linkType:LinkType.SingleInstance,colour:"yellow")]
    public class SubGrain : Grain, ISubGrain, IAsyncObserver<Guid> 
    {
        private StreamSubscriptionHandle<Guid> _subscriptionHandle;


        

        public override async Task OnActivateAsync()
        {
            var streamProvider = GetStreamProvider("SMSProvider");
            var incomingStream = streamProvider.GetStream<Guid>(this.GetPrimaryKey(), "TestStream");
            _subscriptionHandle = await incomingStream.SubscribeAsync((IAsyncObserver<Guid>)this);
            await base.OnActivateAsync();
        }


        public Task SayHallo()
        {
           return TaskDone.Done;
        }

        public async Task OnNextAsync(Guid item, StreamSequenceToken token = null)
        {
            var msg = $"Hallo from Grain : {this.GetPrimaryKey()}";
            Console.WriteLine(msg);



            var t = GrainFactory.GetGrain<IFooGrain>(item);
            await t.KeepAlive();


            await Task.FromResult(msg);
        }

        public Task OnCompletedAsync()
        {
            return TaskDone.Done;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return TaskDone.Done;
        }

        //public Task<object> Invoke(MethodInfo method, InvokeMethodRequest request, IGrainMethodInvoker invoker)
        //{
        //    Debug.WriteLine($".....called method : {method.Name}");
        //    return invoker.Invoke(this, request);
        //}
    }
}
