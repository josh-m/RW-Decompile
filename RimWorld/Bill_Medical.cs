using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Bill_Medical : Bill
	{
		private BodyPartRecord part;

		private int partIndex = -1;

		public override bool CheckIngredientsIfSociallyProper
		{
			get
			{
				return false;
			}
		}

		public override bool CompletableEver
		{
			get
			{
				return !this.recipe.targetsBodyPart || this.recipe.Worker.GetPartsToApplyOn(this.GiverPawn, this.recipe).Contains(this.part);
			}
		}

		public BodyPartRecord Part
		{
			get
			{
				if (this.part == null && this.partIndex >= 0)
				{
					this.part = this.GiverPawn.RaceProps.body.GetPartAtIndex(this.partIndex);
				}
				return this.part;
			}
			set
			{
				if (this.billStack == null)
				{
					Log.Error("Can only set Bill_Medical.Part after the bill has been added to a pawn's bill stack.");
					return;
				}
				if (value != null)
				{
					this.partIndex = this.GiverPawn.RaceProps.body.GetIndexOfPart(value);
				}
				else
				{
					this.partIndex = -1;
				}
				this.part = value;
			}
		}

		private Pawn GiverPawn
		{
			get
			{
				Pawn pawn = this.billStack.billGiver as Pawn;
				Corpse corpse = this.billStack.billGiver as Corpse;
				if (corpse != null)
				{
					pawn = corpse.InnerPawn;
				}
				if (pawn == null)
				{
					throw new InvalidOperationException("Medical bill on non-pawn.");
				}
				return pawn;
			}
		}

		public override string Label
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(this.recipe.Worker.GetLabelWhenUsedOn(this.GiverPawn, this.part));
				if (this.Part != null && !this.recipe.hideBodyPartNames)
				{
					stringBuilder.Append(" (" + this.Part.def.label + ")");
				}
				return stringBuilder.ToString();
			}
		}

		public Bill_Medical()
		{
		}

		public Bill_Medical(RecipeDef recipe) : base(recipe)
		{
		}

		public override bool ShouldDoNow()
		{
			return !this.suspended;
		}

		public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
		{
			if (this.CompletableEver)
			{
				Pawn giverPawn = this.GiverPawn;
				this.recipe.Worker.ApplyOnPawn(giverPawn, this.Part, billDoer, ingredients);
				if (giverPawn.RaceProps.IsFlesh)
				{
					giverPawn.records.Increment(RecordDefOf.OperationsReceived);
					billDoer.records.Increment(RecordDefOf.OperationsPerformed);
				}
			}
			this.billStack.Delete(this);
		}

		public override void Notify_DoBillStarted()
		{
			if (!this.GiverPawn.Dead && this.recipe.anesthesize)
			{
				HealthUtility.TryAnesthesize(this.GiverPawn);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.partIndex, "partIndex", 0, false);
			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				if (this.partIndex < 0)
				{
					this.part = null;
				}
				else
				{
					this.part = this.GiverPawn.RaceProps.body.GetPartAtIndex(this.partIndex);
				}
			}
		}
	}
}
