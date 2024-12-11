using System.Collections.Generic;

namespace LRT.Smith.Statistics.Editor
{
	public class StatisticTagsDrawer : TagsDrawer
	{
		public override List<string> GetOptions() => StatisticsData.Instance.statisticTags;
	}
}
