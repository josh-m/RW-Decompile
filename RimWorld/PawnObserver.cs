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
			RoomGroup roomGroup = this.pawn.GetRoomGroup();
			Map map = this.pawn.Map;
			int num = 0;
			while ((float)num < 100f)
			{
				IntVec3 intVec = this.pawn.Position + GenRadial.RadialPattern[num];
				if (intVec.InBounds(map))
				{
					if (intVec.GetRoomGroup(map) == roomGroup)
					{
						if (GenSight.LineOfSight(intVec, this.pawn.Position, map, true, null, 0, 0))
						{
							List<Thing> thingList = intVec.GetThingList(map);
							for (int i = 0; i < thingList.Count; i++)
							{
								IThoughtGiver thoughtGiver = thingList[i] as IThoughtGiver;
								if (thoughtGiver != null)
								{
									Thought_Memory thought_Memory = thoughtGiver.GiveObservedThought();
									if (thought_Memory != null)
									{
										this.pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Memory, null);
									}
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
