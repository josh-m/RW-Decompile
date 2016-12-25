using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class WorldDynamicDrawManager
	{
		private HashSet<WorldObject> drawObjects = new HashSet<WorldObject>();

		private bool drawingNow;

		public void RegisterDrawable(WorldObject o)
		{
			if (o.def.useDynamicDrawer)
			{
				if (this.drawingNow)
				{
					Log.Warning("Cannot register drawable " + o + " while drawing is in progress. WorldObjects shouldn't be spawned in Draw methods.");
				}
				this.drawObjects.Add(o);
			}
		}

		public void DeRegisterDrawable(WorldObject o)
		{
			if (o.def.useDynamicDrawer)
			{
				if (this.drawingNow)
				{
					Log.Warning("Cannot deregister drawable " + o + " while drawing is in progress. WorldObjects shouldn't be despawned in Draw methods.");
				}
				this.drawObjects.Remove(o);
			}
		}

		public void DrawDynamicWorldObjects()
		{
			this.drawingNow = true;
			try
			{
				foreach (WorldObject current in this.drawObjects)
				{
					try
					{
						if (!current.def.expandingIcon || ExpandableWorldObjectsUtility.TransitionPct < 1f)
						{
							current.Draw();
						}
					}
					catch (Exception ex)
					{
						Log.Error(string.Concat(new object[]
						{
							"Exception drawing ",
							current,
							": ",
							ex
						}));
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error("Exception drawing dynamic world objects: " + arg);
			}
			this.drawingNow = false;
		}
	}
}
