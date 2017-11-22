using System.Linq;
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
        public override async Task OnConnected()
        {
            await Groups.Add(Context.ConnectionId, "userGroup");

            var grainsSessionCookie = Context.RequestCookies.FirstOrDefault(x => x.Key == "GrainSessionId").Value;
            await OrniscientObserver.Instance.RegisterGrainClient(grainsSessionCookie.Value);
            
            await base.OnConnected();
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            var grainsSessionCookie = Context.RequestCookies.FirstOrDefault(x => x.Key == "GrainSessionId").Value;
            await OrniscientObserver.Instance.UnregisterGrainClient(grainsSessionCookie.Value);
            await base.OnDisconnected(stopCalled);
        }

        [HubMethodName("GetCurrentSnapshot")]
        public async Task<DiffModel> GetCurrentSnapshot(AppliedFilter filter = null)
        {
            var grainsSessionCookie = Context.RequestCookies.FirstOrDefault(x => x.Key == "GrainSessionId").Value;
            return await OrniscientObserver.Instance.GetCurrentSnapshot(filter, grainsSessionCookie.Value);
        }
    }
}