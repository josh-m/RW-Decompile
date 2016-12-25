using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public static class NameUseChecker
	{
		public static IEnumerable<Name> AllPawnsNamesEverUsed
		{
			get
			{
				foreach (Pawn p in PawnsFinder.AllMapsAndWorld_AliveOrDead)
				{
					if (p.Name != null)
					{
						yield return p.Name;
					}
				}
			}
		}

		public static bool NameWordIsUsed(string singleName)
		{
			foreach (Name current in NameUseChecker.AllPawnsNamesEverUsed)
			{
				NameTriple nameTriple = current as NameTriple;
				if (nameTriple != null && (singleName == nameTriple.First || singleName == nameTriple.Nick || singleName == nameTriple.Last))
				{
					bool result = true;
					return result;
				}
				NameSingle nameSingle = current as NameSingle;
				if (nameSingle != null && nameSingle.Name == singleName)
				{
					bool result = true;
					return result;
				}
			}
			return false;
		}

		public static bool NameSingleIsUsedOnAnyMap(string candidate)
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				foreach (Pawn current in maps[i].mapPawns.AllPawns)
				{
					NameSingle nameSingle = current.Name as NameSingle;
					if (nameSingle != null && nameSingle.Name == candidate)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
