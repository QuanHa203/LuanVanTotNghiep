using CarServer.BackgroundServices;
using CarServer.Databases;
using CarServer.Middleware;
using CarServer.Repositories.Implementations;
using CarServer.Repositories.Interfaces;
using CarServer.Services.Email;
using CarServer.Services.WebSockets;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.WebHost.UseKestrel(options => options.ListenAnyIP(1234));

builder.Services.AddDbContext<CarServerDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("SqlServer") ?? throw new Exception("Cannot get ConnectionString!");
    options.UseSqlServer(connectionString);
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:1234", "http://192.168.1.100:1234")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Cookie.Name = "CarServerAuth";
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(20);
                    options.SlidingExpiration = true;

                    options.Events = new CookieAuthenticationEvents()
                    {
                        OnRedirectToLogin = (redirectContext) =>
                        {
                            redirectContext.HttpContext.Response.StatusCode = 401;
                            return Task.CompletedTask;
                        },
                        OnRedirectToAccessDenied = (redirectContext) =>
                        {
                            redirectContext.HttpContext.Response.StatusCode = 403;
                            return Task.CompletedTask;
                        }
                    };
                });

builder.Services.AddAuthorization();
builder.Services.AddSession();
builder.Services.AddControllersWithViews();

builder.Services.AddHostedService<OnlineStatusChecker>();

builder.Services.AddSingleton<PendingWebSocketRequests>();
builder.Services.AddSingleton<WebSocketHandler>();
builder.Services.AddTransient<ProtectedMediasFolderMiddleware>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.Configure<EmailSetting>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseMiddleware<ProtectedMediasFolderMiddleware>();

app.UseStaticFiles();

app.UseSession();


app.UseRouting();
app.UseCors("AllowSpecificOrigin");

app.UseAuthorization();
app.UseWebSockets();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

app.Run();
