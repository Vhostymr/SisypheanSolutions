using System.Web.Mvc;

namespace MySite.Controllers
{
    public class ErrorController : Controller
    {
        public ViewResult Index()
        {
            return View("GeneralError");
        }

        public ViewResult NotFound()
        {
            return View("NotFound");
        }

        public ViewResult InternalServer()
        {
            return View("InternalServer");
        }
    }
}