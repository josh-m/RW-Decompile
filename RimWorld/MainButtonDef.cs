using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MainButtonDef : Def
	{
		public Type workerClass = typeof(MainButtonWorker_ToggleTab);

		public Type tabWindowClass;

		public bool buttonVisible = true;

		public int order;

		public KeyCode defaultHotKey;

		public bool canBeTutorDenied = true;

		public bool validWithoutMap;

		[Unsaved]
		public KeyBindingDef hotKey;

		[Unsaved]
		public string cachedTutorTag;

		[Unsaved]
		public string cachedHighlightTagClosed;

		[Unsaved]
		private MainButtonWorker workerInt;

		[Unsaved]
		private MainTabWindow tabWindowInt;

		[Unsaved]
		private string cachedShortenedLabelCap;

		[Unsaved]
		private float cachedLabelCapWidth = -1f;

		[Unsaved]
		private float cachedShortenedLabelCapWidth = -1f;

		public const int ButtonHeight = 35;

		public MainButtonWorker Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					this.workerInt = (MainButtonWorker)Activator.CreateInstance(this.workerClass);
					this.workerInt.def = this;
				}
				return this.workerInt;
			}
		}

		public MainTabWindow TabWindow
		{
			get
			{
				if (this.tabWindowInt == null && this.tabWindowClass != null)
				{
					this.tabWindowInt = (MainTabWindow)Activator.CreateInstance(this.tabWindowClass);
					this.tabWindowInt.def = this;
				}
				return this.tabWindowInt;
			}
		}

		public string ShortenedLabelCap
		{
			get
			{
				if (this.cachedShortenedLabelCap == null)
				{
					this.cachedShortenedLabelCap = base.LabelCap.Shorten();
				}
				return this.cachedShortenedLabelCap;
			}
		}

		public float LabelCapWidth
		{
			get
			{
				if (this.cachedLabelCapWidth < 0f)
				{
					GameFont font = Text.Font;
					Text.Font = GameFont.Small;
					this.cachedLabelCapWidth = Text.CalcSize(base.LabelCap).x;
					Text.Font = font;
				}
				return this.cachedLabelCapWidth;
			}
		}

		public float ShortenedLabelCapWidth
		{
			get
			{
				if (this.cachedShortenedLabelCapWidth < 0f)
				{
					GameFont font = Text.Font;
					Text.Font = GameFont.Small;
					this.cachedShortenedLabelCapWidth = Text.CalcSize(this.ShortenedLabelCap).x;
					Text.Font = font;
				}
				return this.cachedShortenedLabelCapWidth;
			}
		}

		public override void PostLoad()
		{
			base.PostLoad();
			this.cachedHighlightTagClosed = "MainTab-" + this.defName + "-Closed";
		}

		public void Notify_SwitchedMap()
		{
			if (this.tabWindowInt != null)
			{
				Find.WindowStack.TryRemove(this.tabWindowInt, true);
				this.tabWindowInt = null;
			}
		}

		public void Notify_ClearingAllMapsMemory()
		{
			this.tabWindowInt = null;
		}
	}
}
