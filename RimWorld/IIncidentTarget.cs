using System;
using Verse;

namespace RimWorld
{
	public interface IIncidentTarget : ILoadReferenceable
	{
		int Tile
		{
			get;
		}

		StoryState StoryState
		{
			get;
		}

		GameConditionManager GameConditionManager
		{
			get;
		}

		IncidentTargetType Type
		{
			get;
		}
	}
}
