using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_DefendAttackedTraderCaravan : LordJob
	{
		private IntVec3 defendSpot;

		public LordJob_DefendAttackedTraderCaravan()
		{
		}

		public LordJob_DefendAttackedTraderCaravan(IntVec3 defendSpot)
		{
			this.defendSpot = defendSpot;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_DefendTraderCaravan lordToil_DefendTraderCaravan = new LordToil_DefendTraderCaravan(this.defendSpot);
			stateGraph.StartingToil = lordToil_DefendTraderCaravan;
			LordToil_ExitMapBest lordToil_ExitMapBest = new LordToil_ExitMapBest(LocomotionUrgency.None, false);
			stateGraph.AddToil(lordToil_ExitMapBest);
			Transition transition = new Transition(lordToil_DefendTraderCaravan, lordToil_ExitMapBest);
			transition.AddTrigger(new Trigger_BecameColonyAlly());
			transition.AddTrigger(new Trigger_TraderAndAllTraderCaravanGuardsLost());
			stateGraph.AddTransition(transition);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_Values.LookValue<IntVec3>(ref this.defendSpot, "defendSpot", default(IntVec3), false);
		}
	}
}
