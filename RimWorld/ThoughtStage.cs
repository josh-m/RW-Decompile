using System;
using Verse;

namespace RimWorld
{
	public class ThoughtStage
	{
		[MustTranslate]
		public string label;

		[MustTranslate]
		public string labelSocial;

		[MustTranslate]
		public string description;

		public float baseMoodEffect;

		public float baseOpinionOffset;

		public bool visible = true;
	}
}
