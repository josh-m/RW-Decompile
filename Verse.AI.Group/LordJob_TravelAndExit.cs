using System;

namespace Verse.AI.Group
{
	public class LordJob_TravelAndExit : LordJob
	{
		private IntVec3 travelDest;

		public LordJob_TravelAndExit()
		{
		}

		public LordJob_TravelAndExit(IntVec3 travelDest)
		{
			this.travelDest = travelDest;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_Travel(this.travelDest).CreateGraph()).StartingToil;
			stateGraph.StartingToil = startingToil;
			LordToil_ExitMapBest lordToil_ExitMapBest = new LordToil_ExitMapBest(LocomotionUrgency.None, false);
			stateGraph.AddToil(lordToil_ExitMapBest);
			stateGraph.AddTransition(new Transition(startingToil, lordToil_ExitMapBest)
			{
				triggers = 
				{
					new Trigger_Memo("TravelArrived")
				}
			});
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_Values.LookValue<IntVec3>(ref this.travelDest, "travelDest", default(IntVec3), false);
		}
	}
}
