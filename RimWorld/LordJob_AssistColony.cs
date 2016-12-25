using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_AssistColony : LordJob
	{
		private Faction faction;

		private IntVec3 fallbackLocation;

		public LordJob_AssistColony()
		{
		}

		public LordJob_AssistColony(Faction faction, IntVec3 fallbackLocation)
		{
			this.faction = faction;
			this.fallbackLocation = fallbackLocation;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_Travel(this.fallbackLocation).CreateGraph()).StartingToil;
			LordToil_HuntEnemies lordToil_HuntEnemies = new LordToil_HuntEnemies(this.fallbackLocation);
			stateGraph.AddToil(lordToil_HuntEnemies);
			StateGraph stateGraph2 = new LordJob_Travel(IntVec3.Invalid).CreateGraph();
			LordToil startingToil2 = stateGraph.AttachSubgraph(stateGraph2).StartingToil;
			LordToil_ExitMapBest lordToil_ExitMapBest = new LordToil_ExitMapBest(LocomotionUrgency.Jog, true);
			stateGraph.AddToil(lordToil_ExitMapBest);
			Transition transition = new Transition(startingToil, lordToil_ExitMapBest);
			transition.AddSource(lordToil_HuntEnemies);
			transition.AddSources(stateGraph2.lordToils);
			transition.AddPreAction(new TransitionAction_Message("MessageVisitorsTrappedLeaving".Translate(new object[]
			{
				this.faction.def.pawnsPlural.CapitalizeFirst(),
				this.faction.Name
			})));
			transition.AddTrigger(new Trigger_PawnCannotReachMapEdge());
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_ExitMapBest, startingToil2);
			transition2.AddTrigger(new Trigger_PawnCanReachMapEdge());
			transition2.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(startingToil, lordToil_HuntEnemies);
			transition3.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition3);
			Transition transition4 = new Transition(lordToil_HuntEnemies, startingToil2);
			transition4.AddPreAction(new TransitionAction_Message("MessageFriendlyFightersLeaving".Translate(new object[]
			{
				this.faction.def.pawnsPlural.CapitalizeFirst(),
				this.faction.Name
			})));
			transition4.AddTrigger(new Trigger_TicksPassed(25000));
			stateGraph.AddTransition(transition4);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.LookReference<Faction>(ref this.faction, "faction", false);
			Scribe_Values.LookValue<IntVec3>(ref this.fallbackLocation, "fallbackLocation", default(IntVec3), false);
		}
	}
}
