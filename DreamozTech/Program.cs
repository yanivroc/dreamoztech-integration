using DreamozTech.Api;
using DreamozTech.Models;
using Serilog;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog configuration from appsettings.json
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

// Add services to the container.
// If you use Razor Pages, add Razor Pages support.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Configure the EmailConfig section from appsettings.json
builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection("EmailConfig"));

// Register your ApiService here!
// Choose the appropriate lifetime: AddScoped, AddTransient, or AddSingleton
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<DreamozTech.Service.IDataService, DreamozTech.Service.DataService>();
builder.Services.AddScoped<DreamozTech.Service.IEmailService, DreamozTech.Service.EmailService>();
builder.Services.AddScoped<DreamozTech.Service.IPostService, DreamozTech.Service.PostService>();

// Register IHttpClientFactory
builder.Services.AddHttpClient();

// Access custom settings from appsettings.json
// Consider strongly-typed configuration for better maintainability
var endPoint1 = builder.Configuration.GetSection("AllowedEndPoints:point1").Value; // Shorter path for nested
var endPoint2 = builder.Configuration.GetSection("AllowedEndPoints:point2").Value; // Shorter path for nested

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            // Null-coalescing operator for safety
            policy.WithOrigins(new string[] { endPoint1 ?? "", endPoint2 ?? "" })
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
    // It's often better to let Kestrel configure this or get it from config if needed
    // options.HttpsPort = 5001; // Comment out or make configurable if not desired to hardcode
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddLogging(config =>
{
    config.AddDebug();
    config.AddConsole();
});

builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.MinifyJsFiles("js/site.js");
    pipeline.MinifyCssFiles("css/site.css", "css/cart.css");
});

var app = builder.Build();

app.UsePathBase("/shop");

// Make sure this is called before any other middleware that you want to log
app.UseSerilogRequestLogging();

app.Logger.LogInformation("Starting Application");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseWebOptimizer();
app.UseStaticFiles();

app.UseRouting();

app.UseCors();

// Map Razor Pages and Controllers
app.MapRazorPages();
app.MapControllers();

// Optional: conventional controller route (keeps previous behaviour)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();