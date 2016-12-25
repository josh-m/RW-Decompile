using System;
using UnityEngine;

namespace Verse
{
	public class DamageFlasher
	{
		private const int DamagedMatTicksTotal = 16;

		private int lastDamageTick = -9999;

		private int DamageFlashTicksLeft
		{
			get
			{
				return this.lastDamageTick + 16 - Find.TickManager.TicksGame;
			}
		}

		public bool FlashingNowOrRecently
		{
			get
			{
				return this.DamageFlashTicksLeft >= -1;
			}
		}

		public DamageFlasher(Pawn pawn)
		{
		}

		public Material GetDamagedMat(Material baseMat)
		{
			return DamagedMatPool.GetDamageFlashMat(baseMat, (float)this.DamageFlashTicksLeft / 16f);
		}

		public void Notify_DamageApplied(DamageInfo dinfo)
		{
			if (dinfo.Def.harmsHealth)
			{
				this.lastDamageTick = Find.TickManager.TicksGame;
			}
		}
	}
}
