using LRT.Easing;
using System;

namespace LRT.Smith.Statistics
{
	[Serializable]
	public class StatisticRange
	{
		public string statisticName;
		public StatisticType valueType;

		public Ease ease;
		public int maxLevel;
		public float maxValue;
		public float minValue;

		public StatisticRange() { }

		public StatisticRange(StatisticRange range)
		{
			statisticName = range.statisticName;
			valueType = range.valueType;
			ease = range.ease;
			maxLevel = range.maxLevel;
			maxValue = range.maxValue;
			minValue = range.minValue;
		}
	}
}

