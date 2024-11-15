using LRT.Utility;

namespace LRT.Smith.Statistics
{
	public class StatisticsData : LazySingletonScriptableObject<StatisticsData>
    {
        public SDictionary<string, StatisticRange> statisticsRange = new SDictionary<string, StatisticRange>();
    }
}

