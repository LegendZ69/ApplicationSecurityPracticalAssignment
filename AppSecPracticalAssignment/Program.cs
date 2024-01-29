using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AppSecPracticalAssignment.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AppSecPracticalAssignment.Settings;
using AppSecPracticalAssignment.Services;
using SendGrid.Extensions.DependencyInjection;
using Twilio.Base;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AuthDbContext>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AuthDbContext>();
/*    .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>("Default");
*/

//P6
builder.Services.ConfigureApplicationCookie(Config =>
{
    //L7.1 Slide 17, Cookie settings
    Config.Cookie.HttpOnly = true;
    Config.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    Config.LoginPath = "/Login";
    Config.AccessDeniedPath = "/Account/AccessDenied";
    Config.SlidingExpiration = true;

    //addons
    /*Config.Cookie.Name = "AuthCookie";
    Config.Cookie.SecurePolicy = CookieSecurePolicy.Always; //Transmits cookie only over HTTPS
    Config.Cookie.SameSite = SameSiteMode.Strict; //Mitigates CSRF attacks by restricting cookie usage in cross-site requests
    Config.LogoutPath = "/Logout";*/
});

//P7
builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
{
    options.Cookie.Name = "MyCookieAuth";
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    //addons
    /*options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;*/
});


//L7.2 Slide 8, Alternative, .AddGoogle() cant be found
/*builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = "YOUR_GOOGLE_CLIENT_ID";
        options.ClientSecret = "YOUR_GOOGLE_CLIENT_SECRET";
        // Other options can be configured here
    });*/

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("MustBelongToHRDepartment", policy => policy.RequireClaim("Department", "HR"));
});

builder.Services.AddDataProtection();


//L4 Slide 24
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
	options.Cookie.SecurePolicy= CookieSecurePolicy.Always;
	options.Cookie.HttpOnly = true;

	//L4 Slide 35
	options.Cookie.Name = "SessionCookie";
	options.Cookie.MaxAge = TimeSpan.FromMinutes(5);
	/*options.Cookie.Domain = "localhost";
	options.Cookie.Path = "/";*/
	options.Cookie.SameSite = SameSiteMode.Strict;
	options.Cookie.IsEssential = true;
});
builder.Services.AddTransient<ApplicationUser>();


//L7.1 Slide 15
builder.Services.AddDbContext<AuthDbContext>(options =>
{
	//CHATGPT
    options.EnableSensitiveDataLogging(); // Logs SQL queries for debugging
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
});

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 12;
    options.Password.RequiredUniqueChars = 1;
    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    // User settings.
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
    //addons
    /*    options.Stores.ProtectPersonalData = true;
    */
/*    options.Stores.MaxLengthForKeys = 5;
*/    /*options.SignIn.RequireConfirmedPhoneNumber = true;
    options.SignIn.RequireConfirmedEmail = true;*/
/*    options.SignIn.RequireConfirmedAccount = true;
*/    /*    options.Tokens.AuthenticatorIssuer = EmailTokenProvider.Default;
    *//*    options.Tokens.ChangePhoneNumberTokenProvider = ?;
    */
});

//Logging
builder.Services.AddLogging(configure =>
{
	configure.AddConsole();
    configure.AddDebug();
    configure.AddEventLog();
    configure.AddEventSourceLogger();
	configure.AddJsonConsole();
	configure.AddSimpleConsole();
});

//Email
builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("SendGridSettings"));
builder.Services.AddSendGrid(options =>
{
    options.ApiKey = builder.Configuration.GetSection("SendGridSettings").GetValue<string>("ApiKey");
});
builder.Services.AddScoped<IEmailSender, EmailSenderService>();

//SMS
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("TwilioSettings"));
builder.Services.AddScoped<ISMSSenderService, SMSSenderService>();

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

app.UseStatusCodePagesWithRedirects("/errors/{0}");

app.UseSession(); //L4 Slide 24

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
