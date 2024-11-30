using LRT.Easing;
using NUnit.Framework;
using System;
using System.Reflection;

namespace LRT.Smith.Statistics.Editor.Tests
{
	public class StatisticTest
	{
		private StatisticRange linearRange = new StatisticRange()
		{
			minValue = 1,
			maxValue = 10,
			maxLevel = 10,
			ease = Ease.Linear,
		};

		private Statistic CreateStatistic(StatisticRange range, StatisticType valueType = StatisticType.Float)
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
			type.GetField("init", flags).SetValue(statistic, true);

			return statistic;
		}

		[Test]
		[TestCase(StatisticType.Int)]
		[TestCase(StatisticType.Float)]
		public void Create_StatisticForTest(StatisticType valueType)
		{
			Type type = typeof(Statistic);
			BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

			Statistic statistic = CreateStatistic(linearRange, valueType);

			Assert.AreEqual(statistic.Level, 1);
			Assert.AreEqual(type.GetField("minValue", flags).GetValue(statistic), linearRange.minValue);
			Assert.AreEqual(type.GetField("maxValue", flags).GetValue(statistic), linearRange.maxValue);
			Assert.AreEqual(type.GetField("maxLevel", flags).GetValue(statistic), linearRange.maxLevel);
			Assert.AreEqual(type.GetField("ease", flags).GetValue(statistic), linearRange.ease);
			Assert.AreEqual(type.GetField("valueType", flags).GetValue(statistic), valueType);
			Assert.AreEqual(type.GetField("init", flags).GetValue(statistic), true);
		}

		[Test]
		[TestCase(1, 1)]
		[TestCase(5, 5)]
		[TestCase(10, 10)]
		public void Set_StatisticLevel_GetValue(int expected, int result)
		{
			Statistic statistic = CreateStatistic(linearRange);

			statistic.Level = expected;

			Assert.AreEqual(statistic.Value, result);
		}

		[Test]
		[TestCase(0, 0)]
		[TestCase(11, 11)]
		public void Set_StatisticLevel_OutOfRangeException(int expected, int result)
		{
			Statistic statistic = CreateStatistic(linearRange);

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				statistic.Level = expected;
			});
		}

		[Test]
		public void Get_ConvertFrom_IntToInt_Implicit()
		{
			Statistic statistic = CreateStatistic(linearRange, StatisticType.Int);

			Assert.DoesNotThrow(() =>
			{
				int value = statistic;
			});
		}

		[Test]
		public void Get_ConvertFrom_IntToFloat_Implicit()
		{
			Statistic statistic = CreateStatistic(linearRange, StatisticType.Int);

			Assert.Throws<InvalidCastException>(() =>
			{
				float value = statistic;
			});
		}

		[Test]
		public void Get_ConvertFrom_FloatToFloat_Implicit()
		{
			Statistic statistic = CreateStatistic(linearRange, StatisticType.Float);

			Assert.DoesNotThrow(() =>
			{
				float value = statistic;
			});
		}

		[Test]
		public void Get_ConvertFrom_FloatToInt_Implicit()
		{
			Statistic statistic = CreateStatistic(linearRange, StatisticType.Float);

			Assert.Throws<InvalidCastException>(() =>
			{
				int value = statistic;
			});
		}

		[Test]
		[TestCase(1, false)]
		[TestCase(2, true)]
		public void Set_EventEmitted_OnValueChanged(int targetLevel, bool shouldtrigger)
		{
			Statistic statistic = CreateStatistic(linearRange);
			bool eventTriggered = false;
			
			statistic.OnValueChanged += s => eventTriggered = true;

			statistic.Level = targetLevel;

			Assert.AreEqual(shouldtrigger, eventTriggered);
		}
	}
}
