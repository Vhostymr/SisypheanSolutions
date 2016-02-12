using System.Web.Mvc;

namespace SisypheanSolutions.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            return PartialView("_GeneralError");
        }

        public ActionResult NotFoundPartial()
        {
            return PartialView("_NotFound");
        }

        public ActionResult UnauthorizedPartial()
        {
            return PartialView("_Unauthorized");
        }

        public ActionResult InternalServerPartial()
        {
            return PartialView("_InternalServer");
        }

        public ActionResult FileNotFoundPartial()
        {
            return PartialView("_FileNotFound");
        }

        public ActionResult DuplicateFilePartial()
        {
            return PartialView("_DuplicateFile");
        }
    }
}