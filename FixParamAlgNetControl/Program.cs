using FixParamAlgNetControl.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace FixParamAlgNetControl
{
    /// <summary>
    /// Represents the main class of the application.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Initializes the application.
        /// </summary>
        /// <param name="args">Represents the parameters for the application.</param>
        static void Main(string[] args)
        {
            // Get the current command-line arguments configuration.
            var configuration = new ConfigurationBuilder().AddCommandLine(args).Build();
            // Get the mode in which to run the application.
            var mode = configuration["Mode"];
            // Get the host to run based on the command-line arguments and build it.
            using var host = (mode == "CLI" ? CreateCLIHostBuilder(args) : CreateDefaultHostBuilder(args)).Build();
            // Get the corresponding logger.
            var logger = host.Services.GetService<ILogger<Program>>();
            // Log a message.
            logger.LogInformation("The application has been started. You can press \"CTRL + C\" (\"Command + C\" on MacOS) to stop at any time.");
            // Try to run the application host.
            try
            {
                // Run the application host.
                host.Run();
            }
            catch (OperationCanceledException)
            {
                // Log a message.
                logger.LogInformation("The application has been stopped by the user.");
            }
        }

        /// <summary>
        /// Creates a CLI host builder with the given parameters.
        /// </summary>
        /// <param name="args">Represents the parameters for the host builder.</param>
        /// <returns>Returns a new host containing the given hosted service.</returns>
        static IHostBuilder CreateCLIHostBuilder(string[] args)
        {
            // Return a host with the given options.
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<CLIHostedService>();
                });
        }

        /// <summary>
        /// Creates a default host builder with the given parameters.
        /// </summary>
        /// <param name="args">Represents the parameters for the host builder.</param>
        /// <returns>Returns a new host containing the given hosted service.</returns>
        static IHostBuilder CreateDefaultHostBuilder(string[] args)
        {
            // Return a host with the given options.
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<DefaultHostedService>();
                });
        }
    }
}
