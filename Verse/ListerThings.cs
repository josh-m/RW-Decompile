using System;
using System.Collections.Generic;

namespace Verse
{
	public sealed class ListerThings
	{
		private Dictionary<ThingDef, List<Thing>> listsByDef = new Dictionary<ThingDef, List<Thing>>(ThingDefComparer.Instance);

		private List<Thing>[] listsByGroup;

		public ListerThingsUse use;

		private static readonly List<Thing> EmptyList = new List<Thing>();

		public List<Thing> AllThings
		{
			get
			{
				return this.listsByGroup[2];
			}
		}

		public ListerThings(ListerThingsUse use)
		{
			this.use = use;
			this.listsByGroup = new List<Thing>[ThingListGroupHelper.AllGroups.Length];
			this.listsByGroup[2] = new List<Thing>();
		}

		public List<Thing> ThingsInGroup(ThingRequestGroup group)
		{
			return this.ThingsMatching(ThingRequest.ForGroup(group));
		}

		public List<Thing> ThingsOfDef(ThingDef def)
		{
			return this.ThingsMatching(ThingRequest.ForDef(def));
		}

		public List<Thing> ThingsMatching(ThingRequest req)
		{
			if (req.singleDef != null)
			{
				List<Thing> result;
				if (!this.listsByDef.TryGetValue(req.singleDef, out result))
				{
					return ListerThings.EmptyList;
				}
				return result;
			}
			else
			{
				if (req.group != ThingRequestGroup.Undefined)
				{
					List<Thing> list = this.listsByGroup[(int)req.group];
					return list ?? ListerThings.EmptyList;
				}
				throw new InvalidOperationException("Invalid ThingRequest " + req);
			}
		}

		public bool Contains(Thing t)
		{
			List<Thing> list = this.listsByGroup[2];
			return list != null && list.Contains(t);
		}

		public void Add(Thing t)
		{
			if (!ListerThings.EverListable(t.def, this.use))
			{
				return;
			}
			List<Thing> list;
			if (!this.listsByDef.TryGetValue(t.def, out list))
			{
				list = new List<Thing>();
				this.listsByDef[t.def] = list;
			}
			list.Add(t);
			for (int i = 0; i < ThingListGroupHelper.AllGroups.Length; i++)
			{
				ThingRequestGroup thingRequestGroup = ThingListGroupHelper.AllGroups[i];
				if (this.use != ListerThingsUse.Region || thingRequestGroup.StoreInRegion())
				{
					if (thingRequestGroup.Includes(t.def))
					{
						List<Thing> list2 = this.listsByGroup[(int)thingRequestGroup];
						if (list2 == null)
						{
							list2 = new List<Thing>();
							this.listsByGroup[(int)thingRequestGroup] = list2;
						}
						list2.Add(t);
					}
				}
			}
		}

		public void Remove(Thing t)
		{
			if (!ListerThings.EverListable(t.def, this.use))
			{
				return;
			}
			this.listsByDef[t.def].Remove(t);
			for (int i = 0; i < ThingListGroupHelper.AllGroups.Length; i++)
			{
				ThingRequestGroup group = ThingListGroupHelper.AllGroups[i];
				if (this.use != ListerThingsUse.Region || group.StoreInRegion())
				{
					if (group.Includes(t.def))
					{
						this.listsByGroup[i].Remove(t);
					}
				}
			}
		}

		public static bool EverListable(ThingDef def, ListerThingsUse use)
		{
			return (def.category != ThingCategory.Mote || (def.drawGUIOverlay && use != ListerThingsUse.Region)) && (def.category != ThingCategory.Projectile || use != ListerThingsUse.Region);
		}
	}
}
