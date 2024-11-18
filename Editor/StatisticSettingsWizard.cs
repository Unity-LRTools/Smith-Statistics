using LRT.Easing;
using LRT.Easing.Editor;
using LRT.Utility;
using LRT.Utility.Editor;
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
		private Dictionary<SSWizardPanelType, SSWizardPanel> panels;

		const string folderPath = "Assets/Scripts/Smith";
		readonly string filePath = Path.Combine(folderPath, "Statistics.cs");

		List<Vector2> resultScrollPositions = new List<Vector2>();
		int resultScrollCount = 1;

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

		[MenuItem("Smith/Statistics")]
		public static void ShowWindow()
		{
			StatisticSettingsWizard wizard = GetWindow<StatisticSettingsWizard>("Statistics Editor");
		}

		void OnGUI()
		{
			if (panels == null)
			{
				panels = new Dictionary<SSWizardPanelType, SSWizardPanel>
				{
					{  SSWizardPanelType.Create, new SSWizardPanelCreate(this) },
					{  SSWizardPanelType.Update, new SSWizardPanelUpdate(this) },
					{  SSWizardPanelType.Edit, new SSWizardPanelEdit(this) },
				};
			}

			DrawTopContainer(EditorGUILayout.GetControlRect(false, 50));

			panels[state].Show();

			DrawBottomContainer();
		}

		public void OpenEditFor(StatisticRange range, int index)
		{
			SSWizardPanelEdit editPanel = panels[SSWizardPanelType.Edit] as SSWizardPanelEdit;
			editPanel.range = new StatisticRange(range);
			editPanel.index = index;
			state = SSWizardPanelType.Edit;
			GUI.FocusControl(null);
		}

		public void DrawEditableRange(StatisticRange range)
		{
			if (range == null)
				return;

			range.statisticName = EditorGUILayout.TextField("Statistic name", range.statisticName);
			range.valueType = (StatisticType)EditorGUILayout.EnumPopup("Value type", range.valueType);
			range.ease = EaseGUILayout.Ease("Growth", range.ease, true);
			range.maxLevel = EditorGUILayout.IntField("Max Level", Mathf.Max(1, range.maxLevel));

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

			if (IsErrors(out List<string> errors, range))
			{
				foreach(string error in errors)
				{
					EditorGUILayout.HelpBox(error, MessageType.Error);
				}
			}
		}

		public void DrawResultValues(StatisticRange range)
		{
			if (range.maxLevel <= 10)
			{
				for (int i = 1; i <= range.maxLevel; i++)
				{
					EditorGUILayout.LabelField($"[{i}] => {CalculateValueForLevel(i)}");
				}
			}
			else
			{
				EditorGUILayout.BeginHorizontal();
				while (resultScrollPositions.Count <= resultScrollCount)
				{
					resultScrollPositions.Add(new Vector2());
				}

				for (int i = 0; i < resultScrollCount; i++)
				{
					using (var scrollView = new EditorGUILayout.ScrollViewScope(resultScrollPositions[i], GUILayout.Height(EditorGUIUtility.singleLineHeight * 11), GUILayout.Width(170)))
					{
						resultScrollPositions[i] = scrollView.scrollPosition;
						for (int j = 1; j <= range.maxLevel; j++)
						{
							EditorGUILayout.LabelField($"[{j}] => {CalculateValueForLevel(j)}", GUILayout.Width(130));
						}
					}
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("+"))
					resultScrollCount++;
				if (GUILayout.Button("-"))
					resultScrollCount--;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
			}

			float CalculateValueForLevel(int level)
			{
				float easedValue = range.ease.Evaluate((level - 1) / (float)(range.maxLevel - 1));
				float value = Mathf.Lerp(range.minValue, range.maxValue, easedValue);

				return range.valueType == StatisticType.Int ? (int)value : value;
			}
		}

		public void SaveSettings()
		{
			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);

			string classes = string.Empty;

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

			string StatisticTypeToClassType(StatisticType type)
			{
				return type switch
				{
					StatisticType.Int => "StatsInt",
					StatisticType.Float => "StatsFloat",
					_ => throw new NotImplementedException($"Statistic type {nameof(type)} is not implemented.")
				};
			}
		}

		public bool IsErrors(out List<string> errors, StatisticRange range)
		{
			errors = new List<string>();

			if (string.IsNullOrEmpty(range.statisticName))
				errors.Add($"The statistic name should not be empty.");

			if (range.minValue == range.maxValue && range.maxLevel > 0)
				errors.Add($"The min value and max value should not be equal when max level is superior to 1");

			if (range.maxLevel == 0)
				errors.Add($"Max level should at least be one.");

			errors.AddRange(panels[state].GetErrors(range));

			return errors.Count != 0;
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

		private void DrawBottomContainer()
		{
			GUILayout.FlexibleSpace();
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			bool wasEnable = GUI.enabled;
			GUI.enabled = panels[state].ActionButtonIsValid();
			if (!string.IsNullOrEmpty(panels[state].ActionButtonLabel()) && GUILayout.Button(panels[state].ActionButtonLabel(), GUILayout.Width(150), GUILayout.Height(35)))
				panels[state].OnActionButton();
			GUI.enabled = wasEnable;
			
			if (GUILayout.Button("Exit", GUILayout.Width(150), GUILayout.Height(35)))
				focusedWindow.Close();

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}

		private enum SSWizardPanelType
		{
			Update,
			Create,
			Edit,
		}

		private abstract class SSWizardPanel
		{
			protected StatisticSettingsWizard wizard;

			public SSWizardPanel(StatisticSettingsWizard wizard)
			{
				this.wizard = wizard;
			}

			public abstract void Show();
			public abstract void OnActionButton();
			public abstract string ActionButtonLabel();
			public abstract bool ActionButtonIsValid();
			public abstract List<string> GetErrors(StatisticRange range);
		}

		private class SSWizardPanelUpdate : SSWizardPanel
		{
			public SSWizardPanelUpdate(StatisticSettingsWizard wizard) : base(wizard) { }

			/// <summary>
			/// <b>Parameters :</b>
			/// <br/> - The clicked range
			/// <br/> - The index of this range in the list
			/// </summary>
			public static event Action<StatisticRange, int> OnStatisticClicked;

			public override void Show()
			{
				Rect startRect = EditorGUILayout.GetControlRect();
				const int width = 215;
				const int height = 65;
				const int space = 10;
				int nbCol = Mathf.Max(Screen.width / width, 1);

				GUIStyle centeredBoldLabel = CustomGUIStyle.Label(fontStyle: FontStyle.Bold, alignment: TextAnchor.UpperCenter);
				GUIStyle centeredLabel = CustomGUIStyle.Label(alignment: TextAnchor.UpperCenter);
				GUIStyle rightLabel = CustomGUIStyle.Label(alignment: TextAnchor.UpperRight);

				for (int i = 0; i < StatisticsData.Instance.statisticsRange.Count; i++)
				{
					StatisticRange range = StatisticsData.Instance.statisticsRange[i];

					// Setup rect
					Rect rect = startRect;
					rect = rect.SetWidth(width);
					rect = rect.SetHeight(height);
					rect = rect.MoveX((i % nbCol) * (width + space));
					rect = rect.MoveY((i / nbCol) * (height + space));

					// Prepare variables
					string name = $"{range.statisticName}";
					string type = $"{range.valueType}";
					string values = $"1 -> [{range.minValue}..{range.maxValue}] <- {range.maxLevel}";
					string ease = $"Growth: {range.ease}";
					Color borderColor = rect.Contains(Event.current.mousePosition) ? Color.red : Color.white;

					// Draw box and cursor
					EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
					CustomGUIUtility.DrawBorder(rect, borderColor, 2);

					// Prepare button before shrinking
					if (GUI.Button(rect, "", GUIStyle.none))
						wizard.OpenEditFor(range, i);

					// Shrink after draw border to keep original size
					rect = rect.Shrink(3);

					// Draw field
					EditorGUI.LabelField(rect.SetHeight(EditorGUIUtility.singleLineHeight), name, centeredBoldLabel);
					EditorGUI.LabelField(RectUtility.lastRect, type, rightLabel);
					EditorGUI.LabelField(RectUtility.lastRect.MoveY(20), values, centeredLabel);
					EditorGUI.LabelField(RectUtility.lastRect.MoveY(20), ease, centeredLabel);
				}

				focusedWindow?.Repaint();
			}

			public override void OnActionButton()
			{
				throw new NotImplementedException();
			}

			public override string ActionButtonLabel() => "";

			public override bool ActionButtonIsValid() => true;

			public override List<string> GetErrors(StatisticRange range) => null;
		}

		private class SSWizardPanelCreate : SSWizardPanel
		{
			private StatisticRange range = new StatisticRange();

			public SSWizardPanelCreate(StatisticSettingsWizard wizard) : base(wizard) { }

			public override void Show()
			{
				wizard.DrawEditableRange(range);
				wizard.DrawResultValues(range);
			}

			public override void OnActionButton()
			{
				StatisticRange newRange = new StatisticRange(range);
				StatisticsData.Instance.statisticsRange.Add(newRange);

				wizard.SaveSettings();
				GUI.FocusControl(null);
			}

			public override string ActionButtonLabel() => "Create";

			public override bool ActionButtonIsValid() => !wizard.IsErrors(out List<string> errors, range);

			public override List<string> GetErrors(StatisticRange range)
			{
				List<string> errors = new List<string>();

				if (StatisticsData.Instance.GetByName(range.statisticName) != null)
					errors.Add($"The statistic name '{range.statisticName}' is already taken.");

				return errors;
			}
		}

		private class SSWizardPanelEdit : SSWizardPanel
		{
			public StatisticRange range;
			public int index;

			public SSWizardPanelEdit(StatisticSettingsWizard wizard) : base(wizard) { }

			public override string ActionButtonLabel() => "Edit";

			public override void Show()
			{
				wizard.DrawEditableRange(range);
				wizard.DrawResultValues(range);
			}

			public override void OnActionButton()
			{
				StatisticsData.Instance.statisticsRange[index] = range;

				wizard.SaveSettings();
				wizard.state = SSWizardPanelType.Update;
				GUI.FocusControl(null);
			}

			public override bool ActionButtonIsValid() => !wizard.IsErrors(out List<string> errors, range);

			public override List<string> GetErrors(StatisticRange editedRange)
			{
				List<string> errors = new List<string>();

				for(int i = 0; i < StatisticsData.Instance.statisticsRange.Count; i++)
				{
					if (i == index)
						continue;

					if (editedRange.statisticName == StatisticsData.Instance.statisticsRange[i].statisticName)
						errors.Add($"The statistic name '{editedRange.statisticName}' is already taken.");
				}

				return errors;
			}
		}
	}
}

