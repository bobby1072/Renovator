﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Renovator.ConsoleApp.Abstract;
using Renovator.ConsoleApp.Concrete;
using Renovator.Domain.Services.Extensions;

try
{
    using var host = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(config =>
        {
            config
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile(Path.GetFullPath("appsettings.json"), false);
        })
        .ConfigureServices((context, services) =>
        {
            services
                .AddRenovatorApplication(context.Configuration)
                .AddLogging(opts =>
                {
                    opts.SetMinimumLevel(LogLevel.None);
                })
                .AddTransient<IConsoleApplicationService, ConsoleApplicationService>()
                .AddHostedService<ConsoleRunnerService>();
        })
        .Build();


    await host.StartAsync();
    await host.StopAsync();
}
catch(Exception e)
{
    Console.Clear();
    Console.WriteLine($"Exception occured during setup with message: {Environment.NewLine}");
    Console.WriteLine(e.Message);
}


