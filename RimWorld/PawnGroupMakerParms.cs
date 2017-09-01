using System;

namespace RimWorld
{
	public class PawnGroupMakerParms
	{
		public int tile = -1;

		public bool inhabitants;

		public float points;

		public Faction faction;

		public TraderKindDef traderKind;

		public bool generateFightersOnly;

		public RaidStrategyDef raidStrategy;

		public bool forceOneIncap;

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"tile=",
				this.tile,
				", inhabitants=",
				this.inhabitants,
				", points=",
				this.points,
				", faction=",
				this.faction,
				", traderKind=",
				this.traderKind,
				", generateFightersOnly=",
				this.generateFightersOnly,
				", raidStrategy=",
				this.raidStrategy,
				", forceOneIncap=",
				this.forceOneIncap
			});
		}
	}
}
