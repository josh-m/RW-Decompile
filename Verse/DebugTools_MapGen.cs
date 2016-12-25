using System;
using System.Collections.Generic;

namespace Verse
{
	public static class DebugTools_MapGen
	{
		public static List<DebugMenuOption> Options_Scatterers()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Type current in typeof(GenStep_Scatterer).AllLeafSubclasses())
			{
				Type localSt = current;
				list.Add(new DebugMenuOption(localSt.ToString(), DebugMenuOptionMode.Tool, delegate
				{
					GenStep_Scatterer genStep_Scatterer = (GenStep_Scatterer)Activator.CreateInstance(localSt);
					genStep_Scatterer.DebugForceScatterAt(UI.MouseCell());
				}));
			}
			return list;
		}
	}
}
