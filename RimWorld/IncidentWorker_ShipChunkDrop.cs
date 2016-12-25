using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_ShipChunkDrop : IncidentWorker
	{
		private static readonly Pair<int, float>[] CountChance = new Pair<int, float>[]
		{
			new Pair<int, float>(1, 1f),
			new Pair<int, float>(2, 0.95f),
			new Pair<int, float>(3, 0.7f),
			new Pair<int, float>(4, 0.4f)
		};

		private int RandomCountToDrop
		{
			get
			{
				float x2 = (float)Find.TickManager.TicksGame / 3600000f;
				float timePassedFactor = Mathf.Clamp(GenMath.LerpDouble(0f, 1.2f, 1f, 0.1f, x2), 0.1f, 1f);
				return IncidentWorker_ShipChunkDrop.CountChance.RandomElementByWeight(delegate(Pair<int, float> x)
				{
					if (x.First == 1)
					{
						return x.Second;
					}
					return x.Second * timePassedFactor;
				}).First;
			}
		}

		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 intVec;
			if (!ShipChunkDropCellFinder.TryFindShipChunkDropCell(map.Center, map, 999999, out intVec))
			{
				return false;
			}
			this.SpawnShipChunks(intVec, map, this.RandomCountToDrop);
			Messages.Message("MessageShipChunkDrop".Translate(), new TargetInfo(intVec, map, false), MessageSound.Standard);
			return true;
		}

		private void SpawnShipChunks(IntVec3 firstChunkPos, Map map, int count)
		{
			this.SpawnChunk(firstChunkPos, map);
			for (int i = 0; i < count - 1; i++)
			{
				IntVec3 pos;
				if (ShipChunkDropCellFinder.TryFindShipChunkDropCell(firstChunkPos, map, 5, out pos))
				{
					this.SpawnChunk(pos, map);
				}
			}
		}

		private void SpawnChunk(IntVec3 pos, Map map)
		{
			IncidentWorker_ShipChunkDrop.<SpawnChunk>c__AnonStorey2A5 <SpawnChunk>c__AnonStorey2A = new IncidentWorker_ShipChunkDrop.<SpawnChunk>c__AnonStorey2A5();
			<SpawnChunk>c__AnonStorey2A.map = map;
			<SpawnChunk>c__AnonStorey2A.cr = CellRect.SingleCell(pos);
			IncidentWorker_ShipChunkDrop.<SpawnChunk>c__AnonStorey2A5 expr_1F_cp_0 = <SpawnChunk>c__AnonStorey2A;
			expr_1F_cp_0.cr.Width = expr_1F_cp_0.cr.Width + 1;
			IncidentWorker_ShipChunkDrop.<SpawnChunk>c__AnonStorey2A5 expr_32_cp_0 = <SpawnChunk>c__AnonStorey2A;
			expr_32_cp_0.cr.Height = expr_32_cp_0.cr.Height + 1;
			RoofCollapserImmediate.DropRoofInCells(from c in <SpawnChunk>c__AnonStorey2A.cr.ExpandedBy(1).ClipInsideMap(<SpawnChunk>c__AnonStorey2A.map).Cells
			where <SpawnChunk>c__AnonStorey2A.cr.Contains(c) || !<SpawnChunk>c__AnonStorey2A.map.thingGrid.CellContains(c, ThingCategory.Pawn)
			select c, <SpawnChunk>c__AnonStorey2A.map);
			GenSpawn.Spawn(ThingDefOf.ShipChunk, pos, <SpawnChunk>c__AnonStorey2A.map);
		}
	}
}
