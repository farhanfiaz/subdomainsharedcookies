using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PrimarySite;

var builder = WebApplication.CreateBuilder(args);
#region DB
builder.Services.AddDbContext<AppDbContext>(config => {
    config.UseInMemoryDatabase("Memory");
});
#endregion
builder.Services.AddDataProtection().PersistKeysToFileSystem(GetKyRingDirectoryInfo()).SetApplicationName("SharedCookieApp");
builder.Services.Configure<CookiePolicyOptions>(options => {
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.AddIdentity<IdentityUser, IdentityRole>(config => {
    config.Password.RequiredLength = 4;
    config.Password.RequireDigit = false;
    config.Password.RequireNonAlphanumeric = false;
    config.Password.RequireUppercase = false;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

builder.Services.AddAuthentication("Identity.Application");
builder.Services.ConfigureApplicationCookie(config => {
    config.Cookie.Name = ".AspNet.SharedCookie";
    config.LoginPath = "/Home/Index";
    //config.Cookie.Domain = ".test.com";
});

// Add services to the container.
builder.Services.AddControllersWithViews();

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
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
