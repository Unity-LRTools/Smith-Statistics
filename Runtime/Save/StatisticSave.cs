namespace LRT.Smith.Statistics
{
	public static class StatisticSaveHelper
	{
		public static StatisticSave Save(this Statistic statistic) => new StatisticSave(statistic);
		public static Statistic Load(StatisticSave save)
		{
			return new Statistic(save);
		}
	}

	public struct StatisticSave
	{
		public string id;
		public int level;

		public StatisticSave(Statistic statistic)
		{
			id = statistic.ID;
			level = statistic.Level;
		}
	}
}

