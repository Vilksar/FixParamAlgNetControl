using System;
using System.Collections.Generic;
using System.Text;

namespace FixParamAlgNetControl.Models
{
    /// <summary>
    /// Represents a result of the algorithm.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Represents the number of nodes in the network corresponding to the result.
        /// </summary>
        public int NodeCount { get; set; }

        /// <summary>
        /// Represents the number of nodes in the network corresponding to the result.
        /// </summary>
        public int EdgeCount { get; set; }

        /// <summary>
        /// Represents the number of nodes in the network corresponding to the result.
        /// </summary>
        public int TargetNodeCount { get; set; }

        /// <summary>
        /// Represents the number of nodes in the network corresponding to the result.
        /// </summary>
        public int SourceNodeCount { get; set; }

        /// <summary>
        /// Represents the number of target nodes controlled by the solution nodes in the result.
        /// </summary>
        public int MaximumRank { get; set; }

        /// <summary>
        /// Represents the number of solution nodes in the result.
        /// </summary>
        public int SolutionNodeCount { get; set; }

        /// <summary>
        /// Represents the solution nodes in the result.
        /// </summary>
        public List<string> SolutionNodes { get; set; }

        /// <summary>
        /// Represents the time elapsed of the result.
        /// </summary>
        public TimeSpan TimeElapsed { get; set; }
    }
}
