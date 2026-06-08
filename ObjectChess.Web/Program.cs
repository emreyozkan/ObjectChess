using Microsoft.AspNetCore.Mvc;
using ObjectChess.Business.Interfaces;
using ObjectChess.Business.Security;
using ObjectChess.Business.Services;
using ObjectChess.Data.Repositories;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // Turns on CSRF protection for every POST so forms can not be faked from other sites
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Read the database connection string from the config
// Fail right away if it is missing so we do not start in a broken state
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

// This is the one place we say when something asks for an interface give it this real class
// This is how dependency injection works so the rest of the code only depends on interfaces
builder.Services.AddScoped<IUserRepository>(_ => new UserRepository(connectionString));
builder.Services.AddScoped<IMatchRepository>(_ => new MatchRepository(connectionString));
// Singleton means one shared instance for the whole app which is fine since these hold no user state
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IPasswordPolicy, PasswordPolicy>();
builder.Services.AddSingleton<IMoveParser, MoveParser>();
// Scoped means a fresh instance for each web request
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMatchService, MatchService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "ObjectChessAuth";
    options.DefaultChallengeScheme = "ObjectChessAuth";
})
.AddCookie("ObjectChessAuth", options =>
{
    // Name the cookie and send anyone who is not logged in to the login page
    options.Cookie.Name = "ObjectChess.Cookie";
    options.LoginPath = "/Auth/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
});

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Order matters here
// Authentication (who are you) must run before authorization (are you allowed)
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Match}/{action=MatchHistory}/{id?}")
    .WithStaticAssets();

app.Run();
