using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public class MentalState_TargetedTantrum : MentalState_Tantrum
	{
		public const int MinMarketValue = 300;

		private static List<Thing> tmpThings = new List<Thing>();

		public override void MentalStateTick()
		{
			if (this.target == null || this.target.Destroyed)
			{
				base.RecoverFromState();
			}
			else if (!this.target.Spawned || !this.pawn.CanReach(this.target, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
			{
				Thing target = this.target;
				if (!this.TryFindNewTarget())
				{
					base.RecoverFromState();
				}
				else
				{
					Messages.Message("MessageTargetedTantrumChangedTarget".Translate(new object[]
					{
						this.pawn.LabelShort,
						target.Label,
						this.target.Label
					}).AdjustedFor(this.pawn), this.pawn, MessageTypeDefOf.NegativeEvent);
					base.MentalStateTick();
				}
			}
			else
			{
				base.MentalStateTick();
			}
		}

		public override void PostStart(string reason)
		{
			base.PostStart(reason);
			this.TryFindNewTarget();
		}

		private bool TryFindNewTarget()
		{
			TantrumMentalStateUtility.GetSmashableThingsNear(this.pawn, this.pawn.Position, MentalState_TargetedTantrum.tmpThings, null, 300, 40);
			bool result = MentalState_TargetedTantrum.tmpThings.TryRandomElementByWeight((Thing x) => x.MarketValue * (float)x.stackCount, out this.target);
			MentalState_TargetedTantrum.tmpThings.Clear();
			return result;
		}

		public override string GetBeginLetterText()
		{
			if (this.target == null)
			{
				Log.Error("No target. This should have been checked in this mental state's worker.");
				return string.Empty;
			}
			return string.Format(this.def.beginLetter, this.pawn.Label, this.target.Label).AdjustedFor(this.pawn).CapitalizeFirst();
		}
	}
}
