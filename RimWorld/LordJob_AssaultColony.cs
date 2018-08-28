using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_AssaultColony : LordJob
	{
		private Faction assaulterFaction;

		private bool canKidnap = true;

		private bool canTimeoutOrFlee = true;

		private bool sappers;

		private bool useAvoidGridSmart;

		private bool canSteal = true;

		private static readonly IntRange AssaultTimeBeforeGiveUp = new IntRange(26000, 38000);

		private static readonly IntRange SapTimeBeforeGiveUp = new IntRange(33000, 38000);

		public override bool GuiltyOnDowned
		{
			get
			{
				return true;
			}
		}

		public LordJob_AssaultColony()
		{
		}

		public LordJob_AssaultColony(Faction assaulterFaction, bool canKidnap = true, bool canTimeoutOrFlee = true, bool sappers = false, bool useAvoidGridSmart = false, bool canSteal = true)
		{
			this.assaulterFaction = assaulterFaction;
			this.canKidnap = canKidnap;
			this.canTimeoutOrFlee = canTimeoutOrFlee;
			this.sappers = sappers;
			this.useAvoidGridSmart = useAvoidGridSmart;
			this.canSteal = canSteal;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil lordToil = null;
			if (this.sappers)
			{
				lordToil = new LordToil_AssaultColonySappers();
				if (this.useAvoidGridSmart)
				{
					lordToil.avoidGridMode = AvoidGridMode.Smart;
				}
				stateGraph.AddToil(lordToil);
				Transition transition = new Transition(lordToil, lordToil, true, true);
				transition.AddTrigger(new Trigger_PawnLost());
				stateGraph.AddTransition(transition, false);
				Transition transition2 = new Transition(lordToil, lordToil, true, false);
				transition2.AddTrigger(new Trigger_PawnHarmed(1f, false, null));
				transition2.AddPostAction(new TransitionAction_CheckForJobOverride());
				stateGraph.AddTransition(transition2, false);
			}
			LordToil lordToil2 = new LordToil_AssaultColony(false);
			if (this.useAvoidGridSmart)
			{
				lordToil2.avoidGridMode = AvoidGridMode.Smart;
			}
			stateGraph.AddToil(lordToil2);
			LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap(LocomotionUrgency.Jog, false);
			lordToil_ExitMap.avoidGridMode = AvoidGridMode.Smart;
			stateGraph.AddToil(lordToil_ExitMap);
			if (this.sappers)
			{
				Transition transition3 = new Transition(lordToil, lordToil2, false, true);
				transition3.AddTrigger(new Trigger_NoFightingSappers());
				stateGraph.AddTransition(transition3, false);
			}
			if (this.assaulterFaction.def.humanlikeFaction)
			{
				if (this.canTimeoutOrFlee)
				{
					Transition transition4 = new Transition(lordToil2, lordToil_ExitMap, false, true);
					if (lordToil != null)
					{
						transition4.AddSource(lordToil);
					}
					transition4.AddTrigger(new Trigger_TicksPassed((!this.sappers) ? LordJob_AssaultColony.AssaultTimeBeforeGiveUp.RandomInRange : LordJob_AssaultColony.SapTimeBeforeGiveUp.RandomInRange));
					transition4.AddPreAction(new TransitionAction_Message("MessageRaidersGivenUpLeaving".Translate(new object[]
					{
						this.assaulterFaction.def.pawnsPlural.CapitalizeFirst(),
						this.assaulterFaction.Name
					}), null, 1f));
					stateGraph.AddTransition(transition4, false);
					Transition transition5 = new Transition(lordToil2, lordToil_ExitMap, false, true);
					if (lordToil != null)
					{
						transition5.AddSource(lordToil);
					}
					FloatRange floatRange = new FloatRange(0.25f, 0.35f);
					float randomInRange = floatRange.RandomInRange;
					transition5.AddTrigger(new Trigger_FractionColonyDamageTaken(randomInRange, 900f));
					transition5.AddPreAction(new TransitionAction_Message("MessageRaidersSatisfiedLeaving".Translate(new object[]
					{
						this.assaulterFaction.def.pawnsPlural.CapitalizeFirst(),
						this.assaulterFaction.Name
					}), null, 1f));
					stateGraph.AddTransition(transition5, false);
				}
				if (this.canKidnap)
				{
					LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_Kidnap().CreateGraph()).StartingToil;
					Transition transition6 = new Transition(lordToil2, startingToil, false, true);
					if (lordToil != null)
					{
						transition6.AddSource(lordToil);
					}
					transition6.AddPreAction(new TransitionAction_Message("MessageRaidersKidnapping".Translate(new object[]
					{
						this.assaulterFaction.def.pawnsPlural.CapitalizeFirst(),
						this.assaulterFaction.Name
					}), null, 1f));
					transition6.AddTrigger(new Trigger_KidnapVictimPresent());
					stateGraph.AddTransition(transition6, false);
				}
				if (this.canSteal)
				{
					LordToil startingToil2 = stateGraph.AttachSubgraph(new LordJob_Steal().CreateGraph()).StartingToil;
					Transition transition7 = new Transition(lordToil2, startingToil2, false, true);
					if (lordToil != null)
					{
						transition7.AddSource(lordToil);
					}
					transition7.AddPreAction(new TransitionAction_Message("MessageRaidersStealing".Translate(new object[]
					{
						this.assaulterFaction.def.pawnsPlural.CapitalizeFirst(),
						this.assaulterFaction.Name
					}), null, 1f));
					transition7.AddTrigger(new Trigger_HighValueThingsAround());
					stateGraph.AddTransition(transition7, false);
				}
			}
			Transition transition8 = new Transition(lordToil2, lordToil_ExitMap, false, true);
			if (lordToil != null)
			{
				transition8.AddSource(lordToil);
			}
			transition8.AddTrigger(new Trigger_BecameNonHostileToPlayer());
			transition8.AddPreAction(new TransitionAction_Message("MessageRaidersLeaving".Translate(new object[]
			{
				this.assaulterFaction.def.pawnsPlural.CapitalizeFirst(),
				this.assaulterFaction.Name
			}), null, 1f));
			stateGraph.AddTransition(transition8, false);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.Look<Faction>(ref this.assaulterFaction, "assaulterFaction", false);
			Scribe_Values.Look<bool>(ref this.canKidnap, "canKidnap", true, false);
			Scribe_Values.Look<bool>(ref this.canTimeoutOrFlee, "canTimeoutOrFlee", true, false);
			Scribe_Values.Look<bool>(ref this.sappers, "sappers", false, false);
			Scribe_Values.Look<bool>(ref this.useAvoidGridSmart, "useAvoidGridSmart", false, false);
			Scribe_Values.Look<bool>(ref this.canSteal, "canSteal", true, false);
		}
	}
}
