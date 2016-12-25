using System;
using System.Collections.Generic;

namespace Verse
{
	public struct BodyPartDamageInfo
	{
		private BodyPartHeight? heightInt;

		private BodyPartDepth? depthInt;

		private BodyPartRecord partInt;

		private List<HediffDef> healHediffInt;

		private bool canMissBodyPartInt;

		private Hediff_Injury injuryInt;

		public BodyPartHeight? Height
		{
			get
			{
				return this.heightInt;
			}
		}

		public BodyPartDepth? Depth
		{
			get
			{
				return this.depthInt;
			}
		}

		public BodyPartRecord Part
		{
			get
			{
				return this.partInt;
			}
		}

		public List<HediffDef> HealHediff
		{
			get
			{
				return this.healHediffInt;
			}
		}

		public bool CanMissBodyPart
		{
			get
			{
				return this.canMissBodyPartInt;
			}
		}

		public Hediff_Injury Injury
		{
			get
			{
				return this.injuryInt;
			}
		}

		public BodyPartDamageInfo(Hediff_Injury injury)
		{
			this.injuryInt = injury;
		}

		public BodyPartDamageInfo(BodyPartHeight? height, BodyPartDepth? depth)
		{
			this.heightInt = height;
			this.depthInt = depth;
		}

		public BodyPartDamageInfo(BodyPartRecord part, bool canMissBodyPart, List<HediffDef> healHediff = null)
		{
			this.partInt = part;
			this.canMissBodyPartInt = canMissBodyPart;
			this.healHediffInt = healHediff;
		}

		public BodyPartDamageInfo(BodyPartRecord part, bool canMissBodyPart, HediffDef healHediff)
		{
			this.partInt = part;
			this.canMissBodyPartInt = canMissBodyPart;
			this.healHediffInt = new List<HediffDef>();
			this.healHediffInt.Add(healHediff);
		}
	}
}
