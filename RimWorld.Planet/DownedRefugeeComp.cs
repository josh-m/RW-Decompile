using System;
using Verse;

namespace RimWorld.Planet
{
	public class DownedRefugeeComp : ImportantPawnComp, IThingHolder
	{
		protected override string PawnSaveKey
		{
			get
			{
				return "refugee";
			}
		}

		protected override void RemovePawnOnWorldObjectRemoved()
		{
			for (int i = this.pawn.Count - 1; i >= 0; i--)
			{
				if (!this.pawn[i].Dead)
				{
					this.pawn[i].Kill(null, null);
				}
			}
			this.pawn.ClearAndDestroyContents(DestroyMode.Vanish);
		}

		public override string CompInspectStringExtra()
		{
			if (this.pawn.Any)
			{
				return "Refugee".Translate() + ": " + this.pawn[0].LabelShort;
			}
			return null;
		}
	}
}
