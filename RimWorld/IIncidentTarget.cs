using System;
using System.Collections.Generic;
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

		float PlayerWealthForStoryteller
		{
			get;
		}

		IEnumerable<Pawn> FreeColonistsForStoryteller
		{
			get;
		}

		FloatRange IncidentPointsRandomFactorRange
		{
			get;
		}

		IEnumerable<IncidentTargetTypeDef> AcceptedTypes();
	}
}
