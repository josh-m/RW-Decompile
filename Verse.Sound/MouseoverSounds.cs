using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.Sound
{
	public static class MouseoverSounds
	{
		private struct MouseoverRegionCall
		{
			public bool mouseIsOver;

			public Rect rect;

			public SoundDef sound;

			public bool IsValid
			{
				get
				{
					return this.rect.x >= 0f;
				}
			}

			public static MouseoverSounds.MouseoverRegionCall Invalid
			{
				get
				{
					return new MouseoverSounds.MouseoverRegionCall
					{
						rect = new Rect(-1000f, -1000f, 0f, 0f)
					};
				}
			}

			public bool Matches(MouseoverSounds.MouseoverRegionCall other)
			{
				return this.rect.Equals(other.rect);
			}

			public override string ToString()
			{
				if (!this.IsValid)
				{
					return "(Invalid)";
				}
				return string.Concat(new object[]
				{
					"(rect=",
					this.rect,
					(!this.mouseIsOver) ? string.Empty : "mouseIsOver",
					")"
				});
			}
		}

		private static List<MouseoverSounds.MouseoverRegionCall> frameCalls = new List<MouseoverSounds.MouseoverRegionCall>();

		private static int lastUsedCallInd = -1;

		private static MouseoverSounds.MouseoverRegionCall lastUsedCall;

		private static int forceSilenceUntilFrame = -1;

		public static void SilenceForNextFrame()
		{
			MouseoverSounds.forceSilenceUntilFrame = Time.frameCount + 1;
		}

		public static void DoRegion(Rect rect)
		{
			MouseoverSounds.DoRegion(rect, SoundDefOf.MouseoverStandard);
		}

		public static void DoRegion(Rect rect, SoundDef sound)
		{
			if (sound == null)
			{
				return;
			}
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			MouseoverSounds.MouseoverRegionCall item = default(MouseoverSounds.MouseoverRegionCall);
			item.rect = rect;
			item.sound = sound;
			item.mouseIsOver = Mouse.IsOver(rect);
			MouseoverSounds.frameCalls.Add(item);
		}

		public static void ResolveFrame()
		{
			for (int i = 0; i < MouseoverSounds.frameCalls.Count; i++)
			{
				if (MouseoverSounds.frameCalls[i].mouseIsOver)
				{
					if (MouseoverSounds.lastUsedCallInd != i && !MouseoverSounds.frameCalls[i].Matches(MouseoverSounds.lastUsedCall) && MouseoverSounds.forceSilenceUntilFrame < Time.frameCount)
					{
						MouseoverSounds.frameCalls[i].sound.PlayOneShotOnCamera();
					}
					MouseoverSounds.lastUsedCallInd = i;
					MouseoverSounds.lastUsedCall = MouseoverSounds.frameCalls[i];
					MouseoverSounds.frameCalls.Clear();
					return;
				}
			}
			MouseoverSounds.lastUsedCall = MouseoverSounds.MouseoverRegionCall.Invalid;
			MouseoverSounds.lastUsedCallInd = -1;
			MouseoverSounds.frameCalls.Clear();
		}
	}
}
