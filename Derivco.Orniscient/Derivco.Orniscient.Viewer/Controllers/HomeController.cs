using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Derivco.Orniscient.TestImplementation;


namespace Derivco.Orniscient.Viewer.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            return RedirectToAction("Index", "Dashboard");

            //var temp = new OrnManagement();
            //var stats = await temp.GetGrainStats();

            //foreach (var grnstat in stats)
            //{
            //    this.Response.Write($"Grain Type : {grnstat.GrainType }, Grain Id : {grnstat.GrainIdentity.PrimaryKey}, Category : {grnstat.Category}");
            //}

            //return View();
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