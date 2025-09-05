using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using Wra10Core2023.Controllers;
using Wra10Core2023.Models;
using Wra10Core2023.Util;

namespace Wra10Core2023;

public class Program
{
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.ProcessExit += (s, e) => SiteUtil.AncadDataHandler?.Stop();
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();

    public class Startup
    {
        string myPolicy = "myPolicy";
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            SiteUtil.Config = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            /*
            services.Configure<IdentityOptions>(options =>
                options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier);
            */

            services.AddIdentity<User, IdentityRole>()
                //.AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                };
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = new PathString("/authen/Login");
                options.LogoutPath = new PathString("/authen/Logout");
            });

            services.AddCors(options =>
            {
                options.AddPolicy(name: myPolicy,
                  policy =>
                  {
                      policy.WithOrigins("*")
                       .SetIsOriginAllowedToAllowWildcardSubdomains()
                       .SetIsOriginAllowed((host) => true)
                       .AllowAnyMethod()
                       .AllowAnyHeader();
                  });
            });

            services.AddResponseCaching(options =>
            {
                options.MaximumBodySize = 1024 * 1024; // Set maximum body size to cache
                options.UseCaseSensitivePaths = true; // Set whether paths are case sensitive
                                                      // Add more options as needed
            });

            //services.AddAuthentication().AddJwtBearer().AddJwtBearer(Configuration["LocalAuthIssuer"]); ;
            //services.AddAuthorization();
            /*
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = new PathString("/authen/Login");
                options.LogoutPath = new PathString("/authen/Logout");
            });
            */

            // 加入 定時排程，取消註解就會啟動
            // services.AddHostedService<ScheduledTask.Services.ScheduledTaskServiceSystem>();
            // services.AddHostedService<ScheduledTask.Services.ScheduledTaskServiceList>();
            // services.AddHostedService<ScheduledTask.Services.ScheduledTaskServiceSensor>();
            // services.AddHostedService<ScheduledTask.Services.ScheduledTaskServiceCamera>();
            // services.AddHostedService<ScheduledTask.Services.ScheduledTaskServiceEarthquake>();

            services.AddControllers();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(double.Parse(Configuration["Session:Timeout"].ToString()));
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RiverMonitor", Version = "v1" });
                c.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "JWT Authorization"
                    });

                c.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = JwtBearerDefaults.AuthenticationScheme,//"Bearer"
                                }
                            },
                            new string[] {}
                        }
                    });
            });

            // Add any other services you need here
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => { });
            }

            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var webPageDir = Path.Combine(assemblyDir, "WebPage");

            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(webPageDir),
                RequestPath = "",
                EnableDefaultFiles = true
            });

#if !DEBUG
            // Do not use https redirection during debugging.
            // It would lead to request errors in the browsers 
            // due to untrusted TLS certificates
            // app.UseHttpsRedirection();
#endif

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors(myPolicy);
            app.UseSession();

            /*
            app.Use(async (context, next) =>
            {
                var JWToken = context.Session.GetString("JWToken");
                if (!string.IsNullOrEmpty(JWToken))
                {
                    context.Request.Headers.Add("Authorization", "Bearer " + JWToken);
                }
                await next();
            });
            */
            app.UseResponseCaching();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                /*
                endpoints.MapControllerRoute(name: "mycustom",
                     pattern: "GetHome",
                     defaults: new { controller = "Home", action = "GetDetails" });
                endpoints.MapControllerRoute(name: "default",
                         pattern: "{controller=Home}/{action=Index}/{system?}");
                */
                endpoints.MapControllers();
            });

            /*
            string imagePath = Configuration["VideoImage:Path"].ToString();
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = "/images",
                FileProvider = new PhysicalFileProvider(Configuration["VideoImage:Path"])
            });
            */

            app.Use(async (context, next) =>
            {
                await next();

                if (context.Response.StatusCode == 404)
                {
                    context.Response.Redirect("/");
                }
            });

            _ = IsoseismalUtil.Loop();
            SiteUtil.AncadDataHandler.StartListening();
        }
    }
}