using System;
using UnityEngine;

namespace Verse
{
	public struct MoteAttachLink
	{
		private TargetInfo targetInt;

		private Vector3 lastDrawPosInt;

		public bool Linked
		{
			get
			{
				return this.targetInt.IsValid;
			}
		}

		public TargetInfo Target
		{
			get
			{
				return this.targetInt;
			}
		}

		public Vector3 LastDrawPos
		{
			get
			{
				return this.lastDrawPosInt;
			}
		}

		public static MoteAttachLink Invalid
		{
			get
			{
				return new MoteAttachLink(TargetInfo.Invalid);
			}
		}

		public MoteAttachLink(TargetInfo target)
		{
			this.targetInt = target;
			this.lastDrawPosInt = Vector3.zero;
			if (target.IsValid)
			{
				this.UpdateDrawPos();
			}
		}

		public void UpdateDrawPos()
		{
			if (this.targetInt.HasThing)
			{
				this.lastDrawPosInt = this.targetInt.Thing.DrawPos;
			}
			else
			{
				this.lastDrawPosInt = this.targetInt.Cell.ToVector3Shifted();
			}
		}
	}
}
