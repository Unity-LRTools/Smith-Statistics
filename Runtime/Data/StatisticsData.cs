using System.Collections.Generic;
using System.Linq;

namespace LRT.Smith.Statistics
{
	public class StatisticsData : LazySingletonScriptableObject<StatisticsData>
    {
        [ReadOnly] public List<StatisticRange> statisticsRange = new List<StatisticRange>();

        public StatisticRange GetByID(string id)
        {
            return statisticsRange.FirstOrDefault(sr => sr.statisticID == id);
        }
    }
}

