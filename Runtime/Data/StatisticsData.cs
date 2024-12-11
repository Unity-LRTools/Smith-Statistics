using System.Collections.Generic;
using System.Linq;

namespace LRT.Smith.Statistics
{
	public class StatisticsData : LazySingletonScriptableObject<StatisticsData>
    {
		private static string GetPath() => "Assets/Smith/Statistics/Resources"; //Called by reflection

        [ReadOnly] public List<StatisticRange> statisticsRange = new List<StatisticRange>();
        [ReadOnly] public List<string> statisticTags = new List<string>();

		public StatisticRange GetByID(string id)
        {
            return statisticsRange.FirstOrDefault(sr => sr.statisticID == id);
        }
    }
}

