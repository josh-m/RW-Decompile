using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class Caravan_PathFollower : IExposable
	{
		public const int MaxMoveTicks = 120000;

		private const int MaxCheckAheadNodes = 20;

		private const int MinCostWalk = 50;

		private const int MinCostAmble = 60;

		public const float DefaultPathCostToPayPerTick = 1f;

		private Caravan caravan;

		private bool moving;

		public int nextTile = -1;

		public float nextTileCostLeft;

		public float nextTileCostTotal = 1f;

		private int destTile;

		public CaravanArrivalAction arrivalAction;

		public WorldPath curPath;

		public int lastPathedTargetTile;

		public int Destination
		{
			get
			{
				return this.destTile;
			}
		}

		public bool Moving
		{
			get
			{
				return this.moving && this.caravan.Spawned;
			}
		}

		public Caravan_PathFollower(Caravan caravan)
		{
			this.caravan = caravan;
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<bool>(ref this.moving, "moving", true, false);
			Scribe_Values.LookValue<int>(ref this.nextTile, "nextTile", 0, false);
			Scribe_Values.LookValue<float>(ref this.nextTileCostLeft, "nextTileCostLeft", 0f, false);
			Scribe_Values.LookValue<float>(ref this.nextTileCostTotal, "nextTileCostTotal", 0f, false);
			Scribe_Values.LookValue<int>(ref this.destTile, "destTile", 0, false);
			Scribe_Deep.LookDeep<CaravanArrivalAction>(ref this.arrivalAction, "arrivalAction", new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && Current.ProgramState != ProgramState.Entry && this.moving)
			{
				this.StartPath(this.destTile, this.arrivalAction, true);
			}
		}

		public void StartPath(int destTile, CaravanArrivalAction arrivalAction, bool repathImmediately = false)
		{
			this.caravan.autoJoinable = false;
			if (!this.IsPassable(this.caravan.Tile) && !this.TryRecoverFromUnwalkablePosition(destTile, arrivalAction))
			{
				return;
			}
			if (this.moving && this.curPath != null && this.destTile == destTile)
			{
				return;
			}
			if (!this.caravan.CanReach(destTile))
			{
				this.PatherFailed();
				return;
			}
			this.destTile = destTile;
			this.arrivalAction = arrivalAction;
			if (this.nextTile < 0 || !this.IsNextTilePassable())
			{
				this.nextTile = this.caravan.Tile;
			}
			if (this.AtDestinationPosition())
			{
				this.PatherArrived();
				return;
			}
			if (this.curPath != null)
			{
				this.curPath.ReleaseToPool();
			}
			this.curPath = null;
			this.moving = true;
			if (repathImmediately)
			{
				this.TrySetNewPath();
			}
		}

		public void StopDead()
		{
			if (this.curPath != null)
			{
				this.curPath.ReleaseToPool();
			}
			this.curPath = null;
			this.moving = false;
			this.nextTile = this.caravan.Tile;
			this.arrivalAction = null;
			this.nextTileCostLeft = 0f;
		}

		public void PatherTick()
		{
			if (this.moving && this.arrivalAction != null && this.arrivalAction.ShouldFail)
			{
				this.StopDead();
			}
			if (this.caravan.CantMove)
			{
				return;
			}
			if (this.nextTileCostLeft > 0f)
			{
				this.nextTileCostLeft -= this.CostToPayThisTick();
			}
			else if (this.moving)
			{
				this.TryEnterNextPathTile();
			}
		}

		public void Notify_Teleported_Int()
		{
			this.StopDead();
			this.ResetToCurrentPosition();
		}

		public void ResetToCurrentPosition()
		{
			this.nextTile = this.caravan.Tile;
		}

		private bool IsPassable(int tile)
		{
			return !Find.World.Impassable(tile);
		}

		public bool IsNextTilePassable()
		{
			return this.IsPassable(this.nextTile);
		}

		private bool TryRecoverFromUnwalkablePosition(int originalDest, CaravanArrivalAction originalArrivalAction)
		{
			int num;
			if (GenWorldClosest.TryFindClosestTile(this.caravan.Tile, (int t) => this.IsPassable(t), out num, 2147483647, true))
			{
				Log.Warning(string.Concat(new object[]
				{
					this.caravan,
					" on unwalkable tile ",
					this.caravan.Tile,
					". Teleporting to ",
					num
				}));
				this.caravan.Tile = num;
				this.moving = false;
				this.nextTile = this.caravan.Tile;
				this.StartPath(originalDest, originalArrivalAction, false);
				return true;
			}
			Find.WorldObjects.Remove(this.caravan);
			Log.Error(string.Concat(new object[]
			{
				this.caravan,
				" on unwalkable tile ",
				this.caravan.Tile,
				". Could not find walkable position nearby. Removed."
			}));
			return false;
		}

		private void PatherArrived()
		{
			CaravanArrivalAction caravanArrivalAction = this.arrivalAction;
			this.StopDead();
			if (caravanArrivalAction != null)
			{
				caravanArrivalAction.Arrived(this.caravan);
			}
			else if (this.caravan.IsPlayerControlled && !this.caravan.VisibleToCameraNow())
			{
				Messages.Message("MessageCaravanArrivedAtDestination".Translate(new object[]
				{
					this.caravan.Label
				}).CapitalizeFirst(), this.caravan, MessageSound.Benefit);
			}
		}

		private void PatherFailed()
		{
			this.StopDead();
		}

		private void TryEnterNextPathTile()
		{
			if (!this.IsNextTilePassable())
			{
				this.PatherFailed();
				return;
			}
			this.caravan.Tile = this.nextTile;
			if (this.NeedNewPath() && !this.TrySetNewPath())
			{
				return;
			}
			if (this.AtDestinationPosition())
			{
				this.PatherArrived();
			}
			else
			{
				if (this.curPath.NodesLeftCount == 0)
				{
					Log.Error(this.caravan + " ran out of path nodes. Force-arriving.");
					this.PatherArrived();
					return;
				}
				this.SetupMoveIntoNextTile();
			}
		}

		private void SetupMoveIntoNextTile()
		{
			if (this.curPath.NodesLeftCount < 2)
			{
				Log.Error(string.Concat(new object[]
				{
					this.caravan,
					" at ",
					this.caravan.Tile,
					" ran out of path nodes while pathing to ",
					this.destTile,
					"."
				}));
				this.PatherFailed();
				return;
			}
			this.nextTile = this.curPath.ConsumeNextNode();
			if (Find.World.Impassable(this.nextTile))
			{
				Log.Error(string.Concat(new object[]
				{
					this.caravan,
					" entering ",
					this.nextTile,
					" which is unwalkable."
				}));
			}
			int num = this.CostToMoveIntoTile(this.nextTile);
			this.nextTileCostTotal = (float)num;
			this.nextTileCostLeft = (float)num;
		}

		private int CostToMoveIntoTile(int tile)
		{
			return Caravan_PathFollower.CostToMoveIntoTile(this.caravan, tile, -1f);
		}

		public static int CostToMoveIntoTile(Caravan caravan, int tile, float yearPercent = -1f)
		{
			int num = caravan.TicksPerMove;
			num += WorldPathGrid.CalculatedCostAt(tile, false, yearPercent);
			return Mathf.Clamp(num, 1, 120000);
		}

		private float CostToPayThisTick()
		{
			float num = 1f;
			if (DebugSettings.fastCaravans)
			{
				num = 100f;
			}
			if (num < this.nextTileCostTotal / 120000f)
			{
				num = this.nextTileCostTotal / 120000f;
			}
			return num;
		}

		private bool TrySetNewPath()
		{
			WorldPath worldPath = this.GenerateNewPath();
			if (!worldPath.Found)
			{
				this.PatherFailed();
				return false;
			}
			if (this.curPath != null)
			{
				this.curPath.ReleaseToPool();
			}
			this.curPath = worldPath;
			return true;
		}

		private WorldPath GenerateNewPath()
		{
			int num = (!this.moving || this.nextTile < 0 || !this.IsNextTilePassable()) ? this.caravan.Tile : this.nextTile;
			this.lastPathedTargetTile = this.destTile;
			WorldPath worldPath = Find.WorldPathFinder.FindPath(num, this.destTile, this.caravan);
			if (worldPath.Found && num != this.caravan.Tile)
			{
				worldPath.AddNode(num);
			}
			return worldPath;
		}

		private bool AtDestinationPosition()
		{
			return this.caravan.Tile == this.destTile || (this.arrivalAction != null && this.arrivalAction.ArriveOnTouch && Find.WorldGrid.IsNeighbor(this.caravan.Tile, this.destTile));
		}

		private bool NeedNewPath()
		{
			if (!this.moving)
			{
				return false;
			}
			if (this.curPath == null || !this.curPath.Found || this.curPath.NodesLeftCount == 0)
			{
				return true;
			}
			int num = 0;
			while (num < 20 && num < this.curPath.NodesLeftCount)
			{
				int tileID = this.curPath.Peek(num);
				if (Find.World.Impassable(tileID))
				{
					return true;
				}
				num++;
			}
			return false;
		}
	}
}
