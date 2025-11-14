using BCrypt.Net;
using MySql.Data.MySqlClient;
using OnlineClinicApplication.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;

namespace OnlineClinicApplication.Controllers
{
    public class PatientController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;

        // GET: Register
        [AllowAnonymous]
        public ActionResult Register() => View();

        // POST: Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Patient patient)
        {
            if (!ModelState.IsValid)
                return View(patient);

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Check duplicate email
                    const string checkQuery = "SELECT COUNT(*) FROM `Patient` WHERE `Email`=@Email";
                    using (var checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", patient.Email);
                        int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (exists > 0)
                        {
                            ViewBag.ErrorMessage = "Email already registered!";
                            return View(patient);
                        }
                    }

                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(patient.Password);

                    const string insertQuery = @"
                        INSERT INTO `Patient`
                        (`Name`, `Gender`, `Age`, `Email`, `Phone`, `Password`, `Address`, `BloodGroup`, `RegistrationDate`, `Role`)
                        VALUES
                        (@Name, @Gender, @Age, @Email, @Phone, @Password, @Address, @BloodGroup, NOW(), 'Patient');";

                    using (var cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", patient.Name);
                        cmd.Parameters.AddWithValue("@Gender", patient.Gender);
                        cmd.Parameters.AddWithValue("@Age", patient.Age);
                        cmd.Parameters.AddWithValue("@Email", patient.Email);
                        cmd.Parameters.AddWithValue("@Phone", patient.Phone);
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);
                        cmd.Parameters.AddWithValue("@Address", patient.Address);
                        cmd.Parameters.AddWithValue("@BloodGroup", patient.BloodGroup);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows <= 0)
                        {
                            ViewBag.ErrorMessage = "Registration failed. Please try again.";
                            return View(patient);
                        }
                    }
                }

                TempData["SuccessMessage"] = "Registration successful! Please login to continue.";
                return RedirectToAction("Login");
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                    ViewBag.ErrorMessage = "Email already registered!";
                else
                    ViewBag.ErrorMessage = "Database error: " + ex.Message;

                return View(patient);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Unexpected error: " + ex.Message;
                return View(patient);
            }
        }

        // GET: Login
        [AllowAnonymous]
        public ActionResult Login() => View();

        // POST: Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string Email, string Password)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    const string query = "SELECT `PatientId`, `Name`, `Password` FROM `Patient` WHERE `Email`=@Email LIMIT 1";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", Email);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                ViewBag.ErrorMessage = "Email not found!";
                                return View();
                            }

                            int patientId = Convert.ToInt32(reader["PatientId"]);
                            string patientName = reader["Name"].ToString();
                            string storedPassword = reader["Password"].ToString();

                            if (BCrypt.Net.BCrypt.Verify(Password, storedPassword))
                            {
                                Session["PatientId"] = patientId;
                                Session["PatientName"] = patientName;
                                return RedirectToAction("Dashboard");
                            }
                        }
                    }

                    ViewBag.ErrorMessage = "Invalid password!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Unexpected error: " + ex.Message;
                return View();
            }
        }

        // GET & POST: Forgot/Reset Password (Single View)
        [HttpGet]
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            ViewBag.ShowReset = false;
            return View(new Patient());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(Patient model, string step)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    const string query = "SELECT `PatientId` FROM `Patient` WHERE `Email`=@Email LIMIT 1";
                    int patientId = 0;

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", model.Email);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                ModelState.AddModelError("Email", "Email not found!");
                                ViewBag.ShowReset = false;
                                return View(model);
                            }

                            patientId = Convert.ToInt32(reader["PatientId"]);
                        }
                    }

                    if (step == "email")
                    {
                        ViewBag.ShowReset = true;
                        return View(model);
                    }
                    else if (step == "reset")
                    {
                        if (string.IsNullOrWhiteSpace(model.Password) || string.IsNullOrWhiteSpace(model.ConfirmPassword))
                        {
                            ModelState.AddModelError("", "Please enter new password and confirm it.");
                            ViewBag.ShowReset = true;
                            return View(model);
                        }

                        if (model.Password != model.ConfirmPassword)
                        {
                            ModelState.AddModelError("", "Passwords do not match.");
                            ViewBag.ShowReset = true;
                            return View(model);
                        }

                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
                        const string updateQuery = "UPDATE `Patient` SET `Password`=@Password WHERE `PatientId`=@PatientId";
                        using (var updateCmd = new MySqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@Password", hashedPassword);
                            updateCmd.Parameters.AddWithValue("@PatientId", patientId);
                            updateCmd.ExecuteNonQuery();
                        }

                        TempData["SuccessMessage"] = "Password reset successful! Please login with your new password.";
                        return RedirectToAction("Login");
                    }
                }

                ViewBag.ShowReset = false;
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unexpected error: " + ex.Message);
                ViewBag.ShowReset = false;
                return View(model);
            }
        }

        // Dashboard - return Patient model with doctors and appointments
        public ActionResult Dashboard()
        {
            if (Session["PatientId"] == null)
                return RedirectToAction("Login");

            int patientId = (int)Session["PatientId"];

            var model = new Patient
            {
                PatientName = Session["PatientName"] != null ? Session["PatientName"].ToString() : "Patient",
                Doctors = new List<Doctor>(),
                Appointments = new List<Appointment>()
            };

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Doctors
                const string docQuery = "SELECT `DoctorId`,`Name`,`Specialization`,`Availability`,`ConsultationFee` FROM `Doctor`";
                using (var cmd = new MySqlCommand(docQuery, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.Doctors.Add(new Doctor
                        {
                            DoctorId = Convert.ToInt32(reader["DoctorId"]),
                            Name = reader["Name"].ToString(),
                            Specialization = reader["Specialization"].ToString(),
                            Availability = reader["Availability"].ToString(),
                            ConsultationFee = Convert.ToDecimal(reader["ConsultationFee"])
                        });
                    }
                }

                // Appointments (note: DB columns are Date/Time; alias them)
                const string apptQuery = @"
                    SELECT 
                        a.`AppointmentId`,
                        a.`Date` AS `AppointmentDate`,
                        a.`Time` AS `AppointmentTime`,
                        a.`Status`,
                        d.`Name` AS `DoctorName`,
                        d.`Specialization`
                    FROM `Appointment` a
                    JOIN `Doctor` d ON a.`DoctorId` = d.`DoctorId`
                    WHERE a.`PatientId` = @PatientId
                    ORDER BY a.`Date` DESC, a.`Time` DESC";

                using (var apptCmd = new MySqlCommand(apptQuery, conn))
                {
                    apptCmd.Parameters.AddWithValue("@PatientId", patientId);
                    using (var r = apptCmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var appt = new Appointment
                            {
                                AppointmentId = Convert.ToInt32(r["AppointmentId"]),
                                AppointmentDate = Convert.ToDateTime(r["AppointmentDate"]),
                                Status = r["Status"].ToString(),
                                DoctorName = r["DoctorName"].ToString(),
                                Specialization = r["Specialization"].ToString()
                            };

                            object timeObj = r["AppointmentTime"];
                            appt.AppointmentTime = timeObj is TimeSpan ts
                                ? ts
                                : TimeSpan.Parse(timeObj.ToString());

                            model.Appointments.Add(appt);
                        }
                    }
                }
            }

            // Compute stats without LINQ
            model.TotalAppointments = model.Appointments.Count;
            int upcoming = 0;
            Appointment next = null;
            DateTime? nextWhen = null;
            DateTime now = DateTime.Now;

            for (int i = 0; i < model.Appointments.Count; i++)
            {
                var a = model.Appointments[i];
                DateTime when = a.AppointmentDate.Date.Add(a.AppointmentTime);
                string s = a.Status != null ? a.Status.ToLower() : string.Empty;

                if (s != "cancelled" && s != "canceled" && when >= now)
                {
                    upcoming++;
                    if (!nextWhen.HasValue || when < nextWhen.Value)
                    {
                        nextWhen = when;
                        next = a;
                    }
                }
            }

            model.UpcomingAppointments = upcoming;
            model.NextAppointment = next;

            return View(model);
        }

        // POST: Book Appointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BookAppointment(Appointment appointment)
        {
            if (Session["PatientId"] == null)
                return RedirectToAction("Login");

            appointment.PatientId = (int)Session["PatientId"];

            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Use Date/Time columns (not AppointmentDate/AppointmentTime) and avoid double-booking
                    const string checkQuery = @"
                        SELECT COUNT(*) FROM `Appointment`
                        WHERE `DoctorId`=@DoctorId
                          AND `Date`=@AppointmentDate
                          AND `Time`=@AppointmentTime
                          AND `Status` <> 'Cancelled'";

                    using (var checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@DoctorId", appointment.DoctorId);
                        checkCmd.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate.Date);
                        checkCmd.Parameters.AddWithValue("@AppointmentTime", appointment.AppointmentTime);

                        int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (exists > 0)
                        {
                            TempData["ErrorMessage"] = "Selected time slot is already booked!";
                            return RedirectToAction("Dashboard");
                        }
                    }

                    const string insertQuery = @"
                        INSERT INTO `Appointment`
                        (`PatientId`,`DoctorId`,`Date`,`Time`,`Status`)
                        VALUES
                        (@PatientId,@DoctorId,@AppointmentDate,@AppointmentTime,'Booked')";

                    using (var insertCmd = new MySqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@PatientId", appointment.PatientId);
                        insertCmd.Parameters.AddWithValue("@DoctorId", appointment.DoctorId);
                        insertCmd.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate.Date);
                        insertCmd.Parameters.AddWithValue("@AppointmentTime", appointment.AppointmentTime);

                        insertCmd.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = "Appointment booked successfully!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unexpected error: " + ex.Message;
                return RedirectToAction("Dashboard");
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}