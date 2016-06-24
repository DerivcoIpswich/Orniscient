using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains.Models;
using Derivco.Orniscient.Proxy.Observers;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains
{
    public interface IOrniscientReportingGrain : IGrainWithGuidKey 
    {
        Task<List<UpdateModel>> GetAll();
        Task<List<UpdateModel>> GetAll(string type);
        Task<DiffModel> GetChanges();
        Task Subscribe(IOrniscientObserver observer);
        Task UnSubscribe(IOrniscientObserver observer);

        Task SetTypeFilter(GrainType[] types);

        Task<string[]> GetSilos();

        Task<GrainType[]> GetGrainTypes();
    }

    public class GrainType
    {
        public GrainType(string fullName)
        {
            FullName = fullName;
        }

        public string  FullName { get; set; }

        public string ShortName => !string.IsNullOrEmpty(FullName) ? FullName.Split('.').LastOrDefault() : "";
    }
}
