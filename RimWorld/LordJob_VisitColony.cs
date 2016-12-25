using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_VisitColony : LordJob
	{
		private Faction faction;

		private IntVec3 chillSpot;

		public LordJob_VisitColony()
		{
		}

		public LordJob_VisitColony(Faction faction, IntVec3 chillSpot)
		{
			this.faction = faction;
			this.chillSpot = chillSpot;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_Travel(this.chillSpot).CreateGraph()).StartingToil;
			stateGraph.StartingToil = startingToil;
			LordToil_DefendPoint lordToil_DefendPoint = new LordToil_DefendPoint(this.chillSpot, 28f);
			stateGraph.AddToil(lordToil_DefendPoint);
			LordToil_TakeWoundedGuest lordToil_TakeWoundedGuest = new LordToil_TakeWoundedGuest();
			stateGraph.AddToil(lordToil_TakeWoundedGuest);
			StateGraph stateGraph2 = new LordJob_TravelAndExit(IntVec3.Invalid).CreateGraph();
			LordToil startingToil2 = stateGraph.AttachSubgraph(stateGraph2).StartingToil;
			LordToil target = stateGraph2.lordToils[1];
			LordToil_ExitMapBest lordToil_ExitMapBest = new LordToil_ExitMapBest(LocomotionUrgency.Walk, true);
			stateGraph.AddToil(lordToil_ExitMapBest);
			Transition transition = new Transition(startingToil, lordToil_ExitMapBest);
			transition.AddSources(new LordToil[]
			{
				lordToil_DefendPoint,
				lordToil_TakeWoundedGuest
			});
			transition.AddSources(stateGraph2.lordToils);
			transition.AddTrigger(new Trigger_PawnCannotReachMapEdge());
			transition.AddPreAction(new TransitionAction_Message("MessageVisitorsTrappedLeaving".Translate(new object[]
			{
				this.faction.def.pawnsPlural.CapitalizeFirst(),
				this.faction.Name
			})));
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_ExitMapBest, startingToil2);
			transition2.AddTrigger(new Trigger_PawnCanReachMapEdge());
			transition2.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
			transition2.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(startingToil, lordToil_DefendPoint);
			transition3.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition3);
			Transition transition4 = new Transition(lordToil_DefendPoint, lordToil_TakeWoundedGuest);
			transition4.AddTrigger(new Trigger_WoundedGuestPresent());
			transition4.AddPreAction(new TransitionAction_Message("MessageVisitorsTakingWounded".Translate(new object[]
			{
				this.faction.def.pawnsPlural.CapitalizeFirst(),
				this.faction.Name
			})));
			stateGraph.AddTransition(transition4);
			Transition transition5 = new Transition(lordToil_DefendPoint, target);
			transition5.AddSources(new LordToil[]
			{
				lordToil_TakeWoundedGuest,
				startingToil
			});
			transition5.AddTrigger(new Trigger_BecameColonyEnemy());
			transition5.AddPreAction(new TransitionAction_WakeAll());
			transition5.AddPreAction(new TransitionAction_SetDefendLocalGroup());
			transition5.AddPreAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition5);
			Transition transition6 = new Transition(lordToil_DefendPoint, startingToil2);
			transition6.AddTrigger(new Trigger_TicksPassed(Rand.Range(8000, 22000)));
			transition6.AddPreAction(new TransitionAction_Message("VisitorsLeaving".Translate(new object[]
			{
				this.faction.Name
			})));
			transition6.AddPreAction(new TransitionAction_WakeAll());
			transition6.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
			stateGraph.AddTransition(transition6);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.LookReference<Faction>(ref this.faction, "faction", false);
			Scribe_Values.LookValue<IntVec3>(ref this.chillSpot, "chillSpot", default(IntVec3), false);
		}
	}
}
