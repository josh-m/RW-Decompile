using System;
using UnityEngine;

namespace Verse
{
	internal class MoteThrownAttached : MoteThrown
	{
		private Vector3 attacheeLastPosition = new Vector3(-1000f, -1000f, -1000f);

		public override void SpawnSetup(Map map)
		{
			base.SpawnSetup(map);
			if (this.link1.Linked)
			{
				this.attacheeLastPosition = this.link1.LastDrawPos;
			}
			this.exactPosition += this.def.mote.attachedDrawOffset;
		}

		protected override Vector3 NextExactPosition(float deltaTime)
		{
			Vector3 vector = base.NextExactPosition(deltaTime);
			if (this.link1.Linked)
			{
				if (!this.link1.Target.ThingDestroyed)
				{
					this.link1.UpdateDrawPos();
				}
				Vector3 b = this.link1.LastDrawPos - this.attacheeLastPosition;
				vector += b;
				vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
				this.attacheeLastPosition = this.link1.LastDrawPos;
			}
			return vector;
		}
	}
}
