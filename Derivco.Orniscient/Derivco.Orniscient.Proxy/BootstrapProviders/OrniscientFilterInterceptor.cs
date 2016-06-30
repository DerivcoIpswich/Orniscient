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
                if (!(grain is IFilterableGrain) ||
                    _grainsWhereTimerWasRegistered.Contains(((Orleans.Grain) grain).IdentityString))
                {
                    return invoker.Invoke(grain, request);
                }

                var grainType = grain.GetType();
                var dynamicMethod = grainType.GetMethod("RegisterTimer",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                dynamicMethod.Invoke(grain, new object[]
                {
                    getTimerFunc(providerRuntime,grain),
                    null,
                    TimeSpan.FromSeconds(0),
                    TimeSpan.FromSeconds(500)
                });

                _grainsWhereTimerWasRegistered.Add(((Orleans.Grain)grain).IdentityString);
                Debug.WriteLine($"........................Currently we have {_grainsWhereTimerWasRegistered.Count} grains where timer was registered");
                return invoker.Invoke(grain, request);

            });
            return Task.FromResult(0);
        }

        private Func<object, Task> getTimerFunc(IProviderRuntime providerRuntime, IGrain grain)
        {
            var grainName = grain.GetType().FullName;
            return async o =>
            {
                var filterableGrain = grain.AsReference<IFilterableGrain>();
                var result = await filterableGrain.GetFilters();
                if (result != null)
                {
                    //Now we just need to send the filters up to some grain that will aggregate them....??????HOW BEST WE DO THAT.............
                    var filterGrain = providerRuntime.GrainFactory.GetGrain<IFilterGrain>(Guid.Empty);
                    await filterGrain.RegisterFilter(grainName, filterableGrain.GetPrimaryKey().ToString(), result);

                    var filterString = string.Join(",", result.Select(p => $"{p.FilterName} : {p.Value}"));
                    Debug.WriteLine(
                        $"Filters for grain [Type : {grainName}][Id : {grain.GetPrimaryKey().ToInt()}][filter : {filterString}]");
                }
                else
                {
                    Debug.WriteLine("Filter was not set yet");
                }
            };
        }

        public Task Close()
        {
            return Task.FromResult(0);
        }

        public string Name => "OrniscientFilterInterceptor";
    }
}
