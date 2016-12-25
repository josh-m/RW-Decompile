using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnAddictionHediffsGenerator
	{
		private const int MaxAddictions = 3;

		private static List<ThingDef> allDrugs = new List<ThingDef>();

		private static readonly FloatRange GeneratedAddictionSeverityRange = new FloatRange(0.6f, 1f);

		public static void GenerateAddictionsFor(Pawn pawn)
		{
			if (!pawn.RaceProps.IsFlesh || !pawn.RaceProps.Humanlike)
			{
				return;
			}
			if (pawn.story != null && pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) < 0)
			{
				return;
			}
			PawnAddictionHediffsGenerator.allDrugs.Clear();
			int i = 0;
			while (i < 3)
			{
				if (Rand.Value < pawn.kindDef.chemicalAddictionChance)
				{
					if (!PawnAddictionHediffsGenerator.allDrugs.Any<ThingDef>())
					{
						PawnAddictionHediffsGenerator.allDrugs.AddRange(from x in DefDatabase<ThingDef>.AllDefsListForReading
						where x.category == ThingCategory.Item && x.GetCompProperties<CompProperties_Drug>() != null
						select x);
					}
					IEnumerable<ChemicalDef> source = from x in DefDatabase<ChemicalDef>.AllDefsListForReading
					where PawnAddictionHediffsGenerator.PossibleWithTechLevel(x, pawn.Faction) && !AddictionUtility.IsAddicted(pawn, x)
					select x;
					ChemicalDef chemicalDef;
					if (source.TryRandomElement(out chemicalDef))
					{
						Hediff hediff = HediffMaker.MakeHediff(chemicalDef.addictionHediff, pawn, null);
						hediff.Severity = PawnAddictionHediffsGenerator.GeneratedAddictionSeverityRange.RandomInRange;
						pawn.health.AddHediff(hediff, null, null);
						i++;
						continue;
					}
				}
				return;
			}
		}

		private static bool PossibleWithTechLevel(ChemicalDef chemical, Faction faction)
		{
			return faction == null || PawnAddictionHediffsGenerator.allDrugs.Any((ThingDef x) => x.GetCompProperties<CompProperties_Drug>().chemical == chemical && x.techLevel <= faction.def.techLevel);
		}
	}
}
