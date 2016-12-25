using System;
using UnityEngine;

namespace Verse
{
	public class Dialog_ResolutionConfirm : Window
	{
		private const float RevertTime = 10f;

		private float startTime;

		private IntVec2 oldRes;

		private bool oldFullscreen;

		private float oldUIScale;

		private float TimeUntilRevert
		{
			get
			{
				return this.startTime + 10f - Time.realtimeSinceStartup;
			}
		}

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(500f, 300f);
			}
		}

		private Dialog_ResolutionConfirm()
		{
			this.startTime = Time.realtimeSinceStartup;
			this.closeOnEscapeKey = false;
			this.absorbInputAroundWindow = true;
		}

		public Dialog_ResolutionConfirm(bool oldFullscreen) : this()
		{
			this.oldFullscreen = oldFullscreen;
			this.oldRes = new IntVec2(Screen.width, Screen.height);
			this.oldUIScale = Prefs.UIScale;
		}

		public Dialog_ResolutionConfirm(IntVec2 oldRes) : this()
		{
			this.oldFullscreen = Screen.fullScreen;
			this.oldRes = oldRes;
			this.oldUIScale = Prefs.UIScale;
		}

		public Dialog_ResolutionConfirm(float oldUIScale) : this()
		{
			this.oldFullscreen = Screen.fullScreen;
			this.oldRes = new IntVec2(Screen.width, Screen.height);
			this.oldUIScale = oldUIScale;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			string label = "ConfirmResolutionChange".Translate(new object[]
			{
				Mathf.CeilToInt(this.TimeUntilRevert)
			});
			Widgets.Label(new Rect(0f, 0f, inRect.width, inRect.height), label);
			if (Widgets.ButtonText(new Rect(0f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), "ResolutionKeep".Translate(), true, false, true))
			{
				this.Close(true);
			}
			if (Widgets.ButtonText(new Rect(inRect.width / 2f + 20f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), "ResolutionRevert".Translate(), true, false, true))
			{
				this.Revert();
				this.Close(true);
			}
		}

		private void Revert()
		{
			if (Prefs.LogVerbose)
			{
				Log.Message(string.Concat(new object[]
				{
					"Reverting screen settings to ",
					this.oldRes.x,
					"x",
					this.oldRes.z,
					", fs=",
					this.oldFullscreen
				}));
			}
			Screen.SetResolution(this.oldRes.x, this.oldRes.z, this.oldFullscreen);
			Prefs.UIScale = this.oldUIScale;
		}

		public override void WindowUpdate()
		{
			if (this.TimeUntilRevert <= 0f)
			{
				this.Revert();
				this.Close(true);
			}
		}
	}
}
