using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains.Models;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains
{
    public interface IDashboardCollectorGrain : IGrainWithGuidKey
    {
        Task<List<UpdateModel>> GetAll();
        Task<List<UpdateModel>> GetAll(string type);
        Task SetTypeFilter(GrainType[] types);
        Task<string[]> GetSilos();
        Task<GrainType[]> GetGrainTypes();
    }
}
