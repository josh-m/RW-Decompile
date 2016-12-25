using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Verse
{
	public static class ShortHashGiver
	{
		private static Dictionary<Type, HashSet<ushort>> takenHashesPerDeftype = new Dictionary<Type, HashSet<ushort>>();

		public static void GiveAllShortHashes()
		{
			ShortHashGiver.takenHashesPerDeftype.Clear();
			List<Def> list = new List<Def>();
			foreach (Type current in GenDefDatabase.AllDefTypesWithDatabases())
			{
				Type type = typeof(DefDatabase<>).MakeGenericType(new Type[]
				{
					current
				});
				PropertyInfo property = type.GetProperty("AllDefs");
				MethodInfo getMethod = property.GetGetMethod();
				IEnumerable enumerable = (IEnumerable)getMethod.Invoke(null, null);
				list.Clear();
				foreach (Def item in enumerable)
				{
					list.Add(item);
				}
				list.SortBy((Def d) => d.defName);
				for (int i = 0; i < list.Count; i++)
				{
					ShortHashGiver.GiveShortHash(list[i]);
				}
			}
		}

		private static void GiveShortHash(Def def)
		{
			if (def.shortHash != 0)
			{
				Log.Error(def + " already has short hash.");
				return;
			}
			HashSet<ushort> hashSet;
			if (!ShortHashGiver.takenHashesPerDeftype.TryGetValue(def.GetType(), out hashSet))
			{
				hashSet = new HashSet<ushort>();
				ShortHashGiver.takenHashesPerDeftype.Add(def.GetType(), hashSet);
			}
			ushort num = (ushort)(GenText.StableStringHash(def.defName) % 65535);
			int num2 = 0;
			while (num == 0 || hashSet.Contains(num))
			{
				num += 1;
				num2++;
				if (num2 > 5000)
				{
					Log.Message("Short hashes are saturated. There are probably too many Defs.");
				}
			}
			def.shortHash = num;
			hashSet.Add(num);
		}
	}
}
