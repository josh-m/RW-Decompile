using System;
using Verse;

namespace RimWorld.BaseGen
{
	public class GlobalSettings
	{
		public Map map;

		public void Clear()
		{
			this.map = null;
		}
	}
}
