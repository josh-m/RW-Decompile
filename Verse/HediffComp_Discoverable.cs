using RimWorld;
using System;

namespace Verse
{
	public class HediffComp_Discoverable : HediffComp
	{
		private bool discovered;

		public HediffCompProperties_Discoverable Props
		{
			get
			{
				return (HediffCompProperties_Discoverable)this.props;
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.Look<bool>(ref this.discovered, "discovered", false, false);
		}

		public override bool CompDisallowVisible()
		{
			return !this.discovered;
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			if (Find.TickManager.TicksGame % 103 == 0)
			{
				this.CheckDiscovered();
			}
		}

		public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			this.CheckDiscovered();
		}

		private void CheckDiscovered()
		{
			if (this.discovered)
			{
				return;
			}
			if (!this.parent.CurStage.becomeVisible)
			{
				return;
			}
			this.discovered = true;
			if (this.Props.sendLetterWhenDiscovered && PawnUtility.ShouldSendNotificationAbout(base.Pawn))
			{
				if (base.Pawn.RaceProps.Humanlike)
				{
					string label;
					if (!this.Props.discoverLetterLabel.NullOrEmpty())
					{
						label = string.Format(this.Props.discoverLetterLabel, base.Pawn.LabelShort.CapitalizeFirst()).CapitalizeFirst();
					}
					else
					{
						label = "LetterLabelNewDisease".Translate() + " (" + base.Def.label + ")";
					}
					string text;
					if (!this.Props.discoverLetterText.NullOrEmpty())
					{
						text = this.Props.discoverLetterText.Formatted(base.Pawn.LabelIndefinite(), base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn, "PAWN").CapitalizeFirst();
					}
					else if (this.parent.Part == null)
					{
						text = "NewDisease".Translate(base.Pawn.Named("PAWN"), base.Def.label, base.Pawn.LabelDefinite()).AdjustedFor(base.Pawn, "PAWN").CapitalizeFirst();
					}
					else
					{
						text = "NewPartDisease".Translate(base.Pawn.Named("PAWN"), this.parent.Part.Label, base.Pawn.LabelDefinite(), base.Def.label).AdjustedFor(base.Pawn, "PAWN").CapitalizeFirst();
					}
					Find.LetterStack.ReceiveLetter(label, text, (this.Props.letterType == null) ? LetterDefOf.NegativeEvent : this.Props.letterType, base.Pawn, null, null);
				}
				else
				{
					string text2;
					if (!this.Props.discoverLetterText.NullOrEmpty())
					{
						text2 = this.Props.discoverLetterText.Formatted(base.Pawn.LabelIndefinite(), base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn, "PAWN").CapitalizeFirst();
					}
					else if (this.parent.Part == null)
					{
						text2 = "NewDiseaseAnimal".Translate(base.Pawn.LabelShort, base.Def.LabelCap, base.Pawn.LabelDefinite(), base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn, "PAWN").CapitalizeFirst();
					}
					else
					{
						text2 = "NewPartDiseaseAnimal".Translate(base.Pawn.LabelShort, this.parent.Part.Label, base.Pawn.LabelDefinite(), base.Def.LabelCap, base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn, "PAWN").CapitalizeFirst();
					}
					Messages.Message(text2, base.Pawn, (this.Props.messageType == null) ? MessageTypeDefOf.NegativeHealthEvent : this.Props.messageType, true);
				}
			}
		}

		public override void Notify_PawnDied()
		{
			this.CheckDiscovered();
		}

		public override string CompDebugString()
		{
			return "discovered: " + this.discovered;
		}
	}
}
