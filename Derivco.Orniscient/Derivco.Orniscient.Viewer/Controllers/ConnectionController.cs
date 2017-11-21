using System;
using System.Linq;
using Derivco.Orniscient.Viewer.Clients;
using Derivco.Orniscient.Viewer.Models.Connection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Derivco.Orniscient.Viewer.Controllers
{
    public class ConnectionController : Controller
    {
        public Task<ViewResult> Index()
        {
            return Task.FromResult(View());
        }

        [HttpPost]
        public async Task<ActionResult> Index(ConnectionInfo connection)
        {
            try
            {
                ValidateModel(connection);

                return RedirectToAction("Index", "Dashboard", connection);
            }
            catch(Exception ex)
            {
                ViewBag.Error = "Connection Unsuccessful";
                return View();
            }
        }
    }
}