using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class Messages
	{
		private class LiveMessage
		{
			private const float DefaultMessageLifespan = 13f;

			private const float FadeoutDuration = 0.6f;

			private int ID;

			public string text;

			private float startingTime;

			public int startingFrame;

			public GlobalTargetInfo lookTarget;

			private Vector2 cachedSize = new Vector2(-1f, -1f);

			public Rect lastDrawRect;

			private static int uniqueID;

			protected float Age
			{
				get
				{
					return Time.time - this.startingTime;
				}
			}

			protected float TimeLeft
			{
				get
				{
					return 13f - this.Age;
				}
			}

			public bool Expired
			{
				get
				{
					return this.TimeLeft <= 0f;
				}
			}

			public float Alpha
			{
				get
				{
					if (this.TimeLeft < 0.6f)
					{
						return this.TimeLeft / 0.6f;
					}
					return 1f;
				}
			}

			public LiveMessage(string text)
			{
				this.text = text;
				this.lookTarget = GlobalTargetInfo.Invalid;
				this.startingFrame = Time.frameCount;
				this.startingTime = Time.time;
				this.ID = Messages.LiveMessage.uniqueID++;
			}

			public LiveMessage(string text, GlobalTargetInfo lookTarget) : this(text)
			{
				this.lookTarget = lookTarget;
			}

			public Rect CalculateRect(float x, float y)
			{
				Text.Font = GameFont.Small;
				if (this.cachedSize.x < 0f)
				{
					this.cachedSize = Text.CalcSize(this.text);
				}
				this.lastDrawRect = new Rect(x, y, this.cachedSize.x, this.cachedSize.y);
				this.lastDrawRect = this.lastDrawRect.ContractedBy(-2f);
				return this.lastDrawRect;
			}

			public void Draw(int xOffset, int yOffset)
			{
				Rect rect = this.CalculateRect((float)xOffset, (float)yOffset);
				Find.WindowStack.ImmediateWindow(Gen.HashCombineInt(this.ID, 45574281), rect, WindowLayer.Super, delegate
				{
					Text.Font = GameFont.Small;
					Text.Anchor = TextAnchor.MiddleLeft;
					Rect rect = rect.AtZero();
					float alpha = this.Alpha;
					GUI.color = new Color(1f, 1f, 1f, alpha);
					if (Messages.ShouldDrawMessageBackground)
					{
						GUI.color = new Color(0.15f, 0.15f, 0.15f, 0.8f * alpha);
						GUI.DrawTexture(rect, BaseContent.WhiteTex);
						GUI.color = new Color(1f, 1f, 1f, alpha);
					}
					if (this.lookTarget.IsValid)
					{
						UIHighlighter.HighlightOpportunity(rect, "Messages");
						Widgets.DrawHighlightIfMouseover(rect);
					}
					Rect rect2 = new Rect(2f, 0f, rect.width - 2f, rect.height);
					Widgets.Label(rect2, this.text);
					if (Current.ProgramState == ProgramState.Playing && this.lookTarget.IsValid && Widgets.ButtonInvisible(rect, false))
					{
						JumpToTargetUtility.TryJumpAndSelect(this.lookTarget);
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.ClickingMessages, KnowledgeAmount.Total);
					}
					Text.Anchor = TextAnchor.UpperLeft;
					GUI.color = Color.white;
					if (Mouse.IsOver(rect))
					{
						Messages.mouseoverMessageIndex = Messages.liveMessages.IndexOf(this);
					}
				}, false, false, 0f);
			}
		}

		private const int MessageYInterval = 26;

		private const int MaxLiveMessages = 12;

		private static List<Messages.LiveMessage> liveMessages = new List<Messages.LiveMessage>();

		private static int mouseoverMessageIndex = -1;

		public static readonly Vector2 MessagesTopLeftStandard = new Vector2(140f, 16f);

		private static bool ShouldDrawMessageBackground
		{
			get
			{
				if (Current.ProgramState != ProgramState.Playing)
				{
					return true;
				}
				WindowStack windowStack = Find.WindowStack;
				for (int i = 0; i < windowStack.Count; i++)
				{
					if (windowStack[i].CausesMessageBackground())
					{
						return true;
					}
				}
				return false;
			}
		}

		public static void Update()
		{
			if (Current.ProgramState == ProgramState.Playing && Messages.mouseoverMessageIndex >= 0 && Messages.liveMessages.Count >= Messages.mouseoverMessageIndex + 1)
			{
				GlobalTargetInfo lookTarget = Messages.liveMessages[Messages.mouseoverMessageIndex].lookTarget;
				if (lookTarget.IsValid && lookTarget.IsMapTarget && lookTarget.Map == Find.VisibleMap)
				{
					GenDraw.DrawArrowPointingAt(((TargetInfo)lookTarget).CenterVector3, false);
				}
			}
			Messages.mouseoverMessageIndex = -1;
			Messages.liveMessages.RemoveAll((Messages.LiveMessage m) => m.Expired);
		}

		public static void Message(string text, GlobalTargetInfo lookTarget, MessageSound sound)
		{
			if (!Messages.AcceptsMessage(text, lookTarget))
			{
				return;
			}
			Messages.LiveMessage msg = new Messages.LiveMessage(text, lookTarget);
			Messages.Message(msg, sound);
		}

		public static void Message(string text, MessageSound sound)
		{
			if (!Messages.AcceptsMessage(text, TargetInfo.Invalid))
			{
				return;
			}
			Messages.LiveMessage msg = new Messages.LiveMessage(text);
			Messages.Message(msg, sound);
		}

		public static void MessagesDoGUI()
		{
			Text.Font = GameFont.Small;
			int xOffset = (int)Messages.MessagesTopLeftStandard.x;
			int num = (int)Messages.MessagesTopLeftStandard.y;
			if (Current.Game != null && Find.ActiveLesson.ActiveLessonVisible)
			{
				num += (int)Find.ActiveLesson.Current.MessagesYOffset;
			}
			for (int i = Messages.liveMessages.Count - 1; i >= 0; i--)
			{
				Messages.liveMessages[i].Draw(xOffset, num);
				num += 26;
			}
		}

		public static bool CollidesWithAnyMessage(Rect rect, out float messageAlpha)
		{
			bool result = false;
			float num = 0f;
			for (int i = 0; i < Messages.liveMessages.Count; i++)
			{
				Messages.LiveMessage liveMessage = Messages.liveMessages[i];
				if (rect.Overlaps(liveMessage.lastDrawRect))
				{
					result = true;
					num = Mathf.Max(num, liveMessage.Alpha);
				}
			}
			messageAlpha = num;
			return result;
		}

		public static void Clear()
		{
			Messages.liveMessages.Clear();
		}

		public static void Notify_LoadedLevelChanged()
		{
			for (int i = 0; i < Messages.liveMessages.Count; i++)
			{
				Messages.liveMessages[i].lookTarget = GlobalTargetInfo.Invalid;
			}
		}

		private static bool AcceptsMessage(string text, GlobalTargetInfo lookTarget)
		{
			if (text.NullOrEmpty())
			{
				return false;
			}
			for (int i = 0; i < Messages.liveMessages.Count; i++)
			{
				if (Messages.liveMessages[i].text == text && Messages.liveMessages[i].lookTarget == lookTarget && Messages.liveMessages[i].startingFrame == Time.frameCount)
				{
					return false;
				}
			}
			return true;
		}

		private static void Message(Messages.LiveMessage msg, MessageSound sound)
		{
			Messages.liveMessages.Add(msg);
			while (Messages.liveMessages.Count > 12)
			{
				Messages.liveMessages.RemoveAt(0);
			}
			if (sound != MessageSound.Silent)
			{
				SoundDef soundDef = null;
				switch (sound)
				{
				case MessageSound.Standard:
					soundDef = SoundDefOf.MessageAlert;
					break;
				case MessageSound.RejectInput:
					soundDef = SoundDefOf.ClickReject;
					break;
				case MessageSound.Benefit:
					soundDef = SoundDefOf.MessageBenefit;
					break;
				case MessageSound.Negative:
					soundDef = SoundDefOf.MessageAlertNegative;
					break;
				case MessageSound.SeriousAlert:
					soundDef = SoundDefOf.MessageSeriousAlert;
					break;
				}
				soundDef.PlayOneShotOnCamera();
			}
		}
	}
}
