using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class GameInitData
	{
		public int startingTile = -1;

		public int mapSize = 250;

		public List<Pawn> startingAndOptionalPawns = new List<Pawn>();

		public int startingPawnCount = -1;

		public Faction playerFaction;

		public Season startingSeason;

		public bool permadeathChosen;

		public bool permadeath;

		public bool startedFromEntry;

		public string gameToLoad;

		public const int DefaultMapSize = 250;

		public bool QuickStarted
		{
			get
			{
				return this.gameToLoad.NullOrEmpty() && !this.startedFromEntry;
			}
		}

		public void ChooseRandomStartingTile()
		{
			this.startingTile = TileFinder.RandomStartingTile();
		}

		public void ResetWorldRelatedMapInitData()
		{
			Current.Game.World = null;
			this.startingAndOptionalPawns.Clear();
			this.playerFaction = null;
			this.startingTile = -1;
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"startedFromEntry: ",
				this.startedFromEntry,
				"\nstartingAndOptionalPawns: ",
				this.startingAndOptionalPawns.Count
			});
		}

		public void PrepForMapGen()
		{
			while (this.startingAndOptionalPawns.Count > this.startingPawnCount)
			{
				PawnComponentsUtility.RemoveComponentsOnDespawned(this.startingAndOptionalPawns[this.startingPawnCount]);
				Find.WorldPawns.PassToWorld(this.startingAndOptionalPawns[this.startingPawnCount], PawnDiscardDecideMode.KeepForever);
				this.startingAndOptionalPawns.RemoveAt(this.startingPawnCount);
			}
			List<Pawn> list = this.startingAndOptionalPawns;
			foreach (Pawn current in list)
			{
				current.SetFactionDirect(Faction.OfPlayer);
				PawnComponentsUtility.AddAndRemoveDynamicComponents(current, false);
			}
			foreach (Pawn current2 in list)
			{
				current2.workSettings.DisableAll();
			}
			foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
			{
				if (w.alwaysStartActive)
				{
					foreach (Pawn current3 in from col in list
					where !col.story.WorkTypeIsDisabled(w)
					select col)
					{
						current3.workSettings.SetPriority(w, 3);
					}
				}
				else
				{
					bool flag = false;
					foreach (Pawn current4 in list)
					{
						if (!current4.story.WorkTypeIsDisabled(w) && current4.skills.AverageOfRelevantSkillsFor(w) >= 6f)
						{
							current4.workSettings.SetPriority(w, 3);
							flag = true;
						}
					}
					if (!flag)
					{
						IEnumerable<Pawn> source = from col in list
						where !col.story.WorkTypeIsDisabled(w)
						select col;
						if (source.Any<Pawn>())
						{
							Pawn pawn = source.InRandomOrder(null).MaxBy((Pawn c) => c.skills.AverageOfRelevantSkillsFor(w));
							pawn.workSettings.SetPriority(w, 3);
						}
					}
				}
			}
		}
	}
}
