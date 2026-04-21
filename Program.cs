using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ObjectChess.Data.Repositories;
using ObjectChess.Business.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IMatchRepository>(provider => 
{
    IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
    string connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
    
    return new MatchRepository(connectionString);
});

builder.Services.AddScoped<MatchService>();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();