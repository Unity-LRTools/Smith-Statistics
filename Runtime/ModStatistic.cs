using System;
using System.Collections.Generic;
using System.Linq;

namespace LRT.Smith.Statistics
{
	public class Modifier
	{
		public float value;
		public string identifier;
	}

	public class ModStatistic : Statistic
	{
		private List<Modifier> offsets;
		private List<Modifier> multipliers;

		protected ModStatistic(StatisticRange range, int level = 1) : base(range, level) { }

		internal ModStatistic(StatisticSave save) : base(save) { }

		protected override float GetValue()
		{
			float baseValue = base.GetValue();

			baseValue += offsets.Sum(m => m.value);
			baseValue *= multipliers.Aggregate(0f, (acc, m) => acc + (m.value - 1));

			return baseValue;
		}
	}
}
