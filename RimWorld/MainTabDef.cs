using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MainTabDef : Def
	{
		public const int TabButtonHeight = 35;

		public Type windowClass;

		public bool showTabButton = true;

		public int order;

		public KeyCode defaultToggleKey;

		public bool canBeTutorDenied = true;

		public bool validWithoutMap;

		[Unsaved]
		public KeyBindingDef toggleHotKey;

		[Unsaved]
		public string cachedTutorTag;

		[Unsaved]
		public string cachedHighlightTagClosed;

		[Unsaved]
		private MainTabWindow windowInt;

		public MainTabWindow Window
		{
			get
			{
				if (this.windowInt == null)
				{
					this.windowInt = (MainTabWindow)Activator.CreateInstance(this.windowClass);
					this.windowInt.def = this;
				}
				return this.windowInt;
			}
		}

		public override void PostLoad()
		{
			base.PostLoad();
			this.cachedHighlightTagClosed = "MainTab-" + this.defName + "-Closed";
		}

		public void Notify_SwitchedMap()
		{
			if (this.windowInt != null)
			{
				Find.WindowStack.TryRemove(this.windowInt, true);
				this.windowInt = null;
			}
		}
	}
}
