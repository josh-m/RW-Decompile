using System;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class ActiveDropPod : Thing, IActiveDropPod, IThingContainerOwner
	{
		public int age;

		private ActiveDropPodInfo contents;

		public ActiveDropPodInfo Contents
		{
			get
			{
				return this.contents;
			}
			set
			{
				if (this.contents != null)
				{
					this.contents.parent = null;
				}
				if (value != null)
				{
					value.parent = this;
				}
				this.contents = value;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.age, "age", 0, false);
			Scribe_Deep.LookDeep<ActiveDropPodInfo>(ref this.contents, "contents", new object[]
			{
				this
			});
		}

		public ThingContainer GetInnerContainer()
		{
			return this.contents.innerContainer;
		}

		public IntVec3 GetPosition()
		{
			return base.PositionHeld;
		}

		public Map GetMap()
		{
			return base.MapHeld;
		}

		public override void Tick()
		{
			this.age++;
			if (this.age > this.contents.openDelay)
			{
				this.PodOpen();
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			this.contents.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
			Map map = base.Map;
			base.Destroy(mode);
			if (mode == DestroyMode.Kill)
			{
				for (int i = 0; i < 1; i++)
				{
					Thing thing = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel, null);
					GenPlace.TryPlaceThing(thing, base.Position, map, ThingPlaceMode.Near, null);
				}
			}
		}

		private void PodOpen()
		{
			for (int i = 0; i < this.contents.innerContainer.Count; i++)
			{
				Thing thing = this.contents.innerContainer[i];
				Thing thing2;
				GenPlace.TryPlaceThing(thing, base.Position, base.Map, ThingPlaceMode.Near, out thing2, delegate(Thing placedThing, int count)
				{
					if (Find.TickManager.TicksGame < 1200 && TutorSystem.TutorialMode && placedThing.def.category == ThingCategory.Item)
					{
						Find.TutorialState.AddStartingItem(placedThing);
					}
				});
				Pawn pawn = thing2 as Pawn;
				if (pawn != null)
				{
					if (pawn.RaceProps.Humanlike)
					{
						TaleRecorder.RecordTale(TaleDefOf.LandedInPod, new object[]
						{
							pawn
						});
					}
					if (pawn.IsColonist && pawn.Spawned && !base.Map.IsPlayerHome)
					{
						pawn.drafter.Drafted = true;
					}
				}
			}
			this.contents.innerContainer.Clear();
			if (this.contents.leaveSlag)
			{
				for (int j = 0; j < 1; j++)
				{
					Thing thing3 = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel, null);
					GenPlace.TryPlaceThing(thing3, base.Position, base.Map, ThingPlaceMode.Near, null);
				}
			}
			SoundDef.Named("DropPodOpen").PlayOneShot(new TargetInfo(base.Position, base.Map, false));
			this.Destroy(DestroyMode.Vanish);
		}

		virtual bool get_Spawned()
		{
			return base.Spawned;
		}
	}
}
