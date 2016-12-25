using System;

namespace RimWorld
{
	public class ITab_Pawn_Prisoner : ITab_Pawn_Visitor
	{
		public override bool IsVisible
		{
			get
			{
				return base.SelPawn.IsPrisonerOfColony;
			}
		}

		public ITab_Pawn_Prisoner()
		{
			this.labelKey = "TabPrisoner";
			this.tutorTag = "Prisoner";
		}
	}
}
