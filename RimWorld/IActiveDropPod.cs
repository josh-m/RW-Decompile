using System;
using Verse;

namespace RimWorld
{
	public interface IActiveDropPod : IThingContainerOwner
	{
		ActiveDropPodInfo Contents
		{
			get;
		}
	}
}
