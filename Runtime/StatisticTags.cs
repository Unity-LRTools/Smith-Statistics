using LRT.Utility;
using System.Collections.Generic;

namespace LRT.Smith.Statistics
{
	public class StatisticTags : Tags
	{
		protected sealed override IEnumerable<string> GetOptions()
		{
			return StatisticsData.Instance.statisticTags;
		}
	}
}

