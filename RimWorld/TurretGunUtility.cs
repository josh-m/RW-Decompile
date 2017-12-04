using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class TurretGunUtility
	{
		public static bool NeedsShells(ThingDef turret)
		{
			return turret.category == ThingCategory.Building && turret.building.IsTurret && turret.building.turretGunDef.HasComp(typeof(CompChangeableProjectile));
		}

		public static ThingDef TryFindRandomShellDef(ThingDef turret, bool allowEMP = true, bool mustHarmHealth = true, TechLevel techLevel = TechLevel.Undefined, bool allowAntigrainWarhead = false, float maxMarketValue = -1f)
		{
			if (!TurretGunUtility.NeedsShells(turret))
			{
				return null;
			}
			ThingFilter fixedFilter = turret.building.turretGunDef.building.fixedStorageSettings.filter;
			ThingDef result;
			if ((from x in DefDatabase<ThingDef>.AllDefsListForReading
			where fixedFilter.Allows(x) && (allowEMP || x.projectileWhenLoaded.projectile.damageDef != DamageDefOf.EMP) && (!mustHarmHealth || x.projectileWhenLoaded.projectile.damageDef.harmsHealth) && (techLevel == TechLevel.Undefined || x.techLevel <= techLevel) && (allowAntigrainWarhead || x != ThingDefOf.Shell_AntigrainWarhead) && (maxMarketValue < 0f || x.BaseMarketValue <= maxMarketValue)
			select x).TryRandomElement(out result))
			{
				return result;
			}
			return null;
		}
	}
}
