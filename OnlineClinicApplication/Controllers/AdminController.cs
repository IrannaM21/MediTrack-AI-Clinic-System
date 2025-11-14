using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
using OnlineClinicApplication.Models;
using BCrypt.Net;

namespace OnlineClinicApplication.Controllers
{
    public class AdminController : Controller
    {
        string connectionString = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;

        // GET: Admin Register
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Admin admin)
        {
            if (!ModelState.IsValid)
                return View(admin);

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Check for existing email
                string checkQuery = "SELECT COUNT(*) FROM Admin WHERE Email=@Email";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@Email", admin.Email);
                int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (exists > 0)
                {
                    ViewBag.ErrorMessage = "Email already registered!";
                    return View(admin);
                }

                // Hash password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(admin.Password);

                // Insert admin
                string insertQuery = @"INSERT INTO Admin (Username, Email, Password, Role) 
                                       VALUES (@Username, @Email, @Password, 'Admin')";
                MySqlCommand cmd = new MySqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@Username", admin.Username);
                cmd.Parameters.AddWithValue("@Email", admin.Email);
                cmd.Parameters.AddWithValue("@Password", hashedPassword);
                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = "Registration successful! Please login.";
            return RedirectToAction("Login");
        }

        // GET: Admin Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string Email, string Password)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT AdminId, Username, Password FROM Admin WHERE Email=@Email";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", Email);

                var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    ViewBag.ErrorMessage = "Email not found!";
                    return View();
                }

                int adminId = Convert.ToInt32(reader["AdminId"]);
                string username = reader["Username"].ToString();
                string storedPassword = reader["Password"].ToString();
                reader.Close();

                if (BCrypt.Net.BCrypt.Verify(Password, storedPassword))
                {
                    Session["AdminId"] = adminId;
                    Session["AdminName"] = username;
                    return RedirectToAction("Dashboard");
                }

                ViewBag.ErrorMessage = "Invalid password!";
                return View();
            }
        }

        // GET: Dashboard
        public ActionResult Dashboard()
        {
            if (Session["AdminId"] == null)
                return RedirectToAction("Login");

            ViewBag.AdminName = Session["AdminName"].ToString();
            return View();
        }

        // GET & POST: Forgot Password (Single View)
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            ViewBag.ShowResetFields = false;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string Email, string NewPassword, string ConfirmPassword, string step)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT AdminId FROM Admin WHERE Email=@Email";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", Email);

                var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    reader.Close();
                    ViewBag.ErrorMessage = "Email not found!";
                    ViewBag.ShowResetFields = false;
                    return View();
                }

                int adminId = Convert.ToInt32(reader["AdminId"]);
                reader.Close();

                if (step == "email") // Show reset fields
                {
                    ViewBag.Email = Email;
                    ViewBag.ShowResetFields = true;
                    return View();
                }
                else if (step == "reset") // Update password
                {
                    if (string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
                    {
                        ViewBag.ErrorMessage = "Please enter new password and confirm it.";
                        ViewBag.Email = Email;
                        ViewBag.ShowResetFields = true;
                        return View();
                    }

                    if (NewPassword != ConfirmPassword)
                    {
                        ViewBag.ErrorMessage = "Passwords do not match!";
                        ViewBag.Email = Email;
                        ViewBag.ShowResetFields = true;
                        return View();
                    }

                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                    string updateQuery = "UPDATE Admin SET Password=@Password WHERE AdminId=@AdminId";
                    MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn);
                    updateCmd.Parameters.AddWithValue("@Password", hashedPassword);
                    updateCmd.Parameters.AddWithValue("@AdminId", adminId);
                    updateCmd.ExecuteNonQuery();

                    TempData["SuccessMessage"] = "Password reset successful! Please login.";
                    return RedirectToAction("Login");
                }
            }

            ViewBag.ShowResetFields = false;
            return View();
        }

        // Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
