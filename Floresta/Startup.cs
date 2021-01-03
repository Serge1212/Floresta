using AutoMapper;
using Floresta.Interfaces;
using Floresta.Models;
using Floresta.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Floresta
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IRepository<Seedling>, SeedlingRepository>();
            services.AddScoped<IRepository<Marker>, MarkerRepository>();
            services.AddScoped<IRepository<News>, NewsRepository>();
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                services.AddDbContext<FlorestaDbContext>(options =>
                   options.UseSqlServer(Configuration.GetConnectionString("RemoteConnection")));
            }
            else
            {
                services.AddDbContext<FlorestaDbContext>(options =>
                   options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            }
            services.BuildServiceProvider().GetService<FlorestaDbContext>().Database.Migrate();

            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;

            })
            .AddEntityFrameworkStores<FlorestaDbContext>()
            .AddDefaultTokenProviders();

            services.AddAuthorization(options =>
            options.AddPolicy("admin",
                policy => policy.RequireClaim("Manager")));

            services.AddAutoMapper(typeof(Startup));
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
