using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_Siege : LordJob
	{
		private Faction faction;

		private IntVec3 siegeSpot;

		private float blueprintPoints;

		public LordJob_Siege()
		{
		}

		public LordJob_Siege(Faction faction, IntVec3 siegeSpot, float blueprintPoints)
		{
			this.faction = faction;
			this.siegeSpot = siegeSpot;
			this.blueprintPoints = blueprintPoints;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_Travel lordToil_Travel = new LordToil_Travel(this.siegeSpot);
			stateGraph.StartingToil = lordToil_Travel;
			LordToil_Siege lordToil_Siege = new LordToil_Siege(this.siegeSpot, this.blueprintPoints);
			stateGraph.AddToil(lordToil_Siege);
			LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_AssaultColony(this.faction, true, true, false, false, true).CreateGraph()).StartingToil;
			Transition transition = new Transition(lordToil_Travel, lordToil_Siege);
			transition.AddTrigger(new Trigger_Memo("TravelArrived"));
			transition.AddTrigger(new Trigger_TicksPassed(5000));
			stateGraph.AddTransition(transition);
			Transition transition2 = new Transition(lordToil_Siege, startingToil);
			transition2.AddTrigger(new Trigger_Memo("NoBuilders"));
			transition2.AddTrigger(new Trigger_Memo("NoArtillery"));
			transition2.AddTrigger(new Trigger_FractionPawnsLost(0.3f));
			transition2.AddTrigger(new Trigger_TicksPassed((int)(60000f * Rand.Range(1.5f, 3f))));
			transition2.AddPreAction(new TransitionAction_Message("MessageSiegersAssaulting".Translate(new object[]
			{
				this.faction.def.pawnsPlural,
				this.faction
			}), MessageSound.SeriousAlert));
			transition2.AddPreAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(transition2);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_References.LookReference<Faction>(ref this.faction, "faction", false);
			Scribe_Values.LookValue<IntVec3>(ref this.siegeSpot, "siegeSpot", default(IntVec3), false);
			Scribe_Values.LookValue<float>(ref this.blueprintPoints, "blueprintPoints", 0f, false);
		}
	}
}
