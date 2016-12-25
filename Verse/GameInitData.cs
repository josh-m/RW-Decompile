using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class GameInitData
	{
		public IntVec2 startingCoords;

		public int mapSize = 250;

		public List<Pawn> startingPawns = new List<Pawn>();

		public Faction playerFaction;

		public Month startingMonth = Month.Undefined;

		public bool permadeath;

		public bool startedFromEntry;

		public string mapToLoad;

		public bool QuickStarted
		{
			get
			{
				return this.mapToLoad.NullOrEmpty() && !this.startedFromEntry;
			}
		}

		public void ChooseRandomStartingWorldSquare()
		{
			this.startingCoords = WorldSquareFinder.RandomStartingWorldSquare();
		}

		public void ResetWorldRelatedMapInitData()
		{
			Current.Game.World = null;
			Find.GameInitData.startingPawns.Clear();
			Find.GameInitData.playerFaction = null;
			Find.GameInitData.startingCoords = IntVec2.Invalid;
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"startedFromEntry: ",
				this.startedFromEntry,
				"\nstartingPawns: ",
				this.startingPawns.Count
			});
		}
	}
}
