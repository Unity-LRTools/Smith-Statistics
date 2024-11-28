using LRT.Utility;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LRT.Smith.Statistics.Editor
{
	[CustomPropertyDrawer(typeof(Statistic))]
	public class StatisticDrawer : PropertyDrawer
	{
		static string[] options;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty level = property.FindPropertyRelative("currentLevel");
			SerializedProperty id = property.FindPropertyRelative("id");

			if (options == null || options.Length == 0)
				options = GetOptions();

			if (string.IsNullOrEmpty(id.stringValue))
				id.stringValue = options[0];

			StatisticRange range = StatisticsData.Instance.GetByID(id.stringValue);

			label.tooltip = $"Range: [1;{range.maxLevel}]";
			EditorGUI.LabelField(position.SliceH(0.33f, 0), label);
			level.intValue = Mathf.Clamp(EditorGUI.IntField(position.SliceH(0.33f, 1), level.intValue), 1, range.maxLevel);
			id.stringValue = options[EditorGUI.Popup(position.SliceH(0.33f, 2), Array.IndexOf(options, id.stringValue), options)];
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
	}
}
