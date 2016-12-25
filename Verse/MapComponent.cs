using System;

namespace Verse
{
	public class MapComponent : IExposable
	{
		public Map map;

		public MapComponent(Map map)
		{
			this.map = map;
		}

		public virtual void MapComponentUpdate()
		{
		}

		public virtual void MapComponentTick()
		{
		}

		public virtual void MapComponentOnGUI()
		{
		}

		public virtual void ExposeData()
		{
		}
	}
}
