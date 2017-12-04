using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public static class PawnNameDatabaseSolid
	{
		private static Dictionary<GenderPossibility, List<NameTriple>> solidNames;

		private const float PreferredNameChance = 0.5f;

		static PawnNameDatabaseSolid()
		{
			PawnNameDatabaseSolid.solidNames = new Dictionary<GenderPossibility, List<NameTriple>>();
			foreach (GenderPossibility key in Enum.GetValues(typeof(GenderPossibility)))
			{
				PawnNameDatabaseSolid.solidNames.Add(key, new List<NameTriple>());
			}
		}

		public static void AddPlayerContentName(NameTriple newName, GenderPossibility genderPos)
		{
			PawnNameDatabaseSolid.solidNames[genderPos].Add(newName);
		}

		public static List<NameTriple> GetListForGender(GenderPossibility gp)
		{
			return PawnNameDatabaseSolid.solidNames[gp];
		}

		[DebuggerHidden]
		public static IEnumerable<NameTriple> AllNames()
		{
			foreach (KeyValuePair<GenderPossibility, List<NameTriple>> kvp in PawnNameDatabaseSolid.solidNames)
			{
				foreach (NameTriple name in kvp.Value)
				{
					yield return name;
				}
			}
		}
	}
}
