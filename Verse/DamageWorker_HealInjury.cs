using System;
using System.Linq;

namespace Verse
{
	public class DamageWorker_HealInjury : DamageWorker
	{
		public override float Apply(DamageInfo dinfo, Thing thing)
		{
			Pawn pawn = thing as Pawn;
			if (pawn != null)
			{
				if (dinfo.Part.Value.HealHediff.NullOrEmpty<HediffDef>() && dinfo.Part.Value.Injury == null)
				{
					Log.Warning("Tried to heal local injury with no specified health diffs to heal.");
					return 0f;
				}
				if (!pawn.health.hediffSet.GetInjuredParts().Any<BodyPartRecord>())
				{
					return 0f;
				}
				float num = 0f;
				bool flag = true;
				BodyPartRecord part = dinfo.Part.Value.Part;
				if (part == null)
				{
					part = pawn.health.hediffSet.GetInjuredParts().RandomElement<BodyPartRecord>();
				}
				Hediff_Injury hediff_Injury = null;
				if (dinfo.Part.Value.Injury != null)
				{
					hediff_Injury = dinfo.Part.Value.Injury;
				}
				else
				{
					foreach (Hediff_Injury current in from x in pawn.health.hediffSet.GetHediffs<Hediff_Injury>()
					where x.Part == part
					select x)
					{
						if (dinfo.Part.Value.HealHediff.Contains(current.def) && (current.Severity > num || flag) && !current.IsOld())
						{
							flag = false;
							num = current.Severity;
							hediff_Injury = current;
						}
					}
				}
				if (hediff_Injury != null)
				{
					pawn.health.HealHediff(hediff_Injury, dinfo.Amount);
				}
			}
			return 0f;
		}
	}
}
