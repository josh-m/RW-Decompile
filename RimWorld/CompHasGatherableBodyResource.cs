using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class CompHasGatherableBodyResource : ThingComp
	{
		protected float fullness;

		protected abstract int GatherResourcesIntervalDays
		{
			get;
		}

		protected abstract int ResourceAmount
		{
			get;
		}

		protected abstract ThingDef ResourceDef
		{
			get;
		}

		protected abstract string SaveKey
		{
			get;
		}

		public float Fullness
		{
			get
			{
				return this.fullness;
			}
		}

		protected virtual bool Active
		{
			get
			{
				return this.parent.Faction != null;
			}
		}

		public bool ActiveAndFull
		{
			get
			{
				return this.Active && this.fullness >= 1f;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.LookValue<float>(ref this.fullness, this.SaveKey, 0f, false);
		}

		public override void CompTick()
		{
			if (this.Active)
			{
				float num = 1f / (float)(this.GatherResourcesIntervalDays * 60000);
				Pawn pawn = this.parent as Pawn;
				if (pawn != null)
				{
					num *= PawnUtility.BodyResourceGrowthSpeed(pawn);
				}
				this.fullness += num;
				if (this.fullness > 1f)
				{
					this.fullness = 1f;
				}
			}
		}

		public void Gathered(Pawn doer)
		{
			if (!this.Active)
			{
				Log.Error(doer + " gathered body resources while not Active: " + this.parent);
			}
			int i = GenMath.RoundRandom((float)this.ResourceAmount * this.fullness);
			while (i > 0)
			{
				int num = Mathf.Clamp(i, 1, this.ResourceDef.stackLimit);
				i -= num;
				Thing thing = ThingMaker.MakeThing(this.ResourceDef, null);
				thing.stackCount = num;
				GenPlace.TryPlaceThing(thing, doer.Position, doer.Map, ThingPlaceMode.Near, null);
			}
			this.fullness = 0f;
		}
	}
}
