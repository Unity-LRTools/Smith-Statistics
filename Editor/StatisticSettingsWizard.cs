using LRT.Easing;
using UnityEditor;

namespace LRT.Smith.Statistics.Editor
{
	public class StatisticSettingsWizard : EditorWindow
	{
		public Ease easing;

		[MenuItem("Smith/Statistics")]
		public static void ShowWindow()
		{
			GetWindow<StatisticSettingsWizard>("Statistics Editor");
		}

		private void OnGUI()
		{
			EditorGUILayout.LabelField("Statistics");
		}
	}
}

