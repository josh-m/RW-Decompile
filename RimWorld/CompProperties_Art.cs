using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_Art : CompProperties
	{
		public RulePackDef nameMaker;

		public RulePackDef descriptionMaker;

		public QualityCategory minQualityForArtistic;

		public bool mustBeFullGrave;

		public bool canBeEnjoyedAsArt = true;

		public CompProperties_Art()
		{
			this.compClass = typeof(CompArt);
		}
	}
}
