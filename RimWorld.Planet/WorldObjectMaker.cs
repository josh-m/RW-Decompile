using System;
using Verse;

namespace RimWorld.Planet
{
	public static class WorldObjectMaker
	{
		public static WorldObject MakeWorldObject(WorldObjectDef def)
		{
			WorldObject worldObject = (WorldObject)Activator.CreateInstance(def.worldObjectClass);
			worldObject.def = def;
			worldObject.ID = Find.UniqueIDsManager.GetNextWorldObjectID();
			worldObject.creationGameTicks = Find.TickManager.TicksGame;
			worldObject.PostMake();
			return worldObject;
		}
	}
}
