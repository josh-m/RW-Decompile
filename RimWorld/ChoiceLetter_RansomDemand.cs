using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ChoiceLetter_RansomDemand : ChoiceLetter
	{
		public Map map;

		public Faction faction;

		public Pawn kidnapped;

		public int fee;

		protected override IEnumerable<DiaOption> Choices
		{
			get
			{
				DiaOption accept = new DiaOption("RansomDemand_Accept".Translate());
				accept.action = delegate
				{
					this.$this.faction.kidnapped.RemoveKidnappedPawn(this.$this.kidnapped);
					Find.WorldPawns.RemovePawn(this.$this.kidnapped);
					IntVec3 intVec;
					if (this.$this.faction.def.techLevel < TechLevel.Spacer)
					{
						if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(this.$this.map) && this.$this.map.reachability.CanReachColony(c), this.$this.map, CellFinder.EdgeRoadChance_Friendly, out intVec) && !CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(this.$this.map), this.$this.map, CellFinder.EdgeRoadChance_Friendly, out intVec))
						{
							Log.Warning("Could not find any edge cell.");
							intVec = DropCellFinder.TradeDropSpot(this.$this.map);
						}
						GenSpawn.Spawn(this.$this.kidnapped, intVec, this.$this.map);
					}
					else
					{
						intVec = DropCellFinder.TradeDropSpot(this.$this.map);
						TradeUtility.SpawnDropPod(intVec, this.$this.map, this.$this.kidnapped);
					}
					CameraJumper.TryJump(intVec, this.$this.map);
					TradeUtility.LaunchSilver(this.$this.map, this.$this.fee);
					Find.LetterStack.RemoveLetter(this.$this);
				};
				accept.resolveTree = true;
				if (!TradeUtility.ColonyHasEnoughSilver(this.map, this.fee))
				{
					accept.Disable("NeedSilverLaunchable".Translate(new object[]
					{
						this.fee.ToString()
					}));
				}
				yield return accept;
				yield return base.Reject;
				yield return base.Postpone;
			}
		}

		public override bool StillValid
		{
			get
			{
				return base.StillValid && Find.Maps.Contains(this.map) && this.faction.kidnapped.KidnappedPawnsListForReading.Contains(this.kidnapped);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Map>(ref this.map, "map", false);
			Scribe_References.Look<Faction>(ref this.faction, "faction", false);
			Scribe_References.Look<Pawn>(ref this.kidnapped, "kidnapped", false);
			Scribe_Values.Look<int>(ref this.fee, "fee", 0, false);
		}
	}
}
