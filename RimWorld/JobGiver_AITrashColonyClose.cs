using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_AITrashColonyClose : ThinkNode_JobGiver
	{
		private const int CloseSearchRadius = 5;

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.HostileTo(Faction.OfPlayer))
			{
				return null;
			}
			bool flag = pawn.natives.IgniteVerb != null && pawn.HostileTo(Faction.OfPlayer);
			CellRect cellRect = CellRect.CenteredOn(pawn.Position, 5);
			for (int i = 0; i < 35; i++)
			{
				IntVec3 randomCell = cellRect.RandomCell;
				if (randomCell.InBounds())
				{
					Building edifice = randomCell.GetEdifice();
					if (edifice != null && TrashUtility.ShouldTrashBuilding(pawn, edifice) && GenSight.LineOfSight(pawn.Position, randomCell, false))
					{
						if (DebugViewSettings.drawDestSearch)
						{
							Find.DebugDrawer.FlashCell(randomCell, 1f, "trash bld");
						}
						return TrashUtility.TrashJob(pawn, edifice);
					}
					if (flag)
					{
						Plant plant = randomCell.GetPlant();
						if (plant != null && TrashUtility.ShouldTrashPlant(pawn, plant) && GenSight.LineOfSight(pawn.Position, randomCell, false))
						{
							if (DebugViewSettings.drawDestSearch)
							{
								Find.DebugDrawer.FlashCell(randomCell, 0.5f, "trash plant");
							}
							return TrashUtility.TrashJob(pawn, plant);
						}
					}
					if (DebugViewSettings.drawDestSearch)
					{
						Find.DebugDrawer.FlashCell(randomCell, 0f, "trash no");
					}
				}
			}
			return null;
		}
	}
}
