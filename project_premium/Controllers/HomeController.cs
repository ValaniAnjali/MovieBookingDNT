using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Mvc;
using project_premium.Models;

namespace project_premium.Controllers
{
    public class HomeController : Controller
    {
        private readonly string connectionString = "Data Source=DESKTOP-VHJCCLR\\SQLEXPRESS;Initial Catalog=Demo;Integrated Security=True";

        // Display the list of movies
        public ActionResult Index()
        {
            List<Movie> movies = new List<Movie>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM Movies", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    movies.Add(new Movie
                    {
                        Id = (int)reader["Id"],
                        Name = (string)reader["Name"],
                        ReleaseDate = (DateTime)reader["ReleaseDate"],
                        Description = (string)reader["Description"],
                        ImagePath = (string)reader["ImagePath"]
                    });
                }
            }

            return View(movies);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
        public ActionResult Booking()
        {
            return View();
        }

        // GET: Create a new movie
        [HttpGet]
        public ActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        // POST: Save a new movie
        [HttpPost]
        public ActionResult Create(Movie movie, HttpPostedFileBase imageFile)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    string imagesPath = Server.MapPath("~/Images/");
                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                    }

                    string path = Path.Combine(imagesPath, Path.GetFileName(imageFile.FileName));
                    imageFile.SaveAs(path);
                    movie.ImagePath = "/Images/" + Path.GetFileName(imageFile.FileName);
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("INSERT INTO Movies (Name, ReleaseDate, Description, ImagePath) VALUES (@Name, @ReleaseDate, @Description, @ImagePath)", connection);
                    command.Parameters.AddWithValue("@Name", movie.Name);
                    command.Parameters.AddWithValue("@ReleaseDate", movie.ReleaseDate);
                    command.Parameters.AddWithValue("@Description", movie.Description);
                    command.Parameters.AddWithValue("@ImagePath", movie.ImagePath ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }

                return RedirectToAction("Index");
            }
            return View(movie);
        }

        // GET: Edit a movie
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login");
            }

            Movie movie = null;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM Movies WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    movie = new Movie
                    {
                        Id = (int)reader["Id"],
                        Name = (string)reader["Name"],
                        ReleaseDate = (DateTime)reader["ReleaseDate"],
                        Description = (string)reader["Description"],
                        ImagePath = (string)reader["ImagePath"]
                    };
                }
            }

            if (movie == null) return HttpNotFound();
            return View(movie);
        }

        // POST: Update a movie
        [HttpPost]
        public ActionResult Edit(Movie movie)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("UPDATE Movies SET Name = @Name, ReleaseDate = @ReleaseDate, Description = @Description WHERE Id = @Id", connection);
                    command.Parameters.AddWithValue("@Name", movie.Name);
                    command.Parameters.AddWithValue("@ReleaseDate", movie.ReleaseDate);
                    command.Parameters.AddWithValue("@Description", movie.Description);
                    command.Parameters.AddWithValue("@Id", movie.Id);
                    command.ExecuteNonQuery();
                }

                return RedirectToAction("Index");
            }
            return View(movie);
        }

        // GET: Delete a movie
        public ActionResult Delete(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login");
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM Movies WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        // GET: Login page
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: Process login
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            bool isValidUser = ValidateUser(username, password); // Implement this method according to your logic

            if (isValidUser)
            {
                Session["Username"] = username; // Set session variable for username
                Session["IsAdmin"] = true; // Or set based on user role
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View();
            }
        }

        // Logout action
        public ActionResult Logout()
        {
            // Clear the session and redirect to the home page
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // Validate user credentials
        private bool ValidateUser(string username, string password)
        {
            //admin credentials
            return username == "admin" && password == "admin";
        }

        private bool IsAdmin()
        {
            return Session["IsAdmin"] != null && (bool)Session["IsAdmin"];
        }
    }
}




//note:in sqlserver 2014: inside demo database create movies table:
//    CREATE TABLE Movies (
//    Id INT PRIMARY KEY IDENTITY,
//    Name NVARCHAR(100) NOT NULL,
//    ReleaseDate DATE NOT NULL,
//    Description NVARCHAR(255),
//    ImagePath NVARCHAR(255)
//);


//select* from Movies