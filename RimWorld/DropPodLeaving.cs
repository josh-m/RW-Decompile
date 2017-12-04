using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class DropPodLeaving : Skyfaller, IActiveDropPod, IThingHolder
	{
		public int groupID = -1;

		public int destinationTile = -1;

		public IntVec3 destinationCell = IntVec3.Invalid;

		public PawnsArriveMode arriveMode;

		public bool attackOnArrival;

		private bool alreadyLeft;

		private static List<Thing> tmpActiveDropPods = new List<Thing>();

		public ActiveDropPodInfo Contents
		{
			get
			{
				return ((ActiveDropPod)this.innerContainer[0]).Contents;
			}
			set
			{
				((ActiveDropPod)this.innerContainer[0]).Contents = value;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.groupID, "groupID", 0, false);
			Scribe_Values.Look<int>(ref this.destinationTile, "destinationTile", 0, false);
			Scribe_Values.Look<IntVec3>(ref this.destinationCell, "destinationCell", default(IntVec3), false);
			Scribe_Values.Look<PawnsArriveMode>(ref this.arriveMode, "arriveMode", PawnsArriveMode.Undecided, false);
			Scribe_Values.Look<bool>(ref this.attackOnArrival, "attackOnArrival", false, false);
			Scribe_Values.Look<bool>(ref this.alreadyLeft, "alreadyLeft", false, false);
		}

		protected override void LeaveMap()
		{
			if (this.alreadyLeft)
			{
				base.LeaveMap();
				return;
			}
			if (this.groupID < 0)
			{
				Log.Error("Drop pod left the map, but its group ID is " + this.groupID);
				this.Destroy(DestroyMode.Vanish);
				return;
			}
			if (this.destinationTile < 0)
			{
				Log.Error("Drop pod left the map, but its destination tile is " + this.destinationTile);
				this.Destroy(DestroyMode.Vanish);
				return;
			}
			Lord lord = TransporterUtility.FindLord(this.groupID, base.Map);
			if (lord != null)
			{
				base.Map.lordManager.RemoveLord(lord);
			}
			TravelingTransportPods travelingTransportPods = (TravelingTransportPods)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.TravelingTransportPods);
			travelingTransportPods.Tile = base.Map.Tile;
			travelingTransportPods.SetFaction(Faction.OfPlayer);
			travelingTransportPods.destinationTile = this.destinationTile;
			travelingTransportPods.destinationCell = this.destinationCell;
			travelingTransportPods.arriveMode = this.arriveMode;
			travelingTransportPods.attackOnArrival = this.attackOnArrival;
			Find.WorldObjects.Add(travelingTransportPods);
			DropPodLeaving.tmpActiveDropPods.Clear();
			DropPodLeaving.tmpActiveDropPods.AddRange(base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ActiveDropPod));
			for (int i = 0; i < DropPodLeaving.tmpActiveDropPods.Count; i++)
			{
				DropPodLeaving dropPodLeaving = DropPodLeaving.tmpActiveDropPods[i] as DropPodLeaving;
				if (dropPodLeaving != null && dropPodLeaving.groupID == this.groupID)
				{
					dropPodLeaving.alreadyLeft = true;
					travelingTransportPods.AddPod(dropPodLeaving.Contents, true);
					dropPodLeaving.Contents = null;
					dropPodLeaving.Destroy(DestroyMode.Vanish);
				}
			}
		}
	}
}
