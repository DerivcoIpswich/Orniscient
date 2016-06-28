using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Extensions;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Orleans;
using Orleans.Providers;

namespace Derivco.Orniscient.Proxy.BootstrapProviders
{
    public class OrniscientFilterInterceptor : IBootstrapProvider
    {
        private readonly List<string> _grainsWhereTimerWasRegistered = new List<string>();
        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            providerRuntime.SetInvokeInterceptor((method, request, grain, invoker) =>
            {

                //TODO : REALLY DO NOT WANT THIS TO HAPPEN EVERY TIME A GRAIN EVENT IS CALLED....... Awaiting feedback from Gitter....

                if (!(grain is IFilterableGrain)) return invoker.Invoke(grain, request);
                //we need to add property to IFilterablegrain that is Reminder Created, then over here if that property is true for the grain, then we do not create the reminder again.

                if (!_grainsWhereTimerWasRegistered.Contains(((Orleans.Grain)grain).IdentityString))
                {
                    var grainType = grain.GetType();

                    Func<object, Task> timerFunc = async o =>
                    {
                        var filterGrain = grain.AsReference<IFilterableGrain>();
                        var result = await filterGrain.GetFilters();
                        if (result != null)
                        {
                            //Now we just need to send the filters up to some grain that will aggregate them....??????HOW BEST WE DO THAT.............
                            //var typeFilterGrain =providerRuntime.GrainFactory.GetGrain<ITypeFilterGrain>(grainType.FullName);
                            //await typeFilterGrain.RegisterFilter(result,grain.GetPrimaryKey());

                            //TODO : This can be removed.
                            var filterString = string.Join(",", result.Select(p => $"{p.Name} : {p.Value}"));
                            Debug.WriteLine($"Filters for grain [Type : {grainType.FullName}][Id : {grain.GetPrimaryKey().ToInt()}][filter : {filterString}]");
                        }
                        else
                        {
                            Debug.WriteLine("Filter was not set yet");
                        }
                    };

                    var dynamicMethod = grainType.GetMethod("RegisterTimer",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    dynamicMethod.Invoke(grain, new object[]
                    {
                            timerFunc,
                            null,
                            TimeSpan.FromSeconds(20),
                            TimeSpan.FromSeconds(500)
                    });

                    _grainsWhereTimerWasRegistered.Add(((Orleans.Grain)grain).IdentityString);
                    Debug.WriteLine($"........................Currently we have {_grainsWhereTimerWasRegistered.Count} grains where timer was registered");
                }
                return invoker.Invoke(grain, request);

            });
            return Task.FromResult(0);
        }

        public Task Close()
        {
            return Task.FromResult(0);
        }

        public string Name => "OrniscientFilterInterceptor";
    }




}
