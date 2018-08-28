using System;

namespace Verse
{
	public class BodyPartGroupDef : Def
	{
		[MustTranslate]
		public string labelShort;

		public int listOrder;

		public string LabelShort
		{
			get
			{
				return (!this.labelShort.NullOrEmpty()) ? this.labelShort : this.label;
			}
		}

		public string LabelShortCap
		{
			get
			{
				return this.LabelShort.CapitalizeFirst();
			}
		}
	}
}
