using System;
using UnityEngine;

namespace Verse
{
	public class ImmediateWindow : Window
	{
		public Action doWindowFunc;

		public override Vector2 InitialSize
		{
			get
			{
				return this.windowRect.size;
			}
		}

		protected override float Margin
		{
			get
			{
				return 0f;
			}
		}

		public ImmediateWindow()
		{
			this.doCloseButton = false;
			this.doCloseX = false;
			this.soundAppear = null;
			this.soundClose = null;
			this.closeOnClickedOutside = false;
			this.closeOnEscapeKey = false;
			this.focusWhenOpened = false;
			this.preventCameraMotion = false;
		}

		public override void DoWindowContents(Rect inRect)
		{
			this.doWindowFunc();
		}
	}
}
