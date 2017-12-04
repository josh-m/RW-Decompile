using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Mineable : Building
	{
		private float yieldPct;

		private const float YieldChanceOnNonMiningKill = 0.2f;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.yieldPct, "yieldPct", 0f, false);
		}

		public override void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			base.PreApplyDamage(dinfo, out absorbed);
			if (absorbed)
			{
				return;
			}
			if (this.def.building.mineableThing != null && this.def.building.mineableYieldWasteable && dinfo.Def == DamageDefOf.Mining && dinfo.Instigator != null && dinfo.Instigator is Pawn)
			{
				this.Notify_TookMiningDamage(dinfo.Amount, (Pawn)dinfo.Instigator);
			}
			absorbed = false;
		}

		public void DestroyMined(Pawn pawn)
		{
			Map map = base.Map;
			base.Destroy(DestroyMode.KillFinalize);
			this.TrySpawnYield(map, this.yieldPct, true);
		}

		public override void Destroy(DestroyMode mode)
		{
			Map map = base.Map;
			base.Destroy(mode);
			if (mode == DestroyMode.KillFinalize)
			{
				this.TrySpawnYield(map, 0.2f, false);
			}
		}

		private void TrySpawnYield(Map map, float yieldChance, bool moteOnWaste)
		{
			if (this.def.building.mineableThing == null)
			{
				return;
			}
			if (Rand.Value > this.def.building.mineableDropChance)
			{
				return;
			}
			int num = this.def.building.mineableYield;
			if (this.def.building.mineableYieldWasteable)
			{
				num = Mathf.Max(1, GenMath.RoundRandom((float)num * this.yieldPct));
			}
			Thing thing = ThingMaker.MakeThing(this.def.building.mineableThing, null);
			thing.stackCount = num;
			GenSpawn.Spawn(thing, base.Position, map);
		}

		public void Notify_TookMiningDamage(int amount, Pawn miner)
		{
			int num = Mathf.Min(amount, this.HitPoints);
			float num2 = (float)num / (float)base.MaxHitPoints;
			this.yieldPct += num2 * miner.GetStatValue(StatDefOf.MiningYield, true);
		}
	}
}
