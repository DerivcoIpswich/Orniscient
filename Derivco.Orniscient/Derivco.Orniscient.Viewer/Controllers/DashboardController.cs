using System.Web.Mvc;

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
    }
}