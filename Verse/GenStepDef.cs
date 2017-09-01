using RimWorld;
using System;

namespace Verse
{
	public class GenStepDef : Def
	{
		public MapGeneratorDef linkWithMapGenerator;

		public SiteDefBase linkWithSite;

		public float order;

		public GenStep genStep;

		public override void PostLoad()
		{
			base.PostLoad();
			this.genStep.def = this;
		}
	}
}
