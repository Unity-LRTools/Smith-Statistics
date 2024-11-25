using LRT.Easing;
using System;
using UnityEngine;

namespace LRT.Smith.Statistics
{
	/// <summary>
	/// Represent a statistic that is modified by his level.
	/// </summary>
	public class Statistic : IFormattable, IComparable
	{
		/// <summary>
		/// Emitted when the value has been changed.
		/// </summary>
		public event Action<Statistic> OnValueChanged;

		/// <summary>
		/// The current value of the statistic based on his level.
		/// </summary>
		public float Value { get => GetValue(); }

		/// <summary>
		/// The level of this statistic used to calculate his real value.
		/// </summary>
		public int Level { get => currentLevel; set => SetCurrentLevel(value); }

		public string Name { get => name; }

		public string ID { get => id; }

		/// <summary>
		/// The minimum value for this statistic when level equal 0.
		/// </summary>
		protected float minValue;

		/// <summary>
		/// The maximum value for this statistic when level equal the maximum level reachable.
		/// </summary>
		protected float maxValue;

		/// <summary>
		/// The ease that will evaluate the value of the statistic.
		/// </summary>
		private Ease ease;

		/// <summary>
		/// The level of this statistic used to calculate his real value.
		/// The level minimum is 1.
		/// </summary>
		private int currentLevel;

		/// <summary>
		/// The display name of the statistic
		/// </summary>
		private string name;

		/// <summary>
		/// The type of this statistic
		/// </summary>
		private string id;

		/// <summary>
		/// The normalized level of this item [0..1].
		/// </summary>
		private float NormalizedLevel => maxLevel == 1 ? 1 : (currentLevel-1) / ((float)maxLevel-1);

		/// <summary>
		/// The maximum level this statistic can reach.
		/// </summary>
		private int maxLevel;

		/// <summary>
		/// The buffered value used to avoid recalculate the value each time
		/// </summary>
		private float? value;

		/// <summary>
		/// The type of the statistic to ensure int value stay int even in float class
		/// </summary>
		private StatisticType valueType;

		/// <summary>
		/// Return the value of this statistic for a specific level
		/// </summary>
		/// <param name="level">Desired level</param>
		/// <returns>The value for the desired level</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public float? GetValueAt(int level)
		{
			if (level > maxLevel)
				throw new ArgumentOutOfRangeException($"Target level '{level}' is out of range");

			return LerpValue(ease.Evaluate(level - 1 / maxLevel - 1));
		}

		/// <returns>
		/// The value calculated, can emit event if value has been changed
		/// </returns>
		protected virtual float GetValue()
		{
			if (!value.HasValue)
				UpdateValue();

			return valueType == StatisticType.Int ? (int)value.Value : value.Value;
		}

		/// <summary>
		/// Ask childs to lerp the value
		/// </summary>
		/// <param name="easedLevel">The value t lerp's value</param>
		/// <returns>The final value of this statistic</returns>
		private float LerpValue(float easedLevel)
		{
			return Mathf.Lerp(minValue, maxValue, easedLevel);
		}

		/// <summary>
		/// Change the level of the statistic
		/// </summary>
		/// <param name="newLevel">The target level</param>
		private void SetCurrentLevel(int newLevel)
		{
			if (newLevel <= 0 || newLevel > maxLevel)
				throw new ArgumentOutOfRangeException($"Level should be between [1..{maxLevel}]");

			if (newLevel != currentLevel)
			{
				currentLevel = newLevel;
				UpdateValue();
			}
		}

		/// <summary>
		/// Update the value and emit and event when the value has been changed
		/// </summary>
		private void UpdateValue()
		{
			float? oldValue = value;

			value = LerpValue(ease.Evaluate(NormalizedLevel));

			if (!oldValue.Equals(value))
				OnValueChanged?.Invoke(this);
		}

		protected Statistic(StatisticRange range, int level = 1)
		{
			maxLevel = range.maxLevel;
			ease = range.ease;
			name = range.name;
			minValue = range.minValue;
			maxValue = range.maxValue;
			valueType = range.valueType;
			id = range.statisticID;
			SetCurrentLevel(Mathf.Max(1, level));
		}

		internal Statistic(StatisticSave save) : this(StatisticsData.Instance.GetByID(save.id), save.level) { }

		#region Operator override
		public static implicit operator float(Statistic stat) => stat.Value;
		public static implicit operator int(Statistic stat)
		{
			if (stat.valueType != StatisticType.Int)
				throw new InvalidCastException();

			return (int)stat.value;
		}
		#endregion

		#region System interfaces implementation
		public int CompareTo(float other) => Value.CompareTo(other);
		public bool Equals(float other) => Value.Equals(other);
		public int CompareTo(object obj) => Value.CompareTo(obj);
		public string ToString(string format, IFormatProvider formatProvider) => Value.ToString(format, formatProvider);
		#endregion
	}
}

