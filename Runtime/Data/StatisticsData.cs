using System.Collections.Generic;

namespace LRT.Smith.Statistics
{
	public class StatisticsData : LazySingletonScriptableObject<StatisticsData>
    {
        public Dictionary<string, StatisticRange> statisticsRange = new Dictionary<string, StatisticRange>();
    }
}

