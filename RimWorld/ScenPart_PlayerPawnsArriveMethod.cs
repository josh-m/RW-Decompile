using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_PlayerPawnsArriveMethod : ScenPart
	{
		private PlayerPawnsArriveMethod method;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<PlayerPawnsArriveMethod>(ref this.method, "method", PlayerPawnsArriveMethod.Standing, false);
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight);
			if (Widgets.ButtonText(scenPartRect, this.method.ToStringHuman(), true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				using (IEnumerator enumerator = Enum.GetValues(typeof(PlayerPawnsArriveMethod)).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						PlayerPawnsArriveMethod localM2 = (PlayerPawnsArriveMethod)((int)enumerator.Current);
						PlayerPawnsArriveMethod localM = localM2;
						list.Add(new FloatMenuOption(localM.ToStringHuman(), delegate
						{
							this.method = localM;
						}, MenuOptionPriority.Default, null, null, 0f, null, null));
					}
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}

		public override string Summary(Scenario scen)
		{
			if (this.method == PlayerPawnsArriveMethod.DropPods)
			{
				return "ScenPart_ArriveInDropPods".Translate();
			}
			return null;
		}

		public override void Randomize()
		{
			this.method = ((Rand.Value >= 0.5f) ? PlayerPawnsArriveMethod.Standing : PlayerPawnsArriveMethod.DropPods);
		}

		public override void GenerateIntoMap(Map map)
		{
			List<List<Thing>> list = new List<List<Thing>>();
			foreach (Pawn current in Find.GameInitData.startingPawns)
			{
				list.Add(new List<Thing>
				{
					current
				});
			}
			List<Thing> list2 = new List<Thing>();
			foreach (ScenPart current2 in Find.Scenario.AllParts)
			{
				list2.AddRange(current2.PlayerStartingThings());
			}
			int num = 0;
			foreach (Thing current3 in list2)
			{
				if (current3.def.CanHaveFaction)
				{
					current3.SetFactionDirect(Faction.OfPlayer);
				}
				list[num].Add(current3);
				num++;
				if (num >= list.Count)
				{
					num = 0;
				}
			}
			bool instaDrop = Find.GameInitData.QuickStarted || this.method != PlayerPawnsArriveMethod.DropPods;
			DropPodUtility.DropThingGroupsNear(MapGenerator.PlayerStartSpot, map, list, 110, instaDrop, true, true);
		}

		public override void PostMapGenerate(Map map)
		{
			if (this.method == PlayerPawnsArriveMethod.DropPods)
			{
				PawnUtility.GiveAllStartingPlayerPawnsThought(ThoughtDefOf.CrashedTogether);
			}
		}
	}
}
