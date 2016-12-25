using System;
using Verse;

namespace RimWorld
{
	public abstract class Thought : IExposable
	{
		public Pawn pawn;

		public ThoughtDef def;

		public abstract int CurStageIndex
		{
			get;
		}

		public ThoughtStage CurStage
		{
			get
			{
				return this.def.stages[this.CurStageIndex];
			}
		}

		public virtual bool VisibleInNeedsTab
		{
			get
			{
				return this.CurStage.visible;
			}
		}

		public virtual string LabelCap
		{
			get
			{
				return this.CurStage.label.CapitalizeFirst();
			}
		}

		protected virtual float BaseMoodOffset
		{
			get
			{
				return this.CurStage.baseMoodEffect;
			}
		}

		public string LabelCapSocial
		{
			get
			{
				if (this.CurStage.labelSocial != null)
				{
					return this.CurStage.labelSocial.CapitalizeFirst();
				}
				return this.LabelCap;
			}
		}

		public string Description
		{
			get
			{
				string description = this.CurStage.description;
				if (description != null)
				{
					return description;
				}
				return this.def.description;
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.LookDef<ThoughtDef>(ref this.def, "def");
		}

		public virtual float MoodOffset()
		{
			if (this.CurStage == null)
			{
				Log.Error(string.Concat(new object[]
				{
					"CurStage is null while ShouldDiscard is false on ",
					this.def.defName,
					" for ",
					this.pawn
				}));
				return 0f;
			}
			float num = this.BaseMoodOffset;
			if (this.def.effectMultiplyingStat != null)
			{
				num *= this.pawn.GetStatValue(this.def.effectMultiplyingStat, true);
			}
			return num;
		}

		public virtual bool TryMergeWithExistingThought()
		{
			return false;
		}

		public virtual bool GroupsWith(Thought other)
		{
			return this.def == other.def;
		}

		public virtual void Init()
		{
		}

		public override string ToString()
		{
			return "(" + this.def.defName + ")";
		}
	}
}
