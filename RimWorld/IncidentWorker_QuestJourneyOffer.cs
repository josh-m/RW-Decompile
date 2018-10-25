using RimWorld.Planet;
using System;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestJourneyOffer : IncidentWorker
	{
		private const int MinTraversalDistance = 180;

		private const int MaxTraversalDistance = 800;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			int num;
			return this.TryFindRootTile(out num);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			int rootTile;
			if (!this.TryFindRootTile(out rootTile))
			{
				return false;
			}
			int tile;
			if (!this.TryFindDestinationTile(rootTile, out tile))
			{
				return false;
			}
			WorldObject journeyDestination = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.EscapeShip);
			journeyDestination.Tile = tile;
			Find.WorldObjects.Add(journeyDestination);
			DiaNode diaNode = new DiaNode("JourneyOffer".Translate());
			DiaOption diaOption = new DiaOption("JumpToLocation".Translate());
			diaOption.action = delegate
			{
				CameraJumper.TryJumpAndSelect(journeyDestination);
			};
			diaOption.resolveTree = true;
			diaNode.options.Add(diaOption);
			DiaOption diaOption2 = new DiaOption("OK".Translate());
			diaOption2.resolveTree = true;
			diaNode.options.Add(diaOption2);
			Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, null));
			Find.Archive.Add(new ArchivedDialog(diaNode.text, null, null));
			return true;
		}

		private bool TryFindRootTile(out int tile)
		{
			int unused;
			return TileFinder.TryFindRandomPlayerTile(out tile, false, (int x) => this.TryFindDestinationTileActual(x, 180, out unused));
		}

		private bool TryFindDestinationTile(int rootTile, out int tile)
		{
			int num = 800;
			for (int i = 0; i < 1000; i++)
			{
				num = (int)((float)num * Rand.Range(0.5f, 0.75f));
				if (num <= 180)
				{
					num = 180;
				}
				if (this.TryFindDestinationTileActual(rootTile, num, out tile))
				{
					return true;
				}
				if (num <= 180)
				{
					return false;
				}
			}
			tile = -1;
			return false;
		}

		private bool TryFindDestinationTileActual(int rootTile, int minDist, out int tile)
		{
			for (int i = 0; i < 2; i++)
			{
				bool canTraverseImpassable = i == 1;
				if (TileFinder.TryFindPassableTileWithTraversalDistance(rootTile, minDist, 800, out tile, (int x) => !Find.WorldObjects.AnyWorldObjectAt(x) && Find.WorldGrid[x].biome.canBuildBase && Find.WorldGrid[x].biome.canAutoChoose, true, true, canTraverseImpassable))
				{
					return true;
				}
			}
			tile = -1;
			return false;
		}
	}
}
