using System;

namespace Verse
{
	public class PawnCapacityDef : Def
	{
		public int listOrder;

		[MustTranslate]
		public string labelMechanoids = string.Empty;

		[MustTranslate]
		public string labelAnimals = string.Empty;

		public bool showOnHumanlikes = true;

		public bool showOnAnimals = true;

		public bool showOnMechanoids = true;

		public bool lethalFlesh;

		public bool lethalMechanoids;

		public float minForCapable;

		public float minValue;

		public bool showOnCaravanHealthTab;

		public string GetLabelFor(Pawn pawn)
		{
			return this.GetLabelFor(pawn.RaceProps.IsFlesh, pawn.RaceProps.Humanlike);
		}

		public string GetLabelFor(bool isFlesh, bool isHumanlike)
		{
			if (isHumanlike)
			{
				return this.label;
			}
			if (isFlesh)
			{
				if (!this.labelAnimals.NullOrEmpty())
				{
					return this.labelAnimals;
				}
				return this.label;
			}
			else
			{
				if (!this.labelMechanoids.NullOrEmpty())
				{
					return this.labelMechanoids;
				}
				return this.label;
			}
		}
	}
}
