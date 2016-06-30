using Derivco.Orniscient.Proxy.Grains.Models;
using Orleans;

namespace Derivco.Orniscient.Proxy.Observers
{
    public interface IOrniscientObserver: IGrainObserver
    {
        void GrainsUpdated(DiffModel model);
    }
}
