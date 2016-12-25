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
			Scribe_Values.LookValue<bool>(ref this.discovered, "discovered", false, false);
		}

		public override bool CompDisallowVisible()
		{
			return !this.discovered;
		}

		public override void CompPostTick()
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
				if (!this.parent.CurStage.everVisible)
				{
					this.discovered = false;
				}
				return;
			}
			if (!this.parent.CurStage.everVisible)
			{
				return;
			}
			this.discovered = true;
			if (this.Props.sendLetterWhenDiscovered && PawnUtility.ShouldSendNotificationAbout(base.Pawn))
			{
				string label;
				if (!this.Props.discoverLetterLabel.NullOrEmpty())
				{
					label = string.Format(this.Props.discoverLetterLabel, base.Pawn.LabelShort).CapitalizeFirst();
				}
				else
				{
					label = "LetterLabelNewDisease".Translate() + " (" + base.Def.label + ")";
				}
				string text;
				if (!this.Props.discoverLetterText.NullOrEmpty())
				{
					text = string.Format(this.Props.discoverLetterText, base.Pawn.LabelIndefinite()).AdjustedFor(base.Pawn).CapitalizeFirst();
				}
				else if (this.parent.Part == null)
				{
					text = "NewDisease".Translate(new object[]
					{
						base.Pawn.LabelIndefinite(),
						base.Def.label,
						base.Pawn.LabelDefinite()
					}).AdjustedFor(base.Pawn).CapitalizeFirst();
				}
				else
				{
					text = "NewPartDisease".Translate(new object[]
					{
						base.Pawn.LabelIndefinite(),
						this.parent.Part.def.label,
						base.Pawn.LabelDefinite(),
						base.Def.LabelCap
					}).AdjustedFor(base.Pawn).CapitalizeFirst();
				}
				if (base.Pawn.RaceProps.Humanlike)
				{
					Find.LetterStack.ReceiveLetter(label, text, LetterType.BadNonUrgent, base.Pawn, null);
				}
				else
				{
					Messages.Message(text, base.Pawn, MessageSound.Standard);
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
