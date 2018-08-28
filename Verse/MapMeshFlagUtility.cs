using System;
using System.Collections.Generic;

namespace Verse
{
	public static class MapMeshFlagUtility
	{
		public static List<MapMeshFlag> allFlags;

		static MapMeshFlagUtility()
		{
			MapMeshFlagUtility.allFlags = new List<MapMeshFlag>();
			foreach (MapMeshFlag mapMeshFlag in Enum.GetValues(typeof(MapMeshFlag)))
			{
				if (mapMeshFlag != MapMeshFlag.None)
				{
					MapMeshFlagUtility.allFlags.Add(mapMeshFlag);
				}
			}
		}
	}
}
