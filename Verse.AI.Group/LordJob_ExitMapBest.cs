using System;

namespace Verse.AI.Group
{
	public class LordJob_ExitMapBest : LordJob
	{
		private LocomotionUrgency locomotion = LocomotionUrgency.Jog;

		private bool canDig;

		public LordJob_ExitMapBest()
		{
		}

		public LordJob_ExitMapBest(LocomotionUrgency locomotion, bool canDig = false)
		{
			this.locomotion = locomotion;
			this.canDig = canDig;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			stateGraph.AddToil(new LordToil_ExitMapBest(this.locomotion, this.canDig)
			{
				avoidGridMode = AvoidGridMode.Smart
			});
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_Values.LookValue<LocomotionUrgency>(ref this.locomotion, "locomotion", LocomotionUrgency.Jog, false);
			Scribe_Values.LookValue<bool>(ref this.canDig, "canDig", false, false);
		}
	}
}
