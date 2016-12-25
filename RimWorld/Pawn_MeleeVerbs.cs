using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Pawn_MeleeVerbs : IExposable
	{
		private const int BestMeleeVerbUpdateInterval = 60;

		private Pawn pawn;

		private Verb curMeleeVerb;

		private int curMeleeVerbUpdateTick;

		private static List<VerbEntry> meleeVerbs = new List<VerbEntry>();

		public Pawn_MeleeVerbs(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public Verb TryGetMeleeVerb()
		{
			if (this.curMeleeVerb == null || Find.TickManager.TicksGame >= this.curMeleeVerbUpdateTick + 60 || !this.curMeleeVerb.IsStillUsableBy(this.pawn))
			{
				this.ChooseMeleeVerb();
			}
			return this.curMeleeVerb;
		}

		private void ChooseMeleeVerb()
		{
			List<VerbEntry> updatedAvailableVerbsList = this.GetUpdatedAvailableVerbsList();
			if (updatedAvailableVerbsList.Count == 0)
			{
				this.SetCurMeleeVerb(null);
			}
			else
			{
				this.SetCurMeleeVerb(updatedAvailableVerbsList.RandomElementByWeight((VerbEntry ve) => ve.SelectionWeight).verb);
			}
		}

		public bool TryMeleeAttack(Thing target, Verb verbToUse = null, bool surpriseAttack = false)
		{
			if (this.pawn.stances.FullBodyBusy)
			{
				return false;
			}
			if (verbToUse != null)
			{
				if (!verbToUse.IsStillUsableBy(this.pawn))
				{
					return false;
				}
				if (!(verbToUse is Verb_MeleeAttack))
				{
					Log.Warning(string.Concat(new object[]
					{
						"Pawn ",
						this.pawn,
						" tried to melee attack ",
						target,
						" with non melee-attack verb ",
						verbToUse,
						"."
					}));
					return false;
				}
			}
			Verb verb;
			if (verbToUse != null)
			{
				verb = verbToUse;
			}
			else
			{
				verb = this.TryGetMeleeVerb();
			}
			if (verb == null)
			{
				return false;
			}
			verb.TryStartCastOn(target, surpriseAttack, true);
			return true;
		}

		public List<VerbEntry> GetUpdatedAvailableVerbsList()
		{
			Pawn_MeleeVerbs.meleeVerbs.Clear();
			if (this.pawn.equipment != null && this.pawn.equipment.Primary != null)
			{
				Verb verb = this.pawn.equipment.PrimaryEq.AllVerbs.Find((Verb x) => x is Verb_MeleeAttack);
				if (verb != null)
				{
					Pawn_MeleeVerbs.meleeVerbs.Add(new VerbEntry(verb, this.pawn, this.pawn.equipment.Primary));
					return Pawn_MeleeVerbs.meleeVerbs;
				}
			}
			List<Verb> allVerbs = this.pawn.verbTracker.AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				if (allVerbs[i] is Verb_MeleeAttack && allVerbs[i].IsStillUsableBy(this.pawn))
				{
					Pawn_MeleeVerbs.meleeVerbs.Add(new VerbEntry(allVerbs[i], this.pawn, null));
				}
			}
			foreach (Verb current in this.pawn.health.hediffSet.GetHediffsVerbs())
			{
				if (current is Verb_MeleeAttack && current.IsStillUsableBy(this.pawn))
				{
					Pawn_MeleeVerbs.meleeVerbs.Add(new VerbEntry(current, this.pawn, null));
				}
			}
			return Pawn_MeleeVerbs.meleeVerbs;
		}

		public void Notify_EquipmentLost()
		{
			this.ReconfirmCurMeleeVerb();
		}

		public void Notify_HediffAddedOrRemoved()
		{
			this.ReconfirmCurMeleeVerb();
		}

		private void ReconfirmCurMeleeVerb()
		{
			if (this.curMeleeVerb != null && !this.curMeleeVerb.IsStillUsableBy(this.pawn))
			{
				this.ChooseMeleeVerb();
			}
		}

		public void Notify_PawnKilled()
		{
			this.SetCurMeleeVerb(null);
		}

		private void SetCurMeleeVerb(Verb v)
		{
			this.curMeleeVerb = v;
			if (Current.ProgramState != ProgramState.Playing)
			{
				this.curMeleeVerbUpdateTick = 0;
			}
			else
			{
				this.curMeleeVerbUpdateTick = Find.TickManager.TicksGame;
			}
		}

		public void ExposeData()
		{
			Scribe_References.LookReference<Verb>(ref this.curMeleeVerb, "curMeleeVerb", false);
			Scribe_Values.LookValue<int>(ref this.curMeleeVerbUpdateTick, "curMeleeVerbUpdateTick", 0, false);
		}
	}
}
