using System.Web.Mvc;

namespace SisypheanSolutions.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            return PartialView("_GeneralErrorPartial");
        }

        public ActionResult NotFound()
        {
            return PartialView("_NotFoundPartial");
        }

        public ActionResult InternalServer()
        {
            return PartialView("_InternalServerPartial");
        }
    }
}