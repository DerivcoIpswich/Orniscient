using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Derivco.Orniscient.Viewer.Clients;
using Derivco.Orniscient.Viewer.Models.Dashboard;
using Orleans;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Derivco.Orniscient.Proxy.Grains.Models;

namespace Derivco.Orniscient.Viewer.Controllers
{
    public class DashboardController : Controller
    {
        private static bool _allowMethodsInvocation;

        // GET: Dashboard
        public Task<ViewResult> Index()
        {
            try
            {
                _allowMethodsInvocation = AllowMethodsInvocation();
                ViewBag.AllowMethodsInvocation = _allowMethodsInvocation;
                return Task.FromResult(View());
            }
            catch (Exception ex)
            {
                return Task.FromResult(View("InitError"));
            }
        }

        public Task<ActionResult> Disconnect()
        {
            GrainClientInitializer.Disconnect();
            ActionResult result = RedirectToAction("Index", "Connection");
            return Task.FromResult(result);
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
			var filters = await filterGrain.GetGroupedFilterValues(filtersRequest.Types);
			return Json(filters, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public async Task<ActionResult> GetGrainInfo(GetGrainInfoRequest grainInfoRequest)
		{
			var typeFilterGrain = GrainClient.GrainFactory.GetGrain<IFilterGrain>(Guid.Empty);
			var filters = await typeFilterGrain.GetFilters(grainInfoRequest.GrainType, grainInfoRequest.GrainId);
			return Json(filters);
		}

		[HttpPost]
		public async Task SetSummaryViewLimit(int summaryViewLimit, int sessionId = 0)
		{
			var dashboardInstanceGrain = GrainClient.GrainFactory.GetGrain<IDashboardInstanceGrain>(sessionId);
			await dashboardInstanceGrain.SetSummaryViewLimit(summaryViewLimit);
		}

		[HttpPost]
		public async Task<ActionResult> GetInfoForGrainType(string type)
		{
			var dashboardCollectorGrain = GrainClient.GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);
			var ids = await dashboardCollectorGrain.GetGrainIdsForType(type);

			var grainInfoGrain = GrainClient.GrainFactory.GetGrain<IMethodInvocationGrain>(type);
			var methods = new List<GrainMethod>();
			if (_allowMethodsInvocation)
			{
				methods = await grainInfoGrain.GetAvailableMethods();
			}
			var keyType = await grainInfoGrain.GetGrainKeyType();

			return Json(new {Methods = methods, Ids = ids, KeyType = keyType}, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public async Task<ActionResult> GetAllGrainTypes()
		{
			var dashboardCollectorGrain = GrainClient.GrainFactory.GetGrain<IDashboardCollectorGrain>(Guid.Empty);
			var types = await dashboardCollectorGrain.GetGrainTypes();
			return Json(types, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public async Task<ActionResult> GetGrainKeyFromType(string type)
		{
			var grainInfoGrain = GrainClient.GrainFactory.GetGrain<IMethodInvocationGrain>(type);
			var grainKeyType = await grainInfoGrain.GetGrainKeyType();
			return Json(grainKeyType, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public async Task<ActionResult> InvokeGrainMethod(string type, string id, string methodId, string parametersJson,
			bool invokeOnNewGrain = false)
		{
			if (_allowMethodsInvocation)
			{
				try
				{
					var methodGrain = GrainClient.GrainFactory.GetGrain<IMethodInvocationGrain>(type);
					var methodReturnData = await methodGrain.InvokeGrainMethod(id, methodId, parametersJson);
					return Json(methodReturnData, JsonRequestBehavior.AllowGet);

				}
				catch (Exception ex)
				{
					return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Error: " + ex.Message);
				}
			}
			return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Error: This method cannot be invoked");
		}

		private static bool AllowMethodsInvocation()
		{
			bool allowMethodsInvocation;
			if (!bool.TryParse(ConfigurationManager.AppSettings["AllowMethodsInvocation"], out allowMethodsInvocation))
			{
				allowMethodsInvocation = true;
			}

			return allowMethodsInvocation;
		}
	}
}