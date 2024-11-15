using LRT.Easing;
using LRT.Easing.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LRT.Smith.Statistics.Editor
{
	public class StatisticSettingsWizard : EditorWindow
	{
		private SSWizardPanelType state = SSWizardPanelType.Create;
		private Dictionary<SSWizardPanelType, SSWizardPanel> panels = new Dictionary<SSWizardPanelType, SSWizardPanel>
		{
			{  SSWizardPanelType.Create, new SSWizardPanelCreate() },
			{  SSWizardPanelType.Update, new SSWizardPanelUpdate() },
		};

		[MenuItem("Smith/Statistics")]
		public static void ShowWindow()
		{
			GetWindow<StatisticSettingsWizard>("Statistics Editor");
		}

		private void OnGUI()
		{
			EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);

			panels[state].Show();

			GUILayout.FlexibleSpace();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Create"))
				panels[state].OnActionButton();
			EditorGUILayout.EndHorizontal();
		}

		private enum SSWizardPanelType
		{
			Update,
			Create,
		}

		private abstract class SSWizardPanel
		{
			public abstract void Show();
			public abstract void OnActionButton();
		}

		private class SSWizardPanelUpdate : SSWizardPanel
		{
			Ease easing;

			public override void Show()
			{
				easing = EaseGUILayout.Ease("Curve", easing);
			}

			public override void OnActionButton()
			{
				throw new NotImplementedException();
			}
		}

		private class SSWizardPanelCreate : SSWizardPanel
		{
			const string folderPath = "Assets/Scripts/Smith";
			readonly string filePath = Path.Combine(folderPath, "Statistics.cs");

			private StatisticRange range = new StatisticRange();

			#region Template
			/// <summary>
			/// Parameters :
			/// [[STATISTIC_NAME]]  - The name of the statistic following CamelCase
			/// [[STATISTIC_TYPE]]  - Weither the statistic is SInt or SFloat
			/// [[STATISTIC_RANGE]] - Following format: ["key"] where key is target range
			/// </summary>
			private const string classTemplate = 
@"	public class [[STATISTIC_NAME]] : [[STATISTIC_TYPE]]
	{
		public [[STATISTIC_NAME]](int level = 0) : base(StatisticsData.Instance.statisticsRange[[STATISTIC_RANGE]], level) { }
	}
";

			/// <summary>
			/// Parameters :
			/// [[CLASSES]] - Insert all the statistic class at this place
			/// </summary>
			private const string fileTemplate = 
@"namespace LRT.Smith.Statistics
{
[[CLASSES]]
}
";
			#endregion

			public override void Show()
			{
				range.statisticName = EditorGUILayout.TextField("Statistic name", range.statisticName);
				range.valueType = (StatisticType)EditorGUILayout.EnumPopup("Value type", range.valueType);
				range.maxLevel = EditorGUILayout.IntField("Max Level", range.maxLevel);
				range.ease = EaseGUILayout.Ease("Growth", range.ease, true);

				if (range.valueType == StatisticType.Int)
				{
					range.minValue = EditorGUILayout.IntField("Min Value", (int)range.minValue);
					range.maxValue = EditorGUILayout.IntField("Max Value", (int)range.maxValue);
				}
				else
				{
					range.minValue = EditorGUILayout.FloatField("Min Value", range.minValue);
					range.maxValue = EditorGUILayout.FloatField("Max Value", range.maxValue);
				}
			}

			public override void OnActionButton()
			{
				if (!Directory.Exists(folderPath))
					Directory.CreateDirectory(folderPath);

				string classes = string.Empty;

				StatisticRange newRange = new StatisticRange(range);
				StatisticsData.Instance.statisticsRange.Add(newRange.statisticName, newRange);

				foreach (KeyValuePair<string, StatisticRange> item in StatisticsData.Instance.statisticsRange)
				{
					string template = classTemplate
						.Replace("[[STATISTIC_NAME]]", item.Value.statisticName)
						.Replace("[[STATISTIC_TYPE]]", StatisticTypeToClassType(item.Value.valueType))
						.Replace("[[STATISTIC_RANGE]]", $"[\"{item.Key}\"]");

					classes += template;
				}

				string fileContent = fileTemplate.Replace("[[CLASSES]]", classes);

				EditorUtility.SetDirty(StatisticsData.Instance);
				File.WriteAllText(filePath, fileContent);
				AssetDatabase.Refresh();
			}

			public string StatisticTypeToClassType(StatisticType type)
			{
				return type switch
				{
					StatisticType.Int => "StatsInt",
					StatisticType.Float => "StatsFloat",
					_ => throw new NotImplementedException($"Statistic type {nameof(type)} is not implemented.")
				};

			}
		}
	}
}

