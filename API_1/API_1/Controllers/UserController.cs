using System.IdentityModel.Tokens.Jwt;
using API_1.Data;
using Dnp.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MailKit.Net.Smtp;
using API_1.Models;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Concurrent;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Net;
using API_1.Security;
using System.Globalization;
using System.Text.Json;


namespace API_1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly string smtpServer = "smtp.gmail.com";
        private readonly int smtpPort = 587;
        private readonly string smtpUser = "samirpadghane@gmail.com";
        private readonly string smtpPassword = "axgnozqovgxgfuqv";
        private readonly string fromEmail = "samirpadghane@gmail.com";
        private readonly string rsaPrivateKey= RsaEncryption.GetPrivateKey();
        private readonly string rsaPublicKey= RsaEncryption.GetPublicKey();


        private readonly AppDbContext _dbContext;
        private readonly Auth _auth;

        public UserController(AppDbContext dbContext, Auth auth)
        {
            _dbContext = dbContext;
            _auth = auth;
        }

        [HttpGet("reset-options")]
        public async Task<IActionResult> GetResetOptions(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return BadRequest(new { message = "Username cannot be null or empty" });
                }

                var user = await _dbContext.user.FirstOrDefaultAsync(u => u.username == username);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var options = new List<string>();

                options.Add("Email OTP");
                if (user.is_question)
                {
                    options.Add("Security Question");
                }
                if (user.is_number)
                {
                    options.Add("Send OTP to Phone Number");
                }

                if (options.Count == 0)
                {
                    return Conflict(new { message = "No password reset options available for this user." });
                }

                return Ok(new
                {
                    message = "Choose a method to reset your password.",
                    username = user.username,
                    resetMethods = options
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = "Database error occurred.", error = dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
            }
        }




        [HttpPost("send-email-otp")]
        public async Task<IActionResult> SendEmail(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return Conflict(new { message = "Username cannot be null or empty." });
            }

            var userotp = await _dbContext.userOtp.FirstOrDefaultAsync(ele => ele.username == username);
            if (userotp == null)
            {
                return Conflict(new { message = "User with this username does not exist." });
            }

            if (string.IsNullOrEmpty(userotp.email))
            {
                return Conflict(new { message = "User does not have a registered email." });
            }

            var rsaPublicKey = RsaEncryption.GetPublicKey();
            if (string.IsNullOrEmpty(rsaPublicKey))
            {
                return StatusCode(500, new { message = "Public key is missing or not configured properly." });
            }

            var recipientEmail = userotp.email;
            try
            {
                Random rand = new Random();
                int randomNumber = rand.Next(100000, 1000000);
                string encryptedOtp = RSAEncryptOtp.EncryptOtp(randomNumber.ToString(), rsaPublicKey);

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("myapp", fromEmail));
                emailMessage.To.Add(new MailboxAddress("Recipient", recipientEmail));
                emailMessage.Subject = "Your OTP for Password Reset";
                emailMessage.Body = new TextPart("plain") { Text = $"Your OTP: {randomNumber}. Valid for 30 minutes." };

                using (var smtpClient = new SmtpClient())
                {
                    await smtpClient.ConnectAsync(smtpServer, smtpPort, false);
                    await smtpClient.AuthenticateAsync(smtpUser, smtpPassword);
                    await smtpClient.SendAsync(emailMessage);
                    await smtpClient.DisconnectAsync(true);
                }

                userotp.otp = encryptedOtp;
                userotp.validity = DateTime.UtcNow.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss");
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "OTP email sent successfully." });
            }
            catch (MailKit.Net.Smtp.SmtpCommandException ex)
            {
                return StatusCode(500, new { message = "SMTP command error.", error = ex.Message });
            }
            catch (MailKit.Net.Smtp.SmtpProtocolException ex)
            {
                return StatusCode(500, new { message = "SMTP protocol error.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while sending the email.", error = ex.Message });
            }
        }




        [HttpPost("check-sms-otp")]
        public async Task<ActionResult<UserOtp>> checkotp(string username, string otp)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(otp))
            {
                return Conflict(new { message = "OTP cannot be null or empty." });
            }

            try
            {
                var userotp = await _dbContext.userNumber.FirstOrDefaultAsync(ele => ele.username == username);
                
                if (userotp == null)
                {
                    return Conflict(new { message = "User not found." });
                }

                if (string.IsNullOrEmpty(userotp.Otp) || string.IsNullOrEmpty(userotp.Validity))
                {
                    return Conflict(new { message = "OTP is missing or invalid." });
                }
                //var expiryTime = userotp.validity;
                if (!DateTime.TryParseExact(userotp.Validity, "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal, out DateTime expiryTime))
                {
                    return StatusCode(500, new { message = "Invalid OTP expiry format." });
                }

                if (expiryTime <= DateTime.UtcNow)
                {
                    return Conflict(new { message = "OTP expired." });
                }

                string decryptedOtp;
                try
                {
                    decryptedOtp = RSADecryptOtp.DecryptOtp(userotp.Otp, rsaPrivateKey);
                    Console.WriteLine($"Decrypted OTP: '{decryptedOtp}'");
                    Console.WriteLine($"Received OTP: '{otp}'");
                    Console.WriteLine($"Are they equal? {decryptedOtp == otp}");

                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error decrypting OTP.", error = ex.Message });
                }

                if (decryptedOtp == otp)
                {
                    return Ok(new { message = "You can reset your password." });
                }
                else
                {
                    return Conflict(new { message = "Incorrect OTP." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }


        [HttpPost("check-email-otp")]
        public async Task<ActionResult<UserOtp>> checkemailotp(string username, string otp)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(otp))
            {
                return Conflict(new { message = "Email and OTP cannot be null or empty." });
            }

            try
            {
                var userotp = await _dbContext.userOtp.FirstOrDefaultAsync(ele => ele.username == username);

                if (userotp == null)
                {
                    return Conflict(new { message = "User not found." });
                }

                if (string.IsNullOrEmpty(userotp.otp) || string.IsNullOrEmpty(userotp.validity))
                {
                    return Conflict(new { message = "OTP is missing or invalid." });
                }
                //var expiryTime = userotp.validity;
                if (!DateTime.TryParseExact(userotp.validity, "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal, out DateTime expiryTime))
                {
                    return StatusCode(500, new { message = "Invalid OTP expiry format." });
                }

                if (expiryTime <= DateTime.UtcNow)
                {
                    return Conflict(new { message = "OTP expired." });
                }

                string decryptedOtp;
                try
                {
                    decryptedOtp = RSADecryptOtp.DecryptOtp(userotp.otp, rsaPrivateKey);
                    Console.WriteLine($"Decrypted OTP: '{decryptedOtp}'");
                    Console.WriteLine($"Received OTP: '{otp}'");
                    Console.WriteLine($"Are they equal? {decryptedOtp == otp}");

                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error decrypting OTP.", error = ex.Message });
                }

                if (decryptedOtp == otp)
                {
                    return Ok(new { message = "You can reset your password." });
                }
                else
                {
                    return Conflict(new { message = "Incorrect OTP." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }




        //[HttpPost("sendEmail")]
        //public async Task<IActionResult> SendEmail(string username, string question, string answer)
        //{
        //    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(question) || string.IsNullOrEmpty(answer))
        //    {
        //        return Conflict(new { message = "Username, question, and answer cannot be null or empty." });
        //    }

        //    try
        //    {
        //        var user1 = await _dbContext.user.FirstOrDefaultAsync(ele => ele.username == username);
        //        if (user1 == null || string.IsNullOrEmpty(user1.email))
        //        {
        //            return Conflict(new { message = "User not found or email missing." });
        //        }

        //        var useremail = await _dbContext.userQuestions.FirstOrDefaultAsync(ele => ele.email == user1.email);
        //        if (useremail == null)
        //        {
        //            return Conflict(new { message = "User with this username does not exist." });
        //        }

        //        if (question != useremail.question)
        //        {
        //            return Conflict(new { message = "Incorrect security question." });
        //        }

        //        if (answer != useremail.answer)
        //        {
        //            return Conflict(new { message = "Incorrect security answer." });
        //        }

        //        return Ok(new { message = "Success!" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
        //    }
        //}



        [HttpPost("resetpassword")]
        public async Task<ActionResult<User>> ResetPassword(string username, string newPassword)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(newPassword))
            {
                return Conflict(new { message = "Username and password cannot be null or empty." });
            }

            try
            {
                var user = await _dbContext.user.FirstOrDefaultAsync(ele => ele.username == username);
                if (user == null)
                {
                    return Conflict(new { message = "User with this username does not exist." });
                }

                if (newPassword.Length < 10)
                {
                    return Conflict(new { message = "Password must be at least 10 characters long." });
                }

                bool hasUpper = false, hasLower = false, hasDigit = false, hasSpecial = false;
                foreach (char ch in newPassword)
                {
                    if (char.IsUpper(ch)) hasUpper = true;
                    else if (char.IsLower(ch)) hasLower = true;
                    else if (char.IsDigit(ch)) hasDigit = true;
                    else hasSpecial = true;
                }

                if (!hasUpper) return Conflict(new { message = "Password must contain at least one uppercase letter." });
                if (!hasLower) return Conflict(new { message = "Password must contain at least one lowercase letter." });
                if (!hasDigit) return Conflict(new { message = "Password must contain at least one numeric value." });
                if (!hasSpecial) return Conflict(new { message = "Password must contain at least one special character." });

                user.password = newPassword;
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Password has been changed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }

        private static ConcurrentDictionary<string, (string otp, DateTime expiry)> otpStore = new();
        private const int OTP_EXPIRY_MINUTES = 30;


        [HttpPost("sendSMS")]
        public async Task<IActionResult> SendOTP(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return Conflict(new { message = "Username cannot be null or empty." });
            }

            var user = await _dbContext.userNumber.FirstOrDefaultAsync(ele => ele.username == username);
            if (user == null)
            {
                return Conflict(new { message = "User not found." });
            }

            string number = user.Number;
            if (string.IsNullOrEmpty(number))
            {
                return Conflict(new { message = "User does not have a registered phone number." });
            }

            try
            {
                string otp = new Random().Next(100000, 999999).ToString();
                string encryptedOtp = RSAEncryptOtp.EncryptOtp(otp, rsaPublicKey);

                Console.WriteLine($"Generated OTP: {otp}");
                Console.WriteLine($"Encrypted OTP: {encryptedOtp}");

                user.Otp = encryptedOtp;
                user.Validity = DateTime.UtcNow.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss");
                await _dbContext.SaveChangesAsync();

                // Reload and verify
                _dbContext.Entry(user).Reload();
                Console.WriteLine($"Reloaded Encrypted OTP from DB: {user.Otp}");

                string decryptedOtp = RSADecryptOtp.DecryptOtp(user.Otp, rsaPrivateKey);
                Console.WriteLine($"Decrypted OTP: {decryptedOtp}");
                Console.WriteLine($"Are they equal? {decryptedOtp == otp}");

                return Ok(new { message = "OTP sent successfully." });
            }
            catch (Twilio.Exceptions.ApiConnectionException ex)
            {
                return StatusCode(500, new { message = "Connection error. Unable to send OTP.", error = ex.Message });
            }
            catch (Twilio.Exceptions.ApiException ex)
            {
                return StatusCode(500, new { message = "Twilio API error.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
            }
        }

        [HttpPost("validate-answers")]
        public async Task<IActionResult> ValidateAnswers([FromBody] ValidateAnswersRequest request)
        {
            var user = await _dbContext.user.FirstOrDefaultAsync(u => u.username == request.Username);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userQuestions = await _dbContext.userQuestions
                .Where(q => q.email == user.email)
                .Select(q => q.SecurityQnA)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(userQuestions))
            {
                return NotFound("Security questions not set.");
            }

            var storedQnA = JsonSerializer.Deserialize<List<KeyValuePair<string, string>>>(userQuestions)
                             ?? new List<KeyValuePair<string, string>>();

            foreach (var submitted in request.SecurityQnA)
            {
                if (submitted.Key == "-1" || submitted.Value == "-1") continue; // Skip empty values

                var correctAnswer = storedQnA.FirstOrDefault(q => q.Key == submitted.Key).Value;
                if (string.IsNullOrEmpty(correctAnswer) || !correctAnswer.Equals(submitted.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized($"Incorrect answer for: {submitted.Key}");
                }
            }

            return Ok("All security questions answered correctly.");
        }

        [HttpGet("get-security-questions")]
        public async Task<IActionResult> GetSecurityQuestionsAsync(string username)
        {
            var user = await _dbContext.user.FirstOrDefaultAsync(u => u.username == username);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userQuestion = await _dbContext.userQuestions
                .Where(q => q.email == user.email)
                .Select(q => q.SecurityQnA)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(userQuestion))
            {
                return NotFound("Security questions not found.");
            }

            var questions = JsonSerializer.Deserialize<List<KeyValuePair<string, string>>>(userQuestion)
                              ?.Select(q => q.Key)
                              .ToList() ?? new List<string>();

            return Ok(questions);
        }


        public class ValidateAnswersRequest
        {
            public string Username { get; set; }
            public List<KeyValuePair<string, string>> SecurityQnA { get; set; }
        }

    }

}
