using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class Caravan_PathFollower : IExposable
	{
		private Caravan caravan;

		private bool moving;

		private bool paused;

		public int nextTile = -1;

		public int previousTileForDrawingIfInDoubt = -1;

		public float nextTileCostLeft;

		public float nextTileCostTotal = 1f;

		private int destTile;

		private CaravanArrivalAction arrivalAction;

		public WorldPath curPath;

		public int lastPathedTargetTile;

		public const int MaxMoveTicks = 30000;

		private const int MaxCheckAheadNodes = 20;

		private const int MinCostWalk = 50;

		private const int MinCostAmble = 60;

		public const float DefaultPathCostToPayPerTick = 1f;

		public const int FinalNoRestPushMaxDurationTicks = 10000;

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

		public bool MovingNow
		{
			get
			{
				return this.Moving && !this.Paused && !this.caravan.CantMove;
			}
		}

		public CaravanArrivalAction ArrivalAction
		{
			get
			{
				return (!this.Moving) ? null : this.arrivalAction;
			}
		}

		public bool Paused
		{
			get
			{
				return this.Moving && this.paused;
			}
			set
			{
				if (value == this.paused)
				{
					return;
				}
				if (!value)
				{
					this.paused = false;
				}
				else if (!this.Moving)
				{
					Log.Error("Tried to pause caravan movement of " + this.caravan.ToStringSafe<Caravan>() + " but it's not moving.", false);
				}
				else
				{
					this.paused = true;
				}
				this.caravan.Notify_DestinationOrPauseStatusChanged();
			}
		}

		public Caravan_PathFollower(Caravan caravan)
		{
			this.caravan = caravan;
		}

		public void ExposeData()
		{
			Scribe_Values.Look<bool>(ref this.moving, "moving", true, false);
			Scribe_Values.Look<bool>(ref this.paused, "paused", false, false);
			Scribe_Values.Look<int>(ref this.nextTile, "nextTile", 0, false);
			Scribe_Values.Look<int>(ref this.previousTileForDrawingIfInDoubt, "previousTileForDrawingIfInDoubt", 0, false);
			Scribe_Values.Look<float>(ref this.nextTileCostLeft, "nextTileCostLeft", 0f, false);
			Scribe_Values.Look<float>(ref this.nextTileCostTotal, "nextTileCostTotal", 0f, false);
			Scribe_Values.Look<int>(ref this.destTile, "destTile", 0, false);
			Scribe_Deep.Look<CaravanArrivalAction>(ref this.arrivalAction, "arrivalAction", new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && Current.ProgramState != ProgramState.Entry && this.moving && !this.StartPath(this.destTile, this.arrivalAction, true, false))
			{
				this.StopDead();
			}
		}

		public bool StartPath(int destTile, CaravanArrivalAction arrivalAction, bool repathImmediately = false, bool resetPauseStatus = true)
		{
			this.caravan.autoJoinable = false;
			if (resetPauseStatus)
			{
				this.paused = false;
			}
			if (arrivalAction != null && !arrivalAction.StillValid(this.caravan, destTile))
			{
				return false;
			}
			if (!this.IsPassable(this.caravan.Tile) && !this.TryRecoverFromUnwalkablePosition())
			{
				return false;
			}
			if (this.moving && this.curPath != null && this.destTile == destTile)
			{
				this.arrivalAction = arrivalAction;
				return true;
			}
			if (!this.caravan.CanReach(destTile))
			{
				this.PatherFailed();
				return false;
			}
			this.destTile = destTile;
			this.arrivalAction = arrivalAction;
			this.caravan.Notify_DestinationOrPauseStatusChanged();
			if (this.nextTile < 0 || !this.IsNextTilePassable())
			{
				this.nextTile = this.caravan.Tile;
				this.nextTileCostLeft = 0f;
				this.previousTileForDrawingIfInDoubt = -1;
			}
			if (this.AtDestinationPosition())
			{
				this.PatherArrived();
				return true;
			}
			if (this.curPath != null)
			{
				this.curPath.ReleaseToPool();
			}
			this.curPath = null;
			this.moving = true;
			if (repathImmediately)
			{
				bool flag = this.TrySetNewPath();
				if (flag && this.nextTileCostLeft <= 0f && this.moving)
				{
					this.TryEnterNextPathTile();
				}
			}
			return true;
		}

		public void StopDead()
		{
			if (this.curPath != null)
			{
				this.curPath.ReleaseToPool();
			}
			this.curPath = null;
			this.moving = false;
			this.paused = false;
			this.nextTile = this.caravan.Tile;
			this.previousTileForDrawingIfInDoubt = -1;
			this.arrivalAction = null;
			this.nextTileCostLeft = 0f;
			this.caravan.Notify_DestinationOrPauseStatusChanged();
		}

		public void PatherTick()
		{
			if (this.moving && this.arrivalAction != null && !this.arrivalAction.StillValid(this.caravan, this.Destination))
			{
				string failMessage = this.arrivalAction.StillValid(this.caravan, this.Destination).FailMessage;
				Messages.Message("MessageCaravanArrivalActionNoLongerValid".Translate(this.caravan.Name).CapitalizeFirst() + ((failMessage == null) ? string.Empty : (" " + failMessage)), this.caravan, MessageTypeDefOf.NegativeEvent, true);
				this.StopDead();
			}
			if (this.caravan.CantMove || this.paused)
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
		}

		private bool IsPassable(int tile)
		{
			return !Find.World.Impassable(tile);
		}

		public bool IsNextTilePassable()
		{
			return this.IsPassable(this.nextTile);
		}

		private bool TryRecoverFromUnwalkablePosition()
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
				}), false);
				this.caravan.Tile = num;
				this.caravan.Notify_Teleported();
				return true;
			}
			Find.WorldObjects.Remove(this.caravan);
			Log.Error(string.Concat(new object[]
			{
				this.caravan,
				" on unwalkable tile ",
				this.caravan.Tile,
				". Could not find walkable position nearby. Removed."
			}), false);
			return false;
		}

		private void PatherArrived()
		{
			CaravanArrivalAction caravanArrivalAction = this.arrivalAction;
			this.StopDead();
			if (caravanArrivalAction != null && caravanArrivalAction.StillValid(this.caravan, this.caravan.Tile))
			{
				caravanArrivalAction.Arrived(this.caravan);
			}
			else if (this.caravan.IsPlayerControlled && !this.caravan.VisibleToCameraNow())
			{
				Messages.Message("MessageCaravanArrivedAtDestination".Translate(this.caravan.Label).CapitalizeFirst(), this.caravan, MessageTypeDefOf.TaskCompletion, true);
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
					Log.Error(this.caravan + " ran out of path nodes. Force-arriving.", false);
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
				}), false);
				this.PatherFailed();
				return;
			}
			this.nextTile = this.curPath.ConsumeNextNode();
			this.previousTileForDrawingIfInDoubt = -1;
			if (Find.World.Impassable(this.nextTile))
			{
				Log.Error(string.Concat(new object[]
				{
					this.caravan,
					" entering ",
					this.nextTile,
					" which is unwalkable."
				}), false);
			}
			int num = this.CostToMove(this.caravan.Tile, this.nextTile);
			this.nextTileCostTotal = (float)num;
			this.nextTileCostLeft = (float)num;
		}

		private int CostToMove(int start, int end)
		{
			return Caravan_PathFollower.CostToMove(this.caravan, start, end, null);
		}

		public static int CostToMove(Caravan caravan, int start, int end, int? ticksAbs = null)
		{
			return Caravan_PathFollower.CostToMove(caravan.TicksPerMove, start, end, ticksAbs, false, null, null);
		}

		public static int CostToMove(int caravanTicksPerMove, int start, int end, int? ticksAbs = null, bool perceivedStatic = false, StringBuilder explanation = null, string caravanTicksPerMoveExplanation = null)
		{
			if (start == end)
			{
				return 0;
			}
			if (explanation != null)
			{
				explanation.Append(caravanTicksPerMoveExplanation);
				explanation.AppendLine();
			}
			StringBuilder stringBuilder = (explanation == null) ? null : new StringBuilder();
			float num;
			if (perceivedStatic && explanation == null)
			{
				num = Find.WorldPathGrid.PerceivedMovementDifficultyAt(end);
			}
			else
			{
				num = WorldPathGrid.CalculatedMovementDifficultyAt(end, perceivedStatic, ticksAbs, stringBuilder);
			}
			float roadMovementDifficultyMultiplier = Find.WorldGrid.GetRoadMovementDifficultyMultiplier(start, end, stringBuilder);
			if (explanation != null)
			{
				explanation.AppendLine();
				explanation.Append("TileMovementDifficulty".Translate() + ":");
				explanation.AppendLine();
				explanation.Append(stringBuilder.ToString().Indented("  "));
				explanation.AppendLine();
				explanation.Append("  = " + (num * roadMovementDifficultyMultiplier).ToString("0.#"));
			}
			int num2 = (int)((float)caravanTicksPerMove * num * roadMovementDifficultyMultiplier);
			num2 = Mathf.Clamp(num2, 1, 30000);
			if (explanation != null)
			{
				explanation.AppendLine();
				explanation.AppendLine();
				explanation.Append("FinalCaravanMovementSpeed".Translate() + ":");
				int num3 = Mathf.CeilToInt((float)num2 / 1f);
				explanation.AppendLine();
				explanation.Append(string.Concat(new string[]
				{
					"  ",
					(60000f / (float)caravanTicksPerMove).ToString("0.#"),
					" / ",
					(num * roadMovementDifficultyMultiplier).ToString("0.#"),
					" = ",
					(60000f / (float)num3).ToString("0.#"),
					" ",
					"TilesPerDay".Translate()
				}));
			}
			return num2;
		}

		public static bool IsValidFinalPushDestination(int tile)
		{
			List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
			for (int i = 0; i < allWorldObjects.Count; i++)
			{
				if (allWorldObjects[i].Tile == tile && !(allWorldObjects[i] is Caravan))
				{
					return true;
				}
			}
			return false;
		}

		private float CostToPayThisTick()
		{
			float num = 1f;
			if (DebugSettings.fastCaravans)
			{
				num = 100f;
			}
			if (num < this.nextTileCostTotal / 30000f)
			{
				num = this.nextTileCostTotal / 30000f;
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
			WorldPath worldPath = Find.WorldPathFinder.FindPath(num, this.destTile, this.caravan, null);
			if (worldPath.Found && num != this.caravan.Tile)
			{
				if (worldPath.NodesLeftCount >= 2 && worldPath.Peek(1) == this.caravan.Tile)
				{
					worldPath.ConsumeNextNode();
					if (this.moving)
					{
						this.previousTileForDrawingIfInDoubt = this.nextTile;
						this.nextTile = this.caravan.Tile;
						this.nextTileCostLeft = this.nextTileCostTotal - this.nextTileCostLeft;
					}
				}
				else
				{
					worldPath.AddNodeAtStart(this.caravan.Tile);
				}
			}
			return worldPath;
		}

		private bool AtDestinationPosition()
		{
			return this.caravan.Tile == this.destTile;
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
