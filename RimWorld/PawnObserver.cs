using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnObserver
	{
		private const int IntervalsBetweenObservations = 4;

		private const float SampleNumCells = 100f;

		private Pawn pawn;

		private int intervalsUntilObserve;

		public PawnObserver(Pawn pawn)
		{
			this.pawn = pawn;
			this.intervalsUntilObserve = Rand.Range(0, 4);
		}

		public void ObserverInterval()
		{
			if (!this.pawn.Spawned)
			{
				return;
			}
			this.intervalsUntilObserve--;
			if (this.intervalsUntilObserve <= 0)
			{
				this.ObserveSurroundingThings();
				this.intervalsUntilObserve = 4 + Rand.RangeInclusive(-1, 1);
			}
		}

		private void ObserveSurroundingThings()
		{
			if (!this.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
			{
				return;
			}
			Room room = RoomQuery.RoomAt(this.pawn);
			int num = 0;
			while ((float)num < 100f)
			{
				IntVec3 c = this.pawn.Position + GenRadial.RadialPattern[num];
				if (c.InBounds(this.pawn.Map))
				{
					if (RoomQuery.RoomAt(c, this.pawn.Map) == room)
					{
						List<Thing> thingList = c.GetThingList(this.pawn.Map);
						for (int i = 0; i < thingList.Count; i++)
						{
							IThoughtGiver thoughtGiver = thingList[i] as IThoughtGiver;
							if (thoughtGiver != null)
							{
								Thought_Memory thought_Memory = thoughtGiver.GiveObservedThought();
								if (thought_Memory != null)
								{
									this.pawn.needs.mood.thoughts.memories.TryGainMemoryThought(thought_Memory, null);
								}
							}
						}
					}
				}
				num++;
			}
		}
	}
}
