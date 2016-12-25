using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class GiveToPackAnimalUtility
	{
		public static Pawn PackAnimalWithTheMostFreeSpace(Map map, Faction faction)
		{
			List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(faction);
			Pawn pawn = null;
			float num = 0f;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].RaceProps.packAnimal)
				{
					float num2 = MassUtility.FreeSpace(list[i]);
					if (pawn == null || num2 > num)
					{
						pawn = list[i];
						num = num2;
					}
				}
			}
			return pawn;
		}
	}
}
