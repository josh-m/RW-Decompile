using RimWorld;
using System;

namespace Verse
{
	public class HediffComp
	{
		public HediffWithComps parent;

		public HediffCompProperties props;

		public Pawn Pawn
		{
			get
			{
				return this.parent.pawn;
			}
		}

		public HediffDef Def
		{
			get
			{
				return this.parent.def;
			}
		}

		public virtual string CompLabelInBracketsExtra
		{
			get
			{
				return null;
			}
		}

		public virtual string CompTipStringExtra
		{
			get
			{
				return null;
			}
		}

		public virtual TextureAndColor CompStateIcon
		{
			get
			{
				return TextureAndColor.None;
			}
		}

		public virtual bool CompShouldRemove
		{
			get
			{
				return false;
			}
		}

		public virtual void CompPostMake()
		{
		}

		public virtual void CompPostTick()
		{
		}

		public virtual void CompExposeData()
		{
		}

		public virtual void CompPostPostAdd(DamageInfo? dinfo)
		{
		}

		public virtual void CompPostMerged(Hediff other)
		{
		}

		public virtual bool CompDisallowVisible()
		{
			return false;
		}

		public virtual void CompModifyChemicalEffect(ChemicalDef chem, ref float effect)
		{
		}

		public virtual void CompPostInjuryHeal(float amount)
		{
		}

		public virtual void CompTended(float quality, int batchPosition = 0)
		{
		}

		public virtual void Notify_PawnDied()
		{
		}

		public virtual string CompDebugString()
		{
			return null;
		}
	}
}
