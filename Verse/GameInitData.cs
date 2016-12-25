using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class GameInitData
	{
		public const int DefaultMapSize = 250;

		public int startingTile = -1;

		public int mapSize = 250;

		public List<Pawn> startingPawns = new List<Pawn>();

		public Faction playerFaction;

		public Month startingMonth = Month.Undefined;

		public bool permadeath;

		public bool startedFromEntry;

		public string gameToLoad;

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
			this.startingPawns.Clear();
			this.playerFaction = null;
			this.startingTile = -1;
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

		public void PrepForMapGen()
		{
			foreach (Pawn current in this.startingPawns)
			{
				current.SetFactionDirect(Faction.OfPlayer);
				PawnComponentsUtility.AddAndRemoveDynamicComponents(current, false);
			}
			foreach (Pawn current2 in this.startingPawns)
			{
				current2.workSettings.DisableAll();
			}
			foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
			{
				if (w.alwaysStartActive)
				{
					foreach (Pawn current3 in from col in this.startingPawns
					where !col.story.WorkTypeIsDisabled(w)
					select col)
					{
						current3.workSettings.SetPriority(w, 3);
					}
				}
				else
				{
					bool flag = false;
					foreach (Pawn current4 in this.startingPawns)
					{
						if (!current4.story.WorkTypeIsDisabled(w) && current4.skills.AverageOfRelevantSkillsFor(w) >= 6f)
						{
							current4.workSettings.SetPriority(w, 3);
							flag = true;
						}
					}
					if (!flag)
					{
						IEnumerable<Pawn> source = from col in this.startingPawns
						where !col.story.WorkTypeIsDisabled(w)
						select col;
						if (source.Any<Pawn>())
						{
							Pawn pawn = source.InRandomOrder(null).MaxBy((Pawn c) => c.skills.AverageOfRelevantSkillsFor(w));
							pawn.workSettings.SetPriority(w, 3);
						}
						else if (w.requireCapableColonist)
						{
							Log.Error("No colonist could do requireCapableColonist work type " + w);
						}
					}
				}
			}
		}
	}
}
