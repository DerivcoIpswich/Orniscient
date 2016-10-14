using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Orleans;

namespace Derivco.Orniscient.Viewer
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        public override void Init()
        {
            if (!GrainClient.IsInitialized)
            {
                GrainClient.Initialize(Server.MapPath("~/DevTestClientConfiguration.xml"));
            }
            base.Init();
        }
    }
}
