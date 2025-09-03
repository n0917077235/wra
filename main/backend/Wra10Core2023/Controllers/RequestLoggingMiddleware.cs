using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Data;
using System.Security.Claims;
using Wra10Core2023.Models;
using SqlHelper = Wra10Core2023.Util.SQLHelper;
using Microsoft.AspNetCore.Authorization;

namespace Wra10Core2023.Controllers
{
    public class RequestLoggingMiddleware : Controller
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration= configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            /*
                var cleanEmail = context.User.FindFirst("preferred_username")?.Value;
            
            //var userId0 = UserManager.GetUserId(H.User);
            if (context.User!=null && context.User.Identity != null)
            {
                var identity = context.User.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    IEnumerable<Claim> claims = identity.Claims;
                    //var v = identity.FindFirst("ClaimName").Value;

                }
            }
            
            //var userId = context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            */
            var auth = ((System.Security.Claims.ClaimsIdentity)context.User.Identity).IsAuthenticated;
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                await _next(context);
                return;
            }
            var endpoint = context.Request.Path;
            try
            {
                string? conn = _configuration.GetConnectionString("Water2022");
                SqlHelper sqlHelper = new SqlHelper(conn);

                string query=@"insert into userlogv2
                           ([UserID]
           ,[FunctionName]
           ,[intime]
           ,[UserIP])
            values 
                (@UserID
           ,@FunctionName
           ,@intime
           ,@UserIP
           )";
                List<SqlParameter> lstParams = new List<SqlParameter>();
                lstParams.Add(new SqlParameter("@UserId", userId));
                lstParams.Add(new SqlParameter("@FunctionName", endpoint.Value));
                lstParams.Add(new SqlParameter("@intime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                string ip = context.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                if (ip == "::1")
                    ip = "127.0.0.1";
                lstParams.Add(new SqlParameter("@userIp", ip ));
                sqlHelper.ExecuteNonQuery(query,lstParams.ToArray());   
            }
            catch (Exception ex)
            {

            }
            _logger.LogInformation($"Request made to endpoint: {endpoint}. User ID: {userId}");

            await _next(context);
        }
    }
}
