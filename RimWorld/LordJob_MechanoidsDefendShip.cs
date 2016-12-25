using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_MechanoidsDefendShip : LordJob
	{
		private Thing shipPart;

		private Faction faction;

		private float defendRadius;

		private IntVec3 defSpot;

		public LordJob_MechanoidsDefendShip()
		{
		}

		public LordJob_MechanoidsDefendShip(Thing shipPart, Faction faction, float defendRadius, IntVec3 defSpot)
		{
			this.shipPart = shipPart;
			this.faction = faction;
			this.defendRadius = defendRadius;
			this.defSpot = defSpot;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			if (!this.defSpot.IsValid)
			{
				Log.Warning("LordJob_MechanoidsDefendShip defSpot is invalid. Returning graph for LordJob_AssaultColony.");
				stateGraph.AttachSubgraph(new LordJob_AssaultColony(this.faction, true, true, false, false, true).CreateGraph());
				return stateGraph;
			}
			LordToil_DefendPoint lordToil_DefendPoint = new LordToil_DefendPoint(this.defSpot, this.defendRadius);
			stateGraph.StartingToil = lordToil_DefendPoint;
			LordToil_AssaultColony lordToil_AssaultColony = new LordToil_AssaultColony();
			stateGraph.AddToil(lordToil_AssaultColony);
			LordToil_ExitMapBest lordToil_ExitMapBest = new LordToil_ExitMapBest(LocomotionUrgency.Walk, true);
			stateGraph.AddToil(lordToil_ExitMapBest);
			Transition transition = new Transition(lordToil_DefendPoint, lordToil_ExitMapBest);
			transition.AddSource(lordToil_AssaultColony);
			transition.AddTrigger(new Trigger_PawnCannotReachMapEdge());
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_ExitMapBest, lordToil_AssaultColony);
			transition2.AddTrigger(new Trigger_PawnCanReachMapEdge());
			transition2.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition2);
			Transition transition3 = new Transition(lordToil_DefendPoint, lordToil_AssaultColony);
			transition3.AddTrigger(new Trigger_ThingDamageTaken(this.shipPart, 0.5f));
			transition3.AddTrigger(new Trigger_Memo("AssaultColony"));
			stateGraph.AddTransition(transition3);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.LookReference<Thing>(ref this.shipPart, "shipPart", false);
			Scribe_References.LookReference<Faction>(ref this.faction, "faction", false);
			Scribe_Values.LookValue<float>(ref this.defendRadius, "defendRadius", 0f, false);
			Scribe_Values.LookValue<IntVec3>(ref this.defSpot, "defSpot", default(IntVec3), false);
		}
	}
}
