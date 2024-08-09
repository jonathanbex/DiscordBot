
using DiscordBot.Middleware;
using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
		.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) // This will log to a file in a 'logs' folder
		.CreateLogger();

builder.Host.UseSerilog(); // Use Serilog for loggin

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDiscordService, DiscordService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting(); // Add this line
									// Add the SignatureValidationMiddleware before MVC or other middlewares
app.UseMiddleware<SignatureValidationMiddleware>();

app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
	endpoints.MapControllerRoute(
			name: "default",
			pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.MapControllers();

app.Run();
