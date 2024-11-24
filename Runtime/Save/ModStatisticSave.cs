using System.Collections.Generic;
using System.Linq;

namespace LRT.Smith.Statistics
{
	public static class ModStatisticSaveExtension
	{
		public static ModStatisticSave Save(this ModStatistic statistic) => new ModStatisticSave(statistic);
		public static ModStatistic Load(ModStatisticSave save)
		{
			return new ModStatistic(save);
		}
	}

	public struct ModStatisticSave
	{
		public StatisticSave baseSave;
		public List<Modifier> offsets;
		public List<Modifier> percentages;
		public bool hasFixedValue;
		public float fixedValue;
		public bool hasFixedPercent;
		public float fixedPercent;

		public ModStatisticSave(ModStatistic statistic)
		{
			baseSave = ((Statistic)statistic).Save();
			offsets = statistic.Offsets.ToList();
			percentages = statistic.Percentages.ToList();
			hasFixedValue = statistic.fixedValue.HasValue;
			fixedValue = statistic.fixedValue.Value;
			hasFixedPercent = statistic.fixedPercentage.HasValue;
			fixedPercent = statistic.fixedPercentage.Value;
		}
	}
}

