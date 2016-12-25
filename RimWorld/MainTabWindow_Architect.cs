using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class MainTabWindow_Architect : MainTabWindow
	{
		public const float WinWidth = 200f;

		private const float ButHeight = 32f;

		private List<ArchitectCategoryTab> desPanelsCached;

		public ArchitectCategoryTab selectedDesPanel;

		public float WinHeight
		{
			get
			{
				if (this.desPanelsCached == null)
				{
					this.CacheDesPanels();
				}
				return (float)Mathf.CeilToInt((float)this.desPanelsCached.Count / 2f) * 32f;
			}
		}

		public override Vector2 RequestedTabSize
		{
			get
			{
				return new Vector2(200f, this.WinHeight);
			}
		}

		protected override float Margin
		{
			get
			{
				return 0f;
			}
		}

		public MainTabWindow_Architect()
		{
			this.CacheDesPanels();
		}

		public override void ExtraOnGUI()
		{
			base.ExtraOnGUI();
			if (this.selectedDesPanel != null)
			{
				this.selectedDesPanel.DesignationTabOnGUI();
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);
			Text.Font = GameFont.Small;
			float num = inRect.width / 2f;
			float num2 = 0f;
			float num3 = 0f;
			for (int i = 0; i < this.desPanelsCached.Count; i++)
			{
				Rect rect = new Rect(num2 * num, num3 * 32f, num, 32f);
				rect.height += 1f;
				if (num2 == 0f)
				{
					rect.width += 1f;
				}
				if (Widgets.ButtonTextSubtle(rect, this.desPanelsCached[i].def.LabelCap, 0f, 8f, SoundDefOf.MouseoverCategory))
				{
					this.ClickedCategory(this.desPanelsCached[i]);
				}
				if (this.selectedDesPanel != this.desPanelsCached[i])
				{
					UIHighlighter.HighlightOpportunity(rect, this.desPanelsCached[i].def.cachedHighlightClosedTag);
				}
				num2 += 1f;
				if (num2 > 1f)
				{
					num2 = 0f;
					num3 += 1f;
				}
			}
		}

		private void CacheDesPanels()
		{
			this.desPanelsCached = new List<ArchitectCategoryTab>();
			foreach (DesignationCategoryDef current in from dc in DefDatabase<DesignationCategoryDef>.AllDefs
			orderby dc.order descending
			select dc)
			{
				this.desPanelsCached.Add(new ArchitectCategoryTab(current));
			}
		}

		protected void ClickedCategory(ArchitectCategoryTab Pan)
		{
			if (this.selectedDesPanel == Pan)
			{
				this.selectedDesPanel = null;
			}
			else
			{
				this.selectedDesPanel = Pan;
			}
			SoundDefOf.ArchitectCategorySelect.PlayOneShotOnCamera();
		}
	}
}
