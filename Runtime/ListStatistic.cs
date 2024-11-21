using System.Collections.Generic;
using System.Linq;

namespace LRT.Smith.Statistics
{
	public class ListStatistic : List<Statistic>
    {
        /// <summary>
        /// Return the sum of the desired type
        /// </summary>
        /// <param name="type">The desired statistic tpe</param>
        /// <returns>A sum</returns>
        public float Sum(string type)
        {
            return this.Where(s => s.ID == type).Sum(s => s.Value);
        }

        /// <summary>
        /// Count the total number of different statistic in the list
        /// </summary>
        /// <returns>The count of different statistic</returns>
        public int CountStatistic()
        {
            return this.GroupBy(s => s.ID).Count();
        }
    }
}
