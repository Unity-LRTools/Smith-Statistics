using LRT.Easing;
using System;

namespace LRT.Smith.Statistics
{
	[Serializable]
	public class StatisticRange
	{
		public string statisticName;
		public StatisticType valueType;

		public int maxLevel;
		public Ease ease;
		public float maxValue;
		public float minValue;
	}
}

