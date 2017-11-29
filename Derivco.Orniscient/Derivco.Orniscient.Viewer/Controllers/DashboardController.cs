using Derivco.Orniscient.Proxy.Grains;
using Derivco.Orniscient.Proxy.Grains.Filters;
using Derivco.Orniscient.Viewer.Clients;
using Derivco.Orniscient.Viewer.Models.Dashboard;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Derivco.Orniscient.Proxy.Grains.Models;
using Derivco.Orniscient.Viewer.Models.Connection;
using Derivco.Orniscient.Viewer.Observers;

namespace Derivco.Orniscient.Viewer.Controllers
{
    public class DashboardController : Controller
    {
        private static bool _allowMethodsInvocation;

        private string GrainSessionId => HttpContext.Request.Cookies.AllKeys.Contains("GrainSessionId") ? HttpContext.Request.Cookies["GrainSessionId"]?.Value : string.Empty;

        // GET: Dashboard
        public async Task<ViewResult> Index(ConnectionInfo connection)
        {
            try
            {
                await CleanupConnection(connection);

                if(!HttpContext.Request.Cookies.AllKeys.Contains("GrainSessionId"))
                {
                    var grainSessionIdKey = GrainClientMultiton.RegisterClient(connection.Address, connection.Port);
                    HttpContext.Response.Cookies.Add(new HttpCookie("GrainSessionId", grainSessionIdKey));
                }

                _allowMethodsInvocation = AllowMethodsInvocation();
                ViewBag.AllowMethodsInvocation = _allowMethodsInvocation;
                return View();
            }
            catch (Exception ex)
            {
                return View("InitError");
            }
        }

        private async Task CleanupConnection(ConnectionInfo connection)
        {
            if (HttpContext.Request.Cookies.AllKeys.Contains("GrainSessionId"))
            {
                var client = GrainClientMultiton.GetClient(GrainSessionId);
                if (client == null)
                    return;

                var gateway = client.Configuration.Gateways.First();
                
                if (gateway.Address.ToString() != connection.Address &&
                    !Equals(Dns.GetHostEntry(connection.Address).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork), gateway.Address))
                {
                    if (gateway.Port != connection.Port)
                    {
                        await CleanupClient();
                        RemoveCookie("GrainSessionId");
                    }
                }
            }
        }

        public async Task<ActionResult> Disconnect()
        {
            await CleanupClient();
            RemoveCookie("GrainSessionId");
            return RedirectToAction("Index", "Connection");
        }

        private async Task CleanupClient()
        {
            await OrniscientObserver.Instance.UnregisterGrainClient(GrainSessionId);
            GrainClientMultiton.RemoveClient(GrainSessionId);
        }

        public async Task<ActionResult> GetDashboardInfo()
		{
            var clusterClient = await GrainClientMultiton.GetAndConnectClient(GrainSessionId);
            var dashboardCollectorGrain = clusterClient.GetGrain<IDashboardCollectorGrain>(Guid.Empty);

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

		    var clusterClient = await GrainClientMultiton.GetAndConnectClient(GrainSessionId);
            var filterGrain = clusterClient.GetGrain<IFilterGrain>(Guid.Empty);
			var filters = await filterGrain.GetGroupedFilterValues(filtersRequest.Types);
			return Json(filters, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public async Task<ActionResult> GetGrainInfo(GetGrainInfoRequest grainInfoRequest)
		{
		    var clusterClient = await GrainClientMultiton.GetAndConnectClient(GrainSessionId);
            var typeFilterGrain = clusterClient.GetGrain<IFilterGrain>(Guid.Empty);
			var filters = await typeFilterGrain.GetFilters(grainInfoRequest.GrainType, grainInfoRequest.GrainId);
			return Json(filters);
		}

		[HttpPost]
		public async Task SetSummaryViewLimit(int summaryViewLimit)
		{
		    var clusterClient = await GrainClientMultiton.GetAndConnectClient(GrainSessionId);
            var dashboardInstanceGrain = clusterClient.GetGrain<IDashboardInstanceGrain>(0);
			await dashboardInstanceGrain.SetSummaryViewLimit(summaryViewLimit);
		}

		[HttpPost]
		public async Task<ActionResult> GetInfoForGrainType(string type)
		{
		    var clusterClient = await GrainClientMultiton.GetAndConnectClient(GrainSessionId);
            var dashboardCollectorGrain = clusterClient.GetGrain<IDashboardCollectorGrain>(Guid.Empty);
			var ids = await dashboardCollectorGrain.GetGrainIdsForType(type);

            var grainInfoGrain = clusterClient.GetGrain<IMethodInvocationGrain>(type);
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
		    var clusterClient = await GrainClientMultiton.GetAndConnectClient(GrainSessionId);
            var dashboardCollectorGrain = clusterClient.GetGrain<IDashboardCollectorGrain>(Guid.Empty);
			var types = await dashboardCollectorGrain.GetGrainTypes();
			return Json(types, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public async Task<ActionResult> GetGrainKeyFromType(string type)
		{
		    var clusterClient = await GrainClientMultiton.GetAndConnectClient(GrainSessionId);
            var grainInfoGrain = clusterClient.GetGrain<IMethodInvocationGrain>(type);
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
				    var clusterClient = await GrainClientMultiton.GetAndConnectClient(GrainSessionId);
                    var methodGrain = clusterClient.GetGrain<IMethodInvocationGrain>(type);
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

        private void RemoveCookie(string cookieName)
        {
            if (Request.Cookies[cookieName] != null)
            {
                HttpCookie myCookie = new HttpCookie(cookieName) {Expires = DateTime.Now.AddDays(-1d)};
                Response.Cookies.Add(myCookie);
            }
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