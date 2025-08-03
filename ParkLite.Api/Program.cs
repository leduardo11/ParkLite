using Microsoft.Data.Sqlite;
using ParkLite.Api.Interfaces;
using ParkLite.Api.Repositories;
using ParkLite.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()
		      .AllowAnyMethod()
		      .AllowAnyHeader();
	});
});

builder.Services.AddScoped(_ =>
{
	var conn = new SqliteConnection(builder.Configuration.GetConnectionString("Default"));
	conn.Open();
	return conn;
});

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowAll");

app.MapControllers();

app.Run();
