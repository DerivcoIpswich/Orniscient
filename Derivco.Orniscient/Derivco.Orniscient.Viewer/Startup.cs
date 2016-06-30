using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Derivco.Orniscient.Viewer.Startup))]

namespace Derivco.Orniscient.Viewer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
