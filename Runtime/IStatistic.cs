using System;
using System.Collections.Generic;

namespace LRT.Smith.Statistics
{
	public interface IStatistic
	{
		public event Action<IStatistic> OnValueChanged;

		public float Value { get; }
		public int Level { get; }
		public string ID { get; }
		public List<string> Tags { get; }

		public float GetValueAt(int level);
	}
}