using Microsoft.EntityFrameworkCore;
using SubApp1.Models;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using SubApp1.Services;
using SubApp1.DAL;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("UserDbContextConnection") ?? throw new InvalidOperationException("Connection string 'UserDbContextConnection' not found.");

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<UserDbContext>(options => {
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:UserDbContextConnection"]);
});

builder.Services.AddSingleton<IEmailSender, MockEmailSender>();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddSession();

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();

var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information() // levels: Trace< Information < Warning < Erorr < Fatal
    .WriteTo.File($"Logs/app_{DateTime.Now:yyyyMMdd_HHmmss}.log");

loggerConfiguration.Filter.ByExcluding(e => e.Properties.TryGetValue("SourceContext", out var value) &&
                            e.Level == LogEventLevel.Information &&
                            e.MessageTemplate.Text.Contains("Executed DbCommand"));

var logger = loggerConfiguration.CreateLogger();
builder.Logging.AddSerilog(logger);

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
app.UseAuthentication();
app.UseRouting();
app.MapRazorPages();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
