using System;
using Verse;

namespace RimWorld
{
	public class CompSpawnerFilth : ThingComp
	{
		private CompProperties_SpawnerFilth Props
		{
			get
			{
				return (CompProperties_SpawnerFilth)this.props;
			}
		}

		public override void PostSpawnSetup()
		{
			for (int i = 0; i < this.Props.spawnCountOnSpawn; i++)
			{
				this.TrySpawnFilth();
			}
		}

		public override void CompTickRare()
		{
			Hive hive = this.parent as Hive;
			if ((hive == null || hive.active) && Rand.MTBEventOccurs(this.Props.spawnMtbHours, 2500f, 250f))
			{
				this.TrySpawnFilth();
			}
		}

		public void TrySpawnFilth()
		{
			IntVec3 c;
			if (!CellFinder.TryFindRandomReachableCellNear(this.parent.Position, this.parent.Map, this.Props.spawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 x) => x.Standable(this.parent.Map), (Region x) => true, out c, 999999))
			{
				return;
			}
			FilthMaker.MakeFilth(c, this.parent.Map, ThingDefOf.FilthSlime, 1);
		}
	}
}
