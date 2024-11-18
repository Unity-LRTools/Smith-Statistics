using LRT.Easing;
using System;
using UnityEngine;

namespace LRT.Smith.Statistics
{
	/// <summary>
	/// Represent a statistic that is modified by his level.
	/// </summary>
	public abstract class Statistic<T> where T : struct, IComparable, IComparable<T>, IEquatable<T>, IFormattable
	{
		/// <summary>
		/// Emitted when the value has been changed.
		/// </summary>
		public event Action<Statistic<T>> OnValueChanged;

		/// <summary>
		/// The current value of the statistic based on his level.
		/// </summary>
		public T Value { get => GetValue(); }

		/// <summary>
		/// The level of this statistic used to calculate his real value.
		/// </summary>
		public int Level { get => currentLevel; set => SetCurrentLevel(value); }

		/// <summary>
		/// The minimum value for this statistic when level equal 0.
		/// </summary>
		protected T minValue;

		/// <summary>
		/// The maximum value for this statistic when level equal the maximum level reachable.
		/// </summary>
		protected T maxValue;

		/// <summary>
		/// The ease that will evaluate the value of the statistic.
		/// </summary>
		private Easing.Ease ease;

		/// <summary>
		/// The level of this statistic used to calculate his real value.
		/// The level minimum is 1.
		/// </summary>
		private int currentLevel;

		/// <summary>
		/// The normalized level of this item [0..1].
		/// </summary>
		private int NormalizedLevel => currentLevel / maxLevel;
		
		/// <summary>
		/// The maximum level this statistic can reach.
		/// </summary>
		private int maxLevel;

		/// <summary>
		/// The buffered value used to avoid recalculate the value each time
		/// </summary>
		private T? value;

		public T? GetValueAt(int level)
		{
			if (level > maxLevel)
				throw new ArgumentOutOfRangeException($"Target level '{level}' is out of range");

			return LerpValue(ease.Evaluate(level - 1 / maxLevel - 1));
		}

		/// <summary>
		/// Ask childs to lerp the value
		/// </summary>
		/// <param name="easedLevel">The value t lerp's value</param>
		/// <returns>The final value of this statistic</returns>
		protected abstract T LerpValue(float easedLevel);

		/// <summary>
		/// Initialize the range value without passing by constructor because we
		/// calculate value at the end of parent class constructor
		/// </summary>
		/// <param name="range">The range data</param>
		protected abstract void InitMinMaxValue(StatisticRange range);

		/// <returns>
		/// The value calculated, can emit event if value has been changed
		/// </returns>
		private T GetValue()
		{
			if (!value.HasValue)
				UpdateValue();

			return value.Value;
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
			T? oldValue = value;

			value = LerpValue(ease.Evaluate(NormalizedLevel));

			if (!oldValue.Equals(value))
				OnValueChanged.Invoke(this);
		}

		protected Statistic(StatisticRange range, int level = 1)
		{
			maxLevel = range.maxLevel;
			ease = range.ease;
			SetCurrentLevel(level);
		}

		#region Operator override
		public static implicit operator T(Statistic<T> stat) => stat.Value;
		#endregion

		#region System interfaces implementation
		public int CompareTo(object obj) => Value.CompareTo(obj);
		public int CompareTo(T other) => Value.CompareTo(other);
		public bool Equals(T other) => Value.Equals(other);
		public string ToString(string format, IFormatProvider formatProvider) => Value.ToString(format, formatProvider);
		#endregion
	}

	/// <summary>
	/// Represent a statistic that is modified by his level.
	/// </summary>
	public class StatsInt : Statistic<int>
	{
		public StatsInt(StatisticRange range, int level) : base(range, level) { }
		
		protected sealed override void InitMinMaxValue(StatisticRange range)
		{
			minValue = (int)range.minValue;
			maxValue = (int)range.maxValue;
		}
 
		protected sealed override int LerpValue(float easedLevel)
		{
			return Mathf.RoundToInt(Mathf.Lerp(minValue, maxValue, easedLevel));
		}
	}

	/// <summary>
	/// Represent a statistic that is modified by his level.
	/// </summary>
	public class StatsFloat : Statistic<float>
	{
		public StatsFloat(StatisticRange range, int level) : base(range, level) { }

		protected sealed override void InitMinMaxValue(StatisticRange range)
		{
			minValue = range.minValue;
			maxValue = range.maxValue;
		}

		protected sealed override float LerpValue(float easedLevel)
		{
			return Mathf.Lerp(minValue, maxValue, easedLevel);
		}
	}
}

