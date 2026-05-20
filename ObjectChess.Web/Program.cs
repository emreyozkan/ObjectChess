using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Services;
using ObjectChess.Data.Repositories;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";

builder.Services.AddScoped<IMatchRepository>(provider => new MatchRepository(connectionString));
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IAuthRepository>(provider => new AuthRepository(connectionString));
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "ObjectChessAuth";
    options.DefaultChallengeScheme = "ObjectChessAuth";
})
.AddCookie("ObjectChessAuth", options =>
{
    options.Cookie.Name = "ObjectChess.Cookie";
    options.LoginPath = "/Auth/Login";
    options.ExpireTimeSpan = System.TimeSpan.FromHours(2);
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Match}/{action=MatchHistory}/{id?}")
    .WithStaticAssets();

app.Run();