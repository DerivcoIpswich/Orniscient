using System.Collections.Generic;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Grains.Models;
using Derivco.Orniscient.Viewer.Observers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Derivco.Orniscient.Viewer.Hubs
{
    [HubName("orniscientHub")]
    public class OrniscientHub : Hub
    {
        private readonly OrniscientObserver _orniscientObserver;

        public OrniscientHub(OrniscientObserver orniscientObserver)
        {
            _orniscientObserver = orniscientObserver;
        }

        public OrniscientHub()
            :this(OrniscientObserver.Instance)
        {}
        [HubMethodName("GetCurrentSnapshot")]
        public async Task<List<UpdateModel>> GetCurrentSnapshot()
        {
            return await _orniscientObserver.GetCurrentSnapshot();
        }
    }
}