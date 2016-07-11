using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;

namespace Derivco.Orniscient.Proxy.BootstrapProviders
{
    public class OrniscientFilterInterceptor : IBootstrapProvider
    {
        private readonly List<string> _grainsWhereTimerWasRegistered = new List<string>();
        private Logger _logger;

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            _logger = providerRuntime.GetLogger(Name);
            _logger.Info("OrniscientFilterInterceptor started.");
            providerRuntime.SetInvokeInterceptor((method, request, grain, invoker) =>
            {
                if (!(grain is IFilterableGrain) ||
                    _grainsWhereTimerWasRegistered.Contains(((Orleans.Grain)grain).IdentityString))
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
                _logger.Verbose($"Currently we have {_grainsWhereTimerWasRegistered.Count} grains where timer was registered");
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
                    var filterGrain = providerRuntime.GrainFactory.GetGrain<ITypeFilterGrain>(grain.GetType().FullName);
                    await filterGrain.RegisterFilter(grainName, filterableGrain.GetPrimaryKey().ToString(), result);

                    var filterString = string.Join(",", result.Select(p => $"{p.FilterName} : {p.Value}"));
                    _logger.Verbose($"Filters for grain [Type : {grainName}] [Id : {grain.GetPrimaryKey()}][Filter : {filterString}]");
                }
                else
                {
                    _logger.Verbose("Filter was not set yet");
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
