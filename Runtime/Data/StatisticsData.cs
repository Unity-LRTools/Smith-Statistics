using System.Collections.Generic;
using System.Linq;

namespace LRT.Smith.Statistics
{
	public class StatisticsData : LazySingletonScriptableObject<StatisticsData>
    {
        [ReadOnly] public List<StatisticRange> statisticsRange = new List<StatisticRange>();

        public StatisticRange GetByName(string name)
        {
            return statisticsRange.FirstOrDefault(sr => sr.statisticName == name);
        }
    }
}

