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

		public override bool ShouldBeRemovedBecauseInvalid
		{
			get
			{
				return !this.recipe.Worker.GetPartsToApplyOn(this.GiverPawn, this.recipe).Contains(this.part);
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
					pawn = corpse.innerPawn;
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
				if (this.recipe == RecipeDefOf.RemoveBodyPart)
				{
					stringBuilder.Append(HealthCardUtility.RemoveBodyPartSpecialLabel(this.GiverPawn, this.part));
				}
				else
				{
					stringBuilder.Append(this.recipe.label);
				}
				if (this.Part != null && !this.recipe.hideBodyPartNames)
				{
					stringBuilder.Append(" (");
					stringBuilder.Append(this.Part.def.label);
					stringBuilder.Append(")");
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
			base.Notify_IterationCompleted(billDoer, ingredients);
			Pawn giverPawn = this.GiverPawn;
			if (this.recipe.Worker.GetPartsToApplyOn(giverPawn, this.recipe).Contains(this.Part))
			{
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
			if (!this.GiverPawn.Dead)
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
				this.part = this.GiverPawn.RaceProps.body.GetPartAtIndex(this.partIndex);
			}
		}
	}
}
