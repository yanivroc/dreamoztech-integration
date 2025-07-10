using OvenBites.Api;
using System.Net; // Keep this as you use HttpStatusCode

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// AddControllersWithViews includes AddControllers
builder.Services.AddControllersWithViews();

// Register your ApiService here!
// Choose the appropriate lifetime: AddScoped, AddTransient, or AddSingleton
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<OvenBites.Services.IDataService, OvenBites.Services.DataService>();

// Add MemoryCache services
builder.Services.AddMemoryCache();

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
    // AddEventLog is Windows-specific and can have performance implications.
    // Ensure it's desired for your deployment environment.
    config.AddEventLog();
});

var app = builder.Build();

app.Logger.LogInformation("Starting Application");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Use only in Development
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Centralized error handling
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection(); // Apply HTTPS redirection here
app.UseStaticFiles();

app.UseRouting(); // Important: must be before UseCors, UseAuthentication, UseAuthorization, and UseEndpoints

app.UseCors(); // Place after UseRouting, before Map* or UseEndpoints if using default policy

// app.UseAuthentication(); // If you add authentication later
// app.UseAuthorization();  // If you add authorization later

// Map endpoints
app.MapControllers(); // Maps attribute-routed controllers
app.MapControllerRoute(
    name: "pageRoute",
    pattern: "{pageName?}", // {pageName?} makes the pageName segment optional
    defaults: new { controller = "Home", action = "Index" }
);
app.Run();