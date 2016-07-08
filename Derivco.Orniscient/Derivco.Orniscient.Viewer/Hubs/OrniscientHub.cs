using System.Collections.Generic;
using System.Threading.Tasks;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains.Models;
using Derivco.Orniscient.Viewer.Observers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Derivco.Orniscient.Viewer.Hubs
{
    [HubName("orniscientHub")]
    public class OrniscientHub : Hub
    {
        private readonly OrniscientObserverContainer _orniscientObserverContainer;
        private int _sessionId;

        public OrniscientHub(OrniscientObserverContainer orniscientObserverContainer)
        {
            _orniscientObserverContainer = orniscientObserverContainer;

            //TODO : Remove this if not needed....
            _sessionId =0;
        }

        public override Task OnConnected()
        {
            //fake sessionid stuff...
            _sessionId = int.Parse(this.Context.Request.QueryString["id"]??"0");
            _orniscientObserverContainer.Get(_sessionId);
            return base.OnConnected();
        }

        public OrniscientHub()
            : this(OrniscientObserverContainer.Instance)
        { }

        [HubMethodName("GetCurrentSnapshot")]
        public async Task<DiffModel> GetCurrentSnapshot(AppliedFilter filter = null)
        {
            return await _orniscientObserverContainer.Get(_sessionId).GetCurrentSnapshot(filter);
        }

    }
}