using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Wra10Core2023.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using SqlHelper = Wra10Core2023.Util.SQLHelper;
using MimeKit;
using MailKit.Net.Smtp;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Wra10Core2023.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<SensorController> _logger;
        private readonly IConfiguration _configuration;
        public UserController(ILogger<SensorController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Authorize]
        [Route("GetUsers")]
        [HttpPost]
        public IActionResult GetUsers(string? userId,string? userName)
        {
            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                List<UserInfo> lstUser = new List<UserInfo>();
                string? conn = _configuration.GetConnectionString("Water2022");
                SqlHelper sqlHelper = new SqlHelper(conn);
                string filter = "where 1=1 ";
                userId = userId == null ? "" : userId;
                userName = userName == null ? "" : userName;
                if (userId == "" && userName == "")
                    filter += "";
                else if (userId!="" && userName!="")
                    filter += " and (userId like '%'+@userId+'%' or userName like '%'+@userName+'%')";
                else if (userId != "")
                    filter += " and userId like '%'+@userId+'%'";
                else if (userName != "")
                    filter += " and userName like '%'+@userName+'%'";
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("@userId", userId));
                lstParams.Add(new SqlParameter("@userName", userName));
                DataTable dt = sqlHelper.ExecuteQuery(@"Select * from users "+filter+" order by userid", lstParams.ToArray());
                
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    bool enabled = false;
                    bool.TryParse(dt.Rows[i]["Enabled"].ToString(), out enabled);
                    bool flag = false;
                    bool.TryParse(dt.Rows[i]["Flag"].ToString(), out flag);
                    int specialUser = 0;
                    int.TryParse(dt.Rows[i]["specialuser"].ToString(), out specialUser);
                    int eventNotice = 0;
                    int.TryParse(dt.Rows[i]["EventNotice"].ToString(), out eventNotice);
                    lstUser.Add(new UserInfo
                    {
                        UserID = dt.Rows[i]["UserId"].ToString(),
                        UserName = dt.Rows[i]["UserName"].ToString(),
                        Belongs= dt.Rows[i]["Belongs"].ToString(),
                        UserGroupID= dt.Rows[i]["UserGroupId"].ToString(),
                        Enabled= enabled,
                        SpecialUser = specialUser,
                        EventNotice=eventNotice,
                        EMail = dt.Rows[i]["EMail"].ToString(),
                        //Flag = flag
                    });
                }
                return Ok(lstUser.ToList());
            }            
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        [Authorize]
        [Route("AddUser")]
        [HttpPost]
        public IActionResult InsertCustomer(UserInfo userInfo)
        {
            
            try
            {
                string? conn = _configuration.GetConnectionString("Water2022");
                SqlHelper sqlHelper = new SqlHelper(conn);

                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("@UserId", userInfo.UserID));
                lstParams.Add(new SqlParameter("@UserName", userInfo.UserName));
                lstParams.Add(new SqlParameter("@UserGroupID", userInfo.UserGroupID));
                lstParams.Add(new SqlParameter("@Belongs", userInfo.Belongs));
                lstParams.Add(new SqlParameter("@SpecialUser", userInfo.SpecialUser));
                lstParams.Add(new SqlParameter("@EventNotice", userInfo.EventNotice));
                lstParams.Add(new SqlParameter("@EMail", userInfo.EMail));
                lstParams.Add(new SqlParameter("@Enabled", userInfo.Enabled));
                /*
                string passwordMd5 = "";
                using (MD5 md5 = MD5.Create())
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes("Wra10Pass+");
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("x2"));
                    }

                    passwordMd5 = sb.ToString();
                }
                */
                lstParams.Add(new SqlParameter("@Password", "Wra10Pass+"));
                string query = @"
            INSERT INTO [dbo].[Users]
           ([UserID]
           ,[UserName]
           ,[UserGroupID]
           ,[Belongs]
           ,[SpecialUser]
           ,[EventNotice]
           ,[EMail]
           ,[Enabled]
,[Password])
     VALUES
           (@UserID
           ,@UserName
           ,@UserGroupID
           ,@Belongs
           ,@SpecialUser
           ,@EventNotice
           ,@EMail
           ,@Enabled,@password)
            ";
                int nAdd = sqlHelper.ExecuteNonQuery(query, lstParams.ToArray());
                return Ok(@"{""addUser"":true,""defaultPassword"":""Wra10Pass+""}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [Route("UpdateUser")]
        [HttpPost]
        public IActionResult UpdateUser(UserInfo userInfo)
        {
            try
            {
                string? conn = _configuration.GetConnectionString("Water2022");
                SqlHelper sqlHelper = new SqlHelper(conn);

                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("@UserId", userInfo.UserID));
                lstParams.Add(new SqlParameter("@UserName", userInfo.UserName));
                lstParams.Add(new SqlParameter("@UserGroupID", userInfo.UserGroupID));
                lstParams.Add(new SqlParameter("@Belongs", userInfo.Belongs));
                lstParams.Add(new SqlParameter("@SpecialUser", userInfo.SpecialUser));
                lstParams.Add(new SqlParameter("@EventNotice", userInfo.EventNotice));
                lstParams.Add(new SqlParameter("@EMail", userInfo.EMail));
                lstParams.Add(new SqlParameter("@Enabled", userInfo.Enabled));

                string query = @"
            UPDATE [dbo].[Users]
            SET 
                [UserName] = @UserName
                ,[UserGroupID] = @UserGroupID
                ,[Belongs] = @Belongs
                ,[SpecialUser] = @SpecialUser
                ,[EventNotice] = @EventNotice
                ,[EMail] = @EMail
                ,[Enabled] = @Enabled
               WHERE [UserID] = @UserID";
                int nUpdate = sqlHelper.ExecuteNonQuery(query, lstParams.ToArray());
                return Ok(nUpdate);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [Route("DeleteUser")]
        [HttpPost]
        public IActionResult DeleteUser(string  userId)
        {
            try
            {

                string query = "DELETE FROM users WHERE userId=@userId";
                string? conn = _configuration.GetConnectionString("Water2022");
                SqlHelper sqlHelper = new SqlHelper(conn);

                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("@UserId", userId));
                int nDelete = sqlHelper.ExecuteNonQuery(query, lstParams.ToArray());
                return Ok(nDelete);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize]
        [Route("ForgotPassword")]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string userEMail, string url)
        {
            try
            {
                string query = @"Select userid,EMail FROM users WHERE EMail=@EMail";
                string? conn = _configuration.GetConnectionString("Water2022");
                SqlHelper sqlHelper = new SqlHelper(conn);
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("@EMail", userEMail));
                DataTable dt = sqlHelper.ExecuteQuery(query, lstParams.ToArray());
                UserInfo userInfo = new UserInfo();
                if (dt.Rows.Count == 0)
                {
                    return NotFound("未發現使用者EMail");
                }
                userInfo.UserID = dt.Rows[0]["userId"].ToString();
                userInfo.EMail = dt.Rows[0]["EMail"].ToString();

                string resetToken = GenerateResetToken();
                query = @"update users set code=@code where userId=@userId";
                lstParams.Clear();
                lstParams.Add(new SqlParameter("@userId", userInfo.UserID));
                lstParams.Add(new SqlParameter("@code", resetToken));
                sqlHelper.ExecuteNonQuery(query, lstParams.ToArray());

                var resetLink = Url.Action("ResetPassword", "Account", new { userId = userInfo.UserID, token = resetToken }, Request.Scheme);

                var emailBody = $"請按  <a href='{resetLink}'>連結</a> 重設密碼.";
                IEmailService _emailService= new EmailService();
                await _emailService.SendEmailAsync(userInfo.EMail, "密碼重設", emailBody);

                return Ok("Reset Password OK.");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);  
            }
        }

        [Authorize]
        [Route("ResetPassword")]
        [HttpPost]
        public IActionResult ResetPassword(string userId, string token, [FromBody] ResetPasswordModel model)
        {
            try
            {
                string query = @"Select userid,EMail FROM users WHERE userId=@userId and code=@token";
                string? conn = _configuration.GetConnectionString("Water2022");
                SqlHelper sqlHelper = new SqlHelper(conn);
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("@userId", userId));
                lstParams.Add(new SqlParameter("@token", token));
                DataTable dt = sqlHelper.ExecuteQuery(query, lstParams.ToArray());

                if (dt.Rows.Count == 0)
                {
                    return NotFound("User not found.");
                }


                string password = model.Password;
                string passwordMd5 = "";
                using (MD5 md5 = MD5.Create())
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(password);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("x2"));
                    }

                    passwordMd5 = sb.ToString();
                }
                query = @"Update users set password=@password WHERE userId=@userId";
                lstParams.Add(new SqlParameter("@password", password));
                sqlHelper.ExecuteNonQuery(query, lstParams.ToArray());
                return Ok("Password reset successfully.");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [Route("GetUserGoup")]
        [HttpGet]
        public IActionResult  GetUserGroup()
        {
            try
            {
                List<UserGroup> lstUserGroup = new List<UserGroup>();
                string query = @"Select UserGroupId,UserGroupName FROM usergroup order by usergroupid";
                string? conn = _configuration.GetConnectionString("Water2022");
                SqlHelper sqlHelper = new SqlHelper(conn);
                DataTable dt = sqlHelper.ExecuteQuery(query);
                for(int i=0;i<dt.Rows.Count;i++)
                {
                    UserGroup ug = new UserGroup
                    {
                        UserGroupID = dt.Rows[i]["UserGroupId"].ToString(),
                        UserGroupName = dt.Rows[i]["UserGroupName"].ToString()
                    };
                    lstUserGroup.Add(ug);
                }
                return Ok(lstUserGroup.ToArray());
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GenerateResetToken()
        {
            byte[] randomBytes = new byte[64];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Wra10 River Monitor System", "ahtsair@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;

            emailMessage.Body = new TextPart("html")
            {
                Text = message
            };

            using var client = new SmtpClient();
            client.Connect("smtp.gmail.com", 587, false); // Use your SMTP server details
            client.Authenticate("srec.developer", "hrobhfobjszswqof"); // Use your SMTP credentials
            client.Send(emailMessage);
            client.Disconnect(true);
        }
    }

}
