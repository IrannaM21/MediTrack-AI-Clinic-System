using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineClinicApplication.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View(); // Views/Home/Register.cshtml
        }

        [HttpPost]
        public ActionResult Register(FormCollection form)
        {
            // You can handle registration logic here
            // e.g., save to database
            ViewBag.Message = "Registration Successful!";
            return View();
        }
    }
}