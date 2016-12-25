using System;
using Verse;

namespace RimWorld
{
	public class PawnGroupMakerParms
	{
		public Map map;

		public float points;

		public Faction faction;

		public TraderKindDef traderKind;

		public bool generateFightersOnly;

		public bool generateMeleeOnly;

		public RaidStrategyDef raidStrategy;

		public bool forceOneIncap;

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"map=",
				this.map,
				", points=",
				this.points,
				", faction=",
				this.faction,
				", traderKind=",
				this.traderKind,
				", generateFightersOnly=",
				this.generateFightersOnly,
				", generateMeleeOnly=",
				this.generateMeleeOnly,
				", raidStrategy=",
				this.raidStrategy,
				", forceOneIncap=",
				this.forceOneIncap
			});
		}
	}
}
