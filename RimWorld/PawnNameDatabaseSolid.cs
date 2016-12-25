using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnNameDatabaseSolid
	{
		private const float PreferredNameChance = 0.5f;

		private static Dictionary<GenderPossibility, List<NameTriple>> solidNames;

		static PawnNameDatabaseSolid()
		{
			PawnNameDatabaseSolid.solidNames = new Dictionary<GenderPossibility, List<NameTriple>>();
			using (IEnumerator enumerator = Enum.GetValues(typeof(GenderPossibility)).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					GenderPossibility key = (GenderPossibility)((byte)enumerator.Current);
					PawnNameDatabaseSolid.solidNames.Add(key, new List<NameTriple>());
				}
			}
		}

		public static void AddPlayerContentName(NameTriple newName, GenderPossibility genderPos)
		{
			PawnNameDatabaseSolid.solidNames[genderPos].Add(newName);
		}

		public static NameTriple RandomUnusedSolidName(Gender gender, string forcedLastName = null)
		{
			List<NameTriple> list = PawnNameDatabaseSolid.solidNames[GenderPossibility.Either];
			List<NameTriple> list2 = (gender != Gender.Male) ? PawnNameDatabaseSolid.solidNames[GenderPossibility.Female] : PawnNameDatabaseSolid.solidNames[GenderPossibility.Male];
			float num = ((float)list.Count + 0.1f) / ((float)(list.Count + list2.Count) + 0.1f);
			List<NameTriple> list3;
			if (Rand.Value < num)
			{
				list3 = list;
			}
			else
			{
				list3 = list2;
			}
			if (Rand.Value < 0.5f)
			{
				string prefName = Prefs.RandomPreferredName;
				if (prefName != null && (forcedLastName == null || prefName == forcedLastName))
				{
					List<NameTriple> list4 = (from name in list3
					where !(name.Last != prefName) && !name.UsedThisGame
					select name).ToList<NameTriple>();
					if (list4.Count > 0)
					{
						return list4.RandomElement<NameTriple>();
					}
				}
			}
			int num2 = 0;
			while (list3.Count != 0)
			{
				NameTriple nameTriple = list3.RandomElement<NameTriple>();
				if ((forcedLastName == null || nameTriple.Last == forcedLastName) && !nameTriple.UsedThisGame)
				{
					return nameTriple;
				}
				num2++;
				if (num2 > 30)
				{
					return null;
				}
			}
			Log.Warning("Empty solid pawn name list for gender: " + gender + ".");
			return null;
		}
	}
}
