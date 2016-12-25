using System;
using System.Collections.Generic;

namespace Verse
{
	public class DefMap<D, V> : IExposable where D : Def, new() where V : new()
	{
		private List<V> values;

		public int Count
		{
			get
			{
				return this.values.Count;
			}
		}

		public V this[D def]
		{
			get
			{
				return this.values[(int)def.index];
			}
			set
			{
				this.values[(int)def.index] = value;
			}
		}

		public V this[int index]
		{
			get
			{
				return this.values[index];
			}
			set
			{
				this.values[index] = value;
			}
		}

		public DefMap()
		{
			int defCount = DefDatabase<D>.DefCount;
			if (defCount == 0)
			{
				throw new Exception(string.Concat(new object[]
				{
					"Constructed DefMap<",
					typeof(D),
					", ",
					typeof(V),
					"> without defs being initialized. Try constructing it in ResolveReferences instead of the constructor."
				}));
			}
			this.values = new List<V>(defCount);
			for (int i = 0; i < defCount; i++)
			{
				this.values.Add((default(V) == null) ? Activator.CreateInstance<V>() : default(V));
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<V>(ref this.values, "vals", LookMode.Undefined, new object[0]);
		}

		public void SetAll(V val)
		{
			for (int i = 0; i < this.values.Count; i++)
			{
				this.values[i] = val;
			}
		}
	}
}
