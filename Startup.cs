using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fsbackend.Data;
using fsbackend.middlewares;
using fsbackend.services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace fsbackend
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
          {
              options.AddPolicy("_myAllowSpecificOrigins",
              builder =>
              {
                  builder.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
              });

          });

            /* services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                });

            }); */

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
             {
                 options.Cookie.HttpOnly = false;
                 options.ExpireTimeSpan = TimeSpan.FromSeconds(300);
                 options.Cookie.MaxAge = TimeSpan.FromSeconds(300);
                 options.Cookie.Name = "FSCOOKIE";
                 options.Cookie.Path = "/api/v1";
                 options.AccessDeniedPath = "/api/v1/noauthorize";



             });





            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(300);
                options.Cookie.HttpOnly = false;
                options.Cookie.Name = "FSCOOKIE";
                options.Cookie.Path = "/api/v1";
                options.Cookie.IsEssential = true;

            });
            services.Configure<FormOptions>(o =>
            {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });
            services.AddTransient<MySqlDatabase>(_ => new MySqlDatabase(Configuration.GetConnectionString("PruebaConnection2")));
            services.AddTransient<AuthService>(sp => new AuthService(sp.GetService<MySqlDatabase>()));
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            string[] lorigins = { "localhost:3000", "localhost:5000", "localhost:5001" };
            //app.UseC
            app.UseCors("_myAllowSpecificOrigins");
            /* app.UseCors(builder => builder
               .AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader()

           ); */

            app.UseHttpsRedirection();

            app.UseRouting();
            //
            //app.UseCors("_myAllowSpecificOrigins");
            /* app.UseCors(builder => builder
                 .AllowAnyOrigin()
                 .AllowAnyMethod()
                 .AllowAnyHeader()

             ); */

            //app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCookiePolicy();
            //app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
