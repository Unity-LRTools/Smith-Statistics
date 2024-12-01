using LRT.Easing;
using NUnit.Framework;
using System;
using System.Reflection;
using UnityEngine;

namespace LRT.Smith.Statistics.Editor.Tests
{
    public class StatisticListTest : MonoBehaviour
    {
		private StatisticRange linearRange = new StatisticRange()
		{
			minValue = 1,
			maxValue = 10,
			maxLevel = 10,
			ease = Ease.Linear,

			//Mod 
			clampMin = -10,
			clampMax = 100,
		};

		private Statistic CreateStatistic(StatisticRange range, string id, StatisticType valueType = StatisticType.Float)
		{
			Statistic statistic = new Statistic();

			Type type = typeof(Statistic);
			BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

			type.GetField("currentLevel", flags).SetValue(statistic, 1);
			type.GetField("minValue", flags).SetValue(statistic, range.minValue);
			type.GetField("maxValue", flags).SetValue(statistic, range.maxValue);
			type.GetField("maxLevel", flags).SetValue(statistic, range.maxLevel);
			type.GetField("ease", flags).SetValue(statistic, range.ease);
			type.GetField("valueType", flags).SetValue(statistic, valueType);
			type.GetField("id", flags).SetValue(statistic, id);
			type.GetField("init", flags).SetValue(statistic, true);

			return statistic;
		}

		[Test]
		[TestCase("id_a", StatisticType.Int)]
		[TestCase("id_b", StatisticType.Float)]
		public void Create_StatisticForTest(string id, StatisticType valueType)
		{
			Type type = typeof(Statistic);
			BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

			Statistic statistic = CreateStatistic(linearRange, id, valueType);

			Assert.AreEqual(statistic.Level, 1);
			Assert.AreEqual(type.GetField("minValue", flags).GetValue(statistic), linearRange.minValue);
			Assert.AreEqual(type.GetField("maxValue", flags).GetValue(statistic), linearRange.maxValue);
			Assert.AreEqual(type.GetField("maxLevel", flags).GetValue(statistic), linearRange.maxLevel);
			Assert.AreEqual(type.GetField("ease", flags).GetValue(statistic), linearRange.ease);
			Assert.AreEqual(type.GetField("valueType", flags).GetValue(statistic), valueType);
			Assert.AreEqual(type.GetField("id", flags).GetValue(statistic), id);
			Assert.AreEqual(type.GetField("init", flags).GetValue(statistic), true);
		}

		private ModStatistic CreateModStatistic(StatisticRange range, string id, StatisticType valueType = StatisticType.Float)
		{
			ModStatistic statistic = new ModStatistic();

			Type type = typeof(ModStatistic);
			BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

			type.BaseType.GetField("currentLevel", flags).SetValue(statistic, 1);
			type.BaseType.GetField("minValue", flags).SetValue(statistic, range.minValue);
			type.BaseType.GetField("maxValue", flags).SetValue(statistic, range.maxValue);
			type.BaseType.GetField("maxLevel", flags).SetValue(statistic, range.maxLevel);
			type.BaseType.GetField("ease", flags).SetValue(statistic, range.ease);
			type.BaseType.GetField("valueType", flags).SetValue(statistic, valueType);
			type.BaseType.GetField("id", flags).SetValue(statistic, id);
			type.BaseType.GetField("init", flags).SetValue(statistic, true);

			//Mod
			type.GetProperty("clampMin").GetSetMethod(true).Invoke(statistic, new object[] { range.clampMin });
			type.GetProperty("clampMax").GetSetMethod(true).Invoke(statistic, new object[] { range.clampMax });

			return statistic;
		}

		[Test]
		[TestCase("id_a", StatisticType.Int)]
		[TestCase("id_b", StatisticType.Float)]
		public void Create_ModStatisticForTest(string id, StatisticType valueType)
		{
			Type type = typeof(ModStatistic);
			BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

			ModStatistic statistic = CreateModStatistic(linearRange, id, valueType);

			Assert.AreEqual(statistic.Level, 1);
			Assert.AreEqual(type.BaseType.GetField("minValue", flags).GetValue(statistic), linearRange.minValue);
			Assert.AreEqual(type.BaseType.GetField("maxValue", flags).GetValue(statistic), linearRange.maxValue);
			Assert.AreEqual(type.BaseType.GetField("maxLevel", flags).GetValue(statistic), linearRange.maxLevel);
			Assert.AreEqual(type.BaseType.GetField("ease", flags).GetValue(statistic), linearRange.ease);
			Assert.AreEqual(type.BaseType.GetField("valueType", flags).GetValue(statistic), valueType);
			Assert.AreEqual(type.BaseType.GetField("id", flags).GetValue(statistic), id);
			Assert.AreEqual(type.BaseType.GetField("init", flags).GetValue(statistic), true);

			//Mod
			var clampMinValue = type.GetProperty("clampMin").GetGetMethod(true).Invoke(statistic, new object[] { });
			var clampMaxValue = type.GetProperty("clampMax").GetGetMethod(true).Invoke(statistic, new object[] { });
			Assert.AreEqual(clampMinValue, linearRange.clampMin);
			Assert.AreEqual(clampMaxValue, linearRange.clampMax);
		}

		[Test]
		public void Method_OnStatistic_Sum()
		{
			StatisticList<Statistic> list = new StatisticList<Statistic>();
			string id = "power";
			Statistic s1 = CreateStatistic(linearRange, id);
			Statistic s2 = CreateStatistic(linearRange, id);
			Statistic s3 = CreateStatistic(linearRange, "other");
			int expected = 2;

			list.Add(s1);
			list.Add(s2);
			list.Add(s3);

			Assert.AreEqual(expected, list.Sum(id));
		}

		[Test]
		public void Method_OnModStatistic_Sum()
		{
			StatisticList<ModStatistic> list = new StatisticList<ModStatistic>();
			string id = "power";
			ModStatistic s1 = CreateModStatistic(linearRange, id);
			ModStatistic s2 = CreateModStatistic(linearRange, id);
			ModStatistic s3 = CreateModStatistic(linearRange, "other");
			int expected = 4;

			s1.AddOffset(1, "id_a"); //+1
			s2.AddPercentage(1, "id_b"); //+100%
			list.Add(s1);
			list.Add(s2);
			list.Add(s3);

			Assert.AreEqual(expected, list.Sum(id));
		}

		[Test]
		public void Method_OnStatisticAndModStatistic_Sum()
		{
			StatisticList<Statistic> list = new StatisticList<Statistic>();
			string id = "power";

			Statistic s1 = CreateStatistic(linearRange, id);
			ModStatistic s2 = CreateModStatistic(linearRange, id);
			ModStatistic s3 = CreateModStatistic(linearRange, "other");
			int expected = 3;

			s2.AddOffset(1, "id_b"); //+1
			list.Add(s1);
			list.Add(s2);
			list.Add(s3);

			Assert.AreEqual(expected, list.Sum(id));
		}

		[Test]
		[TestCase(0, 1)]
		[TestCase(1, 2)]
		[TestCase(4, 2)]
		public void Method_CountStatistic(int targetCount, int expected)
		{
			StatisticList<Statistic> list = new StatisticList<Statistic>();
			string id = "power";
			Statistic other = CreateStatistic(linearRange, "other");

			for(int i = 0; i < targetCount; i++)
			{
 				Statistic statistic = CreateStatistic(linearRange, id);
				list.Add(statistic);
			}

			list.Add(other);

			Assert.AreEqual(expected, list.CountStatistic());
		}
	}
}

