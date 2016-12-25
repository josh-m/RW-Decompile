using System;
using UnityEngine;

namespace Verse
{
	public abstract class SubEffecter_Sprayer : SubEffecter
	{
		public SubEffecter_Sprayer(SubEffecterDef def) : base(def)
		{
		}

		protected void MakeMote(TargetInfo A, TargetInfo B)
		{
			Vector3 vector = Vector3.zero;
			switch (this.def.spawnLocType)
			{
			case MoteSpawnLocType.OnSource:
				vector = A.Cell.ToVector3Shifted();
				break;
			case MoteSpawnLocType.BetweenPositions:
			{
				Vector3 vector2 = (!A.HasThing) ? A.Cell.ToVector3Shifted() : A.Thing.DrawPos;
				Vector3 vector3 = (!B.HasThing) ? B.Cell.ToVector3Shifted() : B.Thing.DrawPos;
				if (A.HasThing && A.Thing.holdingContainer != null)
				{
					vector = vector3;
				}
				else if (B.HasThing && B.Thing.holdingContainer != null)
				{
					vector = vector2;
				}
				else
				{
					vector = vector2 * this.def.positionLerpFactor + vector3 * (1f - this.def.positionLerpFactor);
				}
				break;
			}
			case MoteSpawnLocType.BetweenTouchingCells:
				vector = A.Cell.ToVector3Shifted() + (B.Cell - A.Cell).ToVector3().normalized * 0.5f;
				break;
			case MoteSpawnLocType.RandomCellOnTarget:
			{
				CellRect cellRect;
				if (B.HasThing)
				{
					cellRect = B.Thing.OccupiedRect();
				}
				else
				{
					cellRect = CellRect.CenteredOn(B.Cell, 0);
				}
				vector = cellRect.RandomCell.ToVector3Shifted();
				break;
			}
			}
			Map map = A.Map ?? B.Map;
			if (map != null && vector.ShouldSpawnMotesAt(map))
			{
				int randomInRange = this.def.burstCount.RandomInRange;
				for (int i = 0; i < randomInRange; i++)
				{
					Mote mote = (Mote)ThingMaker.MakeThing(this.def.moteDef, null);
					GenSpawn.Spawn(mote, vector.ToIntVec3(), map);
					mote.Scale = this.def.scale.RandomInRange;
					mote.exactPosition = vector + Gen.RandomHorizontalVector(this.def.positionRadius);
					mote.rotationRate = this.def.rotationRate.RandomInRange;
					float num = (!this.def.absoluteAngle) ? (B.Cell - A.Cell).AngleFlat : 0f;
					mote.exactRotation = this.def.rotation.RandomInRange + num;
					MoteThrown moteThrown = mote as MoteThrown;
					if (moteThrown != null)
					{
						moteThrown.airTimeLeft = this.def.airTime.RandomInRange;
						moteThrown.SetVelocity(this.def.angle.RandomInRange + num, this.def.speed.RandomInRange);
					}
				}
			}
		}
	}
}
