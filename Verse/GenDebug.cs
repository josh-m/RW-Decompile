using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class GenDebug
	{
		public static void DebugPlaceSphere(Vector3 Loc, float Scale)
		{
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			gameObject.transform.position = Loc;
			gameObject.transform.localScale = new Vector3(Scale, Scale, Scale);
		}

		public static void LogList<T>(IEnumerable<T> list)
		{
			foreach (T current in list)
			{
				Log.Message("    " + current.ToString());
			}
		}

		public static void ClearArea(CellRect r, Map map)
		{
			r.ClipInsideMap(map);
			foreach (IntVec3 current in r)
			{
				map.roofGrid.SetRoof(current, null);
			}
			foreach (IntVec3 current2 in r)
			{
				foreach (Thing current3 in current2.GetThingList(map).ToList<Thing>())
				{
					if (current3.def.destroyable)
					{
						current3.Destroy(DestroyMode.Vanish);
					}
				}
			}
		}
	}
}
