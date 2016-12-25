using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class DropPodIncoming : Thing, IActiveDropPod, IThingContainerOwner
	{
		protected const int MinTicksToImpact = 120;

		protected const int MaxTicksToImpact = 200;

		protected const int RoofHitPreDelay = 15;

		private const int SoundAnticipationTicks = 100;

		private ActiveDropPodInfo contents;

		protected int ticksToImpact = 120;

		private bool soundPlayed;

		public override Vector3 DrawPos
		{
			get
			{
				return DropPodAnimationUtility.DrawPosAt(this.ticksToImpact, base.Position);
			}
		}

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

		public override void PostMake()
		{
			base.PostMake();
			this.ticksToImpact = Rand.RangeInclusive(120, 200);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.ticksToImpact, "ticksToImpact", 0, false);
			Scribe_Deep.LookDeep<ActiveDropPodInfo>(ref this.contents, "contents", new object[]
			{
				this
			});
		}

		public override void Tick()
		{
			this.ticksToImpact--;
			if (this.ticksToImpact == 15)
			{
				this.HitRoof();
			}
			if (this.ticksToImpact <= 0)
			{
				this.Impact();
			}
			if (!this.soundPlayed && this.ticksToImpact < 100)
			{
				this.soundPlayed = true;
				SoundDefOf.DropPodFall.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
			}
		}

		private void HitRoof()
		{
			if (!base.Position.Roofed(base.Map))
			{
				return;
			}
			RoofCollapserImmediate.DropRoofInCells(this.OccupiedRect().ExpandedBy(1).Cells.Where(delegate(IntVec3 c)
			{
				if (!c.InBounds(base.Map))
				{
					return false;
				}
				if (c == base.Position)
				{
					return true;
				}
				if (base.Map.thingGrid.CellContains(c, ThingCategory.Pawn))
				{
					return false;
				}
				Building edifice = c.GetEdifice(base.Map);
				return edifice == null || !edifice.def.holdsRoof;
			}), base.Map);
		}

		public override void DrawAt(Vector3 drawLoc)
		{
			base.DrawAt(drawLoc);
			DropPodAnimationUtility.DrawDropSpotShadow(this, this.ticksToImpact);
		}

		private void Impact()
		{
			for (int i = 0; i < 6; i++)
			{
				Vector3 loc = base.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1f);
				MoteMaker.ThrowDustPuff(loc, base.Map, 1.2f);
			}
			MoteMaker.ThrowLightningGlow(base.Position.ToVector3Shifted(), base.Map, 2f);
			ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
			activeDropPod.Contents = this.contents;
			GenSpawn.Spawn(activeDropPod, base.Position, base.Map, base.Rotation);
			RoofDef roof = base.Position.GetRoof(base.Map);
			if (roof != null)
			{
				if (!roof.soundPunchThrough.NullOrUndefined())
				{
					roof.soundPunchThrough.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
				}
				if (roof.filthLeaving != null)
				{
					for (int j = 0; j < 3; j++)
					{
						FilthMaker.MakeFilth(base.Position, base.Map, roof.filthLeaving, 1);
					}
				}
			}
			this.Destroy(DestroyMode.Vanish);
		}

		virtual bool get_Spawned()
		{
			return base.Spawned;
		}
	}
}
