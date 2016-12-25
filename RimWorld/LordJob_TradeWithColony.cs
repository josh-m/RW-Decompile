using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_TradeWithColony : LordJob
	{
		private Faction faction;

		private IntVec3 chillSpot;

		public LordJob_TradeWithColony()
		{
		}

		public LordJob_TradeWithColony(Faction faction, IntVec3 chillSpot)
		{
			this.faction = faction;
			this.chillSpot = chillSpot;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_Travel lordToil_Travel = new LordToil_Travel(this.chillSpot);
			stateGraph.StartingToil = lordToil_Travel;
			LordToil_DefendTraderCaravan lordToil_DefendTraderCaravan = new LordToil_DefendTraderCaravan();
			stateGraph.AddToil(lordToil_DefendTraderCaravan);
			LordToil_DefendTraderCaravan lordToil_DefendTraderCaravan2 = new LordToil_DefendTraderCaravan(this.chillSpot);
			stateGraph.AddToil(lordToil_DefendTraderCaravan2);
			LordToil_ExitMapAndEscortCarriers lordToil_ExitMapAndEscortCarriers = new LordToil_ExitMapAndEscortCarriers();
			stateGraph.AddToil(lordToil_ExitMapAndEscortCarriers);
			LordToil_ExitMapBest lordToil_ExitMapBest = new LordToil_ExitMapBest(LocomotionUrgency.None, false);
			stateGraph.AddToil(lordToil_ExitMapBest);
			LordToil_ExitMapBest lordToil_ExitMapBest2 = new LordToil_ExitMapBest(LocomotionUrgency.Walk, true);
			stateGraph.AddToil(lordToil_ExitMapBest2);
			Transition transition = new Transition(lordToil_Travel, lordToil_ExitMapBest2);
			transition.AddSources(new LordToil[]
			{
				lordToil_DefendTraderCaravan,
				lordToil_DefendTraderCaravan2,
				lordToil_ExitMapAndEscortCarriers,
				lordToil_ExitMapBest
			});
			transition.AddPreAction(new TransitionAction_Message("MessageVisitorsTrappedLeaving".Translate(new object[]
			{
				this.faction.def.pawnsPlural.CapitalizeFirst(),
				this.faction.Name
			})));
			transition.AddTrigger(new Trigger_PawnCannotReachMapEdge());
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_ExitMapBest2, lordToil_ExitMapAndEscortCarriers);
			transition2.AddTrigger(new Trigger_PawnCanReachMapEdge());
			transition2.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_Travel, lordToil_DefendTraderCaravan);
			transition3.AddTrigger(new Trigger_PawnHarmed());
			transition3.AddPreAction(new TransitionAction_WakeAll());
			transition3.AddPreAction(new TransitionAction_SetDefendTrader());
			transition3.AddPreAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition3);
			Transition transition4 = new Transition(lordToil_Travel, lordToil_DefendTraderCaravan2);
			transition4.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition4);
			Transition transition5 = new Transition(lordToil_DefendTraderCaravan, lordToil_Travel);
			transition5.AddTrigger(new Trigger_TicksPassedWithoutHarm(1200));
			stateGraph.AddTransition(transition5);
			Transition transition6 = new Transition(lordToil_DefendTraderCaravan2, lordToil_ExitMapAndEscortCarriers);
			transition6.AddTrigger(new Trigger_TicksPassed(Rand.Range(27000, 45000)));
			transition6.AddPreAction(new TransitionAction_Message("MessageTraderCaravanLeaving".Translate(new object[]
			{
				this.faction.Name
			})));
			transition6.AddPreAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(transition6);
			Transition transition7 = new Transition(lordToil_ExitMapAndEscortCarriers, lordToil_ExitMapAndEscortCarriers);
			transition7.canMoveToSameState = true;
			transition7.AddTrigger(new Trigger_PawnLost());
			stateGraph.AddTransition(transition7);
			Transition transition8 = new Transition(lordToil_ExitMapAndEscortCarriers, lordToil_ExitMapBest);
			transition8.AddTrigger(new Trigger_TicksPassed(60000));
			transition8.AddPreAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(transition8);
			Transition transition9 = new Transition(lordToil_DefendTraderCaravan2, lordToil_ExitMapAndEscortCarriers);
			transition9.AddSources(new LordToil[]
			{
				lordToil_Travel,
				lordToil_DefendTraderCaravan
			});
			transition9.AddTrigger(new Trigger_ImportantTraderCaravanPeopleLost());
			transition9.AddTrigger(new Trigger_BecameColonyEnemy());
			transition9.AddPreAction(new TransitionAction_WakeAll());
			transition9.AddPreAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition9);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.LookReference<Faction>(ref this.faction, "faction", false);
			Scribe_Values.LookValue<IntVec3>(ref this.chillSpot, "chillSpot", default(IntVec3), false);
		}
	}
}
