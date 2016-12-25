using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class FireUtility
	{
		public static bool CanEverAttachFire(this Thing t)
		{
			return !t.Destroyed && t.FlammableNow && t.def.category == ThingCategory.Pawn;
		}

		public static bool FireCanExistIn(IntVec3 c, Map map)
		{
			Building edifice = c.GetEdifice(map);
			if (edifice != null && edifice.def.passability == Traversability.Impassable && edifice.OccupiedRect().ContractedBy(1).Contains(c))
			{
				return false;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def.category == ThingCategory.Filth && !thingList[i].def.filth.allowsFire)
				{
					return false;
				}
			}
			return true;
		}

		public static void TryStartFireIn(IntVec3 c, Map map, float fireSize)
		{
			bool flag = false;
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].def == ThingDefOf.Fire)
				{
					return;
				}
				if (list[i].FlammableNow)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				return;
			}
			if (!FireUtility.FireCanExistIn(c, map))
			{
				return;
			}
			Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire, null);
			fire.fireSize = fireSize;
			GenSpawn.Spawn(fire, c, map, Rot4.North);
		}

		public static void TryAttachFire(this Thing t, float fireSize)
		{
			if (!t.CanEverAttachFire())
			{
				return;
			}
			if (t.HasAttachment(ThingDefOf.Fire))
			{
				return;
			}
			Fire fire = ThingMaker.MakeThing(ThingDefOf.Fire, null) as Fire;
			fire.fireSize = fireSize;
			fire.AttachTo(t);
			GenSpawn.Spawn(fire, t.Position, t.Map, Rot4.North);
			Pawn pawn = t as Pawn;
			if (pawn != null)
			{
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
				pawn.records.Increment(RecordDefOf.TimesOnFire);
			}
		}

		public static bool IsBurning(this TargetInfo t)
		{
			if (t.HasThing)
			{
				return t.Thing.IsBurning();
			}
			return t.Cell.ContainsStaticFire(t.Map);
		}

		public static bool IsBurning(this Thing t)
		{
			if (t.Destroyed || !t.Spawned)
			{
				return false;
			}
			if (!(t.def.size == IntVec2.One))
			{
				CellRect.CellRectIterator iterator = t.OccupiedRect().GetIterator();
				while (!iterator.Done())
				{
					if (iterator.Current.ContainsStaticFire(t.Map))
					{
						return true;
					}
					iterator.MoveNext();
				}
				return false;
			}
			if (t is Pawn)
			{
				return t.HasAttachment(ThingDefOf.Fire);
			}
			return t.Position.ContainsStaticFire(t.Map);
		}

		public static bool ContainsStaticFire(this IntVec3 c, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Fire fire = list[i] as Fire;
				if (fire != null && fire.parent == null)
				{
					return true;
				}
			}
			return false;
		}

		public static bool ContainsTrap(this IntVec3 c, Map map)
		{
			Building edifice = c.GetEdifice(map);
			return edifice != null && edifice is Building_Trap;
		}
	}
}
