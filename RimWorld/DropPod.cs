using System;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class DropPod : Thing
	{
		public int age;

		public DropPodInfo info;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.age, "age", 0, false);
			Scribe_Deep.LookDeep<DropPodInfo>(ref this.info, "info", new object[0]);
		}

		public override void Tick()
		{
			this.age++;
			if (this.age > this.info.openDelay)
			{
				this.PodOpen();
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			for (int i = this.info.containedThings.Count - 1; i >= 0; i--)
			{
				this.info.containedThings[i].Destroy(DestroyMode.Vanish);
			}
			base.Destroy(mode);
			if (mode == DestroyMode.Kill)
			{
				for (int j = 0; j < 1; j++)
				{
					Thing thing = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel, null);
					GenPlace.TryPlaceThing(thing, base.Position, ThingPlaceMode.Near, null);
				}
			}
		}

		private void PodOpen()
		{
			for (int i = 0; i < this.info.containedThings.Count; i++)
			{
				Thing thing = this.info.containedThings[i];
				Thing thing2;
				GenPlace.TryPlaceThing(thing, base.Position, ThingPlaceMode.Near, out thing2, delegate(Thing placedThing, int count)
				{
					if (Find.TickManager.TicksGame < 1200 && TutorSystem.TutorialMode && placedThing.def.category == ThingCategory.Item)
					{
						Find.TutorialState.AddStartingItem(placedThing);
					}
				});
				Pawn pawn = thing2 as Pawn;
				if (pawn != null && pawn.RaceProps.Humanlike)
				{
					TaleRecorder.RecordTale(TaleDefOf.LandedInPod, new object[]
					{
						pawn
					});
				}
			}
			this.info.containedThings.Clear();
			if (this.info.leaveSlag)
			{
				for (int j = 0; j < 1; j++)
				{
					Thing thing3 = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel, null);
					GenPlace.TryPlaceThing(thing3, base.Position, ThingPlaceMode.Near, null);
				}
			}
			SoundDef.Named("DropPodOpen").PlayOneShot(base.Position);
			this.Destroy(DestroyMode.Vanish);
		}
	}
}
