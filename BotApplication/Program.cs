﻿using BotApplication.Helper;
using BotApplication.Methods;
using BotApplication.Queue;
using BotApplication.Worker;
using Domain.DependencyInjection;
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
    var configuration = TryGetConfigurationHelper.LoadConfiguration("appsettings.json");

    var configurationValidator = new ConfigurationValidator(configuration);
    configurationValidator.ValidateAndUpdateConfiguration();

    // Create a HostBuilder for DI etc.
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
          services.AddSingleton<IConfiguration>(configuration);
#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
          services.AddScoped(serviceProvider =>
          {
            var connectionString = context.Configuration.GetConnectionString("DiscordBotEntitites");

            if (string.IsNullOrEmpty(connectionString))
            {
              return null;
            }

            return new DiscordbotContext(connectionString);
          });
#pragma warning restore CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
          services.AddScoped<RepositoryResolver>();
          services.AddScoped(serviceProvider => serviceProvider.GetService<RepositoryResolver>().ResolveServerCommandRepository());
          services.AddScoped(serviceProvider => serviceProvider.GetService<RepositoryResolver>().ResolveGuildLineupRepository());
          services.AddScoped<IServerCommandService, ServerCommandService>();
          services.AddScoped<IGuildLineupService, GuildLineupService>();
          services.AddScoped<ClearingHelper>();
          services.AddScoped<CommandHelper>();
          services.AddScoped<RoleHelper>();
          services.AddScoped<GuildLineupHelper>();
          services.AddSingleton<TaskQueue>();


          services.AddSingleton<Bot>();


        })
        .Build();

    // Resolve the Bot service and run it
    var bot = host.Services.GetRequiredService<Bot>();
    await bot.RunAsync();
  }
}