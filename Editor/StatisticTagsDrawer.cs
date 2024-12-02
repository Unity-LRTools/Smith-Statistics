using LRT.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LRT.Smith.Statistics.Editor
{
    [CustomPropertyDrawer(typeof(StatisticTags))]
    public class StatisticTagsDrawer : PropertyDrawer
    {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Rect pos1 = position.SliceH(0.33f, 0);
			Rect pos2 = position.RemainderH(0.33f).SliceH(0.65f, 0);
			Rect pos3 = position.RemainderH(0.33f).RemainderH(0.65f);

			EditorGUI.LabelField(pos1, label);
			property.enumValueFlag = (int)(StatisticTags)EditorGUI.EnumFlagsField(pos2, (StatisticTags)property.enumValueFlag);
			if (GUI.Button(pos3, "edit"))
				StatisticTagsWizard.CreateWizard();
		}
	}

	public static class StatisticTagsLayout
	{
		public static StatisticTags TagsFlagField(string label, StatisticTags tags) => TagsFlagField(new GUIContent(label), tags);

		public static StatisticTags TagsFlagField(GUIContent label, StatisticTags tags)
		{
			Rect start = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
			Rect pos1 = start.SetWidth(EditorGUIUtility.labelWidth);
			Rect pos2 = start.MoveX(EditorGUIUtility.labelWidth + 2).ChangeWidth(-EditorGUIUtility.labelWidth - 2).SliceH(0.7f, 0);
			Rect pos3 = start.MoveX(EditorGUIUtility.labelWidth + 2).ChangeWidth(-EditorGUIUtility.labelWidth - 2).RemainderH(0.7f);

			EditorGUI.LabelField(pos1, label);
			tags = (StatisticTags)EditorGUI.EnumFlagsField(pos2, tags);
			if (GUI.Button(pos3, "Edit"))
				StatisticTagsWizard.CreateWizard();

			return tags;
		}
	}
}

