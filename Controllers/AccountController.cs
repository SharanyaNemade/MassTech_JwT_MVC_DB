using JwTDBLogin.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Data;
using Microsoft.Data.SqlClient;

namespace JwTDBLogin.Controllers
{
    public class AccountController : Controller
    {
        private IConfiguration _config;

        public AccountController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Login","Account");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            string cs = _config.GetConnectionString("dbconn")!;

            using (SqlConnection conn = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("sp_UserLogin", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Username", SqlDbType.VarChar).Value = model.Username;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = model.Password;

                conn.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string token = GenerateJwtToken(model.Username);

                    Response.Cookies.Append("jwt", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false
                    });

                    return RedirectToAction("Dashboard", "Home");
                }
            }

            ViewBag.Error = "Invalid Username or Password";
            return View();
        }

        private string GenerateJwtToken(string username)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username)
            };

            var token = new JwtSecurityToken(
                issuer: "JwtMvcDemo",
                audience: "JwtMvcDemoUsers",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        
    }
}