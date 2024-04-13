using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.DataProtection;
using SubSiteTwo.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDataProtection().PersistKeysToFileSystem(GetKyRingDirectoryInfo()).SetApplicationName("SharedCookieApp");
builder.Services.Configure<CookiePolicyOptions>(options => {
    // This lambda determines whether user consent for nonessential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.AddAuthentication("Identity.Application").AddCookie("Identity.Application", option => {
    option.Cookie.Name = ".AspNet.SharedCookie";
    option.Events.OnRedirectToLogin = (context) => {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
DirectoryInfo GetKyRingDirectoryInfo()
{
    string applicationBasePath = System.AppContext.BaseDirectory;
    DirectoryInfo directoryInof = new DirectoryInfo(applicationBasePath);
    string keyRingPath = builder.Configuration.GetSection("AppKeys").GetValue<string>("keyRingPath");
    do
    {
        directoryInof = directoryInof.Parent;
        DirectoryInfo keyRingDirectoryInfo = new DirectoryInfo($"{directoryInof.FullName}{keyRingPath}");
        if (keyRingDirectoryInfo.Exists)
        {
            return keyRingDirectoryInfo;
        }
    }
    while (directoryInof.Parent != null);
    throw new Exception($"key ring path not found");
}
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
