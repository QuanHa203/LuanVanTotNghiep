using CarServer.BackgroundServices;
using CarServer.Databases;
using CarServer.Services.WebSockets;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.WebHost.UseKestrel(options => options.ListenAnyIP(1234));
builder.Services.AddControllersWithViews();

builder.Services.AddHostedService<OnlineStatusChecker>();

builder.Services.AddDbContext<CarServerDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("SqlServer") ?? throw new Exception("Cannot get ConnectionString!");
    options.UseSqlServer(connectionString);
});

builder.Services.AddSingleton<PendingWebSocketRequests>();
builder.Services.AddSingleton<WebSocketHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseWebSockets();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Car}/{action=Index}");

app.Run();
