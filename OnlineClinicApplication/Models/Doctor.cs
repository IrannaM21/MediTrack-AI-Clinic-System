using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace OnlineClinicApplication.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can contain only letters and spaces")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Phone must be 10-15 digits")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&]).{6,}$",
            ErrorMessage = "Password must be at least 6 characters, include 1 letter, 1 number, and 1 special character")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Specialization is required")]
        public string Specialization { get; set; }

        public string Qualification { get; set; }

        [Range(0, 50, ErrorMessage = "Experience must be between 0-50 years")]
        public int Experience { get; set; }

        public string ClinicAddress { get; set; }

        public string Availability { get; set; }

        [Range(0, 10000, ErrorMessage = "Consultation fee must be valid")]
        public decimal ConsultationFee { get; set; }

        public string Status { get; set; } = "Pending";

        public string Photo { get; set; }

        public string DocumentFile { get; set; }

        public string Role { get; set; } = "Doctor";

        // For file upload
        [Display(Name = "Profile Photo")]
        public HttpPostedFileBase PhotoFile { get; set; }

        [Display(Name = "Document File")]
        public HttpPostedFileBase DocumentUpload { get; set; }
    }
}
