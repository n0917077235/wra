using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.DirectoryServices.AccountManagement;
using SqlHelper = SQLHelper.SQLHelper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.DirectoryServices.Protocols;
using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using System;
using Wra10Core2023.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Microsoft.VisualBasic.Logging;
using static System.Net.Mime.MediaTypeNames;

namespace Wra10Core2023.Controllers
{
    [Route("api/authen")]
    public class AuthenController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;
        public AuthenController(IConfiguration configuration, IDistributedCache cache) 
        {
            _cache=cache;
            _configuration = configuration;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public IActionResult Login(LoginModel model)
        {

            if (model != null)
            {
                string? username = model.Username;
                string? password = model.Password;
               
                var user = AuthenticateUser(username, password);
                if (user != null)
                {
                    var token = GenerateToken(user);
                    LoginResult lr = new LoginResult()
                    {
                        UserId = user.UserId,
                        UserName = user.UserName,
                        Token = token,
                    };
                    setUser(user);
                    return Ok(lr);
                }
            }
            return Unauthorized();
        }

        private async void setUser(User user)
        {
            try
            {
                string? conn = _configuration.GetConnectionString("Water2022");
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                if (!string.IsNullOrEmpty(conn))
                {
                    SqlHelper sqlHelper = new SqlHelper(conn);
                    string cmd = @"INSERT INTO UserLogV2 (userid,FunctionName,userip,intime) values (@userid,@funcname,@userip,@intime)";
                    List<SqlParameter> lstParam = new List<SqlParameter>();
                    lstParam.Add(new SqlParameter("@userid", user.UserId));
                    lstParam.Add(new SqlParameter("@funcname", "登入系統"));
                    lstParam.Add(new SqlParameter("@userip", ipAddress));
                    lstParam.Add(new SqlParameter("@intime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    sqlHelper.ExecuteNonQuery(cmd, lstParam.ToArray());

                }
               

            }
            catch(Exception ex)
            {

            }
        }

        [HttpPost("LoginAD")]
        [AllowAnonymous]
        public IActionResult LoginAD(LoginModel model)
        {
            // Authenticate user credentials and generate token
            try
            {
                var user = AuthenticateUserAD(model.Username, model.Password);
                if (user != null)
                {
                    var token = GenerateToken(user);
                    LoginResult lr = new LoginResult()
                    {
                        UserId = user.UserId,
                        UserName = user.UserName,
                        Token = token,
                    };
                    setUser(user);
                    return Ok(lr);
                }

                return Unauthorized();
            }
            catch(Exception ex)
            {
                return Unauthorized();
            }
        }

        [HttpPost("LoginSmart")]
        [AllowAnonymous]
        public IActionResult LoginSmart(LoginModel model)
        {

            if (model != null)
            {
                string? username = model.Username;
                string? password = model.Password;

                var user = AuthenticateUser(username, password);
                if (user != null)
                {
                    var token = GenerateToken(user);
                    LoginResult lr = new LoginResult()
                    {
                        UserId = user.UserId,
                        UserName = user.UserName,
                        Token = token,
                    };
                    setUser(user);
                    return Ok(lr);
                }
                else
                {
                    try
                    {
                        user = AuthenticateUserAD(model.Username, model.Password);
                        if (user != null)
                        {
                            var token = GenerateToken(user);
                            LoginResult lr = new LoginResult()
                            {
                                UserId = user.UserId,
                                UserName = user.UserName,
                                Token = token,
                            };
                            setUser(user);
                            return Ok(lr);
                        }

                        return Unauthorized();
                    }
                    catch (Exception ex)
                    {
                        return Unauthorized();
                    }
                }
            }
            return Unauthorized();
        }

        static string CalculateMD5Hash(string? input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }

        private User? AuthenticateUser(string? username, string? password)
        {
            try
            {
                string? conn = _configuration.GetConnectionString("Water2022");

                if (!string.IsNullOrEmpty(conn))
                {
                    SqlHelper sqlHelper = new SqlHelper(conn);

                    List<SqlParameter> lstParam = new List<SqlParameter>();
                    lstParam.Add(new SqlParameter("@userid", username));
                    lstParam.Add(new SqlParameter("@password", CalculateMD5Hash(password)));
                    lstParam.Add(new SqlParameter("@password2", password));
                    DataTable dt = sqlHelper.ExecuteQuery("select 'ok',userId,userName from users where userid=@userid and (password=@password or password=@password2)", lstParam.ToArray());
                    //string sql = "select 'ok' from users where username='" + username + "' and password='" + CalculateMD5Hash(password) + "'";
                    //DataTable dt = sqlHelper.ExecuteQuery(sql);
                    if (dt.Rows.Count > 0)
                    {
                        if (dt.Rows[0][0].ToString() == "ok")
                        {
                            User user = new User();
                            user.UserId = dt.Rows[0][1].ToString();
                            user.Password = password;
                            user.UserName = dt.Rows[0][2].ToString();
                            return user;
                        }
                    }
                    /*
                    else
                    {
                        lstParam.Clear();
                        lstParam.Add(new SqlParameter("@userid", username));
                        lstParam.Add(new SqlParameter("@password", password));
                        dt.Clear();
                        dt = sqlHelper.ExecuteQuery("select 'ok',userId,userName from users where userid=@userid and password=@password", lstParam.ToArray());
                        
                        if (dt.Rows.Count > 0)
                        {
                            if (dt.Rows[0][0].ToString() == "ok")
                            {
                                User user = new User();
                                user.UserId = dt.Rows[0][1].ToString();
                                user.Password = password;
                                user.UserName = dt.Rows[0][2].ToString();
                                return user;
                            }
                        }
                    }
                    */
                }
                return null;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        private User? AuthenticateUserAD(string? username, string? password)
        {
            // Perform authentication logic here
            // Return user object if credentials are valid, otherwise null
            try
            {
                User user = new User();
                user.Password = password;
                user.UserName = username;
                bool isValid = false;

                if (!string.IsNullOrEmpty(user.UserName) && !string.IsNullOrEmpty(user.Password))
                {

                    using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, _configuration["AdDomain:Domain"], user.UserName, user.Password))
                    {
                        user.UserId = user.UserName;
                        isValid = pc.ValidateCredentials(user.UserName, user.Password);
                        if (isValid)
                        {
                            UserPrincipal up = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, user.UserId);

                            if (up != null)
                                user.UserName = up.Name;
                        }

                    }

                }
                if (isValid)
                    return user;
                return null;
            }
            catch(Exception  ex) {
                return null;
            }
        }

        [HttpPost("Logout")]
        //[Authorize]
        public async Task<IActionResult> Logout(string userId)
        {
            try
            {
                string cmd = @"Update UserLogV2 set outtime=Convert(varchar,'" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "',120) where OutTime is null and FunctionName='登入系統' and UserID=@userId";
                List<SqlParameter> lstParameter = new List<SqlParameter>();
                string? conn = _configuration.GetConnectionString("Water2022");
                lstParameter.Add(new SqlParameter("@userId", userId));
                SqlHelper sqlHelper = new SqlHelper(conn);
                sqlHelper.ExecuteNonQuery(cmd, lstParameter.ToArray());

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (HttpContext.Session != null)
                {
                    HttpContext.Session.Clear();
                }
                
                await _cache.RemoveAsync("myCacheKey");
                return Ok("LogoutOK");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("SetCacheData")]
        public async Task<IActionResult> SetCacheData(string cacheKey, [FromBody] string data)
        {
            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };

                await _cache.SetAsync(cacheKey, dataBytes, cacheOptions);

                return Ok("Cache " + cacheKey + " Saved");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetCachedData")]
        public async Task<IActionResult> GetCachedData(string cacheKey)
        {
            try
            {
                var cachedData = await _cache.GetStringAsync(cacheKey);

                /*
                if (cachedData == null)
                {

                    cachedData = "Some data to be cached";

                    byte[] dataBytes = Encoding.UTF8.GetBytes(cachedData);

                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
                    };

                    await _cache.SetAsync(cacheKey, dataBytes, cacheOptions);


                }
                */

                return Ok(cachedData);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GenerateToken(User user)
        {
            try
            {
                if (!user.UserName.IsNullOrEmpty() && !user.Password.IsNullOrEmpty())
                {

                    var claims = new List<Claim>
                    {
                    //new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
                    new Claim(ClaimTypes.NameIdentifier, user.UserId),
                    //new Claim(ClaimTypes.Email, user.UserId),
                    //new Claim(ClaimTypes.Name, user.UserId)
                    };
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    /*
                    string randomToken = GenerateRandomToken(32);

                    var claims = new List<Claim>
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, randomToken),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID
                    };
                    */
                    var token = new JwtSecurityToken(
                        issuer: _configuration["Jwt:Issuer"],
                        audience: _configuration["Jwt:Audience"],
                        expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Session:Timeout"].ToString())),
                        signingCredentials: creds,
                        claims: claims
                    );
                    var claimsIdentity = new ClaimsIdentity(claims, "login");
                    
                    /*
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            // Include any additional claims or user data here
                        }),
                        Expires = DateTime.UtcNow.AddDays(7),


                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
                    };
                    IssuerValidator issuer = new IssuerValidator(_configuration["Jwt:issuer"]);
                    var token = tokenHandler.CreateToken(tokenDescriptor);

                    return tokenHandler.WriteToken(token);
                    */
                    //string strToken=GenerateRandomToken(256);

                    string strToken = new JwtSecurityTokenHandler().WriteToken(token);
                    var userClaims = User.Claims.ToList();
                    return strToken;
                }
                else
                {
                    return "";
                }
            }
            catch(Exception ex)
            {
                return "";
            }
        }

        
    }

    
}
