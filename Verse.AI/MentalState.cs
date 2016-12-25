using RimWorld;
using System;

namespace Verse.AI
{
	public class MentalState : IExposable
	{
		private const int TickInterval = 150;

		public Pawn pawn;

		public MentalStateDef def;

		private int age;

		public bool causedByMood;

		public int Age
		{
			get
			{
				return this.age;
			}
		}

		public virtual string InspectLine
		{
			get
			{
				return this.def.baseInspectLine;
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.LookDef<MentalStateDef>(ref this.def, "def");
			Scribe_Values.LookValue<int>(ref this.age, "age", 0, false);
			Scribe_Values.LookValue<bool>(ref this.causedByMood, "causedByMood", false, false);
		}

		public virtual void PostStart(string reason)
		{
		}

		public virtual void PostEnd()
		{
			if (!this.def.recoveryMessage.NullOrEmpty() && PawnUtility.ShouldSendNotificationAbout(this.pawn))
			{
				string text = null;
				try
				{
					text = string.Format(this.def.recoveryMessage, this.pawn.NameStringShort);
				}
				catch (Exception arg)
				{
					Log.Error("Exception formatting string: " + arg);
				}
				if (!text.NullOrEmpty())
				{
					Messages.Message(text.AdjustedFor(this.pawn), this.pawn, MessageSound.Silent);
				}
			}
		}

		public virtual void MentalStateTick()
		{
			if (this.pawn.IsHashIntervalTick(150))
			{
				this.age += 150;
				if (this.age >= this.def.maxTicksBeforeRecovery || (this.age >= this.def.minTicksBeforeRecovery && Rand.MTBEventOccurs(this.def.recoveryMtbDays, 60000f, 150f)))
				{
					this.RecoverFromState();
					return;
				}
				if (this.def.recoverFromSleep && !this.pawn.Awake())
				{
					this.RecoverFromState();
					return;
				}
				if (this.def.recoverFromDowned && this.pawn.Downed)
				{
					this.RecoverFromState();
					return;
				}
			}
		}

		public void RecoverFromState()
		{
			if (this.pawn.MentalState != this)
			{
				Log.Error(string.Concat(new object[]
				{
					"Recovered from ",
					this.def,
					" but pawn's mental state is not this, it is ",
					this.pawn.MentalState
				}));
			}
			this.pawn.mindState.mentalStateHandler.ClearMentalStateDirect();
			if (this.causedByMood && this.def.moodRecoveryThought != null && this.pawn.needs.mood != null)
			{
				this.pawn.needs.mood.thoughts.memories.TryGainMemoryThought(this.def.moodRecoveryThought, null);
			}
			this.PostEnd();
		}

		public virtual bool ForceHostileTo(Thing t)
		{
			return false;
		}

		public virtual bool ForceHostileTo(Faction f)
		{
			return false;
		}

		public EffecterDef CurrentStateEffecter()
		{
			return this.def.stateEffecter;
		}

		public virtual RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.SuperActive;
		}
	}
}
