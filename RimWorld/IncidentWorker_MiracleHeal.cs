using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_MiracleHeal : IncidentWorker
	{
		protected override bool CanFireNowSub()
		{
			return (from col in Find.MapPawns.FreeColonists
			where col.Downed
			select col).Any<Pawn>();
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Pawn pawn;
			if ((from col in Find.MapPawns.FreeColonists
			where col.Downed
			select col).TryRandomElement(out pawn))
			{
				int amount = 999;
				for (int i = 0; i < 10; i++)
				{
					if (!pawn.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>())
					{
						break;
					}
					BodyPartDamageInfo value = new BodyPartDamageInfo(pawn.health.hediffSet.GetNaturallyHealingInjuredParts().First<BodyPartRecord>(), false, null);
					pawn.TakeDamage(new DamageInfo(DamageDefOf.HealInjury, amount, null, new BodyPartDamageInfo?(value), null));
				}
				Find.LetterStack.ReceiveLetter("LetterLabelMiracleHeal".Translate(), "MiracleHeal".Translate(new object[]
				{
					pawn.Name.ToStringShort
				}), LetterType.BadNonUrgent, pawn, null);
				return true;
			}
			return true;
		}
	}
}
