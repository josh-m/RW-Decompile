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

		public static bool FireCanExistIn(IntVec3 c)
		{
			Building edifice = c.GetEdifice();
			if (edifice != null && edifice.def.passability == Traversability.Impassable && edifice.OccupiedRect().ContractedBy(1).Contains(c))
			{
				return false;
			}
			List<Thing> thingList = c.GetThingList();
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def.category == ThingCategory.Filth && !thingList[i].def.filth.allowsFire)
				{
					return false;
				}
			}
			return true;
		}

		public static void TryStartFireIn(IntVec3 c, float fireSize)
		{
			bool flag = false;
			List<Thing> list = Find.ThingGrid.ThingsListAt(c);
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
			if (!FireUtility.FireCanExistIn(c))
			{
				return;
			}
			Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire, null);
			fire.fireSize = fireSize;
			GenSpawn.Spawn(fire, c, Rot4.North);
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
			GenSpawn.Spawn(fire, t.Position, Rot4.North);
			Pawn pawn = t as Pawn;
			if (pawn != null)
			{
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
				pawn.records.Increment(RecordDefOf.TimesOnFire);
			}
		}

		public static bool IsBurning(this TargetInfo t)
		{
			if (t.HasThing)
			{
				return t.Thing.IsBurning();
			}
			return t.Cell.ContainsStaticFire();
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
					if (iterator.Current.ContainsStaticFire())
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
			return t.Position.ContainsStaticFire();
		}

		public static bool ContainsStaticFire(this IntVec3 c)
		{
			List<Thing> list = Find.ThingGrid.ThingsListAt(c);
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

		public static bool ContainsTrap(this IntVec3 c)
		{
			Building edifice = c.GetEdifice();
			return edifice != null && edifice is Building_Trap;
		}
	}
}
