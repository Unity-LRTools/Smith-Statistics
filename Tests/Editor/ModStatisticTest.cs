using LRT.Easing;
using NUnit.Framework;
using System;
using System.Reflection;

namespace LRT.Smith.Statistics.Editor.Tests
{
	public class ModStatisticTest
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

		private ModStatistic CreateStatistic(StatisticRange range, StatisticType valueType = StatisticType.Float)
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
			type.BaseType.GetField("init", flags).SetValue(statistic, true);

			//Mod
			type.GetProperty("clampMin").GetSetMethod(true).Invoke(statistic, new object[] { range.clampMin });
			type.GetProperty("clampMax").GetSetMethod(true).Invoke(statistic, new object[] { range.clampMax });

			return statistic;
		}

		[Test]
		[TestCase(StatisticType.Int)]
		[TestCase(StatisticType.Float)]
		public void Create_StatisticForTest(StatisticType valueType)
		{
			Type type = typeof(ModStatistic);
			BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

			ModStatistic statistic = CreateStatistic(linearRange, valueType);

			Assert.AreEqual(statistic.Level, 1);
			Assert.AreEqual(type.BaseType.GetField("minValue", flags).GetValue(statistic), linearRange.minValue);
			Assert.AreEqual(type.BaseType.GetField("maxValue", flags).GetValue(statistic), linearRange.maxValue);
			Assert.AreEqual(type.BaseType.GetField("maxLevel", flags).GetValue(statistic), linearRange.maxLevel);
			Assert.AreEqual(type.BaseType.GetField("ease", flags).GetValue(statistic), linearRange.ease);
			Assert.AreEqual(type.BaseType.GetField("valueType", flags).GetValue(statistic), valueType);
			Assert.AreEqual(type.BaseType.GetField("init", flags).GetValue(statistic), true);

			//Mod
			var clampMinValue = type.GetProperty("clampMin").GetGetMethod(true).Invoke(statistic, new object[] {  });
			var clampMaxValue = type.GetProperty("clampMax").GetGetMethod(true).Invoke(statistic, new object[] {  });
			Assert.AreEqual(clampMinValue, linearRange.clampMin);
			Assert.AreEqual(clampMaxValue, linearRange.clampMax);
		}

		[Test]
		[TestCase(5)]
		[TestCase(999)]
		[TestCase(-999)]
		public void Set_FixedValue_GetValue(float fixedValue)
		{
			ModStatistic statistic = CreateStatistic(linearRange);

			statistic.SetFixedValue(fixedValue);

			Assert.AreEqual(fixedValue, (float)statistic);
		}

		[Test]
		[TestCase(5)]
		[TestCase(999)]
		[TestCase(-999)]
		public void Remove_FixedValueAndLessPriority_GetValue(float fixedValue)
		{
			ModStatistic statistic = CreateStatistic(linearRange);

			statistic.SetFixedValue(fixedValue);

			Assert.AreEqual(fixedValue, (float)statistic);

			statistic.RemoveFixedValue();

			Assert.AreEqual(1, (float)statistic);
		}

		[Test]
		public void Set_FixedValueAndLessPriority_GetValue()
		{
			ModStatistic statistic = CreateStatistic(linearRange);
			float fixedValue = 5;

			statistic.SetFixedValue(fixedValue);
			statistic.SetFixedPercentage(0.8f);
			statistic.AddOffset(12, "id_a");
			statistic.AddPercentage(0.3f, "id_b");

			Assert.AreEqual(fixedValue, (float)statistic);
		}

		[Test]
		[TestCase(5)]
		[TestCase(999)]
		[TestCase(0.5f)]
		public void Set_FixedPercentage_GetValue(float fixedPercentage)
		{
			ModStatistic statistic = CreateStatistic(linearRange);

			statistic.SetFixedPercentage(fixedPercentage);

			Assert.AreEqual(fixedPercentage, (float)statistic);
		}

		[Test]
		[TestCase(5)]
		[TestCase(999)]
		[TestCase(0.5f)]
		public void Remove_FixedPercentage_GetValue(float fixedPercentage)
		{
			ModStatistic statistic = CreateStatistic(linearRange);

			statistic.SetFixedValue(fixedPercentage);

			Assert.AreEqual(fixedPercentage, (float)statistic);

			statistic.RemoveFixedValue();

			Assert.AreEqual(1, (float)statistic);
		}

		[Test]
		public void Set_FixedPercentageAndLessPriority_GetValue()
		{
			ModStatistic statistic = CreateStatistic(linearRange);
			float fixedPercentage = 5;

			statistic.SetFixedPercentage(fixedPercentage);
			statistic.AddOffset(12, "id_a");
			statistic.AddPercentage(0.3f, "id_b");

			Assert.AreEqual(fixedPercentage, (float)statistic);
		}

		[Test]
		[TestCase(5)]
		[TestCase(-6)]
		public void Add_OffsetModifier_GetValue(float offset)
		{
			ModStatistic statistic = CreateStatistic(linearRange);
			float expected = statistic + offset;

			statistic.AddOffset(offset, "id_a");

			Assert.AreEqual(expected, (float)statistic);
		}

		[Test]
		[TestCase(5)]
		[TestCase(-6)]
		public void Remove_OffsetModifier_GetValue(float offset)
		{
			ModStatistic statistic = CreateStatistic(linearRange);
			float expectedWithOffset = statistic + offset;
			float expectedAfterRemove = statistic;
			string identifier = "id_a";

			statistic.AddOffset(offset, identifier);

			Assert.AreEqual(expectedWithOffset, (float)statistic);

			statistic.RemoveOffset(identifier);

			Assert.AreEqual(expectedAfterRemove, (float)statistic);
		}

		[Test]
		[TestCase(1)]
		[TestCase(-0.5f)]
		public void Add_PercentageModifier_GetValue(float percentage)
		{
			ModStatistic statistic = CreateStatistic(linearRange);
			float expected = statistic * (1 + percentage);

			statistic.AddPercentage(percentage, "id_a");

			Assert.AreEqual(expected, (float)statistic);
		}

		[Test]
		[TestCase(1,2)]
		[TestCase(-0.25f,-0.25f)]
		[TestCase(0.5f,-0.5f)]
		public void Add_MultiplePercentageModifier_GetValue(float percentage1, float percentage2)
		{
			ModStatistic statistic = CreateStatistic(linearRange);
			float expected = statistic * (1 + percentage1 + percentage2);

			statistic.AddPercentage(percentage1, "id_a");
			statistic.AddPercentage(percentage2, "id_b");

			Assert.AreEqual(expected, (float)statistic);
		}

		[Test]
		[TestCase(1)]
		[TestCase(-0.5f)]
		public void Remove_PercentageModifier_GetValue(float percentage)
		{
			ModStatistic statistic = CreateStatistic(linearRange);
			float expectedWithPercentage = statistic * (1 + percentage);
			float expectedAfterRemove = statistic;
			string identifier = "id_a";

			statistic.AddPercentage(percentage, identifier);

			Assert.AreEqual(expectedWithPercentage, (float)statistic);

			statistic.RemovePercentage(identifier);

			Assert.AreEqual(expectedAfterRemove, (float)statistic);
		}

		[Test]
		[TestCase(5, 1)] //Expected: (1 + 5) * (1+1) = 12
		[TestCase(-1, 1)] //Expected: (1 - 1) * (1+1) = 0
		[TestCase(1, -0.75f)] //Expected: (1 + 1) * (1-0.75) = 0.5
		public void Add_OffsetAndPercentageModifier_GetValue(float offset, float percentage)
		{
			ModStatistic statistic = CreateStatistic(linearRange);
			float expected = (statistic + offset) * (1 + percentage);

			statistic.AddOffset(offset, "id_a");
			statistic.AddPercentage(percentage, "id_b");

			Assert.AreEqual(expected, (float)statistic);
		}

		[Test]
		public void Add_CheckSuperiorClamping_GetValue()
		{
			ModStatistic statistic = CreateStatistic(linearRange);
			float expected = linearRange.clampMax;

			statistic.AddOffset(linearRange.clampMax + 999, "id_a");

			Assert.AreEqual(expected, (float)statistic);
		}

		[Test]
		public void Add_CheckInferiorClamping_GetValue()
		{
			ModStatistic statistic = CreateStatistic(linearRange);
			float expected = linearRange.clampMin;

			statistic.AddOffset(linearRange.clampMin - 999, "id_a");

			Assert.AreEqual(expected, (float)statistic);
		}

		[Test]
		[TestCase(1, false)]
		[TestCase(2, true)]
		public void Set_EventEmitted_OnValueChanged(int targetLevel, bool shouldtrigger)
		{
			ModStatistic statistic = CreateStatistic(linearRange);
			bool eventTriggered = false;

			statistic.OnValueChanged += s => eventTriggered = true;

			statistic.Level = targetLevel;

			Assert.AreEqual(shouldtrigger, eventTriggered);
		}
	}
}