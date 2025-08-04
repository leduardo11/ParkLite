using Microsoft.Data.Sqlite;
using ParkLite.Api.Interfaces;
using ParkLite.Api.Repositories;
using ParkLite.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped(_ =>
{
	var conn = new SqliteConnection(builder.Configuration.GetConnectionString("Default"));
	conn.Open();
	return conn;
});

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddControllers();

builder.WebHost.ConfigureKestrel(options =>
{
	var port = Environment.GetEnvironmentVariable("PORT");
	if (!int.TryParse(port, out var portNumber)) portNumber = 5000;
	options.ListenAnyIP(portNumber);
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
