using MathNet.Numerics.LinearAlgebra;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FixParamAlgNetControl.Models
{
    /// <summary>
    /// Represents the first algorithm.
    /// </summary>
    public class Algorithm
    {
        /// <summary>
        /// Represents the nodes of the network corresponding to the algorithm.
        /// </summary>
        public List<string> Nodes { get; set; }

        /// <summary>
        /// Represents the edges of the network corresponding to the algorithm.
        /// </summary>
        public List<(string, string)> Edges { get; set; }

        /// <summary>
        /// Represents the target nodes of the network corresponding to the algorithm.
        /// </summary>
        public List<string> TargetNodes { get; set; }

        /// <summary>
        /// Represents the source nodes corresponding to the algorithm.
        /// </summary>
        public List<string> SourceNodes { get; set; }

        /// <summary>
        /// Represents the maximum path length corresponding to the algorithm.
        /// </summary>
        public Parameters Parameters { get; set; }

        /// <summary>
        /// Checks if the parameters are valid for the current algorithm.
        /// </summary>
        /// <param name="logger">The application logger.</param>
        /// <returns>True if the parameters are valid, false otherwise.</returns>
        public bool HasValidParameters(ILogger logger)
        {
            // Check if the parameters are null.
            if (Parameters == null)
            {
                // Log a message.
                logger.LogError($"{DateTime.Now}: The parameters are null.");
                // Return false.
                return false;
            }
            // Check if the random seed is not valid.
            if (Parameters.RandomSeed < 0)
            {
                // Log a message.
                logger.LogError($"{DateTime.Now}: The random seed must be a non-negative integer.");
                // Return false.
                return false;
            }
            // Check if the maximum path length is not valid.
            if (Parameters.MaximumPathLength <= 0)
            {
                // Log a message.
                logger.LogError($"{DateTime.Now}: The maximum path length must be a positive integer.");
                // Return false.
                return false;
            }
            // Check if the number of rank computations is not valid.
            if (Parameters.RankComputations <= 0)
            {
                // Log a message.
                logger.LogError($"{DateTime.Now}: The number of rank computations must be a positive integer.");
                // Return false.
                return false;
            }
            // Return true.
            return true;
        }

        /// <summary>
        /// Checks if the algorithm is ready to run.
        /// </summary>
        /// <param name="logger">The application logger.</param>
        /// <returns>True if the algorithm is ready to run, false otherwise.</returns>
        public bool IsReadyToRun(ILogger logger)
        {
            // Check if the list of nodes is null or empty.
            if (Nodes == null || !Nodes.Any())
            {
                // Log a message.
                logger.LogError($"{DateTime.Now}: The list of nodes is null or empty.");
                // Return false.
                return false;
            }
            // Check if the list of edges is null or empty.
            if (Edges == null || !Edges.Any())
            {
                // Log a message.
                logger.LogError($"{DateTime.Now}: The list of edges is null or empty.");
                // Return false.
                return false;
            }
            // Check if the list of nodes is null or empty.
            if (TargetNodes == null || !TargetNodes.Any())
            {
                // Log a message.
                logger.LogError($"{DateTime.Now}: The list of target nodes is null or empty.");
                // Return false.
                return false;
            }
            // Check if the list of nodes is null or empty.
            if (SourceNodes == null || !SourceNodes.Any())
            {
                // Log a message.
                logger.LogError($"{DateTime.Now}: The list of source nodes is null or empty.");
                // Return false.
                return false;
            }
            // Check if the parameters are valid.
            if (!HasValidParameters(logger))
            {
                // Log a message.
                logger.LogError($"{DateTime.Now}: The parameters are invalid for the current algorithm.");
                // Return false.
                return false;
            }
            // Return true.
            return true;
        }

        /// <summary>
        /// Runs the algorithm.
        /// </summary>
        /// <param name="logger">The application logger.</param>
        /// <param name="cancellationToken">The cancellation token for the application.</param>
        /// <returns>A JSON object containing the final result.</returns>
        public Result Run(ILogger logger, CancellationToken cancellationToken)
        {
            // Log a message.
            logger.LogInformation($"{DateTime.Now}: The algorithm has started.");
            // Check if the algorithm isn't ready to run.
            if (!IsReadyToRun(logger))
            {
                // Log a message.
                logger.LogError($"{DateTime.Now}: The algorithm failed the initial check. Please check the provided files and values.");
                // Return.
                return null;
            }
            // Define a new stopwatch to measure the running time.
            var stopwatch = new Stopwatch();
            // Start the stopwatch.
            stopwatch.Start();
            // Log a message.
            logger.LogInformation($"{DateTime.Now}: Computing the variables needed for the algorithm.");
            // Define a new random variable.
            var random = new Random(Parameters.RandomSeed);
            // Define the variables needed for the algorithm.
            var nodeIndices = GetNodeIndices(Nodes);
            var listMatrixA = Enumerable.Range(0, Parameters.RankComputations)
                .Select(_ => GetMatrixA(nodeIndices, Edges, random));
            var matrixC = GetMatrixC(nodeIndices, TargetNodes);
            var listPowersMatrixA = listMatrixA
                .Select(item => GetPowersMatrixA(item, Parameters.MaximumPathLength));
            var listPowersMatrixCA = listPowersMatrixA
                .Select(item => GetPowersMatrixCA(matrixC, item));
            // Get the maximum number of targets that can be controlled with the given source nodes.
            var maximumRank = GetStructuralKalmanMatrixRank(GetMatrixB(nodeIndices, SourceNodes), listPowersMatrixCA);
            // Log a message.
            logger.LogInformation($"{DateTime.Now}: The given source nodes can control at most {maximumRank} target nodes within a path of maximum length {Parameters.MaximumPathLength}.");
            // Get the initial best solution.
            var bestSolution = new List<string>();
            var bestSolutionCount = maximumRank + 1;
            // Define the variables required for the loop.
            var checkedSubsets = (long)0;
            var totalSubsets = (long)Math.Pow(2, SourceNodes.Count());
            // Get all of the subsets of source nodes.
            var subsets = GetAllSubsets(SourceNodes);
            // Use a new timer to display the progress.
            using (new Timer(item =>
                {
                    // Get the required variables.
                    var localCheckedSubsets = Interlocked.Read(ref checkedSubsets);
                    var localBestSolutionCount = Interlocked.CompareExchange(ref bestSolutionCount, 0, 0);
                    var localBestSolutionString = localBestSolutionCount == maximumRank + 1 ? "no" : localBestSolutionCount.ToString();
                    // Log a message.
                    logger.LogInformation($"{DateTime.Now}: {localCheckedSubsets} / {totalSubsets} subset(s) checked ({localBestSolutionString} best solution) with a total running time of {stopwatch.Elapsed}.");
                }, null, TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(30.0)))
            {
                // Check if the algorithm should run in parallel.
                if (Parameters.RunInParallel)
                {
                    // Go over all subsets of source nodes.
                    Parallel.ForEach(subsets, new ParallelOptions { CancellationToken = cancellationToken }, subset =>
                    {
                        // Increment the count of the checked subsets.
                        Interlocked.Increment(ref checkedSubsets);
                        // Get the number of nodes in the current subset.
                        var subsetCount = subset.Count();
                        var bestCount = Interlocked.CompareExchange(ref bestSolutionCount, 0, 0);
                        // Check if the current subset is empty or is larger or equal to the best solution.
                        if (subsetCount == 0 || bestCount <= subsetCount)
                        {
                            // Continue.
                            return;
                        }
                        // Get the number of controlled targets.
                        var rank = GetStructuralKalmanMatrixRank(GetMatrixB(nodeIndices, subset), listPowersMatrixCA);
                        // Check if the rank is equal to the maximum rank.
                        if (rank == maximumRank)
                        {
                            // Update the best solution.
                            Interlocked.Exchange(ref bestSolution, subset.ToList());
                            Interlocked.Exchange(ref bestSolutionCount, bestSolution.Count());
                        }
                    });
                }
                else
                {
                    // Go over all subsets of source nodes.
                    foreach (var subset in subsets)
                    {
                        // Check if the application close was requested.
                        if (cancellationToken.IsCancellationRequested)
                        {
                            // Log a message.
                            logger.LogInformation($"{DateTime.Now}: The algorithm has been stopped, as the application is closing.");
                            // Break.
                            break;
                        }
                        // Increment the count of the checked subsets.
                        checkedSubsets += 1;
                        // Get the number of nodes in the current subset.
                        var subsetCount = subset.Count();
                        var bestCount = bestSolutionCount;
                        // Check if the current subset is empty or is larger or equal to the best solution.
                        if (subsetCount == 0 || bestCount <= subsetCount)
                        {
                            // Continue.
                            continue;
                        }
                        // Get the number of controlled targets.
                        var rank = GetStructuralKalmanMatrixRank(GetMatrixB(nodeIndices, subset), listPowersMatrixCA);
                        // Check if the rank is equal to the maximum rank.
                        if (rank == maximumRank)
                        {
                            // Update the best solution.
                            bestSolution = subset.ToList();
                            bestSolutionCount = bestSolution.Count();
                        }
                    }
                }
            }
            // Stop the measuring watch.
            stopwatch.Stop();
            // Log a message.
            logger.LogInformation($"{DateTime.Now}: The algorithm has ended.");
            // Return the results.
            return new Result
            {
                NodeCount = Nodes.Count(),
                EdgeCount = Edges.Count(),
                TargetNodeCount = TargetNodes.Count(),
                SourceNodeCount = SourceNodes.Count(),
                MaximumRank = maximumRank,
                SolutionNodeCount = bestSolutionCount,
                SolutionNodes = bestSolution,
                TimeElapsed = stopwatch.Elapsed,
            };
        }

        /// <summary>
        /// Gets the dictionary containing, for each node, its index in the node list, for faster reference.
        /// </summary>
        /// <param name="nodes">The nodes of the graph.</param>
        /// <returns>The dictionary containing, for each node, its index in the node list, for faster reference.</returns>
        private static Dictionary<string, int> GetNodeIndices(List<string> nodes)
        {
            // Return the dictionary for nodes and their indices.
            return nodes.Select((item, index) => (item, index)).ToDictionary(item => item.item, item => item.index);
        }

        /// <summary>
        /// Computes the A matrix (corresponding to the adjacency matrix).
        /// </summary>
        /// <param name="nodeIndices">The dictionary containing, for each node, its index in the node list.</param>
        /// <param name="edges">The edges of the graph.</param>
        /// <returns>The A matrix (corresponding to the adjacency matrix).</returns>
        private static Matrix<double> GetMatrixA(Dictionary<string, int> nodeIndices, List<(string SourceNode, string TargetNode)> edges, Random random)
        {
            // Initialize the adjacency matrix with zero.
            var matrixA = Matrix<double>.Build.DenseDiagonal(nodeIndices.Count(), nodeIndices.Count(), 0.0);
            // Go over each of the edges.
            foreach (var (SourceNode, TargetNode) in edges)
            {
                // Set to 1.0 the corresponding entry in the matrix (source nodes are on the columns, target nodes are on the rows).
                matrixA[nodeIndices[TargetNode], nodeIndices[SourceNode]] = 1.0 + (random != null ? random.NextDouble() : 0.0);
            }
            // Return the matrix.
            return matrixA;
        }

        /// <summary>
        /// Computes the B matrix (corresponding to the source nodes).
        /// </summary>
        /// <param name="nodeIndices">The dictionary containing, for each node, its index in the node list.</param>
        /// <param name="sourceNodes">The source nodes for the algorithm.</param>
        /// <returns>The B matrix (corresponding to the source nodes).</returns>
        private static Matrix<double> GetMatrixB(Dictionary<string, int> nodeIndices, List<string> sourceNodes)
        {
            // Initialize the B matrix with zero.
            var matrixB = Matrix<double>.Build.Dense(nodeIndices.Count(), sourceNodes.Count());
            // Go over each source node,
            for (int index = 0; index < sourceNodes.Count(); index++)
            {
                // Set to 1.0 the corresponding entry in the matrix.
                matrixB[nodeIndices[sourceNodes[index]], index] = 1.0;
            }
            // Return the matrix.
            return matrixB;
        }

        /// <summary>
        /// Computes the C matrix (corresponding to the target nodes).
        /// </summary>
        /// <param name="nodeIndices">The dictionary containing, for each node, its index in the node list.</param>
        /// <param name="targetNodes">The target nodes for the algorithm.</param>
        /// <returns>The C matrix (corresponding to the target nodes).</returns>
        private static Matrix<double> GetMatrixC(Dictionary<string, int> nodeIndices, List<string> targetNodes)
        {
            // Initialize the C matrix with zero.
            var matrixC = Matrix<double>.Build.Dense(targetNodes.Count(), nodeIndices.Count());
            // Go over each target node,
            for (int index = 0; index < targetNodes.Count(); index++)
            {
                // Set to 1.0 the corresponding entry in the matrix.
                matrixC[index, nodeIndices[targetNodes[index]]] = 1.0;
            }
            // Return the matrix.
            return matrixC;
        }

        /// <summary>
        /// Computes the powers of the adjacency matrix A, up to a given maximum power.
        /// </summary>
        /// <param name="matrixA">The adjacency matrix of the graph.</param>
        /// <param name="maximumPathLength">The maximum path length for control in the graph.</param>
        /// <returns>The powers of the adjacency matrix A, up to a given maximum power.</returns>
        private static List<Matrix<double>> GetPowersMatrixA(Matrix<double> matrixA, int maximumPathLength)
        {
            // Initialize a matrix list with the identity matrix.
            var powers = new List<Matrix<double>>(maximumPathLength + 1)
            {
                Matrix<double>.Build.DenseIdentity(matrixA.RowCount)
            };
            // Up to the maximum power, starting from the first element.
            for (int index = 1; index < maximumPathLength + 1; index++)
            {
                // Multiply the previous element with the matrix itself.
                powers.Add(matrixA.Multiply(powers[index - 1]));
            }
            // Return the list.
            return powers;
        }

        /// <summary>
        /// Computes the powers of the combination between the target matrix C and the adjacency matrix A.
        /// </summary>
        /// <param name="matrixC">The matrix corresponding to the target nodes in the graph.</param>
        /// <param name="powersMatrixA">The list of powers of the adjacency matrix A.</param>
        /// <returns>The powers of the combination between the target matrix C and the adjacency matrix A.</returns>
        private static List<Matrix<double>> GetPowersMatrixCA(Matrix<double> matrixC, List<Matrix<double>> powersMatrixA)
        {
            // Initialize a new empty list.
            var powers = new List<Matrix<double>>(powersMatrixA.Count());
            // Go over each power of the adjacency matrix.
            foreach (var power in powersMatrixA)
            {
                // Left-multiply with the target matrix C.
                powers.Add(matrixC.Multiply(power));
            }
            // Return the list.
            return powers;
        }

        /// <summary>
        /// Get the rank corresponding to the provided list of source nodes.
        /// </summary>
        /// <param name="matrixB">The matrix corresponding to the source nodes in the graph.</param>
        /// <param name="powersMatrixCA">The list containing the different powers of the matrix (CA, CA^2, CA^3, ... ).</param>
        /// <returns>True if the chromosome is a solution, false otherwise</returns>
        private static int GetKalmanMatrixRank(Matrix<double> matrixB, List<Matrix<double>> powersMatrixCA)
        {
            // Initialize the R matrix.
            var matrixR = Matrix<double>.Build.DenseOfMatrix(powersMatrixCA[0]).Multiply(matrixB);
            // Repeat until we reach the maximum power.
            for (int index = 1; index < powersMatrixCA.Count(); index++)
            {
                // Compute the current power matrix.
                matrixR = matrixR.Append(powersMatrixCA[index].Multiply(matrixB));
            }
            // Return the rank of the matrix.
            return matrixR.Rank();
        }

        /// <summary>
        /// Get the structural rank corresponding to the provided list of source nodes. The structural rank is computed by repeatedly computing the rank and returning the first repeated value, or the largest.
        /// </summary>
        /// <param name="matrixB">The matrix corresponding to the source nodes in the graph.</param>
        /// <param name="listPowersMatrixCA">The list containing the lists of different powers of the matrix (CA, CA^2, CA^3, ... ).</param>
        /// <returns>True if the chromosome is a solution, false otherwise</returns>
        private static int GetStructuralKalmanMatrixRank(Matrix<double> matrixB, IEnumerable<List<Matrix<double>>> listPowersMatrixCA)
        {
            // Define the variable to return.
            var structuralRank = 0;
            // Go over each list of powers of the CA matrix.
            foreach (var powersMatrixCA in listPowersMatrixCA)
            {
                // Get the corresponding rank.
                var rank = GetKalmanMatrixRank(matrixB, powersMatrixCA);
                // Check if the current rank is different than the previous one.
                if (structuralRank < rank)
                {
                    // Assign the new structural rank.
                    structuralRank = rank;
                }
                else if (structuralRank == rank)
                {
                    // Break.
                    break;
                }
            }
            // Return the structural rank.
            return structuralRank;
        }

        /// <summary>
        /// Gets all of the subsets of a given list.
        /// </summary>
        /// <param name="list">The initial list.</param>
        /// <returns>All subsets of the list.</returns>
        private static IOrderedEnumerable<List<string>> GetAllSubsets(List<string> list)
        {
            // Return all of the subsets of the list, ordered by their size.
            return Enumerable.Range(0, 1 << list.Count())
                .Select(item => Enumerable.Range(0, list.Count())
                    .Where(item1 => (item & (1 << item1)) != 0)
                    .Select(item1 => list[item1])
                    .ToList())
                .OrderBy(item => item.Count());
        }
    }
}
