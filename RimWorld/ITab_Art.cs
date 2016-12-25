using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ITab_Art : ITab
	{
		private static string cachedImageDescription;

		private static CompArt cachedImageSource;

		private static TaleReference cachedTaleRef;

		private static readonly Vector2 WinSize = new Vector2(400f, 300f);

		private CompArt SelectedCompArt
		{
			get
			{
				Thing thing = Find.Selector.SingleSelectedThing;
				MinifiedThing minifiedThing = thing as MinifiedThing;
				if (minifiedThing != null)
				{
					thing = minifiedThing.InnerThing;
				}
				if (thing == null)
				{
					return null;
				}
				return thing.TryGetComp<CompArt>();
			}
		}

		public override bool IsVisible
		{
			get
			{
				return this.SelectedCompArt != null && this.SelectedCompArt.Active;
			}
		}

		public ITab_Art()
		{
			this.size = ITab_Art.WinSize;
			this.labelKey = "TabArt";
			this.tutorTag = "Art";
		}

		protected override void FillTab()
		{
			Rect rect = new Rect(0f, 0f, ITab_Art.WinSize.x, ITab_Art.WinSize.y).ContractedBy(10f);
			Rect rect2 = rect;
			Text.Font = GameFont.Medium;
			Widgets.Label(rect2, this.SelectedCompArt.Title);
			if (ITab_Art.cachedImageSource != this.SelectedCompArt || ITab_Art.cachedTaleRef != this.SelectedCompArt.TaleRef)
			{
				ITab_Art.cachedImageDescription = this.SelectedCompArt.GenerateImageDescription();
				ITab_Art.cachedImageSource = this.SelectedCompArt;
				ITab_Art.cachedTaleRef = this.SelectedCompArt.TaleRef;
			}
			Rect rect3 = rect;
			rect3.yMin += 35f;
			Text.Font = GameFont.Small;
			Widgets.Label(rect3, ITab_Art.cachedImageDescription);
		}
	}
}
