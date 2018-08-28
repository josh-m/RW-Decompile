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

		public override IEnumerable<DiaOption> Choices
		{
			get
			{
				if (base.ArchivedOnly)
				{
					yield return base.Option_Close;
				}
				else
				{
					DiaOption accept = new DiaOption("RansomDemand_Accept".Translate());
					accept.action = delegate
					{
						this.$this.faction.kidnapped.RemoveKidnappedPawn(this.$this.kidnapped);
						Find.WorldPawns.RemovePawn(this.$this.kidnapped);
						IntVec3 intVec;
						if (this.$this.faction.def.techLevel < TechLevel.Industrial)
						{
							if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(this.$this.map) && this.$this.map.reachability.CanReachColony(c), this.$this.map, CellFinder.EdgeRoadChance_Friendly, out intVec) && !CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(this.$this.map), this.$this.map, CellFinder.EdgeRoadChance_Friendly, out intVec))
							{
								Log.Warning("Could not find any edge cell.", false);
								intVec = DropCellFinder.TradeDropSpot(this.$this.map);
							}
							GenSpawn.Spawn(this.$this.kidnapped, intVec, this.$this.map, WipeMode.Vanish);
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
					yield return base.Option_Reject;
					yield return base.Option_Postpone;
				}
			}
		}

		public override bool CanShowInLetterStack
		{
			get
			{
				return base.CanShowInLetterStack && Find.Maps.Contains(this.map) && this.faction.kidnapped.KidnappedPawnsListForReading.Contains(this.kidnapped);
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
