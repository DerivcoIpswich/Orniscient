using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Web.Mvc;
using Derivco.Orniscient.Proxy.Filters;
using Derivco.Orniscient.Proxy.Grains;
using Microsoft.Owin.Security;
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

        public async Task<ActionResult> GetFilters(GetFiltersRequest filtersRequest)
        {
            if(filtersRequest?.Types == null)
                return null;
            
            //TODO : Implement this method and return the filters for all the selected types.
            //Should return 

            var reportingGrain = GrainClient.GrainFactory.GetGrain<IOrniscientReportingGrain>(Guid.Empty);
            var filter = await reportingGrain.GetFilters(new[] { "TestHost.Grains.FooGrain" },null);



            //going to mock the return data now so that we can just draw the form to show the filters.
            var result = new List<TypeFilter>();
            foreach (var type in filtersRequest.Types)
            {
                result.Add(new TypeFilter()
                {
                    TypeName = type,
                    Filters = new List<AggregatedFilterRow>()
                    {
                        new AggregatedFilterRow() {FilterName = "Filter One",Values = new List<string>() {"Item1","Item2","Item3"} },
                        new AggregatedFilterRow() {FilterName = "Filter Two",Values = new List<string>() {"Item1","Item2","Item3"} }
                    }
                });
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Type()
        {
            var reportingGrain = GrainClient.GrainFactory.GetGrain<IOrniscientReportingGrain>(Guid.Empty);
            var filter = await reportingGrain.GetFilters(new[] {"TestHost.Grains.FooGrain"},null);

            //var tpes = managementGrain.GetFilters()
            return Json(filter, JsonRequestBehavior.AllowGet);
        }
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

    public class TypeFilter
    {
        public string TypeName { get; set; }
        public List<AggregatedFilterRow> Filters { get; set; }
        
    }

    public class AggregatedFilterRow
    {
        public string FilterName { get; set; }
        public List<string> Values { get; set; }
    }
}