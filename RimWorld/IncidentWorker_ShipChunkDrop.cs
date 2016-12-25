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
			IntVec3 intVec;
			if (!ShipChunkDropCellFinder.TryFindShipChunkDropCell(out intVec, Find.Map.Center, 999999))
			{
				return false;
			}
			this.SpawnShipChunks(intVec, this.RandomCountToDrop);
			Messages.Message("MessageShipChunkDrop".Translate(), intVec, MessageSound.Standard);
			return true;
		}

		private void SpawnShipChunks(IntVec3 firstChunkPos, int count)
		{
			this.SpawnChunk(firstChunkPos);
			for (int i = 0; i < count - 1; i++)
			{
				IntVec3 pos;
				if (ShipChunkDropCellFinder.TryFindShipChunkDropCell(out pos, firstChunkPos, 5))
				{
					this.SpawnChunk(pos);
				}
			}
		}

		private void SpawnChunk(IntVec3 pos)
		{
			IncidentWorker_ShipChunkDrop.<SpawnChunk>c__AnonStorey266 <SpawnChunk>c__AnonStorey = new IncidentWorker_ShipChunkDrop.<SpawnChunk>c__AnonStorey266();
			<SpawnChunk>c__AnonStorey.cr = CellRect.SingleCell(pos);
			IncidentWorker_ShipChunkDrop.<SpawnChunk>c__AnonStorey266 expr_18_cp_0 = <SpawnChunk>c__AnonStorey;
			expr_18_cp_0.cr.Width = expr_18_cp_0.cr.Width + 1;
			IncidentWorker_ShipChunkDrop.<SpawnChunk>c__AnonStorey266 expr_2B_cp_0 = <SpawnChunk>c__AnonStorey;
			expr_2B_cp_0.cr.Height = expr_2B_cp_0.cr.Height + 1;
			RoofCollapserImmediate.DropRoofInCells(from c in <SpawnChunk>c__AnonStorey.cr.ExpandedBy(1).ClipInsideMap().Cells
			where <SpawnChunk>c__AnonStorey.cr.Contains(c) || !Find.ThingGrid.CellContains(c, ThingCategory.Pawn)
			select c);
			GenSpawn.Spawn(ThingDefOf.ShipChunk, pos);
		}
	}
}
