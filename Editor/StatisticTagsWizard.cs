using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LRT.Smith.Statistics.Editor
{
	public class StatisticTagsWizard : EditorWindow
	{
		public Vector2 scrollPosition;
		Dictionary<int, string> enumNames = new Dictionary<int, string>();

		//const string folderPath = "Packages/Smith-Statistics/Runtime/Generated"; //BUILD MODE
		const string folderPath = "Assets/Smith-Statistics/Runtime/Generated"; //TEST MODE

		readonly string filePath = Path.Combine(folderPath, "StatisticTags.cs");

		/// <summary>
		/// Parameters :
		/// [[DATE]] - Time when it has been generated
		/// [[ENUM_FLAGS]] - All the enum value with their flag index
		/// </summary>
		private const string enumTemplate =
@"/**
* DISCLAIMER: This code has been generated automatically.
* Generated on: [[DATE]]
*/
using System;

namespace LRT.Smith.Statistics
{
	[Flags]
    public enum StatisticTags
    {
[[ENUM_FLAGS]]
    }
}";

		/// <summary>
		/// Parameters :
		/// [[TAG_NAME]]  - The name of the enum
		/// [[TAG_INDEX]]  - The index of the flag
		/// </summary>
		private const string enumFlag = @"[[TAG_NAME]] = 1 << [[TAG_INDEX]],";

		public static void CreateWizard()
		{
			GetWindow<StatisticTagsWizard>("Statistic Tags");
		}

		private void OnEnable()
		{
			for (int i = 0; i < 32; i++)
			{
				enumNames.Add((int)Mathf.Pow(2, i), null);
			}

			foreach (StatisticTags tag in Enum.GetValues(typeof(StatisticTags)))
			{
				enumNames[(int)tag] = tag.ToString();
			}
		}

		void OnGUI()
		{
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

			for (int i = 0; i < 32; i++)
			{
				int index = (int)Mathf.Pow(2, i);
				enumNames[index] =	EditorGUILayout.TextField($"Tag {i+1}", enumNames[index]);
			}

			EditorGUILayout.EndScrollView();

			GUILayout.FlexibleSpace();
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Apply"))
				OnApplyButton();
			if (GUILayout.Button("Cancel"))
				OnCancelButton();
			GUILayout.Space(5);
			EditorGUILayout.EndHorizontal();
		}

		private void OnApplyButton()
		{
			string enumFlags = string.Empty;
			string fileContent = enumTemplate;

			for(int i = 0; i < 32; i++)
			{
				string name = enumNames[(int)Mathf.Pow(2, i)];
				if (!string.IsNullOrEmpty(name))
				{
					string line = enumFlag.Replace("[[TAG_NAME]]", name).Replace("[[TAG_INDEX]]", i.ToString());
					enumFlags += "\t\t" + line + "\n";
				}
			}

			if (!string.IsNullOrEmpty(enumFlags))
				enumFlags = enumFlags.Remove(enumFlags.Length - 2);

			fileContent = fileContent
				.Replace("[[DATE]]", DateTime.Now.ToString())
				.Replace("[[ENUM_FLAGS]]", enumFlags);

			File.WriteAllText(filePath, fileContent);
			AssetDatabase.Refresh();
		}

		private void OnCancelButton()
		{
			Close();
		}
	}
}
