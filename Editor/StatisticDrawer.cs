using LRT.Utility;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace LRT.Smith.Statistics.Editor
{
	[CustomPropertyDrawer(typeof(Statistic))]
	public class StatisticDrawer : PropertyDrawer
	{
		static string[] options;
		bool quickMatch;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty level = property.FindPropertyRelative("currentLevel");
			SerializedProperty id = property.FindPropertyRelative("id");

			if (options == null || options.Length == 0)
				options = GetOptions();

			int idByLabel = Array.IndexOf(options, FirstLetterToUpper(label.text));
			quickMatch = idByLabel >= 0;
			
			if (string.IsNullOrEmpty(id.stringValue))
			{
				id.stringValue = idByLabel != -1 ? options[idByLabel] : options[0];
			}

			StatisticRange range = StatisticsData.Instance.GetByID(id.stringValue);

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

		public string[] GetOptions()
		{
			Type statisticType = typeof(Statistic);
			Type modStatisticType = typeof(ModStatistic);

			string[] derivedTypesNames = statisticType.Assembly.GetTypes()
									   .Where(t => t.IsClass && !t.IsAbstract && (t.IsSubclassOf(statisticType) || t.IsSubclassOf(modStatisticType)) && t != modStatisticType)
									   .Select(t => t.Name)
									   .ToArray();

			return derivedTypesNames;
		}

		private string FirstLetterToUpper(string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			return char.ToUpper(input[0]) + input[1..];
		}
	}
}
