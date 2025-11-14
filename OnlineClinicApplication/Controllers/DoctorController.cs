using BCrypt.Net;
using MySql.Data.MySqlClient;
using OnlineClinicApplication.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace OnlineClinicApplication.Controllers
{
    public class DoctorController : Controller
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;

        // GET: Doctor/Register
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // POST: Doctor/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Doctor doctor)
        {
            if (!ModelState.IsValid)
                return View(doctor);

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Check duplicate email
                string checkQuery = "SELECT COUNT(*) FROM Doctor WHERE Email=@Email";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@Email", doctor.Email);
                int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (exists > 0)
                {
                    ViewBag.ErrorMessage = "Email already registered!";
                    return View(doctor);
                }

                // Hash password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(doctor.Password);

                // Photo upload
                string photoPath = null;
                if (doctor.PhotoFile != null && doctor.PhotoFile.ContentLength > 0)
                {
                    string photoFileName = Guid.NewGuid() + "_" + System.IO.Path.GetFileName(doctor.PhotoFile.FileName);
                    string physicalPath = Server.MapPath("~/Uploads/Photos/" + photoFileName);
                    doctor.PhotoFile.SaveAs(physicalPath);
                    photoPath = "/Uploads/Photos/" + photoFileName;
                }

                // Document upload
                string docPath = null;
                if (doctor.DocumentUpload != null && doctor.DocumentUpload.ContentLength > 0)
                {
                    string docFileName = Guid.NewGuid() + "_" + System.IO.Path.GetFileName(doctor.DocumentUpload.FileName);
                    string physicalPath = Server.MapPath("~/Uploads/Documents/" + docFileName);
                    doctor.DocumentUpload.SaveAs(physicalPath);
                    docPath = "/Uploads/Documents/" + docFileName;
                }

                // Insert doctor
                string insertQuery = @"INSERT INTO Doctor 
                            (Name, Gender, Email, Phone, Password, Specialization, Qualification, Experience, ClinicAddress, Availability, ConsultationFee, Status, Role, Photo, DocumentFile)
                            VALUES (@Name, @Gender, @Email, @Phone, @Password, @Specialization, @Qualification, @Experience, @ClinicAddress, @Availability, @ConsultationFee, 'Pending', 'Doctor', @Photo, @Document)";
                MySqlCommand cmd = new MySqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@Name", doctor.Name);
                cmd.Parameters.AddWithValue("@Gender", doctor.Gender);
                cmd.Parameters.AddWithValue("@Email", doctor.Email);
                cmd.Parameters.AddWithValue("@Phone", doctor.Phone);
                cmd.Parameters.AddWithValue("@Password", hashedPassword);
                cmd.Parameters.AddWithValue("@Specialization", doctor.Specialization);
                cmd.Parameters.AddWithValue("@Qualification", doctor.Qualification);
                cmd.Parameters.AddWithValue("@Experience", doctor.Experience);
                cmd.Parameters.AddWithValue("@ClinicAddress", doctor.ClinicAddress);
                cmd.Parameters.AddWithValue("@Availability", doctor.Availability);
                cmd.Parameters.AddWithValue("@ConsultationFee", doctor.ConsultationFee);
                cmd.Parameters.AddWithValue("@Photo", photoPath);
                cmd.Parameters.AddWithValue("@Document", docPath);
                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = "Registration successful! Please login.";
            return RedirectToAction("Login");
        }

        // GET: Doctor/Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: Doctor/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string Email, string Password)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT DoctorId, Name, Password FROM Doctor WHERE Email=@Email";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", Email);

                var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    ViewBag.ErrorMessage = "Email not found!";
                    return View();
                }

                int doctorId = Convert.ToInt32(reader["DoctorId"]);
                string doctorName = reader["Name"].ToString();
                string storedPassword = reader["Password"].ToString();
                reader.Close();

                if (BCrypt.Net.BCrypt.Verify(Password, storedPassword))
                {
                    Session["DoctorId"] = doctorId;
                    Session["DoctorName"] = doctorName;
                    Session["DoctorEmail"] = Email;

                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid credentials!";
                    return View();
                }
            }
        }

        // GET: Doctor/Dashboard
        // GET: Doctor/Dashboard
        public ActionResult Dashboard()
        {
            if (Session["DoctorId"] == null)
                return RedirectToAction("Login");

            int doctorId = Convert.ToInt32(Session["DoctorId"]);
            string doctorName = Session["DoctorName"]?.ToString() ?? "Doctor";

            List<Appointment> appointments = new List<Appointment>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"SELECT a.AppointmentId, a.Date, a.Time, a.Status, p.Name AS PatientName
                         FROM Appointment a
                         JOIN Patient p ON a.PatientId = p.PatientId
                         WHERE a.DoctorId=@DoctorId
                         ORDER BY a.Date ASC, a.Time ASC";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@DoctorId", doctorId);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    appointments.Add(new Appointment
                    {
                        AppointmentId = Convert.ToInt32(reader["AppointmentId"]),
                        Date = Convert.ToDateTime(reader["Date"]),
                        Time = (TimeSpan)reader["Time"],
                        Status = reader["Status"].ToString(),
                        PatientName = reader["PatientName"].ToString()
                    });
                }
            }

            // Group appointments by date
            var groupedAppointments = appointments.GroupBy(a => a.Date).ToList();
            ViewBag.GroupedAppointments = groupedAppointments;
            ViewBag.DoctorName = doctorName;

            return View();
        }


       

        // GET: Doctor/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
