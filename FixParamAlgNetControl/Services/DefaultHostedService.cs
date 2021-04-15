using FixParamAlgNetControl.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FixParamAlgNetControl.Services
{
    /// <summary>
    /// Represents the default hosted service.
    /// </summary>
    public class DefaultHostedService : BackgroundService
    {
        /// <summary>
        /// Represents the configuration.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Represents the logger.
        /// </summary>
        private readonly ILogger<DefaultHostedService> _logger;

        /// <summary>
        /// Represents the host application lifetime.
        /// </summary>
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="configuration">Represents the application configuration.</param>
        /// <param name="logger">Represents the logger.</param>
        /// <param name="hostApplicationLifetime">Represents the application lifetime.</param>
        public DefaultHostedService(IConfiguration configuration, ILogger<DefaultHostedService> logger, IHostApplicationLifetime hostApplicationLifetime)
        {
            _configuration = configuration;
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        /// <summary>
        /// Executes the background service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token corresponding to the task.</param>
        /// <returns>A runnable task.</returns>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Wait for a completed task, in order to not get a warning about having an async method.
            await Task.CompletedTask;
            // Get the parameters from the configuration.
            var mode = _configuration["Mode"];
            var generateParametersFileString = _configuration["GenerateParametersFile"];
            // Log a message.
            _logger.LogInformation(string.Concat(
                "\n\tWelcome to the FixedParamAlgStructTargetControl application!",
                "\n\t",
                "\n\t---",
                "\n\t",
                "\n\tAll argument names and values are case-sensitive. The following arguments can be provided:",
                "\n\t--Mode\tUse this argument to apecify the mode in which the application will run. The possible values are \"CLI\" (the application will run in the command-line) and \"Help\" (the application will display this help message). The default value is \"Help\".",
                "\n\tArguments for \"Help\" mode:",
                "\n\t--GenerateParametersFile\tUse this argument to instruct the application to generate, in the current directory, a model of the parameters JSON file (containing the default parameter values) required for running the algorithm. Writing permission is needed for the current directory. The default value is \"False\".",
                "\n\tArguments for \"Cli\" mode:",
                "\n\t--Edges\tUse this argument to specify the path to the file containing the edges of the network. Each edge should be on a new line, with its source and target nodes being separated by a semicolon character. This argument has no default value.",
                "\n\t--Targets\tUse this argument to specify the path to the file containing the target nodes of the network. Only nodes appearing in the network will be considered. Each node should be on a new line. This argument has no default value.",
                "\n\t--Sources\tUse this argument to specify the path to the file containing the source nodes of the network. Only nodes appearing in the network will be considered. Each node should be on a new line. This argument has no default value.",
                "\n\t--Parameters\tUse this argument to specify the path to the file containing the parameter values for the analysis. The file should be in JSON format. You can generate a model file by running the application with the \"Mode\" argument set to \"Help\" and the \"GenerateParametersFile\" argument set to \"True\". This argument has no default value.",
                "\n\t--Output\t(optional) Use this argument to specify the path to the output file where the results will be written. Writing permission is needed for the corresponding directory. If a file with the same name already exists, it will be automatically overwritten. The default value is the name of the file containing the edges, followed by the current date and time.",
                "\n\t",
                "\n\t---",
                "\n\t",
                "\n\tExamples of posible usage:",
                "\n\t--Mode \"Help\"",
                "\n\t--Mode \"Help\" --GenerateParametersFile \"True\"",
                "\n\t--Mode \"Cli\" --Edges \"Path/To/FileContainingEdges.extension\" --Targets \"Path/To/FileContainingTargetNodes.extension\" --Sources \"Path/To/FileContainingSourceNodes.extension\" --Parameters \"Path/To/FileContainingParameters.extension\"",
                "\n\t--Mode \"Cli\" --Edges \"Path/To/FileContainingEdges.extension\" --Targets \"Path/To/FileContainingTargetNodes.extension\" --Sources \"Path/To/FileContainingSourceNodes.extension\" --Parameters \"Path/To/FileContainingParameters.extension\" --Output \"Path/To/OutputFile.extension\"",
                "\n\t"));
            // Check if the mode is not valid.
            if (!string.IsNullOrEmpty(mode) && mode != "Help")
            {
                // Log an error.
                _logger.LogError($"The provided mode \"{mode}\" for running the application is not valid.");
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Check if the parameters file should be generated.
            if (bool.TryParse(generateParametersFileString, out var generateParametersFile) && generateParametersFile)
            {
                // Define the output file.
                var outputFile = "DefaultParameters.json";
                // Get the output text.
                var outputText = JsonSerializer.Serialize(new Parameters(), new JsonSerializerOptions { WriteIndented = true });
                // Try to write to the specified file.
                try
                {
                    // Write the text to the file.
                    File.WriteAllText(outputFile, outputText);
                    // Log a message.
                    _logger.LogInformation($"The default parameters have been written to the file \"{outputFile}\".");
                }
                catch (Exception exception)
                {
                    // Log an error.
                    _logger.LogError($"The error \"{exception.Message}\" occurred while writing the default parameters to the file \"{outputFile}\". The default parameters will be displayed below instead.");
                    // Log the output text.
                    _logger.LogInformation(outputText);
                    // Stop the application.
                    _hostApplicationLifetime.StopApplication();
                    // Return a successfully completed task.
                    return;
                }
            }
            // Stop the application.
            _hostApplicationLifetime.StopApplication();
            // Return a successfully completed task.
            return;
        }
    }
}
