using LRT.Easing;
using System;

namespace LRT.Smith.Statistics
{
	[Serializable]
	public class StatisticRange
	{
		string statisticType;
		StatisticType valueType;

		public float maxValue;
		public float minValue;
		public int maxLevel;
		public Ease ease;
	}
}

