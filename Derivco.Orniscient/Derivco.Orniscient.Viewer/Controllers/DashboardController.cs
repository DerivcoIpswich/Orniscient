using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Derivco.Orniscient.Viewer.Models.Dashboard;
using Orleans;

namespace Derivco.Orniscient.Viewer.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public async Task<ActionResult> Index(int id=0)
        {
            if (!GrainClient.IsInitialized)
            {
                GrainClient.Initialize(Server.MapPath("~/DevTestClientConfiguration.xml"));
                //await OrniscientObserver.Instance.SetTypeFilter(p => p.Contains("SpinSport"));
            }
            return View();
        }

        public async Task<ActionResult> GetDashboardInfo()
        {
            var dashboardCollectorGrain = GrainClient.GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);

            var types = await dashboardCollectorGrain.GetGrainTypes();
            var dashboardInfo = new DashboardInfo
            {
                Silos = await dashboardCollectorGrain.GetSilos(),
                AvailableTypes = types
            };

            return Json(dashboardInfo, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetFilters(GetFiltersRequest filtersRequest)
        {
            if (filtersRequest?.Types == null)
                return null;

            var filterGrain = GrainClient.GrainFactory.GetGrain<IFilterGrain>(Guid.Empty);
            var filters = await filterGrain.GetFilters(filtersRequest.Types);
            return Json(filters, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> GetGrainInfo(GetGrainInfoRequest grainInfoRequest)
        {
            var typeFilterGrain = GrainClient.GrainFactory.GetGrain<ITypeFilterGrain>(grainInfoRequest.GrainType);
            var filters = await typeFilterGrain.Getfilters(grainInfoRequest.GrainId);
            return Json(filters);
        }
    }
}