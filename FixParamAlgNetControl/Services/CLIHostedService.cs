using FixParamAlgNetControl.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FixParamAlgNetControl.Services
{
    /// <summary>
    /// Represents the CLI hosted service.
    /// </summary>
    public class CLIHostedService : BackgroundService
    {
        /// <summary>
        /// Represents the configuration.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Represents the logger.
        /// </summary>
        private readonly ILogger<CLIHostedService> _logger;

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
        public CLIHostedService(IConfiguration configuration, ILogger<CLIHostedService> logger, IHostApplicationLifetime hostApplicationLifetime)
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
            var edgesFilepath = _configuration["Edges"];
            var targetNodesFilepath = _configuration["Targets"];
            var sourceNodesFilepath = _configuration["Sources"];
            var parametersFilepath = _configuration["Parameters"];
            var outputFilepath = _configuration["Output"];
            // Check if we have a file containing the edges.
            if (string.IsNullOrEmpty(edgesFilepath))
            {
                // Log an error.
                _logger.LogError("No file containing the network edges has been provided.");
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Check if we have a file containing the targets.
            if (string.IsNullOrEmpty(targetNodesFilepath))
            {
                // Log an error.
                _logger.LogError("No file containing the network target nodes has been provided.");
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Check if we have a file containing the sources.
            if (string.IsNullOrEmpty(sourceNodesFilepath))
            {
                // Log an error.
                _logger.LogError("No file containing the network source nodes has been provided.");
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Check if we have a file containing the targets.
            if (string.IsNullOrEmpty(parametersFilepath))
            {
                // Log an error.
                _logger.LogError("No file containing the parameters has been provided.");
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Get the current directory.
            var currentDirectory = Directory.GetCurrentDirectory();
            // Check if the file containing the edges exists.
            if (!File.Exists(edgesFilepath))
            {
                // Log an error.
                _logger.LogError($"The file \"{edgesFilepath}\" (containing the network edges) could not be found in the current directory \"{currentDirectory}\".");
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Check if the file containing the target nodes exists.
            if (!File.Exists(targetNodesFilepath))
            {
                // Log an error.
                _logger.LogError($"The file \"{targetNodesFilepath}\" (containing the network target nodes) could not be found in the current directory \"{currentDirectory}\".");
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Check if the file containing the source nodes exists.
            if (!File.Exists(sourceNodesFilepath))
            {
                // Log an error.
                _logger.LogError($"The file \"{sourceNodesFilepath}\" (containing the network source nodes) could not be found in the current directory \"{currentDirectory}\".");
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Check if the file containing the parameters exists.
            if (!File.Exists(parametersFilepath))
            {
                // Log an error.
                _logger.LogError($"The file \"{parametersFilepath}\" (containing the parameters) could not be found in the current directory \"{currentDirectory}\".");
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Define the variables needed for the analysis.
            var nodes = new List<string>();
            var edges = new List<(string SourceNode, string TargetNode)>();
            var targetNodes = new List<string>();
            var sourceNodes = new List<string>();
            var parameters = new Parameters();
            // Try to read the edges from the file.
            try
            {
                // Read all the rows in the file and parse them into edges.
                edges = File.ReadAllLines(edgesFilepath)
                    .Select(item => item.Split(";"))
                    .Where(item => item.Length > 1)
                    .Where(item => !string.IsNullOrEmpty(item[0]) && !string.IsNullOrEmpty(item[1]))
                    .Select(item => (item[0], item[1]))
                    .Distinct()
                    .ToList();
            }
            catch (Exception exception)
            {
                // Log an error.
                _logger.LogError($"The error \"{exception.Message}\" occured while reading the file \"{edgesFilepath}\" (containing the edges).");
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Try to read the target nodes from the file.
            try
            {
                // Read all the rows in the file and parse them into nodes.
                targetNodes = File.ReadAllLines(targetNodesFilepath)
                    .Where(item => !string.IsNullOrEmpty(item))
                    .Distinct()
                    .ToList();
            }
            catch (Exception exception)
            {
                // Log an error.
                _logger.LogError($"The error \"{exception.Message}\" occured while reading the file \"{targetNodesFilepath}\" (containing the target nodes).");
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Check if there are any preferred nodes to read.
            if (!string.IsNullOrEmpty(sourceNodesFilepath))
            {
                // Try to read the preferred nodes from the file.
                try
                {
                    // Read all the rows in the file and parse them into nodes.
                    sourceNodes = File.ReadAllLines(sourceNodesFilepath)
                        .Where(item => !string.IsNullOrEmpty(item))
                        .Distinct()
                        .ToList();
                }
                catch (Exception exception)
                {
                    // Log an error.
                    _logger.LogError($"The error \"{exception.Message}\" occured while reading the file \"{sourceNodesFilepath}\" (containing the preferred nodes).");
                    // Stop the application.
                    _hostApplicationLifetime.StopApplication();
                    // Return a successfully completed task.
                    return;
                }
            }
            // Try to read the parameters from the file.
            try
            {
                // Read and parse the parameters from the file.
                parameters = JsonSerializer.Deserialize<Parameters>(File.ReadAllText(parametersFilepath));
            }
            catch (Exception exception)
            {
                // Log an error.
                _logger.LogError($"The error \"{exception.Message}\" occured while reading the file \"{parametersFilepath}\" (containing the parameters).");
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Check if there isn't an output filepath provided.
            if (string.IsNullOrEmpty(outputFilepath))
            {
                // Assign the default value to the output filepath.
                outputFilepath = edgesFilepath.Replace(Path.GetExtension(edgesFilepath), $"_Output_{DateTime.Now:yyyyMMddHHmmss}.json");
            }
            // Try to write to the output file.
            try
            {
                // Write to the specified output file.
                File.WriteAllText(outputFilepath, string.Empty);
            }
            catch (Exception exception)
            {
                // Log an error.
                _logger.LogError($"The error \"{exception.Message}\" occured while trying to write to the output file \"{outputFilepath}\".");
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Check if there weren't any edges found.
            if (!edges.Any())
            {
                // Log an error.
                _logger.LogError($"No edges could be read from the file \"{edgesFilepath}\". Please check again the file and make sure that it is in the required format.");
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Get the actual nodes in the network.
            nodes = edges.Select(item => item.SourceNode)
                .Concat(edges.Select(item => item.TargetNode))
                .Distinct()
                .ToList();
            // Get the actual target nodes in the network.
            targetNodes = targetNodes.Intersect(nodes)
                .ToList();
            // Check if there weren't any target nodes found.
            if (!targetNodes.Any())
            {
                // Log an error.
                _logger.LogError($"No target nodes could be read from the file \"{targetNodesFilepath}\", or none of them could be found in the network. Please check again the file and make sure that it is in the required format.");
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Get the actual preferred nodes in the network.
            sourceNodes = sourceNodes.Intersect(nodes)
                .ToList();
            // Check if we should generate a new random seed.
            if (parameters.RandomSeed == -1)
            {
                // Generate a random random seed.
                parameters.RandomSeed = new Random().Next();
            }
            // Log a message about the loaded data.
            _logger.LogInformation(string.Concat("The following data has been loaded.",
                $"\n\t{edges.Count()} edge(s) and {nodes.Count()} nodes loaded from \"{edgesFilepath}\".",
                $"\n\t{targetNodes.Count()} target node(s) loaded from \"{targetNodesFilepath}\".",
                $"\n\t{sourceNodes.Count()} source node(s) loaded{(string.IsNullOrEmpty(sourceNodesFilepath) ? string.Empty : $" from {sourceNodesFilepath}")}."));
            // Log a message about the parameters.
            _logger.LogInformation(string.Concat($"The following parameters have been loaded from \"{parametersFilepath}\".",
                string.Join(string.Empty, typeof(Parameters).GetProperties().Select(item => $"\n\t{item.Name} = {item.GetValue(parameters)}"))));
            // Define the algorithm result.
            var result = new Result();
            // Try to run the algorithm.
            try
            {
                // Define the algorithm.
                var algorithm1 = new Algorithm
                {
                    Nodes = nodes,
                    Edges = edges,
                    SourceNodes = sourceNodes,
                    TargetNodes = targetNodes,
                    Parameters = parameters
                };
                // Run the algorithm on the given data.
                result = algorithm1.Run(_logger, _hostApplicationLifetime);
            }
            catch (Exception exception)
            {
                // Log an error.
                _logger.LogError($"The error \"{exception.Message}\" occured while running the algorithm.");
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Get the output text.
            var outputText = JsonSerializer.Serialize(new
            {
                Name = Path.GetFileNameWithoutExtension(edgesFilepath),
                Targets = Path.GetFileNameWithoutExtension(targetNodesFilepath),
                Sources = Path.GetFileNameWithoutExtension(sourceNodesFilepath),
                Parameters = parameters,
                Result = result
            }, new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true });
            // Try to write to the specified file.
            try
            {
                // Write the text to the file.
                File.WriteAllText(outputFilepath, outputText);
                // Log a message.
                _logger.LogInformation($"The results have been written in JSON format to the file \"{outputFilepath}\".");
            }
            catch (Exception exception)
            {
                // Log an error.
                _logger.LogError($"The error \"{exception.Message}\" occured while writing the results to the file \"{outputFilepath}\". The results will be displayed below instead.");
                // Log the output text.
                _logger.LogInformation(outputText);
                // Stop the application.
                _hostApplicationLifetime.StopApplication();
                // Return a successfully completed task.
                return;
            }
            // Stop the application.
            _hostApplicationLifetime.StopApplication();
            // Return a successfully completed task.
            return;
        }
    }
}
