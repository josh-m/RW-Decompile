using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_LoadAndEnterTransporters : LordJob
	{
		public int transportersGroup = -1;

		public LordJob_LoadAndEnterTransporters()
		{
		}

		public LordJob_LoadAndEnterTransporters(int transportersGroup)
		{
			this.transportersGroup = transportersGroup;
		}

		public override void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.transportersGroup, "transportersGroup", 0, false);
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_LoadAndEnterTransporters lordToil_LoadAndEnterTransporters = new LordToil_LoadAndEnterTransporters(this.transportersGroup);
			stateGraph.StartingToil = lordToil_LoadAndEnterTransporters;
			LordToil_End lordToil_End = new LordToil_End();
			stateGraph.AddToil(lordToil_End);
			Transition transition = new Transition(lordToil_LoadAndEnterTransporters, lordToil_End);
			transition.AddTrigger(new Trigger_PawnLost());
			transition.AddPreAction(new TransitionAction_Message("MessageFailedToLoadTransportersBecauseColonistLost".Translate(), MessageSound.Negative));
			transition.AddPreAction(new TransitionAction_Custom(new Action(this.CancelLoadingProcess)));
			stateGraph.AddTransition(transition);
			return stateGraph;
		}

		private void CancelLoadingProcess()
		{
			List<Thing> list = this.lord.Map.listerThings.ThingsInGroup(ThingRequestGroup.Transporter);
			for (int i = 0; i < list.Count; i++)
			{
				CompTransporter compTransporter = list[i].TryGetComp<CompTransporter>();
				if (compTransporter.groupID == this.transportersGroup)
				{
					compTransporter.CancelLoad();
					break;
				}
			}
		}
	}
}
