using LRT.Easing;
using System;

namespace LRT.Smith.Statistics
{
	[Serializable]
	public class StatisticRange
	{
		public string statisticID;
		public string name;
		public StatisticType valueType;

		public StatisticTags tags;
		public Ease ease;
		public int maxLevel;
		public float maxValue;
		public float minValue;
		public bool isModdable;
		public bool isClamp;
		public float clampMin;
		public float clampMax;

		public StatisticRange() { }

		public StatisticRange(StatisticRange range)
		{
			statisticID = range.statisticID;
			valueType = range.valueType;
			tags = range.tags;
			ease = range.ease;
			maxLevel = range.maxLevel;
			maxValue = range.maxValue;
			minValue = range.minValue;
			isModdable = range.isModdable;
			isClamp = range.isClamp;
			clampMin = range.clampMin;
			clampMax = range.clampMax;
		}
	}
}

