using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class SignalAction_Letter : SignalAction
	{
		public Letter letter;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look<Letter>(ref this.letter, "letter", new object[0]);
		}

		protected override void DoAction(object[] args)
		{
			Pawn pawn = null;
			if (args != null && args.Any<object>())
			{
				pawn = (args[0] as Pawn);
			}
			if (pawn != null)
			{
				ChoiceLetter choiceLetter = this.letter as ChoiceLetter;
				if (choiceLetter != null)
				{
					choiceLetter.text = string.Format(choiceLetter.text, pawn.NameStringShort).AdjustedFor(pawn);
				}
				if (!this.letter.lookTarget.IsValid)
				{
					this.letter.lookTarget = pawn;
				}
			}
			Find.LetterStack.ReceiveLetter(this.letter, null);
		}
	}
}
