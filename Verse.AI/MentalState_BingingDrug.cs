using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse.AI
{
	public class MentalState_BingingDrug : MentalState_Binging
	{
		public ChemicalDef chemical;

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
			Scribe_Defs.LookDef<ChemicalDef>(ref this.chemical, "chemical");
		}

		public override void PostStart(string reason)
		{
			base.PostStart(reason);
			this.ChooseRandomChemical();
			if (PawnUtility.ShouldSendNotificationAbout(this.pawn))
			{
				string label = "MentalBreakLetterLabel".Translate() + ": " + "LetterLabelDrugBinge".Translate(new object[]
				{
					this.chemical.label
				});
				string text = "LetterDrugBinge".Translate(new object[]
				{
					this.pawn.Label,
					this.chemical.label
				}).CapitalizeFirst();
				if (reason != null)
				{
					text = text + "\n\n" + "MentalBreakReason".Translate(new object[]
					{
						reason
					});
				}
				Find.LetterStack.ReceiveLetter(label, text, LetterType.BadNonUrgent, this.pawn, null);
			}
		}

		public override void PostEnd()
		{
			base.PostEnd();
			if (PawnUtility.ShouldSendNotificationAbout(this.pawn))
			{
				Messages.Message("MessageNoLongerBingingOnDrug".Translate(new object[]
				{
					this.pawn.NameStringShort,
					this.chemical.label
				}), this.pawn, MessageSound.Silent);
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
				MentalState_BingingDrug.addictions.Clear();
			}
			else
			{
				if ((from x in DefDatabase<ChemicalDef>.AllDefsListForReading
				where AddictionUtility.CanBingeOnNow(this.pawn, x, this.def.drugCategory)
				select x).TryRandomElement(out this.chemical))
				{
					return;
				}
				if ((from x in DefDatabase<ChemicalDef>.AllDefsListForReading
				where AddictionUtility.CanBingeOnNow(this.pawn, x, DrugCategory.Any)
				select x).TryRandomElement(out this.chemical))
				{
					return;
				}
				this.chemical = DefDatabase<ChemicalDef>.AllDefsListForReading.RandomElement<ChemicalDef>();
			}
		}
	}
}
