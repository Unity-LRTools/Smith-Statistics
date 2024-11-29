using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LRT.Smith.Statistics
{
	[Serializable]
	public class Modifier
	{
		public float value;
		public string identifier;

		public Modifier(float value, string identifier)
		{
			this.value = value;
			this.identifier = identifier;
		}
	}

	/// <summary>
	/// Counting priorities are :
	/// <br/>1/ Fixed Value (ignore next)
	/// <br/>2/ Fixed Percentages (ignore next)
	/// <br/>3/ Offsets 
	/// <br/>4/ Percentages
	/// <br/>5/ Clamping
	/// <code>
	/// stat.GetValue()		// => 10
	/// stat.AddOffset(5)		// => 10 + 5
	/// stat.AddPercentage(0.3)	// => (10 + 5) * 0.3
	/// stat.AddPercentage (-0.1)	// => (10 + 5) * (1+0.3-0.1)
	/// </code>
	/// </summary>
	[Serializable]
	public class ModStatistic : Statistic
	{
		public IReadOnlyList<Modifier> Offsets => offsets.AsReadOnly();
		public IReadOnlyList<Modifier> Percentages => percentages.AsReadOnly();
		public float clampMin { get; private set; }
		public float clampMax { get; private set; }
		public float? fixedValue { get; private set; }
		public float? fixedPercentage { get; private set; }

		private List<Modifier> offsets = new List<Modifier>();
		private List<Modifier> percentages = new List<Modifier>();

		public ModStatistic() : base() { }

		protected ModStatistic(string rangeID, int level = 1) : base(rangeID, level) { }

		internal ModStatistic(ModStatisticSave save) : this(save.baseSave.id, save.baseSave.level)
		{
			offsets = save.offsets;
			percentages = save.percentages;
			
			if (save.hasFixedValue)
				fixedValue = save.fixedValue;

			if (save.hasFixedPercent)
				fixedPercentage = save.fixedPercent;
		}

		protected sealed override void OnInit(StatisticRange range)
		{
			clampMin = range.clampMin;
			clampMax = range.clampMax;
		}

		/// <summary>
		/// The value of this statistic modded by the offset, multipliers, min and max clamping.
		/// Note that if a fixedValue has been set it has the top most priority, then it's the turn
		/// to the fixedMutiplier. It mean it will ignore the clamping and offsets/multipliers.
		/// </summary>
		/// <returns></returns>
		protected override float GetValue()
		{
			if (fixedValue.HasValue)
				return fixedValue.Value;

			float baseValue = base.GetValue();

			if (fixedPercentage.HasValue)
				return baseValue * fixedPercentage.Value;

			baseValue += Offsets.Sum(m => m.value);
			baseValue *= Percentages.Aggregate(1f, (acc, m) => acc + (m.value - 1));

			float clamp = Mathf.Clamp(baseValue, clampMin, clampMax);

			return clamp;
		}

		/// <summary>
		/// Add an offset the base value of this statistic.
		/// The offset can be negative or positive.
		/// </summary>
		/// <param name="value">The offset value</param>
		/// <param name="identifier">The identifier to help remove it</param>
		public void AddOffset(float value, string identifier)
		{
			offsets.Add(new Modifier(value, identifier));
		}

		/// <summary>
		/// Remove an offset from this statistic.
		/// </summary>
		/// <param name="identifier">Identifier to remove</param>
		public void RemoveOffset(string identifier)
		{
			RemoveFrom(offsets, identifier);
		}

		/// <summary>
		/// Add a percentage modifier. 
		/// <code>
		/// stat.AddPercentage(0.2, "a"); // Add 20%
		/// stat.AddPercentage(-0.4, "b"); // Reduce by 40%
		/// </code>
		/// </summary>
		/// <param name="value"></param>
		/// <param name="identifier"></param>
		public void AddPercentage(float value, string identifier)
		{
			if (value == 0)
				throw new ArgumentException("Value should not be equal to 0");

			percentages.Add(new Modifier(value, identifier));
		}

		/// <summary>
		/// Remove a percentage from this statistic.
		/// </summary>
		/// <param name="identifier">Identifier to remove</param>
		public void RemovePercentage(string identifier)
		{
			RemoveFrom(percentages, identifier);
		}

		/// <summary>
		/// Set the fixed value.
		/// </summary>
		/// <param name="value">The fixed value</param>
		public void SetFixedValue(float value)
		{
			fixedValue = value;
		}

		public void RemoveFixedValue() => fixedValue = null;

		/// <summary>
		/// Set the fixed percentage.
		/// </summary>
		/// <param name="value">The fixed percentage</param>
		public void SetFixedPercentage(float value)
		{
			fixedPercentage = value;
		}

		public void RemoveFixedPercentage() => fixedPercentage = null;

		private void RemoveFrom(List<Modifier> list, string identifier)
		{
			Modifier target = Offsets.FirstOrDefault(o => o.identifier == identifier);

			if (target == null)
			{
				Debug.LogWarning($"No target found for identifier '{identifier}'");
				return;
			}

			list.Remove(target);
		}
	}
}
