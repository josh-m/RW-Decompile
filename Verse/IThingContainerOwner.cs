using System;

namespace Verse
{
	public interface IThingContainerOwner
	{
		bool Spawned
		{
			get;
		}

		ThingContainer GetContainer();

		IntVec3 GetPosition();
	}
}
