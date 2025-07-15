using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Net.NetworkInformation;
using SqlHelper = SQLHelper.SQLHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Wra10MaintainWebApi.Controllers.LoginController;
using System.Data;
using DocumentFormat.OpenXml.Office2010.Excel;
using Wra10MaintainWebApi.Model;

namespace Wra10MaintainWebApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        
        private readonly string _connection;
        private readonly string _connectionWater2022;
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connection = _configuration.GetConnectionString("DefaultConnection");
            _connectionWater2022 = _configuration.GetConnectionString("Water2022Connection");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (model == null)
            {
                return BadRequest("Invalid client request");
            }

            var user = await AuthenticateUser(model.UserId, model.Password);

            if (user != null)
            {
                //GenerateJwtToken(user);
                user.Token = new Guid().ToString();
                return Ok(user);
            }

            return Unauthorized();
        }

        private async Task<User> AuthenticateUser(string userid, string password)
        {
            User user = null;

            SqlHelper sqlHelper = new SqlHelper(_connectionWater2022);
            var query = "SELECT [UserID],[UserGroupID] FROM Users WHERE [userid]=@userid and [Password] = @password";
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@userid", userid));
            parameters.Add(new SqlParameter("@password", password));
            DataTable dt = sqlHelper.ExecuteQuery(query, parameters.ToArray());
            if (dt.Rows.Count > 0)
            {
                user = new User
                {
                    UserId = dt.Rows[0]["userid"].ToString(),
                    UserType = dt.Rows[0]["uSerGroupId"].ToString(),
                };
            }

            return user;
        }

        private bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512(Convert.FromBase64String(storedSalt));
            var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return computedHash == storedHash;
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public class User
        {
            public string UserId { get; set; }
            public string UserType { get; set; }
            public string Token { get; set; }
        }
    }

    
}
