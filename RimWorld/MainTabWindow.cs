using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class MainTabWindow : Window
	{
		public MainTabDef def;

		public virtual Vector2 RequestedTabSize
		{
			get
			{
				return new Vector2(1010f, 684f);
			}
		}

		public virtual MainTabWindowAnchor Anchor
		{
			get
			{
				return MainTabWindowAnchor.Left;
			}
		}

		public override Vector2 InitialSize
		{
			get
			{
				Vector2 requestedTabSize = this.RequestedTabSize;
				if (requestedTabSize.y > (float)(UI.screenHeight - 35))
				{
					requestedTabSize.y = (float)(UI.screenHeight - 35);
				}
				if (requestedTabSize.x > (float)UI.screenWidth)
				{
					requestedTabSize.x = (float)UI.screenWidth;
				}
				return requestedTabSize;
			}
		}

		public virtual float TabButtonBarPercent
		{
			get
			{
				return 0f;
			}
		}

		public MainTabWindow()
		{
			this.layer = WindowLayer.GameUI;
			this.soundAppear = null;
			this.soundClose = null;
			this.doCloseButton = false;
			this.doCloseX = false;
			this.closeOnEscapeKey = true;
			this.preventCameraMotion = false;
		}

		public override void DoWindowContents(Rect inRect)
		{
			this.SetInitialSizeAndPosition();
		}

		protected override void SetInitialSizeAndPosition()
		{
			base.SetInitialSizeAndPosition();
			if (this.Anchor == MainTabWindowAnchor.Left)
			{
				this.windowRect.x = 0f;
			}
			else
			{
				this.windowRect.x = (float)UI.screenWidth - this.windowRect.width;
			}
			this.windowRect.y = (float)(UI.screenHeight - 35) - this.windowRect.height;
		}
	}
}
