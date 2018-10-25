using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_SleepThenAssaultColony : LordJob
	{
		private Faction faction;

		private bool wakeUpIfColonistClose;

		private const int AnyColonistCloseCheckIntervalTicks = 30;

		private const float AnyColonistCloseCheckRadius = 6f;

		public override bool GuiltyOnDowned
		{
			get
			{
				return true;
			}
		}

		public LordJob_SleepThenAssaultColony()
		{
		}

		public LordJob_SleepThenAssaultColony(Faction faction, bool wakeUpIfColonistClose)
		{
			this.faction = faction;
			this.wakeUpIfColonistClose = wakeUpIfColonistClose;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_Sleep lordToil_Sleep = new LordToil_Sleep();
			stateGraph.StartingToil = lordToil_Sleep;
			LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_AssaultColony(this.faction, true, true, false, false, true).CreateGraph()).StartingToil;
			Transition transition = new Transition(lordToil_Sleep, startingToil, false, true);
			transition.AddTrigger(new Trigger_PawnHarmed(1f, false, null));
			transition.AddPreAction(new TransitionAction_Message("MessageSleepingPawnsWokenUp".Translate(this.faction.def.pawnsPlural).CapitalizeFirst(), MessageTypeDefOf.ThreatBig, null, 1f));
			transition.AddPostAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(transition, false);
			if (this.wakeUpIfColonistClose)
			{
				transition.AddTrigger(new Trigger_Custom((TriggerSignal x) => Find.TickManager.TicksGame % 30 == 0 && this.AnyColonistClose()));
			}
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.Look<Faction>(ref this.faction, "faction", false);
			Scribe_Values.Look<bool>(ref this.wakeUpIfColonistClose, "wakeUpIfColonistClose", false, false);
		}

		private bool AnyColonistClose()
		{
			int num = GenRadial.NumCellsInRadius(6f);
			Map map = base.Map;
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				Pawn pawn = this.lord.ownedPawns[i];
				for (int j = 0; j < num; j++)
				{
					IntVec3 intVec = pawn.Position + GenRadial.RadialPattern[j];
					if (intVec.InBounds(map) && this.AnyColonistAt(intVec) && GenSight.LineOfSight(pawn.Position, intVec, map, false, null, 0, 0))
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool AnyColonistAt(IntVec3 c)
		{
			List<Thing> thingList = c.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Pawn pawn = thingList[i] as Pawn;
				if (pawn != null && pawn.IsColonist)
				{
					return true;
				}
			}
			return false;
		}
	}
}
