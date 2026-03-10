using JwTDBLogin.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Data;
using Microsoft.Data.SqlClient;
using OtpNet;
using QRCoder;

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
            return RedirectToAction("Login", "Account");
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

        // LOGIN STEP
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
                    // Generate Secret Key
                    byte[] secretKey = KeyGeneration.GenerateRandomKey(20);
                    string base32Secret = Base32Encoding.ToString(secretKey);

                    HttpContext.Session.SetString("User", model.Username);
                    HttpContext.Session.SetString("TwoFASecret", base32Secret);

                    return RedirectToAction("Setup2FA");
                }
            }

            ViewBag.Error = "Invalid Username or Password";
            return View();
        }

        // DISPLAY QR CODE
        public IActionResult Setup2FA()
        {
            string? secret = HttpContext.Session.GetString("TwoFASecret");
            string? username = HttpContext.Session.GetString("User");

            if (secret == null || username == null)
                return RedirectToAction("Login");

            string qrCodeImage = GenerateQrCode(secret, username);

            ViewBag.QRCode = qrCodeImage;
            ViewBag.Secret = secret;

            return View();
        }

        // VERIFY AUTHENTICATOR CODE
        [HttpPost]
        public IActionResult Verify2FA(string code)
        {
            string? secret = HttpContext.Session.GetString("TwoFASecret");
            string? username = HttpContext.Session.GetString("User");

            if (secret == null || username == null)
                return RedirectToAction("Login");

            byte[] secretBytes = Base32Encoding.ToBytes(secret);

            Totp totp = new Totp(secretBytes);

            bool isValid = totp.VerifyTotp(code, out long step);

            if (isValid)
            {
                string token = GenerateJwtToken(username);

                Response.Cookies.Append("jwt", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false
                });

                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid Code";
            return View("Setup2FA");
        }

        // QR CODE GENERATION
        private string GenerateQrCode(string secretKey, string username)
        {
            string uri = $"otpauth://totp/JwtMvcDemo:{username}?secret={secretKey}&issuer=JwtMvcDemo";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);

            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20);

            return "data:image/png;base64," + Convert.ToBase64String(qrCodeImage);
        }

        // JWT TOKEN
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