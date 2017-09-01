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
					this.<>f__this.faction.kidnapped.RemoveKidnappedPawn(this.<>f__this.kidnapped);
					Find.WorldPawns.RemovePawn(this.<>f__this.kidnapped);
					IntVec3 intVec;
					if (this.<>f__this.faction.def.techLevel < TechLevel.Spacer)
					{
						if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(this.<>f__this.map) && this.<>f__this.map.reachability.CanReachColony(c), this.<>f__this.map, CellFinder.EdgeRoadChance_Friendly, out intVec) && !CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(this.<>f__this.map), this.<>f__this.map, CellFinder.EdgeRoadChance_Friendly, out intVec))
						{
							Log.Warning("Could not find any edge cell.");
							intVec = DropCellFinder.TradeDropSpot(this.<>f__this.map);
						}
						GenSpawn.Spawn(this.<>f__this.kidnapped, intVec, this.<>f__this.map);
					}
					else
					{
						intVec = DropCellFinder.TradeDropSpot(this.<>f__this.map);
						TradeUtility.SpawnDropPod(intVec, this.<>f__this.map, this.<>f__this.kidnapped);
					}
					CameraJumper.TryJump(intVec, this.<>f__this.map);
					TradeUtility.LaunchSilver(this.<>f__this.map, this.<>f__this.fee);
					Find.LetterStack.RemoveLetter(this.<>f__this);
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
