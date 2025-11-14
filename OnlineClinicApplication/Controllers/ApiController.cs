using System.Collections.Generic;
using System.Web.Mvc;

namespace OnlineClinicApplication.Controllers
{
    public class ApiController : Controller
    {
        [HttpPost]
        public JsonResult Predict(string text)
        {
            // Simple AI placeholder logic
            string specialty = "General Physician";
            string urgency = "Normal";

            if (text.ToLower().Contains("fever")) specialty = "Internal Medicine";
            if (text.ToLower().Contains("chest pain")) { specialty = "Cardiology"; urgency = "High"; }

            var scores = new Dictionary<string, int>
            {
                {"Cardiology", text.Contains("chest")?90:10 },
                {"Neurology", text.Contains("headache")?80:20 },
                {"General", 50 }
            };

            return Json(new { specialty, urgency, scores });
        }
    }
}
