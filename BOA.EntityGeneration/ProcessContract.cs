using System;

namespace BOA.EntityGeneration
{
    /// <summary>
    ///     The process information
    /// </summary>
    [Serializable]
    public class ProcessContract
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the current.
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        ///     Gets the percentage of completion.
        /// </summary>
        public int PercentageOfCompletion
        {
            get
            {
                if (Current == 0)
                {
                    return 0;
                }

                return (int) (Current / (double) Total * 100);
            }
        }

        /// <summary>
        ///     Gets or sets the text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        ///     Gets or sets the total.
        /// </summary>
        public int Total { get; set; }
        #endregion
    }
}