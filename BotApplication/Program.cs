﻿using BotApplication.Helper;
using BotApplication.Methods;
using BotApplication.Queue;
using BotApplication.Worker;
using Domain.Infrastructure.Context;
using Domain.Infrastructure.Repositories.Implementation;
using Domain.Infrastructure.Repositories.Interfaces;
using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
  public static async Task Main(string[] args)
  {
    // Build the configuration
    var configuration = TryGetConfigurationHelper.LoadConfiguration("appsettings.json");

    // Create a HostBuilder
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
          services.AddSingleton<IConfiguration>(configuration);
          services.AddScoped(serviceProvider =>
          {
            var connectionString = context.Configuration.GetConnectionString("DiscordBotEntitites");

            if (string.IsNullOrEmpty(connectionString))
            {
              return null;
            }

            return new DiscordbotContext(connectionString);
          });
          services.AddScoped<IServerCommandRepository, ServerCommandRepository>();
          services.AddScoped<IServerCommandService, ServerCommandService>();
          services.AddScoped<ClearingHelper>();
          services.AddScoped<CommandHelper>();
          services.AddScoped<RoleHelper>();
          services.AddSingleton<TaskQueue>();


          services.AddSingleton<Bot>();


        })
        .Build();

    // Resolve the Bot service and run it
    var bot = host.Services.GetRequiredService<Bot>();
    await bot.RunAsync();
  }
}