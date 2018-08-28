using System;
using Verse;

namespace RimWorld
{
	public class Inspiration : IExposable
	{
		public Pawn pawn;

		public InspirationDef def;

		private int age;

		public int Age
		{
			get
			{
				return this.age;
			}
		}

		public float AgeDays
		{
			get
			{
				return (float)this.age / 60000f;
			}
		}

		public virtual string InspectLine
		{
			get
			{
				int numTicks = (int)((this.def.baseDurationDays - this.AgeDays) * 60000f);
				return string.Concat(new string[]
				{
					this.def.baseInspectLine,
					" (",
					"ExpiresIn".Translate(),
					": ",
					numTicks.ToStringTicksToPeriod(),
					")"
				});
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.Look<InspirationDef>(ref this.def, "def");
			Scribe_Values.Look<int>(ref this.age, "age", 0, false);
		}

		public virtual void InspirationTick()
		{
			this.age++;
			if (this.AgeDays >= this.def.baseDurationDays)
			{
				this.End();
			}
		}

		public virtual void PostStart()
		{
			this.SendBeginLetter();
		}

		public virtual void PostEnd()
		{
			this.AddEndMessage();
		}

		protected void End()
		{
			this.pawn.mindState.inspirationHandler.EndInspiration(this);
		}

		protected virtual void SendBeginLetter()
		{
			if (this.def.beginLetter.NullOrEmpty())
			{
				return;
			}
			if (!PawnUtility.ShouldSendNotificationAbout(this.pawn))
			{
				return;
			}
			string text = string.Format(this.def.beginLetter.AdjustedFor(this.pawn, "PAWN"), this.pawn.LabelCap);
			string label = (this.def.beginLetterLabel ?? this.def.LabelCap).CapitalizeFirst() + ": " + this.pawn.LabelShortCap;
			Find.LetterStack.ReceiveLetter(label, text, this.def.beginLetterDef, this.pawn, null, null);
		}

		protected virtual void AddEndMessage()
		{
			if (this.def.endMessage.NullOrEmpty())
			{
				return;
			}
			if (!PawnUtility.ShouldSendNotificationAbout(this.pawn))
			{
				return;
			}
			string text = string.Format(this.def.endMessage.AdjustedFor(this.pawn, "PAWN"), this.pawn.LabelCap);
			Messages.Message(text, this.pawn, MessageTypeDefOf.NeutralEvent, true);
		}
	}
}
