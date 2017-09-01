using System;
using Verse;

namespace RimWorld
{
	public class IncidentParms : IExposable
	{
		public IIncidentTarget target;

		public float points = -1f;

		public IntVec3 spawnCenter = IntVec3.Invalid;

		public Rot4 spawnRotation = Rot4.South;

		public Faction faction;

		public bool forced;

		public TraderKindDef traderKind;

		public bool generateFightersOnly;

		public RaidStrategyDef raidStrategy;

		public PawnsArriveMode raidArrivalMode;

		public int raidPodOpenDelay = 140;

		public bool raidForceOneIncap;

		public bool raidNeverFleeIndividual;

		public void ExposeData()
		{
			Scribe_References.Look<IIncidentTarget>(ref this.target, "target", false);
			Scribe_Values.Look<float>(ref this.points, "threatPoints", 0f, false);
			Scribe_Values.Look<IntVec3>(ref this.spawnCenter, "spawnCenter", default(IntVec3), false);
			Scribe_References.Look<Faction>(ref this.faction, "faction", false);
			Scribe_Values.Look<bool>(ref this.forced, "forced", false, false);
			Scribe_Defs.Look<TraderKindDef>(ref this.traderKind, "traderKind");
			Scribe_Values.Look<bool>(ref this.generateFightersOnly, "generateFightersOnly", false, false);
			Scribe_Defs.Look<RaidStrategyDef>(ref this.raidStrategy, "raidStrategy");
			Scribe_Values.Look<PawnsArriveMode>(ref this.raidArrivalMode, "raidArrivalMode", PawnsArriveMode.Undecided, false);
			Scribe_Values.Look<int>(ref this.raidPodOpenDelay, "raidPodOpenDelay", 140, false);
			Scribe_Values.Look<bool>(ref this.raidForceOneIncap, "raidForceIncap", false, false);
		}

		public override string ToString()
		{
			string text = "(";
			if (this.target != null)
			{
				string text2 = text;
				text = string.Concat(new object[]
				{
					text2,
					"target=",
					this.target,
					" "
				});
			}
			if (this.points >= 0f)
			{
				string text2 = text;
				text = string.Concat(new object[]
				{
					text2,
					"points=",
					this.points,
					" "
				});
			}
			if (this.generateFightersOnly)
			{
				string text2 = text;
				text = string.Concat(new object[]
				{
					text2,
					"generateFightersOnly=",
					this.generateFightersOnly,
					" "
				});
			}
			if (this.raidStrategy != null)
			{
				text = text + "raidStrategy=" + this.raidStrategy.defName + " ";
			}
			return text + ")";
		}
	}
}
