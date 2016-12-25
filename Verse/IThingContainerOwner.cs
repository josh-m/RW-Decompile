using System;

namespace Verse
{
	public interface IThingContainerOwner
	{
		bool Spawned
		{
			get;
		}

		Map GetMap();

		ThingContainer GetInnerContainer();

		IntVec3 GetPosition();
	}
}
