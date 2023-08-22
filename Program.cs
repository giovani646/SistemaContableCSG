using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaContableCSG.Data;
using SistemaContableCSG.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using SistemaContableCSG.Models;
using Microsoft.AspNetCore.Authorization;
using SistemaContableCSG.Permissions;
using SistemaContableCSG.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

//Bloque para personalizar el nombre del login path y access denied
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/AccesoDenegado";
    options.Cookie.Name = "LoginCookie";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(3);
    options.LoginPath = "/Login";
    // ReturnUrlParameter requires 
    //using Microsoft.AspNetCore.Authentication.Cookies;
    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.SlidingExpiration = true;
});

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    // Invalidar la cookie cada 0 segundos para que cualquier cambio en roles se reflejen inmediatamente en el usuario en cualquier dispositivo
    options.ValidationInterval = TimeSpan.FromSeconds(0);
});

builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    googleOptions.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents()
    {
        OnRemoteFailure = (ctx) =>
        {
            if (ctx.Failure?.Message == "Correlation failed.")
            {
                ctx.Response.Redirect("/Error/AuthError");
                ctx.HandleResponse();
            }

            return Task.CompletedTask;
        },
    };
}).AddMicrosoftAccount(microsoftOptions =>
{
    microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
    microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
}).AddTwitter(twitterOptions =>
{
    twitterOptions.ConsumerKey = builder.Configuration["Authentication:Twitter:ConsumerAPIKey"];
    twitterOptions.ConsumerSecret = builder.Configuration["Authentication:Twitter:ConsumerSecret"];
    twitterOptions.Events = new Microsoft.AspNetCore.Authentication.Twitter.TwitterEvents()
    {
        OnRemoteFailure = (ctx) =>
        {
            if (ctx.Failure?.Message == "Access was denied by the resource owner or by the remote server.")
            {
                ctx.Response.Redirect("/Login");
                ctx.HandleResponse();
            }

            return Task.CompletedTask;
        },
    };
});

builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<RolesController>();
builder.Services.AddScoped<IPeriodoHelper, PeriodoHelper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
