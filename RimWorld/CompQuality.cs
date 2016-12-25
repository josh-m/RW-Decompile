using System;
using Verse;

namespace RimWorld
{
	public class CompQuality : ThingComp
	{
		private QualityCategory qualityInt = QualityCategory.Normal;

		public QualityCategory Quality
		{
			get
			{
				return this.qualityInt;
			}
		}

		public void SetQuality(QualityCategory q, ArtGenerationContext source)
		{
			this.qualityInt = q;
			CompArt compArt = this.parent.TryGetComp<CompArt>();
			if (compArt != null)
			{
				compArt.InitializeArt(source);
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.LookValue<QualityCategory>(ref this.qualityInt, "quality", QualityCategory.Awful, false);
		}

		public override bool AllowStackWith(Thing other)
		{
			QualityCategory qualityCategory;
			return other.TryGetQuality(out qualityCategory) && this.qualityInt == qualityCategory;
		}

		public override void PostSplitOff(Thing piece)
		{
			base.PostSplitOff(piece);
			piece.TryGetComp<CompQuality>().qualityInt = this.qualityInt;
		}

		public override string CompInspectStringExtra()
		{
			return "QualityIs".Translate(new object[]
			{
				this.Quality.GetLabel().CapitalizeFirst()
			});
		}
	}
}
