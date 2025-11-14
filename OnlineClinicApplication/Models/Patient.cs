using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineClinicApplication.Models
{
    public class Patient
    {
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Name should contain letters and spaces only")]
        public string Name { get; set; }

        // Alias to keep your view code using Model.PatientName working
        public string PatientName
        {
            get => Name;
            set => Name = value;
        }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Age is required")]
        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter valid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone must be 10 digits")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@#$%^&+=!]).{8,}$",
            ErrorMessage = "Password must be at least 8 characters, contain uppercase, lowercase, number, and special character")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Password and Confirm Password must match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Blood Group is required")]
        [RegularExpression(@"^(A|B|AB|O)[+-]$", ErrorMessage = "Enter valid blood group (e.g., A+, O-)")]
        public string BloodGroup { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public string Role { get; set; } = "Patient";

        // Dashboard properties
        public List<Doctor> Doctors { get; set; } = new List<Doctor>();
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
        public int TotalAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
        public Appointment NextAppointment { get; set; }
    }
}