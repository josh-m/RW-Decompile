using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Mineable : Building
	{
		private int nonMiningDamageTaken;

		public override void PreApplyDamage(DamageInfo damage, out bool absorbed)
		{
			base.PreApplyDamage(damage, out absorbed);
			if (absorbed)
			{
				return;
			}
			if (damage.Def.harmsHealth && damage.Def != DamageDefOf.Mining)
			{
				int num = Math.Min(damage.Amount, this.HitPoints);
				this.nonMiningDamageTaken += num;
			}
			absorbed = false;
		}

		public override void Destroy(DestroyMode mode)
		{
			Map map = base.Map;
			base.Destroy(mode);
			if (mode == DestroyMode.Kill)
			{
				if (this.def.building.soundMined != null)
				{
					this.def.building.soundMined.PlayOneShot(new TargetInfo(base.Position, map, false));
				}
				if (this.def.building.mineableThing != null && Rand.Value < this.def.building.mineableDropChance)
				{
					Thing thing = ThingMaker.MakeThing(this.def.building.mineableThing, null);
					if (thing.def.stackLimit == 1)
					{
						thing.stackCount = 1;
					}
					else
					{
						float num = this.def.building.mineableNonMinedEfficiency + (1f - this.def.building.mineableNonMinedEfficiency) * (1f - (float)this.nonMiningDamageTaken / (float)base.MaxHitPoints);
						thing.stackCount = Mathf.CeilToInt((float)this.def.building.mineableYield * num);
					}
					GenSpawn.Spawn(thing, base.Position, map);
				}
			}
		}
	}
}
