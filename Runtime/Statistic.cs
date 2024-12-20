using LRT.Easing;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LRT.Smith.Statistics
{
	/// <summary>
	/// Represent a statistic that is modified by his level.
	/// </summary>
	[Serializable]
	public class Statistic : IFormattable, IComparable, IStatistic, ICloneable
	{
		/// <summary>
		/// Emitted when the value has been changed.
		/// </summary>
		public event Action<IStatistic> OnValueChanged;

		/// <summary>
		/// The current value of the statistic based on his level.
		/// </summary>
		public float Value { get => GetValue(); }

		/// <summary>
		/// The level of this statistic used to calculate his real value.
		/// </summary>
		public int Level { get => currentLevel; set => SetCurrentLevel(value); }

		/// <summary>
		/// The display name of the statistic
		/// </summary>
		public string Name { get => name; }

		/// <summary>
		/// The unique id of the statistic
		/// </summary>
		public string ID { get => id; }

		/// <summary>
		/// List of tag associated to the statistic
		/// </summary>
		public StatisticTags Tags { get => tags; }

		/// <summary>
		/// List of tags associated to the statistic
		/// </summary>
		List<string> IStatistic.Tags => Tags.Values;

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
		[SerializeField] private int currentLevel;

		/// <summary>
		/// The type of this statistic
		/// </summary>
		[SerializeField] private string id;

		/// <summary>
		/// The display name of the statistic
		/// </summary>
		private string name;

		/// <summary>
		/// The normalized level of this item [0..1].
		/// </summary>
		private float NormalizedLevel => maxLevel == 1 ? 1 : (currentLevel - 1) / ((float)maxLevel - 1);

		/// <summary>
		/// The maximum level this statistic can reach.
		/// </summary>
		private int maxLevel;

		/// <summary>
		/// The buffered value used to avoid recalculate the value each time
		/// </summary>
		private float? value;

		private StatisticTags tags;

		private bool init;

		/// <summary>
		/// The type of the statistic to ensure int value stay int even in float class
		/// </summary>
		protected StatisticType valueType;

		public static float GetValueFor(int level, StatisticRange range)
			=> GetValueFor(level, range.minValue, range.maxValue, range.maxLevel, range.ease);

		public static float GetValueFor(int level, float rangeMin, float rangeMax, int maxLevel, Ease ease)
		{
			if (level < 1 || level > maxLevel)
				throw new ArgumentOutOfRangeException();
			
			return GetValueFor((level - 1f) / (maxLevel - 1), rangeMin, rangeMax, ease);
		}

		public static float GetValueFor(float normalizedLevel, float rangeMin, float rangeMax, Ease ease)
		{
			if (normalizedLevel < 0 || normalizedLevel > 1)
				throw new ArgumentException("Normalized level should be clamped between range [0..1]");

			return Mathf.Lerp(rangeMin, rangeMax, ease.Evaluate(normalizedLevel));
		}

		/// <summary>
		/// Return the value of this statistic for a specific level
		/// </summary>
		/// <param name="level">Desired level</param>
		/// <returns>The value for the desired level</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public float GetValueAt(int level)
		{
			Init();

			if (level > maxLevel || level < 1)
				throw new ArgumentOutOfRangeException($"Target level '{level}' is out of range");

			return GetValueFor(level, minValue, maxValue, maxLevel, ease);
		}

		/// <returns>
		/// The value calculated, can emit event if value has been changed
		/// </returns>
		protected virtual float GetValue()
		{
			Init();

			if (!value.HasValue)
				UpdateValue();

			return valueType == StatisticType.Int ? (int)value.Value : value.Value;
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

			value = GetValueFor(NormalizedLevel, minValue, maxValue, ease);

			if (!oldValue.Equals(value))
				OnValueChanged?.Invoke(this);
		}

		protected void Init()
		{
			if (init)
				return;

			StatisticRange range = StatisticsData.Instance.GetByID(id);

			maxLevel = range.maxLevel;
			ease = range.ease;
			name = range.name;
			minValue = range.minValue;
			maxValue = range.maxValue;
			valueType = range.valueType;
			tags = range.tags;

			OnInit(range);

			init = true;
		}

		protected virtual void OnInit(StatisticRange range) { }

		public Statistic() { }

		protected Statistic(string rangeID, int level = 1)
		{
			id = rangeID;
			currentLevel = level;
		}

		internal Statistic(StatisticSave save) : this(save.id, save.level) { }

		#region Operator override
		public static implicit operator float(Statistic stat)
		{
			if (stat.valueType != StatisticType.Float)
				throw new InvalidCastException();

			return stat.Value;
		}
		public static implicit operator int(Statistic stat)
		{
			if (stat.valueType != StatisticType.Int)
				throw new InvalidCastException();

			return (int)stat.Value;
		}
		#endregion

		#region System interfaces implementation
		public int CompareTo(float other) => Value.CompareTo(other);
		public bool Equals(float other) => Value.Equals(other);
		public int CompareTo(object obj) => Value.CompareTo(obj);
		public string ToString(string format, IFormatProvider formatProvider) => Value.ToString(format, formatProvider);
		public override string ToString()
		{
			return $"{ID} ({Value})";
		}
		public object Clone()
		{
			return new Statistic()
			{
				id = id,
				name = name,
				maxLevel = maxLevel,
				currentLevel = currentLevel,
				value = value,
				valueType = valueType,
				minValue = minValue,
				maxValue = maxValue,
				tags = tags,
				ease = ease,
				init = init,
			};
		}
		#endregion
	}
}

