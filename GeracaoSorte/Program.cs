using GeracaoSorte.Data;
using GeracaoSorte.Services.Clientes;
using GeracaoSorte.Services.Home;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<HomeService>();

builder.Services.AddScoped<IClientesService, ClientesService>();
builder.Services.AddScoped<ClientesService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAntiforgery(options =>{});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});


builder.Services.AddOpenApi();

var app = builder.Build();

app.UseStaticFiles();
app.UseDefaultFiles();

app.UseRouting();
app.UseAntiforgery();
app.MapGet("/", (HttpContext context) =>
{
    context.Response.Redirect("/html/index.html");
});

app.UseCors("AllowAllOrigins");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseAntiforgery();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
