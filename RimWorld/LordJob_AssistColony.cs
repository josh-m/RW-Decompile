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
			LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap(LocomotionUrgency.None, false);
			stateGraph.AddToil(lordToil_ExitMap);
			LordToil_ExitMap lordToil_ExitMap2 = new LordToil_ExitMap(LocomotionUrgency.Jog, true);
			stateGraph.AddToil(lordToil_ExitMap2);
			Transition transition = new Transition(startingToil, startingToil2);
			transition.AddSource(lordToil_HuntEnemies);
			transition.AddPreAction(new TransitionAction_Message("MessageVisitorsDangerousTemperature".Translate(new object[]
			{
				this.faction.def.pawnsPlural.CapitalizeFirst(),
				this.faction.Name
			})));
			transition.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
			transition.AddTrigger(new Trigger_PawnExperiencingDangerousTemperatures());
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(startingToil, lordToil_ExitMap2);
			transition2.AddSource(lordToil_HuntEnemies);
			transition2.AddSource(lordToil_ExitMap);
			transition2.AddSources(stateGraph2.lordToils);
			transition2.AddPreAction(new TransitionAction_Message("MessageVisitorsTrappedLeaving".Translate(new object[]
			{
				this.faction.def.pawnsPlural.CapitalizeFirst(),
				this.faction.Name
			})));
			transition2.AddTrigger(new Trigger_PawnCannotReachMapEdge());
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_ExitMap2, startingToil2);
			transition3.AddTrigger(new Trigger_PawnCanReachMapEdge());
			transition3.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
			stateGraph.AddTransition(transition3);
			Transition transition4 = new Transition(startingToil, lordToil_HuntEnemies);
			transition4.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition4);
			Transition transition5 = new Transition(lordToil_HuntEnemies, startingToil2);
			transition5.AddPreAction(new TransitionAction_Message("MessageFriendlyFightersLeaving".Translate(new object[]
			{
				this.faction.def.pawnsPlural.CapitalizeFirst(),
				this.faction.Name
			})));
			transition5.AddTrigger(new Trigger_TicksPassed(25000));
			transition5.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
			stateGraph.AddTransition(transition5);
			Transition transition6 = new Transition(startingToil2, lordToil_ExitMap);
			transition6.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(transition6);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.Look<Faction>(ref this.faction, "faction", false);
			Scribe_Values.Look<IntVec3>(ref this.fallbackLocation, "fallbackLocation", default(IntVec3), false);
		}
	}
}
