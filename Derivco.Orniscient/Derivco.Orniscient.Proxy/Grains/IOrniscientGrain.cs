using System.Collections.Generic;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains.Models;
using Derivco.Orniscient.Proxy.Observers;
using Orleans;

namespace Derivco.Orniscient.Proxy.Grains
{
    public interface IOrniscientReportingGrain : IGrainWithGuidKey 
    {
        Task<List<UpdateModel>> GetAll();
        Task<DiffModel> GetChanges();
        Task Subscribe(IOrniscientObserver observer);
        Task UnSubscribe(IOrniscientObserver observer);
    }
}
