using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Bill_ProductionWithUft : Bill_Production
	{
		private UnfinishedThing boundUftInt;

		protected override string StatusString
		{
			get
			{
				if (this.BoundWorker == null)
				{
					return null;
				}
				return "BoundWorkerIs".Translate(new object[]
				{
					this.BoundWorker.NameStringShort
				});
			}
		}

		public Pawn BoundWorker
		{
			get
			{
				if (this.boundUftInt == null)
				{
					return null;
				}
				Pawn creator = this.boundUftInt.Creator;
				if (creator == null || creator.Downed || creator.HostFaction != null || creator.Destroyed || !creator.Spawned)
				{
					this.boundUftInt = null;
					return null;
				}
				Thing thing = this.billStack.billGiver as Thing;
				if (thing != null)
				{
					WorkTypeDef workTypeDef = null;
					List<WorkGiverDef> allDefsListForReading = DefDatabase<WorkGiverDef>.AllDefsListForReading;
					for (int i = 0; i < allDefsListForReading.Count; i++)
					{
						if (allDefsListForReading[i].fixedBillGiverDefs != null && allDefsListForReading[i].fixedBillGiverDefs.Contains(thing.def))
						{
							workTypeDef = allDefsListForReading[i].workType;
							break;
						}
					}
					if (workTypeDef != null && !creator.workSettings.WorkIsActive(workTypeDef))
					{
						this.boundUftInt = null;
						return null;
					}
				}
				return creator;
			}
		}

		public UnfinishedThing BoundUft
		{
			get
			{
				return this.boundUftInt;
			}
		}

		public Bill_ProductionWithUft()
		{
		}

		public Bill_ProductionWithUft(RecipeDef recipe) : base(recipe)
		{
		}

		public void SetBoundUft(UnfinishedThing value, bool setOtherLink = true)
		{
			if (value == this.boundUftInt)
			{
				return;
			}
			UnfinishedThing unfinishedThing = this.boundUftInt;
			this.boundUftInt = value;
			if (setOtherLink)
			{
				if (unfinishedThing != null && unfinishedThing.BoundBill == this)
				{
					unfinishedThing.BoundBill = null;
				}
				if (value != null && value.BoundBill != this)
				{
					this.boundUftInt.BoundBill = this;
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.LookReference<UnfinishedThing>(ref this.boundUftInt, "boundUft", false);
		}

		public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
		{
			this.ClearBoundUft();
			base.Notify_IterationCompleted(billDoer, ingredients);
		}

		public void ClearBoundUft()
		{
			this.boundUftInt = null;
		}
	}
}
