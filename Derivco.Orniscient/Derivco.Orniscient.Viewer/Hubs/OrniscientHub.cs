using System.Diagnostics;
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
        public override Task OnConnected()
        {
            Groups.Add(this.Context.ConnectionId, _sessionId.ToString());
            return base.OnConnected();
        }

        [HubMethodName("GetCurrentSnapshot")]
        public async Task<DiffModel> GetCurrentSnapshot(AppliedFilter filter = null)
        {
            return await OrniscientObserver.Instance.GetCurrentSnapshot(filter, _sessionId);
        }

        private int _sessionId
        {
            get
            {
                var sessionId = 0;
                int.TryParse(this.Context.Request.QueryString["id"], out sessionId);
                return sessionId;
            }
        }
    }
}