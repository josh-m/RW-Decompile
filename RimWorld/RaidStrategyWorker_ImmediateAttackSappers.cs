using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class RaidStrategyWorker_ImmediateAttackSappers : RaidStrategyWorker
	{
		public override float SelectionChance
		{
			get
			{
				float num = base.SelectionChance;
				float strengthRating = Find.StoryWatcher.watcherStrength.StrengthRating;
				if (strengthRating > 10f)
				{
					float num2 = 1f + Mathf.Clamp01(Mathf.InverseLerp(10f, 30f, strengthRating));
					num *= num2;
				}
				return num;
			}
		}

		public override bool CanUseWith(IncidentParms parms)
		{
			return parms.faction.def.humanlikeFaction && parms.faction.def.techLevel >= TechLevel.Industrial && this.PawnGenOptionsWithSappers(parms.faction).Any<PawnGroupMaker_Normal>() && base.CanUseWith(parms);
		}

		public override float MinimumPoints(Faction faction)
		{
			return this.CheapestSapperCost(faction);
		}

		public override float MinMaxAllowedPawnGenOptionCost(Faction faction)
		{
			return this.CheapestSapperCost(faction);
		}

		private float CheapestSapperCost(Faction faction)
		{
			IEnumerable<PawnGroupMaker_Normal> enumerable = this.PawnGenOptionsWithSappers(faction);
			if (!enumerable.Any<PawnGroupMaker_Normal>())
			{
				Log.Error(string.Concat(new string[]
				{
					"Tried to get MinimumPoints for ",
					base.GetType().ToString(),
					" for faction ",
					faction.ToString(),
					" but the faction has no groups with sappers."
				}));
				return 99999f;
			}
			float num = 9999999f;
			foreach (PawnGroupMaker_Normal current in enumerable)
			{
				foreach (PawnGenOption current2 in from op in current.options
				where RaidStrategyWorker_ImmediateAttackSappers.CanBeSapper(op.kind)
				select op)
				{
					if (current2.Cost < num)
					{
						num = current2.Cost;
					}
				}
			}
			return num;
		}

		public override bool CanUsePawnGenOption(PawnGenOption opt, List<PawnGenOption> chosenOpts)
		{
			return chosenOpts.Count != 0 || (opt.kind.weaponTags.Count == 1 && RaidStrategyWorker_ImmediateAttackSappers.CanBeSapper(opt.kind));
		}

		private IEnumerable<PawnGroupMaker_Normal> PawnGenOptionsWithSappers(Faction faction)
		{
			return from gm in faction.def.pawnGroupMakers.OfType<PawnGroupMaker_Normal>()
			where gm.options.Any((PawnGenOption op) => RaidStrategyWorker_ImmediateAttackSappers.CanBeSapper(op.kind))
			select gm;
		}

		public static bool CanBeSapper(PawnKindDef kind)
		{
			return !kind.weaponTags.NullOrEmpty<string>() && kind.weaponTags[0] == "GrenadeDestructive";
		}

		public override LordJob MakeLordJob(ref IncidentParms parms)
		{
			return new LordJob_AssaultColony(parms.faction, true, true, true, true, true);
		}
	}
}
