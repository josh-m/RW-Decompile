using System;
using Verse;

namespace RimWorld
{
	public class CompBreakdownable : ThingComp
	{
		private const int BreakdownMTBTicks = 18000000;

		public const string BreakdownSignal = "Breakdown";

		private bool brokenDownInt;

		private CompPowerTrader powerComp;

		public bool BrokenDown
		{
			get
			{
				return this.brokenDownInt;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.LookValue<bool>(ref this.brokenDownInt, "brokenDown", false, false);
		}

		public override void PostDraw()
		{
			if (this.brokenDownInt)
			{
				OverlayDrawer.DrawOverlay(this.parent, OverlayTypes.BrokenDown);
			}
		}

		public override void PostSpawnSetup()
		{
			base.PostSpawnSetup();
			this.powerComp = this.parent.GetComp<CompPowerTrader>();
			Find.Map.GetComponent<BreakdownManager>().Register(this);
		}

		public override void PostDeSpawn()
		{
			base.PostDeSpawn();
			Find.Map.GetComponent<BreakdownManager>().Deregister(this);
		}

		public void CheckForBreakdown()
		{
			if (this.CanBreakdownNow() && Rand.MTBEventOccurs(1.8E+07f, 1f, 1041f))
			{
				this.DoBreakdown();
			}
		}

		protected bool CanBreakdownNow()
		{
			return !this.BrokenDown && (this.powerComp == null || this.powerComp.PowerOn);
		}

		public void Notify_Repaired()
		{
			this.brokenDownInt = false;
			Find.Map.GetComponent<BreakdownManager>().Notify_Repaired(this.parent);
			if (this.parent is Building_PowerSwitch)
			{
				PowerNetManager.Notfiy_TransmitterTransmitsPowerNowChanged(this.parent.GetComp<CompPower>());
			}
		}

		public void DoBreakdown()
		{
			this.brokenDownInt = true;
			this.parent.BroadcastCompSignal("Breakdown");
			Find.Map.GetComponent<BreakdownManager>().Notify_BrokenDown(this.parent);
			if (this.parent.Faction == Faction.OfPlayer)
			{
				Find.LetterStack.ReceiveLetter("LetterLabelBuildingBrokenDown".Translate(new object[]
				{
					this.parent.LabelShort
				}), "LetterBuildingBrokenDown".Translate(new object[]
				{
					this.parent.LabelShort
				}), LetterType.BadNonUrgent, this.parent, null);
			}
		}

		public override string CompInspectStringExtra()
		{
			if (this.BrokenDown)
			{
				return "BrokenDown".Translate();
			}
			return null;
		}
	}
}
