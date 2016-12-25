using System;

namespace Verse
{
	public class Hediff_Implant : HediffWithComps
	{
		public override bool ShouldRemove
		{
			get
			{
				return false;
			}
		}

		public override void PostAdd(DamageInfo? dinfo)
		{
			if (base.Part == null)
			{
				Log.Error("Part is null. It should be set before PostAdd for " + this.def + ".");
				return;
			}
		}
	}
}
