using LRT.Easing.Editor;
using LRT.Utility;
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
			DrawTopContainer(EditorGUILayout.GetControlRect(false, 50));

			panels[state].Show();

			DrawBottomContainer();
		}

		private void DrawBottomContainer()
		{
			GUILayout.FlexibleSpace();
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (GUILayout.Button(panels[state].ActionButtonLabel(), GUILayout.Width(150), GUILayout.Height(35)))
				panels[state].OnActionButton();
			if (GUILayout.Button("Exit", GUILayout.Width(150), GUILayout.Height(35)))
				focusedWindow.Close();

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}

		private void DrawTopContainer(Rect container)
		{
			ShowMenuButton(container.LeftHalf(), "Update", SSWizardPanelType.Update);
			ShowMenuButton(container.RightHalf(), "Create", SSWizardPanelType.Create);

			void ShowMenuButton(Rect right, string label, SSWizardPanelType target)
			{
				if (state == target)
					GUI.enabled = false;

				if (GUI.Button(right, label))
					state = target;

				GUI.enabled = true;
			}
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
			public abstract string ActionButtonLabel();
		}

		private class SSWizardPanelUpdate : SSWizardPanel
		{
			public override void Show()
			{
				foreach (StatisticRange range in StatisticsData.Instance.statisticsRange)
				{
					EditorGUILayout.LabelField($"{range.statisticName} - ({range.valueType})");
				}
			}

			public override void OnActionButton()
			{
				throw new NotImplementedException();
			}

			public override string ActionButtonLabel() => "Update";
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
		public [[STATISTIC_NAME]](int level = 0) : base(StatisticsData.Instance.GetByName([[STATISTIC_RANGE]]), level) { }
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
				range.ease = EaseGUILayout.Ease("Growth", range.ease, true);
				range.maxLevel = EditorGUILayout.IntField("Max Level", range.maxLevel);

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
				StatisticsData.Instance.statisticsRange.Add(newRange);

				foreach (StatisticRange item in StatisticsData.Instance.statisticsRange)
				{
					string template = classTemplate
						.Replace("[[STATISTIC_NAME]]", item.statisticName)
						.Replace("[[STATISTIC_TYPE]]", StatisticTypeToClassType(item.valueType))
						.Replace("[[STATISTIC_RANGE]]", $"\"{item.statisticName}\"");

					classes += template;
				}

				string fileContent = fileTemplate.Replace("[[CLASSES]]", classes);

				File.WriteAllText(filePath, fileContent);
				EditorUtility.SetDirty(StatisticsData.Instance);
				AssetDatabase.SaveAssetIfDirty(StatisticsData.Instance);
				AssetDatabase.Refresh();
			}

			public override string ActionButtonLabel() => "Create";

			private string StatisticTypeToClassType(StatisticType type)
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

