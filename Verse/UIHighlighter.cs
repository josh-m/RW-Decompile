using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class UIHighlighter
	{
		private const float PulseFrequency = 1.2f;

		private const float PulseAmplitude = 0.7f;

		private static List<Pair<string, int>> liveTags = new List<Pair<string, int>>();

		private static readonly Texture2D TutorHighlightAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/TutorHighlightAtlas", true);

		public static void HighlightTag(string tag)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			if (tag.NullOrEmpty())
			{
				return;
			}
			for (int i = 0; i < UIHighlighter.liveTags.Count; i++)
			{
				if (UIHighlighter.liveTags[i].First == tag && UIHighlighter.liveTags[i].Second == Time.frameCount)
				{
					return;
				}
			}
			UIHighlighter.liveTags.Add(new Pair<string, int>(tag, Time.frameCount));
		}

		public static void HighlightOpportunity(Rect rect, string tag)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			for (int i = 0; i < UIHighlighter.liveTags.Count; i++)
			{
				Pair<string, int> pair = UIHighlighter.liveTags[i];
				if (tag == pair.First && Time.frameCount == pair.Second + 1)
				{
					Rect rect2 = rect.ContractedBy(-10f);
					GUI.color = new Color(1f, 1f, 1f, Pulser.PulseBrightness(1.2f, 0.7f));
					Widgets.DrawAtlas(rect2, UIHighlighter.TutorHighlightAtlas);
					GUI.color = Color.white;
				}
			}
		}

		public static void UIHighlighterUpdate()
		{
			UIHighlighter.liveTags.RemoveAll((Pair<string, int> pair) => Time.frameCount > pair.Second + 1);
		}
	}
}
