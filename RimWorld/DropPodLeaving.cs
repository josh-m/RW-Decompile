using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
	public class DropPodLeaving : Thing, IActiveDropPod, IThingContainerOwner
	{
		private const int MinTicksSinceStart = -40;

		private const int MaxTicksSinceStart = -15;

		private const int TicksSinceStartToPlaySound = -10;

		private const int LeaveMapAfterTicks = 220;

		private ActiveDropPodInfo contents;

		public int groupID = -1;

		public int destinationTile = -1;

		public IntVec3 destinationCell = IntVec3.Invalid;

		public PawnsArriveMode arriveMode;

		public bool attackOnArrival;

		private int ticksSinceStart;

		private bool alreadyLeft;

		private bool soundPlayed;

		private static List<Thing> tmpActiveDropPods = new List<Thing>();

		public override Vector3 DrawPos
		{
			get
			{
				return DropPodAnimationUtility.DrawPosAt(this.ticksSinceStart, base.Position);
			}
		}

		public ActiveDropPodInfo Contents
		{
			get
			{
				return this.contents;
			}
			set
			{
				if (this.contents != null)
				{
					this.contents.parent = null;
				}
				if (value != null)
				{
					value.parent = this;
				}
				this.contents = value;
			}
		}

		public ThingContainer GetInnerContainer()
		{
			return this.contents.innerContainer;
		}

		public IntVec3 GetPosition()
		{
			return base.PositionHeld;
		}

		public Map GetMap()
		{
			return base.MapHeld;
		}

		public override void PostMake()
		{
			base.PostMake();
			this.ticksSinceStart = Rand.RangeInclusive(-40, -15);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.groupID, "groupID", 0, false);
			Scribe_Values.LookValue<int>(ref this.destinationTile, "destinationTile", 0, false);
			Scribe_Values.LookValue<IntVec3>(ref this.destinationCell, "destinationCell", default(IntVec3), false);
			Scribe_Values.LookValue<PawnsArriveMode>(ref this.arriveMode, "arriveMode", PawnsArriveMode.Undecided, false);
			Scribe_Values.LookValue<bool>(ref this.attackOnArrival, "attackOnArrival", false, false);
			Scribe_Values.LookValue<int>(ref this.ticksSinceStart, "ticksSinceStart", 0, false);
			Scribe_Deep.LookDeep<ActiveDropPodInfo>(ref this.contents, "contents", new object[]
			{
				this
			});
			Scribe_Values.LookValue<bool>(ref this.alreadyLeft, "alreadyLeft", false, false);
			Scribe_Values.LookValue<bool>(ref this.soundPlayed, "soundPlayed", false, false);
		}

		public override void Tick()
		{
			if (!this.soundPlayed && this.ticksSinceStart >= -10)
			{
				SoundDefOf.DropPodLeaving.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
				this.soundPlayed = true;
			}
			this.ticksSinceStart++;
			if (!this.alreadyLeft && this.ticksSinceStart >= 220)
			{
				this.GroupLeftMap();
			}
		}

		public override void DrawAt(Vector3 drawLoc)
		{
			base.DrawAt(drawLoc);
			DropPodAnimationUtility.DrawDropSpotShadow(this, this.ticksSinceStart);
		}

		private void GroupLeftMap()
		{
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
					travelingTransportPods.AddPod(dropPodLeaving.contents, true);
					dropPodLeaving.contents = null;
					dropPodLeaving.Destroy(DestroyMode.Vanish);
				}
			}
		}

		virtual bool get_Spawned()
		{
			return base.Spawned;
		}
	}
}
