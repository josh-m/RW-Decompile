using System;
using System.Collections;
using System.Collections.Generic;

namespace Verse
{
	internal static class MapMeshFlagUtility
	{
		public static List<MapMeshFlag> allFlags;

		static MapMeshFlagUtility()
		{
			MapMeshFlagUtility.allFlags = new List<MapMeshFlag>();
			using (IEnumerator enumerator = Enum.GetValues(typeof(MapMeshFlag)).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					MapMeshFlag mapMeshFlag = (MapMeshFlag)((int)enumerator.Current);
					if (mapMeshFlag != MapMeshFlag.None)
					{
						MapMeshFlagUtility.allFlags.Add(mapMeshFlag);
					}
				}
			}
		}
	}
}
