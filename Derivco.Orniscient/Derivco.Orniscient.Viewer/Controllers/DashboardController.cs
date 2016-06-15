using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Derivco.Orniscient.Viewer.Observers;
using Orleans;
using React.Exceptions;

namespace Derivco.Orniscient.Viewer.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public async Task<ActionResult> Index()
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
            var reportingGrain = GrainClient.GrainFactory.GetGrain<IOrniscientReportingGrain>(Guid.Empty);

            var types = await reportingGrain.GetGrainTypes();


            var dashboardInfo = new DashboardInfo
            {
                Silos = await reportingGrain.GetSilos(),
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

        //public async Task<ActionResult> Search(SearchFilter filter)
        //{
        //    //the filter will need to be stored on the server temporarily, so that the push service can honour it.
        //    //we need to return the grains, then we will consolidate on the client, add new grains / remove old ones.
        //    //clear search should just pull the entire dashboard again



        //    return Json(null, JsonRequestBehavior.AllowGet);
        //}
    }


public class DashboardInfo
    {
        public string[] Silos { get; set; }
        public string[] AvailableTypes { get; set; } 
    }

    public class GetFiltersRequest
    {
        public string[] Types { get; set; }
    }
}