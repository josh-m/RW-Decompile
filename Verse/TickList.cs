using System;
using System.Collections.Generic;

namespace Verse
{
	public class TickList
	{
		private TickerType tickType;

		private List<List<Thing>> thingLists = new List<List<Thing>>();

		private List<Thing> thingsToRegister = new List<Thing>();

		private List<Thing> thingsToDeregister = new List<Thing>();

		private int TickInterval
		{
			get
			{
				switch (this.tickType)
				{
				case TickerType.Normal:
					return 1;
				case TickerType.Rare:
					return 250;
				case TickerType.Long:
					return 2000;
				default:
					return -1;
				}
			}
		}

		public TickList(TickerType tickType)
		{
			this.tickType = tickType;
			for (int i = 0; i < this.TickInterval; i++)
			{
				this.thingLists.Add(new List<Thing>());
			}
		}

		public void Reset()
		{
			for (int i = 0; i < this.thingLists.Count; i++)
			{
				this.thingLists[i].Clear();
			}
			this.thingsToRegister.Clear();
			this.thingsToDeregister.Clear();
		}

		public void RemoveWhere(Predicate<Thing> predicate)
		{
			for (int i = 0; i < this.thingLists.Count; i++)
			{
				this.thingLists[i].RemoveAll(predicate);
			}
			this.thingsToRegister.RemoveAll(predicate);
			this.thingsToDeregister.RemoveAll(predicate);
		}

		public void RegisterThing(Thing t)
		{
			this.thingsToRegister.Add(t);
		}

		public void DeregisterThing(Thing t)
		{
			this.thingsToDeregister.Add(t);
		}

		public void Tick()
		{
			for (int i = 0; i < this.thingsToRegister.Count; i++)
			{
				this.BucketOf(this.thingsToRegister[i]).Add(this.thingsToRegister[i]);
			}
			this.thingsToRegister.Clear();
			for (int j = 0; j < this.thingsToDeregister.Count; j++)
			{
				this.BucketOf(this.thingsToDeregister[j]).Remove(this.thingsToDeregister[j]);
			}
			this.thingsToDeregister.Clear();
			if (DebugSettings.fastEcology)
			{
				List<Map> maps = Find.Maps;
				for (int k = 0; k < maps.Count; k++)
				{
					maps[k].mapTemperature.UpdateCachedData();
				}
				for (int l = 0; l < this.thingLists.Count; l++)
				{
					List<Thing> list = this.thingLists[l];
					for (int m = 0; m < list.Count; m++)
					{
						if (list[m].def.category == ThingCategory.Plant)
						{
							list[m].TickLong();
						}
					}
				}
			}
			List<Thing> list2 = this.thingLists[Find.TickManager.TicksGame % this.TickInterval];
			for (int n = 0; n < list2.Count; n++)
			{
				if (!list2[n].Destroyed)
				{
					try
					{
						switch (this.tickType)
						{
						case TickerType.Normal:
							list2[n].Tick();
							break;
						case TickerType.Rare:
							list2[n].TickRare();
							break;
						case TickerType.Long:
							list2[n].TickLong();
							break;
						}
					}
					catch (Exception ex)
					{
						if (Prefs.DevMode)
						{
							Log.Error("Exception ticking " + list2[n].ToString() + ": " + ex.ToString());
						}
					}
				}
			}
		}

		private List<Thing> BucketOf(Thing t)
		{
			int num = t.GetHashCode();
			if (num < 0)
			{
				num *= -1;
			}
			int index = num % this.TickInterval;
			return this.thingLists[index];
		}
	}
}
