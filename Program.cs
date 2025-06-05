using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using AccessMigrationApp.Areas.Identity;
using AccessMigrationApp.Data;
using AccessMigrationApp.Data.BCMEA;
using AccessMigrationApp.Data.CorporateMaster;
using AccessMigrationApp.Data.LabourDB;
using AccessMigrationApp.Data.LockerDB;
using AccessMigrationApp.Services;
using QuestPDF.Infrastructure;

namespace AccessMigrationApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);        // Add services to the container.
        var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        // Configure the DbContexts for each database
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(defaultConnection));
            
        builder.Services.AddDbContext<BCMEAContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("BCMEAConnection") ?? defaultConnection));
            
        builder.Services.AddDbContext<CorporateMasterContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("CorporateMasterConnection") ?? defaultConnection));
            
        builder.Services.AddDbContext<LockerDbContext>(options =>
            options.UseSqlServer(defaultConnection));
            
        builder.Services.AddDbContext<LabourDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("LabourDBConnection") ?? defaultConnection));
            
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
        builder.Services.AddSingleton<WeatherForecastService>();

        // Register report service
        builder.Services.AddScoped<IReportService, ReportService>();

        // Register QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        // Ensure HTTPS redirection is configured before other middleware
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        app.MapControllers();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }
}
