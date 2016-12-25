using System;
using UnityEngine;

namespace Verse
{
	public abstract class SubEffecter_DrifterEmote : SubEffecter
	{
		public SubEffecter_DrifterEmote(SubEffecterDef def) : base(def)
		{
		}

		protected void MakeMote(TargetInfo A)
		{
			Vector3 vector = (!A.HasThing) ? A.Cell.ToVector3Shifted() : A.Thing.DrawPos;
			if (vector.ShouldSpawnMotesAt(A.Map))
			{
				int randomInRange = this.def.burstCount.RandomInRange;
				for (int i = 0; i < randomInRange; i++)
				{
					Mote mote = (Mote)ThingMaker.MakeThing(this.def.moteDef, null);
					mote.Scale = this.def.scale.RandomInRange;
					mote.exactPosition = vector + Gen.RandomHorizontalVector(this.def.positionRadius);
					mote.rotationRate = this.def.rotationRate.RandomInRange;
					mote.exactRotation = this.def.rotation.RandomInRange;
					MoteThrown moteThrown = mote as MoteThrown;
					if (moteThrown != null)
					{
						moteThrown.airTimeLeft = this.def.airTime.RandomInRange;
						moteThrown.SetVelocity(this.def.angle.RandomInRange, this.def.speed.RandomInRange);
					}
					if (A.HasThing)
					{
						mote.Attach(A);
					}
					GenSpawn.Spawn(mote, vector.ToIntVec3(), A.Map);
				}
			}
		}
	}
}
