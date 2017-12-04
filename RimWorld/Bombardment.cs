using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Bombardment : OrbitalStrike
	{
		private bool anyExplosion;

		public const int EffectiveRadius = 23;

		private const int SingleExplosionRadius = 8;

		private const int ExplosionCenterRadius = 15;

		public const int RandomFireRadius = 25;

		private const float BombMTBRealSeconds = 0.375f;

		private const int StartRandomFireEveryTicks = 20;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.anyExplosion, "anyExplosion", false, false);
		}

		public override void StartStrike()
		{
			base.StartStrike();
			MoteMaker.MakeBombardmentMote(base.Position, base.Map);
		}

		public override void Tick()
		{
			base.Tick();
			if (base.Destroyed)
			{
				return;
			}
			if (Rand.MTBEventOccurs(0.375f, 60f, 1f))
			{
				this.CreateRandomExplosion();
			}
			if (Find.TickManager.TicksGame % 20 == 0)
			{
				this.StartRandomFire();
			}
			if (base.TicksPassed >= this.duration / 2 && !this.anyExplosion)
			{
				this.CreateRandomExplosion();
			}
		}

		private void CreateRandomExplosion()
		{
			IntVec3 intVec = (from x in GenRadial.RadialCellsAround(base.Position, 15f, true)
			where x.InBounds(base.Map)
			select x).RandomElement<IntVec3>();
			IntVec3 center = intVec;
			Map map = base.Map;
			float radius = 8f;
			DamageDef bomb = DamageDefOf.Bomb;
			Thing instigator = this.instigator;
			ThingDef def = this.def;
			ThingDef weaponDef = this.weaponDef;
			GenExplosion.DoExplosion(center, map, radius, bomb, instigator, -1, null, weaponDef, def, null, 0f, 1, false, null, 0f, 1, 0f, false);
			this.anyExplosion = true;
		}

		private void StartRandomFire()
		{
			IntVec3 c = (from x in GenRadial.RadialCellsAround(base.Position, 25f, true)
			where x.InBounds(base.Map)
			select x).RandomElement<IntVec3>();
			FireUtility.TryStartFireIn(c, base.Map, Rand.Range(0.1f, 0.925f));
		}
	}
}
