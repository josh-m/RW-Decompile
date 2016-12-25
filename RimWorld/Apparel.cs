using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class Apparel : ThingWithComps
	{
		public Pawn wearer;

		public virtual void DrawWornExtras()
		{
		}

		public virtual bool CheckPreAbsorbDamage(DamageInfo dinfo)
		{
			return false;
		}

		public virtual bool AllowVerbCast(IntVec3 root, TargetInfo targ)
		{
			return true;
		}

		[DebuggerHidden]
		public virtual IEnumerable<Gizmo> GetWornGizmos()
		{
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			base.Destroy(mode);
			if (base.Destroyed && this.wearer != null)
			{
				this.wearer.apparel.Notify_WornApparelDestroyed(this);
			}
		}
	}
}
