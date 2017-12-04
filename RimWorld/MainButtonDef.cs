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
	}
}
