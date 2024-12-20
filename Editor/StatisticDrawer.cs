using LRT.Utility;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LRT.Smith.Statistics.Editor
{
	[CustomPropertyDrawer(typeof(Statistic), true)]
	public class StatisticDrawer : PropertyDrawer
	{
		static string[] options;
		bool quickMatch;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty level = property.FindPropertyRelative("currentLevel");
			SerializedProperty id = property.FindPropertyRelative("id");

			if (options == null || options.Length == 0)
				options = StatisticsData.Instance.statisticsRange.Select(s => s.statisticID).ToArray();

			int idByLabel = Array.IndexOf(options, FirstLetterToUpper(property.name));
			quickMatch = idByLabel >= 0;

			if (string.IsNullOrEmpty(id.stringValue)) 
				id.stringValue = idByLabel != -1 ? options[idByLabel] : options[0];

			StatisticRange range = StatisticsData.Instance.GetByID(id.stringValue);

			if (range == null)
			{
				id.stringValue = options[0];
				range = StatisticsData.Instance.GetByID(id.stringValue);
			}

			float value = Statistic.GetValueFor(level.intValue, range);
			string valueLabel = range.valueType == StatisticType.Int ? ((int)value).ToString() : value.ToString();
			label.tooltip = $"Range: [1..{range.maxLevel}]\nValue: {valueLabel}";

			if (quickMatch)
			{
				EditorGUI.LabelField(position.SliceH(0.3f, 0), label);
				level.intValue = EditorGUI.IntSlider(position.RemainderH(0.3f).SliceH(0.8f, 0), level.intValue, 1, range.maxLevel);
				EditorGUI.LabelField(position.RemainderH(0.3f).RemainderH(0.8f), valueLabel);
			}
			else
			{
				EditorGUI.LabelField(position.SliceH(0.33f, 0), label);
				level.intValue = Mathf.Clamp(EditorGUI.IntField(position.SliceH(0.33f, 1), level.intValue), 1, range.maxLevel);
				id.stringValue = options[EditorGUI.Popup(position.SliceH(0.33f, 2), Array.IndexOf(options, id.stringValue), options)];
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label);
		}

		private string FirstLetterToUpper(string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			return char.ToUpper(input[0]) + input[1..];
		}
	}

	public static class StatisticGUILayout
	{
		public static Statistic StatisticField(string label, Statistic statistic)
		{
			string[] options = StatisticsData.Instance.statisticsRange.Select(s => s.statisticID).ToArray();
			FieldInfo idField = GetField("id");
			FieldInfo levelField = GetField("currentLevel");

			if (string.IsNullOrEmpty(statistic.ID))
				idField.SetValue(statistic, options[0]);

			StatisticRange range = StatisticsData.Instance.GetByID((string)idField.GetValue(statistic));

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField(label, GUILayout.MaxWidth(135));

			levelField.SetValue(statistic, EditorGUILayout.IntSlider((int)levelField.GetValue(statistic), 1, range.maxLevel));

			string id = (string)idField.GetValue(statistic);
			int index = Array.IndexOf(options, id);
			idField.SetValue(statistic, options[EditorGUILayout.Popup(index, options)]);

			float value = Statistic.GetValueFor((int)levelField.GetValue(statistic), range);
			string valueText = (range.valueType == StatisticType.Int ? (int)value : value).ToString();
			EditorGUILayout.LabelField($"Result: {valueText}", GUILayout.Width(150));

			EditorGUILayout.EndHorizontal();

			return statistic;
		}

		public static FieldInfo GetField(string fieldName)
		{
			return typeof(Statistic).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
		}
	}
}
