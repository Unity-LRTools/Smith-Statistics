namespace LRT.Smith.Statistics.Editor
{
	public class StatisticSettings : LazySingletonScriptableObject<StatisticSettings>
	{
		private static string GetPath() => "Assets/Smith/Statistics/Resources"; //Called by reflection
		
		public string exportPath;
		public bool developerMode;
	}
}
