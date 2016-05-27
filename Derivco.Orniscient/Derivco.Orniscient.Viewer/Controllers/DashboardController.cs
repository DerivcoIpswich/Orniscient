using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Derivco.Orniscient.Proxy.Grains;
using Orleans;
using Orleans.Runtime;

namespace Derivco.Orniscient.Viewer.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult IndexTemp()
        {
            return View();
        }

        public async Task<ActionResult> GetDashboardInfo()
        {
            var reportingGrain = GrainClient.GrainFactory.GetGrain<IOrniscientReportingGrain>(Guid.Empty);
            var dashboardInfo = new DashboardInfo
            {
                Silos = await reportingGrain.GetSilos(),
                AvailableTypes = await reportingGrain.GetGrainTypes()
            };

            return Json(dashboardInfo, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Type()
        {
            var reportingGrain = GrainClient.GrainFactory.GetGrain<IOrniscientReportingGrain>(Guid.Empty);
            var filter = await reportingGrain.GetFilters(new[] {"TestHost.Grains.FooGrain"});

            //var tpes = managementGrain.GetFilters()
            return Json(filter, JsonRequestBehavior.AllowGet);
        }
    }

    public class DashboardInfo
    {
        public string[] Silos { get; set; }
        public string[] AvailableTypes { get; set; } 
    }
}