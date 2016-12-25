using RimWorld;
using System;

namespace Verse
{
	public class Pawn_InventoryTracker : IExposable, IThingContainerOwner
	{
		public Pawn pawn;

		public ThingContainer container;

		public bool Spawned
		{
			get
			{
				return this.pawn.Spawned;
			}
		}

		public Pawn_InventoryTracker(Pawn pawn)
		{
			this.pawn = pawn;
			this.container = new ThingContainer(this, false);
		}

		public void ExposeData()
		{
			Scribe_Deep.LookDeep<ThingContainer>(ref this.container, "container", new object[]
			{
				this
			});
		}

		public void InventoryTrackerTick()
		{
			this.container.ThingContainerTick();
		}

		public void DropAllNearPawn(IntVec3 pos, bool forbid = false)
		{
			while (this.container.Count > 0)
			{
				Thing thing;
				this.container.TryDrop(this.container[0], pos, ThingPlaceMode.Near, out thing, delegate(Thing t, int unused)
				{
					if (forbid)
					{
						t.SetForbiddenIfOutsideHomeArea();
					}
					if (t.def.IsPleasureDrug)
					{
						LessonAutoActivator.TeachOpportunity(ConceptDefOf.DrugBurning, OpportunityType.Important);
					}
				});
			}
		}

		public void DestroyAll(DestroyMode mode = DestroyMode.Vanish)
		{
			this.container.ClearAndDestroyContents(mode);
		}

		public bool Contains(Thing item)
		{
			return this.container.Contains(item);
		}

		public ThingContainer GetContainer()
		{
			return this.container;
		}

		public IntVec3 GetPosition()
		{
			return this.pawn.Position;
		}
	}
}
