using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class DropPodIncoming : Thing
	{
		protected const int MinTicksToImpact = 120;

		protected const int MaxTicksToImpact = 200;

		protected const int RoofHitPreDelay = 15;

		private const int SoundAnticipationTicks = 100;

		public DropPodInfo contents;

		protected int ticksToImpact = 120;

		private bool soundPlayed;

		private static readonly Material ShadowMat = MaterialPool.MatFrom("Things/Special/DropPodShadow", ShaderDatabase.Transparent);

		public override Vector3 DrawPos
		{
			get
			{
				Vector3 result = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.FlyingItem);
				float num = (float)(this.ticksToImpact * this.ticksToImpact) * 0.01f;
				result.x -= num * 0.4f;
				result.z += num * 0.6f;
				return result;
			}
		}

		public override void SpawnSetup()
		{
			base.SpawnSetup();
			this.ticksToImpact = Rand.RangeInclusive(120, 200);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.ticksToImpact, "ticksToImpact", 0, false);
			Scribe_Deep.LookDeep<DropPodInfo>(ref this.contents, "contents", new object[0]);
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
				SoundDefOf.DropPodFall.PlayOneShot(base.Position);
			}
		}

		private void HitRoof()
		{
			if (!base.Position.Roofed())
			{
				return;
			}
			RoofCollapserImmediate.DropRoofInCells(this.OccupiedRect().ExpandedBy(1).Cells.Where(delegate(IntVec3 c)
			{
				if (!c.InBounds())
				{
					return false;
				}
				if (c == base.Position)
				{
					return true;
				}
				if (Find.ThingGrid.CellContains(c, ThingCategory.Pawn))
				{
					return false;
				}
				Building edifice = c.GetEdifice();
				return edifice == null || !edifice.def.holdsRoof;
			}));
		}

		public override void DrawAt(Vector3 drawLoc)
		{
			base.DrawAt(drawLoc);
			Vector3 pos = this.TrueCenter();
			pos.y = Altitudes.AltitudeFor(AltitudeLayer.Shadows);
			float num = 2f + (float)this.ticksToImpact / 100f;
			Vector3 s = new Vector3(num, 1f, num);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(pos, base.Rotation.AsQuat, s);
			Graphics.DrawMesh(MeshPool.plane10Back, matrix, DropPodIncoming.ShadowMat, 0);
		}

		private void Impact()
		{
			for (int i = 0; i < 6; i++)
			{
				Vector3 loc = base.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1f);
				MoteMaker.ThrowDustPuff(loc, 1.2f);
			}
			MoteMaker.ThrowLightningGlow(base.Position.ToVector3Shifted(), 2f);
			DropPod dropPod = (DropPod)ThingMaker.MakeThing(ThingDefOf.DropPod, null);
			dropPod.info = this.contents;
			GenSpawn.Spawn(dropPod, base.Position, base.Rotation);
			RoofDef roof = base.Position.GetRoof();
			if (roof != null)
			{
				if (!roof.soundPunchThrough.NullOrUndefined())
				{
					roof.soundPunchThrough.PlayOneShot(base.Position);
				}
				if (roof.filthLeaving != null)
				{
					for (int j = 0; j < 3; j++)
					{
						FilthMaker.MakeFilth(base.Position, roof.filthLeaving, 1);
					}
				}
			}
			this.Destroy(DestroyMode.Vanish);
		}
	}
}
