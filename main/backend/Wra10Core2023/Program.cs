using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.IO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Wra10Core2023.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.AspNetCore.Authentication.Cookies;
using Wra10Core2023.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Wra10Core2023;

public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();

    public class Startup
    {
        string myPolicy = "myPolicy";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
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
                    //NameClaimType = "name",
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
                //c.DocumentFilter<AlphabeticalDocumentFilter>();
            });

            // Add any other services you need here
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    //c.SwaggerEndpoint("/swagger/v1/swagger.json", "StaticFiles v1");
                    //c.RoutePrefix = "";
                });
                /*
                app.UseSwaggerUI(config =>
                {
                    config.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object>
                    {
                        ["activated"] = false
                    };
                });*/
            }

            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "WebPage")),
                RequestPath = "/WebPage",
                EnableDefaultFiles = true
            });

#if !DEBUG
                // Do not use https redirection during debugging.
                // It would lead to request errors in the browsers 
                // due to untrusted TLS certificates
                app.UseHttpsRedirection();
#endif

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
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "Files")),
                RequestPath = "/Files",
            });
        }
    }

    public class AlphabeticalDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Order paths (actions) alphabetically
            var orderedPaths = new OpenApiPaths();

            foreach (var path in swaggerDoc.Paths.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                orderedPaths.Add(path.Key, path.Value);
            }

            // Set the Paths property to the ordered paths
            swaggerDoc.Paths = orderedPaths;
        }
    }
}