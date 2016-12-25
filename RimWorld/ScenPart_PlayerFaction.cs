using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_PlayerFaction : ScenPart
	{
		internal FactionDef factionDef;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.LookDef<FactionDef>(ref this.factionDef, "factionDef");
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight);
			if (Widgets.ButtonText(scenPartRect, this.factionDef.LabelCap, true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (FactionDef current in from d in DefDatabase<FactionDef>.AllDefs
				where d.isPlayer
				select d)
				{
					FactionDef localFd = current;
					list.Add(new FloatMenuOption(localFd.LabelCap, delegate
					{
						this.factionDef = localFd;
					}, MenuOptionPriority.Medium, null, null, 0f, null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}

		public override string Summary(Scenario scen)
		{
			return "ScenPart_PlayerFaction".Translate(new object[]
			{
				this.factionDef.label
			});
		}

		public override void Randomize()
		{
			this.factionDef = (from fd in DefDatabase<FactionDef>.AllDefs
			where fd.isPlayer
			select fd).RandomElement<FactionDef>();
		}

		public override void PostWorldLoad()
		{
			Find.GameInitData.playerFaction = FactionGenerator.NewGeneratedFaction(this.factionDef);
		}

		public override void PreMapGenerate()
		{
			Find.GameInitData.playerFaction.homeSquare = Find.GameInitData.startingCoords;
			Find.FactionManager.Add(Find.GameInitData.playerFaction);
			FactionGenerator.EnsureRequiredEnemies(Find.GameInitData.playerFaction);
		}

		public override void PostGameStart()
		{
			Find.GameInitData.playerFaction = null;
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			if (this.factionDef == null)
			{
				yield return "factionDef is null";
			}
		}
	}
}
