using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.Noise
{
	public static class NoiseDebugUI
	{
		private class NoiseRender
		{
			public Texture2D tex;

			public string name;

			public NoiseRender(Texture2D tex, string name)
			{
				this.tex = tex;
				this.name = name;
			}
		}

		private static List<NoiseDebugUI.NoiseRender> storedRenders = new List<NoiseDebugUI.NoiseRender>();

		public static IntVec2 RenderSize
		{
			set
			{
				NoiseRenderer.renderSize = value;
			}
		}

		public static void StoreNoiseRender(ModuleBase noise, string name, IntVec2 renderSize)
		{
			NoiseDebugUI.RenderSize = renderSize;
			NoiseDebugUI.StoreNoiseRender(noise, name);
		}

		public static void StoreNoiseRender(ModuleBase noise, string name)
		{
			if (!Prefs.DevMode || !DebugViewSettings.drawRecordedNoise)
			{
				return;
			}
			Texture2D tex = NoiseRenderer.NoiseRendered(noise);
			NoiseDebugUI.NoiseRender item = new NoiseDebugUI.NoiseRender(tex, name);
			NoiseDebugUI.storedRenders.Add(item);
		}

		public static void NoiseDebugOnGUI()
		{
			if (!Prefs.DevMode || !DebugViewSettings.drawRecordedNoise)
			{
				return;
			}
			if (Widgets.ButtonText(new Rect(100f, 0f, 200f, 30f), "Clear noise renders", true, false, true))
			{
				NoiseDebugUI.Clear();
			}
			if (Widgets.ButtonText(new Rect(300f, 0f, 200f, 30f), "Hide noise renders", true, false, true))
			{
				DebugViewSettings.drawRecordedNoise = false;
			}
			float num = 0f;
			float num2 = 50f;
			Text.Font = GameFont.Tiny;
			foreach (NoiseDebugUI.NoiseRender current in NoiseDebugUI.storedRenders)
			{
				if (num + (float)current.tex.width + 5f > (float)Screen.width)
				{
					num = 0f;
					num2 += (float)(current.tex.height + 5 + 25);
				}
				Rect position = new Rect(num, num2, (float)current.tex.width, (float)current.tex.height);
				GUI.DrawTexture(position, current.tex);
				Rect rect = new Rect(num, num2 - 15f, (float)current.tex.width, (float)current.tex.height);
				Widgets.Label(rect, current.name);
				num += (float)(current.tex.width + 5);
			}
		}

		public static void Clear()
		{
			NoiseDebugUI.storedRenders.Clear();
		}
	}
}
