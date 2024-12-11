using LRT.Easing;
using LRT.Easing.Editor;
using LRT.Utility;
using LRT.Utility.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LRT.Smith.Statistics.Editor
{
	public class StatisticSettingsWizard : EditorWindow
	{
		#region Fields
		private SSWizardPanelType state = SSWizardPanelType.Create;
		private Dictionary<SSWizardPanelType, SSWizardPanel> panels;

		const string folderPath = "Assets/Smith/Statistics/";

		readonly string filePath = Path.Combine(folderPath, "Statistics.cs");

		Vector2 scrollPosition; //Scroll position for whole window
		List<Vector2> resultScrollPositions = new List<Vector2>();
		int resultScrollCount = 1;
		#endregion

		#region Template
		/// <summary>
		/// Parameters :
		/// [[STATISTIC_ID]]  - The name of the statistic following CamelCase
		/// [[STATISTIC_CLASS]]  - The class of this statistic if it's moddable or not
		/// </summary>
		private const string classTemplate =
@"	public class [[STATISTIC_ID]] : [[STATISTIC_CLASS]]
	{
		public [[STATISTIC_ID]](int level = 1) : base(nameof([[STATISTIC_ID]]), level) { }
	}
";

		/// <summary>
		/// Parameters :
		/// [[DATE]] - The date the code has been generated
		/// [[CLASSES]] - Insert all the statistic class at this place
		/// </summary>
		private const string fileTemplate =
@"/**
* DISCLAIMER: This code has been generated automatically.
* Generated on: [[DATE]]
*/
namespace LRT.Smith.Statistics
{
[[CLASSES]]
}
";
		#endregion

		[MenuItem("Smith/Statistics")]
		public static void ShowWindow()
		{
			StatisticSettingsWizard wizard = GetWindow<StatisticSettingsWizard>("Statistics");
			wizard.titleContent = new GUIContent("Statistics", EditorGUIUtility.IconContent("CustomTool@2x").image);
		}

		private void OnEnable()
		{
			if (panels == null)
			{
				panels = new Dictionary<SSWizardPanelType, SSWizardPanel>
				{
					{  SSWizardPanelType.Create, new SSWizardPanelCreate(this) },
					{  SSWizardPanelType.Read, new SSWizardPanelRead(this) },
					{  SSWizardPanelType.Edit, new SSWizardPanelEdit(this) },
					{  SSWizardPanelType.Settings, new SSWizardPanelSettings(this) },
				};
				state = SSWizardPanelType.Read;
			}
		}

		void OnGUI()
		{
			DrawTopContainer(EditorGUILayout.GetControlRect(false, 50));

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			panels[state].Show();
			EditorGUILayout.EndScrollView();

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

		int mask = 0;
		public void DrawEditableRange(StatisticRange range)
		{
			if (range == null)
				return;

			GUIContent modGUIContent = new GUIContent("Is Moddable", "Moddable statistic can be modified using offsets and percentage.");

			bool wasGuiEnabled = GUI.enabled;
			GUI.enabled = StatisticSettings.Instance.developerMode;
			range.statisticID = EditorGUILayout.TextField("Statistic id", range.statisticID);
			GUI.enabled = wasGuiEnabled;

			range.valueType = (StatisticType)EditorGUILayout.EnumPopup("Value type", range.valueType);
			range.tags = (StatisticTags)TagsLayout.TagsFlagField("Tags", range.tags, StatisticsData.Instance.statisticTags);
			range.isModdable = EditorGUILayout.Toggle(modGUIContent, range.isModdable);
			range.name = EditorGUILayout.TextField("Statistic name", range.name);
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

			if (range.isModdable)
			{
				range.isClamp = EditorGUILayout.Toggle("Is Clamp", range.isClamp);
				if (range.isClamp)
				{
					if (range.valueType == StatisticType.Int)
					{
						range.clampMin = EditorGUILayout.IntField("Clamp Min", (int)range.clampMin);
						range.clampMax = EditorGUILayout.IntField("Clamp Max", (int)range.clampMax);
					}
					else
					{
						range.clampMin = EditorGUILayout.FloatField("Clamp Min", range.clampMin);
						range.clampMax = EditorGUILayout.FloatField("Clamp Max", range.clampMax);
					}
				}
			}

			if (IsErrors(out List<string> errors, range))
			{
				foreach (string error in errors)
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
					bool secondScroller = resultScrollPositions.Count == 1;
					resultScrollPositions.Add(secondScroller ? Vector2.one : new Vector2());
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
				if (resultScrollCount > 0 && GUILayout.Button("-"))
					resultScrollCount--;
				if (GUILayout.Button("+"))
					resultScrollCount++;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
			}

			float CalculateValueForLevel(int level)
			{
				float easedValue = range.ease.Evaluate((level - 1) / (float)(range.maxLevel - 1));

				if (range.maxLevel == 1)
					easedValue = 1;

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
				if (!item.isClamp)
				{
					item.clampMin = float.MinValue;
					item.clampMax = float.MaxValue;
				}

				string template = classTemplate
					.Replace("[[STATISTIC_ID]]", item.statisticID)
					.Replace("[[STATISTIC_CLASS]]", item.isModdable ? nameof(ModStatistic) : nameof(Statistic));
				classes += template;
			}

			string fileContent = fileTemplate
				.Replace("[[DATE]]", DateTime.Now.ToString())
				.Replace("[[CLASSES]]", classes);

			File.WriteAllText(filePath, fileContent);
			EditorUtility.SetDirty(StatisticsData.Instance);
			AssetDatabase.SaveAssetIfDirty(StatisticsData.Instance);
			AssetDatabase.Refresh();
		}

		public bool IsErrors(out List<string> errors, StatisticRange range)
		{
			errors = new List<string>();

			if (string.IsNullOrEmpty(range.statisticID))
				errors.Add($"The statistic id should not be empty.");

			if (range.minValue == range.maxValue && range.maxLevel > 1)
				errors.Add($"The min value and max value should not be equal when max level is superior to 1");

			if (range.maxLevel == 0)
				errors.Add($"Max level should at least be one.");

			if (!CompilerHelper.IsValidCSharpClassName(range.statisticID))
				errors.Add("The statistic id is not valid for compilation.");

			errors.AddRange(panels[state].GetErrors(range));

			return errors.Count != 0;
		}

		private void DrawTopContainer(Rect container)
		{
			if (StatisticSettings.Instance.developerMode)
			{
				ShowMenuButton(container.SliceH(0.333f, 0), "Read", SSWizardPanelType.Read);
				ShowMenuButton(container.SliceH(0.333f, 1), "Create", SSWizardPanelType.Create);
				ShowMenuButton(container.SliceH(0.333f, 2), "Settings", SSWizardPanelType.Settings);
			}
			else
			{
				ShowMenuButton(container.SliceH(0.5f, 0), "Read", SSWizardPanelType.Read);
				ShowMenuButton(container.SliceH(0.5f, 1), "Settings", SSWizardPanelType.Settings);
			}

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
			Read,
			Create,
			Edit,
			Settings,
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

		private class SSWizardPanelRead : SSWizardPanel
		{
			public SSWizardPanelRead(StatisticSettingsWizard wizard) : base(wizard) { }

			/// <summary>
			/// <b>Parameters :</b>
			/// <br/> - The clicked range
			/// <br/> - The index of this range in the list
			/// </summary>
			public static event Action<StatisticRange, int> OnStatisticClicked;

			public override void Show()
			{
				float windowWidth = wizard.position.width;
				const int width = 215;
				const int height = 65;
				const int space = 10;
				int nbCol = Mathf.Max((int)windowWidth / width, 1);
				int nbRow = Mathf.CeilToInt(StatisticsData.Instance.statisticsRange.Count / (float)nbCol);
				int windowHeight = (nbRow * height) + (nbRow * space);
				Rect startRect = EditorGUILayout.GetControlRect(false, windowHeight);

				GUIStyle centeredBoldLabel = CustomGUIStyle.Label(fontStyle: FontStyle.Bold, alignment: TextAnchor.UpperCenter);
				GUIStyle centeredLabel = CustomGUIStyle.Label(alignment: TextAnchor.UpperCenter);
				GUIStyle leftLabel = CustomGUIStyle.Label(alignment: TextAnchor.UpperLeft);
				GUIStyle npButton = CustomGUIStyle.Button(padding: new RectOffset(0, 0, 0, 0));

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
					string id = $"{range.statisticID}";
					string type = $"{range.valueType}";
					string values = $"1 -> [{range.minValue}..{range.maxValue}] <- {range.maxLevel}";
					string ease = $"Growth: {range.ease}";
					Color borderColor = rect.Contains(Event.current.mousePosition) ? Color.red : Color.white;

					// Draw box and cursor
					EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
					CustomGUIUtility.DrawBorder(rect, borderColor, 2);

					// Create delete button
					if (StatisticSettings.Instance.developerMode && GUI.Button(rect.MoveX(rect.width - 23).MoveY(3).SetWidth(22).SetHeight(22), EditorGUIUtility.IconContent("TreeEditor.Trash"), npButton))
						DeleteStatistic(range);

					// Prepare button before shrinking
					if (GUI.Button(rect, "", GUIStyle.none))
						wizard.OpenEditFor(range, i);

					// Shrink after draw border to keep original size
					rect = rect.Shrink(3);

					// Draw field
					EditorGUI.LabelField(rect.SetHeight(EditorGUIUtility.singleLineHeight), id, centeredBoldLabel);
					EditorGUI.LabelField(RectUtility.lastRect, type, leftLabel);
					EditorGUI.LabelField(RectUtility.lastRect.MoveY(20), values, centeredLabel);
					EditorGUI.LabelField(RectUtility.lastRect.MoveY(20), ease, centeredLabel);
				}

				focusedWindow?.Repaint();

				void DeleteStatistic(StatisticRange range)
				{
					if (EditorUtility.DisplayDialog($"Delete {range.statisticID}", $"Do you really want to delete the statistic {range.statisticID} ?",
						"Yes",
						"Oh, no !"))
					{
						StatisticsData.Instance.statisticsRange.Remove(range);
						wizard.SaveSettings();
					}
				}
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

				if (StatisticsData.Instance.GetByID(range.statisticID) != null)
					errors.Add($"The statistic name '{range.statisticID}' is already taken.");

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
				wizard.state = SSWizardPanelType.Read;
				GUI.FocusControl(null);
			}

			public override bool ActionButtonIsValid() => !wizard.IsErrors(out List<string> errors, range);

			public override List<string> GetErrors(StatisticRange editedRange)
			{
				List<string> errors = new List<string>();

				for (int i = 0; i < StatisticsData.Instance.statisticsRange.Count; i++)
				{
					if (i == index)
						continue;

					if (editedRange.statisticID == StatisticsData.Instance.statisticsRange[i].statisticID)
						errors.Add($"The statistic name '{editedRange.statisticID}' is already taken.");
				}

				return errors;
			}
		}

		private class SSWizardPanelSettings : SSWizardPanel
		{
			TextAsset file;

			public SSWizardPanelSettings(StatisticSettingsWizard wizard) : base(wizard) { }

			public override bool ActionButtonIsValid() => false;

			public override string ActionButtonLabel() => "";

			public override List<string> GetErrors(StatisticRange range) => new List<string>();

			public override void OnActionButton() { }

			public override void Show()
			{
				StatisticSettings.Instance.exportPath = EditorGUILayout.TextField("Export Path", StatisticSettings.Instance.exportPath);
				DrawButton("Export statistics to .json", ExportToJson);
				GUILayout.Space(10);
				file = (TextAsset)EditorGUILayout.ObjectField("Import File", file, typeof(TextAsset), false);
				DrawButton("Import statistics from .json", ImportFromJson);

				void DrawButton(string label, Action action)
				{
					if (GUILayout.Button(label, GUILayout.Width(200), GUILayout.Height(35)))
						action();
				}
			}

			private void ExportToJson()
			{
				StatisticsRangeWrapper wrapper = new StatisticsRangeWrapper() { items = StatisticsData.Instance.statisticsRange };
				string json = JsonUtility.ToJson(wrapper);

				File.WriteAllText(Path.Combine(StatisticSettings.Instance.exportPath, "Smith_StatisticsData.json"), json);
				AssetDatabase.Refresh();
			}

			private void ImportFromJson()
			{
				if (file == null)
					return;

				StatisticsRangeWrapper wrapper = JsonUtility.FromJson<StatisticsRangeWrapper>(file.text);
				StatisticsData.Instance.statisticsRange = wrapper.items;
				wizard.SaveSettings();
			}
		}
	}

	internal static class CompilerHelper
	{
		static string[] reservedKeywords = new string[]
		{
			"abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char",
			"checked", "class", "const", "continue", "decimal", "default", "delegate",
			"do", "double", "else", "enum", "event", "explicit", "extern", "false",
			"finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit",
			"in", "int", "interface", "internal", "is", "lock", "long", "namespace",
			"new", "null", "object", "operator", "out", "override", "params", "private",
			"protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
			"short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
			"this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
			"unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
		};

		static string pattern = @"^[_a-zA-Z][_a-zA-Z0-9]*$";

		private static bool IsReservedKeyword(string className)
		{
			return reservedKeywords.Contains(className);
		}

		private static bool IsValidClassName(string className)
		{
			if (string.IsNullOrEmpty(className))
				return false;

			return Regex.IsMatch(className, pattern);
		}

		/// <summary>
		/// Guarantee the class name is valid for csharp compiler. It does not check the project classes.
		/// </summary>
		/// <param name="className">The class name to check</param>
		/// <returns>Weither the class name is valid or not.</returns>
		public static bool IsValidCSharpClassName(string className)
		{
			return !IsReservedKeyword(className) && IsValidClassName(className);
		}
	}

	internal class StatisticsRangeWrapper
	{
		public List<StatisticRange> items;
	}
}

