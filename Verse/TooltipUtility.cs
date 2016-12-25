using RimWorld;
using System;
using System.Text;

namespace Verse
{
	public static class TooltipUtility
	{
		public static string ShotCalculationTipString(Thing target)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (Find.Selector.SingleSelectedThing != null)
			{
				Verb verb = null;
				Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
				if (pawn != null && pawn != target && pawn.equipment != null && pawn.equipment.Primary != null && pawn.equipment.PrimaryEq.PrimaryVerb is Verb_LaunchProjectile)
				{
					verb = pawn.equipment.PrimaryEq.PrimaryVerb;
				}
				Building_TurretGun building_TurretGun = Find.Selector.SingleSelectedThing as Building_TurretGun;
				if (building_TurretGun != null && building_TurretGun != target)
				{
					verb = building_TurretGun.AttackVerb;
				}
				if (verb != null)
				{
					stringBuilder.AppendLine();
					stringBuilder.Append("ShotBy".Translate(new object[]
					{
						Find.Selector.SingleSelectedThing.LabelShort
					}) + ": ");
					if (verb.CanHitTarget(target))
					{
						stringBuilder.Append(ShotReport.HitReportFor(verb.caster, verb, target).GetTextReadout());
					}
					else
					{
						stringBuilder.Append("CannotHit".Translate());
					}
				}
			}
			return stringBuilder.ToString();
		}
	}
}
