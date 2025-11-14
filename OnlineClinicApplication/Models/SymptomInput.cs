using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace OnlineClinicApplication.Models
{
    public class SymptomInput
    {
        [Required(ErrorMessage = "Please describe your symptoms.")]
        [Display(Name = "Describe Symptoms")]
        public string Description { get; set; }
    }
}