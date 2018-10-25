using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.AI
{
	public class MentalState_BingingDrug : MentalState_Binging
	{
		public ChemicalDef chemical;

		public DrugCategory drugCategory;

		private static List<ChemicalDef> addictions = new List<ChemicalDef>();

		public override string InspectLine
		{
			get
			{
				return string.Format(base.InspectLine, this.chemical.label);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look<ChemicalDef>(ref this.chemical, "chemical");
			Scribe_Values.Look<DrugCategory>(ref this.drugCategory, "drugCategory", DrugCategory.None, false);
		}

		public override void PostStart(string reason)
		{
			base.PostStart(reason);
			this.ChooseRandomChemical();
			if (PawnUtility.ShouldSendNotificationAbout(this.pawn))
			{
				string label = "LetterLabelDrugBinge".Translate(this.chemical.label).CapitalizeFirst() + ": " + this.pawn.LabelShortCap;
				string text = "LetterDrugBinge".Translate(this.pawn.Label, this.chemical.label, this.pawn).CapitalizeFirst();
				if (!reason.NullOrEmpty())
				{
					text = text + "\n\n" + reason;
				}
				Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.ThreatSmall, this.pawn, null, null);
			}
		}

		public override void PostEnd()
		{
			base.PostEnd();
			if (PawnUtility.ShouldSendNotificationAbout(this.pawn))
			{
				Messages.Message("MessageNoLongerBingingOnDrug".Translate(this.pawn.LabelShort, this.chemical.label, this.pawn), this.pawn, MessageTypeDefOf.SituationResolved, true);
			}
		}

		private void ChooseRandomChemical()
		{
			MentalState_BingingDrug.addictions.Clear();
			List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				Hediff_Addiction hediff_Addiction = hediffs[i] as Hediff_Addiction;
				if (hediff_Addiction != null && AddictionUtility.CanBingeOnNow(this.pawn, hediff_Addiction.Chemical, DrugCategory.Any))
				{
					MentalState_BingingDrug.addictions.Add(hediff_Addiction.Chemical);
				}
			}
			if (MentalState_BingingDrug.addictions.Count > 0)
			{
				this.chemical = MentalState_BingingDrug.addictions.RandomElement<ChemicalDef>();
				this.drugCategory = DrugCategory.Any;
				MentalState_BingingDrug.addictions.Clear();
			}
			else
			{
				this.chemical = (from x in DefDatabase<ChemicalDef>.AllDefsListForReading
				where AddictionUtility.CanBingeOnNow(this.pawn, x, this.def.drugCategory)
				select x).RandomElementWithFallback(null);
				if (this.chemical != null)
				{
					this.drugCategory = this.def.drugCategory;
					return;
				}
				this.chemical = (from x in DefDatabase<ChemicalDef>.AllDefsListForReading
				where AddictionUtility.CanBingeOnNow(this.pawn, x, DrugCategory.Any)
				select x).RandomElementWithFallback(null);
				if (this.chemical != null)
				{
					this.drugCategory = DrugCategory.Any;
					return;
				}
				this.chemical = DefDatabase<ChemicalDef>.AllDefsListForReading.RandomElement<ChemicalDef>();
				this.drugCategory = DrugCategory.Any;
			}
		}
	}
}
