using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class TravelingTransportPods : WorldObject
	{
		private const float TravelSpeed = 0.00025f;

		public int destinationTile = -1;

		public IntVec3 destinationCell = IntVec3.Invalid;

		public PawnsArriveMode arriveMode;

		public bool attackOnArrival;

		private List<ActiveDropPodInfo> pods = new List<ActiveDropPodInfo>();

		private bool arrived;

		private int initialTile = -1;

		private float traveledPct;

		private static List<Pawn> tmpPawns = new List<Pawn>();

		private Vector3 Start
		{
			get
			{
				return Find.WorldGrid.GetTileCenter(this.initialTile);
			}
		}

		private Vector3 End
		{
			get
			{
				return Find.WorldGrid.GetTileCenter(this.destinationTile);
			}
		}

		public override Vector3 DrawPos
		{
			get
			{
				return Vector3.Slerp(this.Start, this.End, this.traveledPct);
			}
		}

		private float TraveledPctStepPerTick
		{
			get
			{
				Vector3 start = this.Start;
				Vector3 end = this.End;
				if (start == end)
				{
					return 1f;
				}
				float num = GenMath.SphericalDistance(start.normalized, end.normalized);
				if (num == 0f)
				{
					return 1f;
				}
				return 0.00025f / num;
			}
		}

		private bool PodsHaveAnyPotentialCaravanOwner
		{
			get
			{
				for (int i = 0; i < this.pods.Count; i++)
				{
					ThingContainer innerContainer = this.pods[i].innerContainer;
					for (int j = 0; j < innerContainer.Count; j++)
					{
						Pawn pawn = innerContainer[j] as Pawn;
						if (pawn != null && CaravanUtility.IsOwner(pawn, base.Faction))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool PodsHaveAnyFreeColonist
		{
			get
			{
				for (int i = 0; i < this.pods.Count; i++)
				{
					ThingContainer innerContainer = this.pods[i].innerContainer;
					for (int j = 0; j < innerContainer.Count; j++)
					{
						Pawn pawn = innerContainer[j] as Pawn;
						if (pawn != null && pawn.IsColonist && pawn.HostFaction == null)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public IEnumerable<Pawn> Pawns
		{
			get
			{
				for (int i = 0; i < this.pods.Count; i++)
				{
					ThingContainer things = this.pods[i].innerContainer;
					for (int j = 0; j < things.Count; j++)
					{
						Pawn p = things[j] as Pawn;
						if (p != null)
						{
							yield return p;
						}
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.LookList<ActiveDropPodInfo>(ref this.pods, "pods", LookMode.Deep, new object[0]);
			Scribe_Values.LookValue<int>(ref this.destinationTile, "destinationTile", 0, false);
			Scribe_Values.LookValue<IntVec3>(ref this.destinationCell, "destinationCell", default(IntVec3), false);
			Scribe_Values.LookValue<PawnsArriveMode>(ref this.arriveMode, "arriveMode", PawnsArriveMode.Undecided, false);
			Scribe_Values.LookValue<bool>(ref this.attackOnArrival, "attackOnArrival", false, false);
			Scribe_Values.LookValue<bool>(ref this.arrived, "arrived", false, false);
			Scribe_Values.LookValue<int>(ref this.initialTile, "initialTile", 0, false);
			Scribe_Values.LookValue<float>(ref this.traveledPct, "traveledPct", 0f, false);
		}

		public override void PostAdd()
		{
			base.PostAdd();
			this.initialTile = base.Tile;
		}

		public override void Tick()
		{
			base.Tick();
			this.traveledPct += this.TraveledPctStepPerTick;
			if (this.traveledPct >= 1f)
			{
				this.traveledPct = 1f;
				this.Arrived();
			}
		}

		public void AddPod(ActiveDropPodInfo contents, bool justLeftTheMap)
		{
			contents.parent = null;
			this.pods.Add(contents);
			ThingContainer innerContainer = contents.innerContainer;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				Pawn pawn = innerContainer[i] as Pawn;
				if (pawn != null && !pawn.IsWorldPawn())
				{
					if (!base.Spawned)
					{
						Log.Warning("Passing pawn " + pawn + " to world, but the TravelingTransportPod is not spawned. This means that WorldPawns can discard this pawn which can cause bugs.");
					}
					if (justLeftTheMap)
					{
						pawn.ExitMap(false);
					}
					else
					{
						Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
					}
				}
			}
			contents.savePawnsWithReferenceMode = true;
		}

		public bool ContainsPawn(Pawn p)
		{
			for (int i = 0; i < this.pods.Count; i++)
			{
				if (this.pods[i].innerContainer.Contains(p))
				{
					return true;
				}
			}
			return false;
		}

		private void Arrived()
		{
			if (this.arrived)
			{
				return;
			}
			this.arrived = true;
			Map map = Current.Game.FindMap(this.destinationTile);
			if (map != null)
			{
				this.SpawnDropPodsInMap(map, null);
			}
			else if (!this.PodsHaveAnyPotentialCaravanOwner)
			{
				for (int i = 0; i < this.pods.Count; i++)
				{
					this.pods[i].innerContainer.ClearAndDestroyContentsOrPassToWorld(DestroyMode.Vanish);
				}
				this.RemoveAllPods();
				Find.WorldObjects.Remove(this);
				Messages.Message("MessageTransportPodsArrivedAndLost".Translate(), new GlobalTargetInfo(this.destinationTile), MessageSound.Negative);
			}
			else
			{
				FactionBase factionBase = Find.WorldObjects.FactionBases.Find((FactionBase x) => x.Tile == this.destinationTile);
				if (factionBase != null && factionBase.Faction != Faction.OfPlayer && this.attackOnArrival)
				{
					LongEventHandler.QueueLongEvent(delegate
					{
						Map map2 = AttackCaravanArrivalActionUtility.GenerateFactionBaseMap(factionBase);
						string extraMessagePart = null;
						if (!factionBase.Faction.HostileTo(Faction.OfPlayer))
						{
							factionBase.Faction.SetHostileTo(Faction.OfPlayer, true);
							extraMessagePart = "MessageTransportPodsArrived_BecameHostile".Translate(new object[]
							{
								factionBase.Faction.Name
							}).CapitalizeFirst();
						}
						Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
						this.SpawnDropPodsInMap(map2, extraMessagePart);
					}, "GeneratingMapForNewEncounter", false, null);
				}
				else
				{
					this.SpawnCaravanAtDestinationTile();
				}
			}
		}

		private void SpawnDropPodsInMap(Map map, string extraMessagePart = null)
		{
			this.RemoveAllPawnsFromWorldPawns();
			IntVec3 intVec;
			if (this.destinationCell.IsValid && this.destinationCell.InBounds(map))
			{
				intVec = this.destinationCell;
			}
			else if (this.arriveMode == PawnsArriveMode.CenterDrop)
			{
				if (!DropCellFinder.TryFindRaidDropCenterClose(out intVec, map))
				{
					intVec = DropCellFinder.FindRaidDropCenterDistant(map);
				}
			}
			else
			{
				if (this.arriveMode != PawnsArriveMode.EdgeDrop && this.arriveMode != PawnsArriveMode.Undecided)
				{
					Log.Warning("Unsupported arrive mode " + this.arriveMode);
				}
				intVec = DropCellFinder.FindRaidDropCenterDistant(map);
			}
			for (int i = 0; i < this.pods.Count; i++)
			{
				IntVec3 c;
				DropCellFinder.TryFindDropSpotNear(intVec, map, out c, false, true);
				DropPodUtility.MakeDropPodAt(c, map, this.pods[i]);
			}
			this.RemoveAllPods();
			Find.WorldObjects.Remove(this);
			string text = "MessageTransportPodsArrived".Translate();
			if (extraMessagePart != null)
			{
				text = text + " " + extraMessagePart;
			}
			Messages.Message(text, new TargetInfo(intVec, map, false), MessageSound.Benefit);
		}

		private void SpawnCaravanAtDestinationTile()
		{
			TravelingTransportPods.tmpPawns.Clear();
			for (int i = 0; i < this.pods.Count; i++)
			{
				ThingContainer innerContainer = this.pods[i].innerContainer;
				for (int j = 0; j < innerContainer.Count; j++)
				{
					Pawn pawn = innerContainer[j] as Pawn;
					if (pawn != null)
					{
						TravelingTransportPods.tmpPawns.Add(pawn);
					}
				}
			}
			int startingTile;
			if (!GenWorldClosest.TryFindClosestPassableTile(this.destinationTile, out startingTile))
			{
				startingTile = this.destinationTile;
			}
			Caravan o = CaravanMaker.MakeCaravan(TravelingTransportPods.tmpPawns, base.Faction, startingTile, true);
			for (int k = 0; k < this.pods.Count; k++)
			{
				ThingContainer innerContainer2 = this.pods[k].innerContainer;
				for (int l = 0; l < innerContainer2.Count; l++)
				{
					if (!(innerContainer2[l] is Pawn))
					{
						Pawn pawn2 = CaravanInventoryUtility.FindPawnToMoveInventoryTo(innerContainer2[l], TravelingTransportPods.tmpPawns, null, null);
						pawn2.inventory.innerContainer.TryAdd(innerContainer2[l], true);
					}
				}
			}
			this.RemoveAllPods();
			Find.WorldObjects.Remove(this);
			Messages.Message("MessageTransportPodsArrived".Translate(), o, MessageSound.Benefit);
		}

		private void RemoveAllPawnsFromWorldPawns()
		{
			for (int i = 0; i < this.pods.Count; i++)
			{
				ThingContainer innerContainer = this.pods[i].innerContainer;
				for (int j = 0; j < innerContainer.Count; j++)
				{
					Pawn pawn = innerContainer[j] as Pawn;
					if (pawn != null && pawn.IsWorldPawn())
					{
						Find.WorldPawns.RemovePawn(pawn);
					}
				}
			}
		}

		private void RemoveAllPods()
		{
			for (int i = 0; i < this.pods.Count; i++)
			{
				this.pods[i].savePawnsWithReferenceMode = false;
			}
			this.pods.Clear();
		}
	}
}
