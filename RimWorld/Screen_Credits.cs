using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Screen_Credits : Window
	{
		private const int ColumnWidth = 800;

		private const float InitialAutoScrollDelay = 1f;

		private const float InitialAutoScrollDelayWonGame = 6f;

		private const float AutoScrollDelayAfterManualScroll = 3f;

		private const float SongStartDelay = 5f;

		private const GameFont Font = GameFont.Medium;

		private List<CreditsEntry> creds;

		public bool wonGame;

		private float timeUntilAutoScroll;

		private float scrollPosition;

		private bool playedMusic;

		public float creationRealtime = -1f;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2((float)UI.screenWidth, (float)UI.screenHeight);
			}
		}

		protected override float Margin
		{
			get
			{
				return 0f;
			}
		}

		private float ViewWidth
		{
			get
			{
				return 800f;
			}
		}

		private float ViewHeight
		{
			get
			{
				GameFont font = Text.Font;
				Text.Font = GameFont.Medium;
				float result = this.creds.Sum((CreditsEntry c) => c.DrawHeight(this.ViewWidth)) + 20f;
				Text.Font = font;
				return result;
			}
		}

		private float MaxScrollPosition
		{
			get
			{
				return Mathf.Max(this.ViewHeight - (float)UI.screenHeight / 2f, 0f);
			}
		}

		private float AutoScrollRate
		{
			get
			{
				if (this.wonGame)
				{
					float num = SongDefOf.EndCreditsSong.clip.length + 5f - 6f;
					return this.MaxScrollPosition / num;
				}
				return 30f;
			}
		}

		public Screen_Credits() : this(string.Empty)
		{
		}

		public Screen_Credits(string preCreditsMessage)
		{
			this.doWindowBackground = false;
			this.doCloseButton = false;
			this.doCloseX = false;
			this.closeOnEscapeKey = true;
			this.forcePause = true;
			this.creds = CreditsAssembler.AllCredits().ToList<CreditsEntry>();
			this.creds.Insert(0, new CreditRecord_Space(100f));
			if (!preCreditsMessage.NullOrEmpty())
			{
				this.creds.Insert(1, new CreditRecord_Space(200f));
				this.creds.Insert(2, new CreditRecord_Text(preCreditsMessage, TextAnchor.UpperLeft));
				this.creds.Insert(3, new CreditRecord_Space(50f));
			}
			this.creds.Add(new CreditRecord_Space(300f));
			this.creds.Add(new CreditRecord_Text("ThanksForPlaying".Translate(), TextAnchor.UpperCenter));
		}

		public override void PreOpen()
		{
			base.PreOpen();
			this.creationRealtime = Time.realtimeSinceStartup;
			if (this.wonGame)
			{
				this.timeUntilAutoScroll = 6f;
			}
			else
			{
				this.timeUntilAutoScroll = 1f;
			}
		}

		public override void WindowUpdate()
		{
			base.WindowUpdate();
			if (this.timeUntilAutoScroll > 0f)
			{
				this.timeUntilAutoScroll -= Time.deltaTime;
			}
			else
			{
				this.scrollPosition += this.AutoScrollRate * Time.deltaTime;
			}
			if (this.wonGame && !this.playedMusic && Time.realtimeSinceStartup > this.creationRealtime + 5f)
			{
				Find.MusicManagerPlay.ForceStartSong(SongDefOf.EndCreditsSong, true);
				this.playedMusic = true;
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = new Rect(0f, 0f, (float)UI.screenWidth, (float)UI.screenHeight);
			GUI.DrawTexture(rect, BaseContent.BlackTex);
			Rect position = new Rect(rect);
			position.yMin += 30f;
			position.yMax -= 30f;
			position.xMin = rect.center.x - 400f;
			position.width = 800f;
			float viewWidth = this.ViewWidth;
			float viewHeight = this.ViewHeight;
			this.scrollPosition = Mathf.Clamp(this.scrollPosition, 0f, this.MaxScrollPosition);
			GUI.BeginGroup(position);
			Rect position2 = new Rect(0f, 0f, viewWidth, viewHeight);
			position2.y -= this.scrollPosition;
			GUI.BeginGroup(position2);
			Text.Font = GameFont.Medium;
			float num = 0f;
			foreach (CreditsEntry current in this.creds)
			{
				float num2 = current.DrawHeight(position2.width);
				Rect rect2 = new Rect(0f, num, position2.width, num2);
				current.Draw(rect2);
				num += num2;
			}
			GUI.EndGroup();
			GUI.EndGroup();
			if (Event.current.type == EventType.ScrollWheel)
			{
				this.Scroll(Event.current.delta.y * 25f);
				Event.current.Use();
			}
			if (Event.current.type == EventType.KeyDown)
			{
				if (Event.current.keyCode == KeyCode.DownArrow)
				{
					this.Scroll(250f);
					Event.current.Use();
				}
				if (Event.current.keyCode == KeyCode.UpArrow)
				{
					this.Scroll(-250f);
					Event.current.Use();
				}
			}
		}

		private void Scroll(float offset)
		{
			this.scrollPosition += offset;
			this.timeUntilAutoScroll = 3f;
		}
	}
}
