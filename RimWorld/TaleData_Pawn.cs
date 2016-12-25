using System;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class TaleData_Pawn : TaleData
	{
		public Pawn pawn;

		public PawnKindDef kind;

		public Faction faction;

		public Gender gender;

		public Name name;

		public ThingDef primaryEquipment;

		public ThingDef notableApparel;

		public override void ExposeData()
		{
			Scribe_References.LookReference<Pawn>(ref this.pawn, "pawn", true);
			Scribe_Defs.LookDef<PawnKindDef>(ref this.kind, "kind");
			Scribe_References.LookReference<Faction>(ref this.faction, "faction", false);
			Scribe_Values.LookValue<Gender>(ref this.gender, "gender", Gender.None, false);
			Scribe_Deep.LookDeep<Name>(ref this.name, "name", new object[0]);
			Scribe_Defs.LookDef<ThingDef>(ref this.primaryEquipment, "peq");
			Scribe_Defs.LookDef<ThingDef>(ref this.notableApparel, "app");
		}

		public override IEnumerable<Rule> GetRules(string prefix)
		{
			return GrammarUtility.RulesForPawn(prefix, this.name, this.kind, this.gender, this.faction);
		}

		public static TaleData_Pawn GenerateFrom(Pawn pawn)
		{
			TaleData_Pawn taleData_Pawn = new TaleData_Pawn();
			taleData_Pawn.pawn = pawn;
			taleData_Pawn.kind = pawn.kindDef;
			taleData_Pawn.faction = pawn.Faction;
			taleData_Pawn.gender = ((!pawn.RaceProps.hasGenders) ? Gender.None : pawn.gender);
			if (pawn.RaceProps.Humanlike)
			{
				taleData_Pawn.name = pawn.Name;
				if (pawn.equipment.Primary != null)
				{
					taleData_Pawn.primaryEquipment = pawn.equipment.Primary.def;
				}
				Apparel apparel;
				if (pawn.apparel.WornApparel.TryRandomElement(out apparel))
				{
					taleData_Pawn.notableApparel = apparel.def;
				}
			}
			return taleData_Pawn;
		}

		public static TaleData_Pawn GenerateRandom()
		{
			PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
			Faction faction = FactionUtility.DefaultFactionFrom(random.defaultFactionType);
			Pawn pawn = PawnGenerator.GeneratePawn(random, faction);
			return TaleData_Pawn.GenerateFrom(pawn);
		}
	}
}
