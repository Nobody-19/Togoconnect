// ============================================================
//  TogoConnect – HomeController
//  Fichier : Controllers/HomeController.cs
// ============================================================
using System.Web.Mvc;

namespace TogoConnect.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
