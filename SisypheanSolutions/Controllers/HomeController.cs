using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SisypheanSolutions.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Home()
        {
            return PartialView("_Home");
        }

        public ActionResult FileManager()
        {
            return PartialView("_FileUpload");
        }
        public ActionResult FileDownload()
        {
            return PartialView("_FileDownload");
        }

        public ActionResult About()
        {
            return PartialView("_About");
        }

        public ActionResult Contact()
        {
            return PartialView("_Contact");
        }
    }
}