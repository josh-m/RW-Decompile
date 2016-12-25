using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_BingeDrug : JobGiver_Binge
	{
		private const int BaseIngestInterval = 600;

		private const float OverdoseSeverityToAvoid = 0.786f;

		private static readonly SimpleCurve IngestIntervalFactorCurve_Drunkness = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(1f, 4f)
		};

		private static readonly SimpleCurve IngestIntervalFactorCurve_DrugOverdose = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(1f, 5f)
		};

		protected override int IngestInterval(Pawn pawn)
		{
			ChemicalDef chemical = this.GetChemical(pawn);
			int num = 600;
			if (chemical == ChemicalDefOf.Alcohol)
			{
				Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.AlcoholHigh);
				if (firstHediffOfDef != null)
				{
					num = (int)((float)num * JobGiver_BingeDrug.IngestIntervalFactorCurve_Drunkness.Evaluate(firstHediffOfDef.Severity));
				}
			}
			else
			{
				Hediff firstHediffOfDef2 = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.DrugOverdose);
				if (firstHediffOfDef2 != null)
				{
					num = (int)((float)num * JobGiver_BingeDrug.IngestIntervalFactorCurve_DrugOverdose.Evaluate(firstHediffOfDef2.Severity));
				}
			}
			return num;
		}

		protected override Thing BestIngestTarget(Pawn pawn)
		{
			ChemicalDef chemical = this.GetChemical(pawn);
			if (chemical == null)
			{
				Log.ErrorOnce("Tried to binge on null chemical.", 1393746152);
				return null;
			}
			Hediff overdose = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.DrugOverdose);
			Predicate<Thing> predicate = delegate(Thing t)
			{
				if (!this.IgnoreForbid(pawn) && t.IsForbidden(pawn))
				{
					return false;
				}
				if (!pawn.CanReserve(t, 1))
				{
					return false;
				}
				CompDrug compDrug = t.TryGetComp<CompDrug>();
				return compDrug.Props.chemical == chemical && (overdose == null || !compDrug.Props.CanCauseOverdose || overdose.Severity + compDrug.Props.overdoseSeverityOffset.max < 0.786f) && (pawn.Position.InHorDistOf(t.Position, 60f) || t.Position.Roofed(t.Map) || pawn.Map.areaManager.Home[t.Position] || t.GetSlotGroup() != null);
			};
			Predicate<Thing> validator = predicate;
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Drug), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
		}

		private ChemicalDef GetChemical(Pawn pawn)
		{
			return ((MentalState_BingingDrug)pawn.MentalState).chemical;
		}
	}
}
