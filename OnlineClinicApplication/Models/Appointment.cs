using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineClinicApplication.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Please select appointment date")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Please select appointment time")]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; }

        // Match DB default
        public string Status { get; set; } = "Booked";

        // Optional display for views
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string Specialization { get; set; }

        // Aliases (not used by EF, only for convenience)
        public DateTime Date
        {
            get => AppointmentDate;
            set => AppointmentDate = value;
        }

        public TimeSpan Time
        {
            get => AppointmentTime;
            set => AppointmentTime = value;
        }
    }
}