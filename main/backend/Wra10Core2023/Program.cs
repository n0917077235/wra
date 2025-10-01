using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Wra10Core2023.Controllers;
using Wra10Core2023.Models;
using Wra10Core2023.Util;
using Wra10Core2023.Util.Earthquake;

namespace Wra10Core2023;

public class Program
{
    // This website is supposed to be deployed under
    // http://rivermonitoring.wra10.gov.tw/v2
    // because the frontend embeds many other sites from the same domain
    // and CORS required our site to be deployed with subfolder url.
    private const string PublicPath = "/v2";

    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.ProcessExit += (s, e) => SiteUtil.AncadDataHandler?.Stop();
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args).UseStartup<Startup>()
                .ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(5000); // 允許外部訪問 HTTP
                }); 

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
            services.AddIdentity<User, IdentityRole>()
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

            // 加入 定時排程前，檢查Line設定
            var lineSection = Configuration.GetSection("Line");
            if (lineSection.Exists())
            {
                //services.AddHostedService<Wra10Core2023.Services.ScheduledTaskServiceSystem>();
                //services.AddHostedService<Wra10Core2023.Services.ScheduledTaskServiceList>();
                // services.AddHostedService<Wra10Core2023.Services.ScheduledTaskServiceSensor>();
                // services.AddHostedService<Wra10Core2023.Services.ScheduledTaskServiceCamera>();
                // services.AddHostedService<Wra10Core2023.Services.ScheduledTaskServiceEarthquake>();
                Console.WriteLine("定時排程服務已啟動。");
            }
            else
            {
                Console.WriteLine("[警告] 未設定 Line 區段，定時排程服務未啟動。");
            }

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

            if (!env.IsDevelopment())
            {
                var webPageDir = new DirectoryInfo("Webpage").FullName;

                app.UseFileServer(new FileServerOptions
                {
                    FileProvider = new PhysicalFileProvider(webPageDir),
                    RequestPath = "",
                    EnableDefaultFiles = true
                });

                app.Use(async (context, next) =>
                {
                    await next();

                    if (context.Response.StatusCode == 404)
                    {
                        context.Response.Redirect(PublicPath);
                    }
                });
            }

            app.UsePathBase(PublicPath);
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

            _ = IsoseismalUtil.LoopAsync();
            SiteUtil.AncadDataHandler.StartListening();
        }
    }
}