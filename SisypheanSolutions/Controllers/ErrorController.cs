using System.Web.Mvc;

namespace SisypheanSolutions.Controllers
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