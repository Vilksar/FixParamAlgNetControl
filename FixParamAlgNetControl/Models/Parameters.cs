using System;
using System.Collections.Generic;
using System.Text;

namespace FixParamAlgNetControl.Models
{
    /// <summary>
    /// Represents the parameters for the algorithm.
    /// </summary>
    public class Parameters
    {
        /// <summary>
        /// Represents the random seed to be used in the algorithm.
        /// </summary>
        public int RandomSeed { get; set; } = -1;

        /// <summary>
        /// Represents the maximum path length to be used in the algorithm.
        /// </summary>
        public int MaximumPathLength { get; set; } = 5;

        /// <summary>
        /// Represents the number of matrix rank computations to determine its structural rank in the algorithm.
        /// </summary>
        public int RankComputations { get; set; } = 3;

        /// <summary>
        /// Represents the parallel flag to be used in the algorithm.
        /// </summary>
        public bool RunInParallel { get; set; } = false;
    }
}
