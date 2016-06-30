using System.Threading.Tasks;
using System.Web.Mvc;

namespace Derivco.Orniscient.Viewer.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}