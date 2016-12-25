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
	}
}
