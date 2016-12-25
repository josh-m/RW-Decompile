using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class Dialog_NodeTree : Window
	{
		private const float InteractivityDelay = 0.5f;

		private Vector2 scrollPosition;

		protected DiaNode curNode;

		public Action closeAction;

		private float makeInteractiveAtTime;

		public Color screenFillColor = Color.clear;

		private float optTotalHeight;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(620f, 480f);
			}
		}

		private bool InteractiveNow
		{
			get
			{
				return Time.realtimeSinceStartup >= this.makeInteractiveAtTime;
			}
		}

		public Dialog_NodeTree(DiaNode nodeRoot, bool delayInteractivity = false, bool radioMode = false)
		{
			this.GotoNode(nodeRoot);
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
			this.closeOnEscapeKey = false;
			if (delayInteractivity)
			{
				this.makeInteractiveAtTime = Time.realtimeSinceStartup + 0.5f;
				this.closeOnEscapeKey = false;
			}
			this.soundAppear = SoundDefOf.CommsWindow_Open;
			this.soundClose = SoundDefOf.CommsWindow_Close;
			if (radioMode)
			{
				this.soundAmbient = SoundDefOf.RadioComms_Ambience;
			}
		}

		public override void PreClose()
		{
			base.PreClose();
			this.curNode.PreClose();
		}

		public override void PostClose()
		{
			base.PostClose();
			if (this.closeAction != null)
			{
				this.closeAction();
			}
		}

		public override void WindowOnGUI()
		{
			if (this.screenFillColor != Color.clear)
			{
				GUI.color = this.screenFillColor;
				GUI.DrawTexture(new Rect(0f, 0f, (float)UI.screenWidth, (float)UI.screenHeight), BaseContent.WhiteTex);
				GUI.color = Color.white;
			}
			base.WindowOnGUI();
		}

		public override void DoWindowContents(Rect inRect)
		{
			this.DrawNode(inRect.AtZero());
		}

		protected void DrawNode(Rect rect)
		{
			if (this.InteractiveNow)
			{
				this.closeOnEscapeKey = true;
			}
			GUI.BeginGroup(rect);
			Text.Font = GameFont.Small;
			if (Event.current.type == EventType.Repaint)
			{
				Rect outRect = new Rect(0f, 0f, rect.width, rect.height - this.optTotalHeight);
				float width = rect.width - 16f;
				Rect rect2 = new Rect(0f, 0f, width, Text.CalcHeight(this.curNode.text, width));
				Widgets.BeginScrollView(outRect, ref this.scrollPosition, rect2);
				Widgets.Label(rect2, this.curNode.text);
				Widgets.EndScrollView();
			}
			float num = rect.height - this.optTotalHeight;
			float num2 = 0f;
			for (int i = 0; i < this.curNode.options.Count; i++)
			{
				Rect rect3 = new Rect(15f, num, rect.width - 30f, 999f);
				float num3 = this.curNode.options[i].OptOnGUI(rect3, this.InteractiveNow);
				num += num3 + 7f;
				num2 += num3 + 7f;
			}
			if (Event.current.type == EventType.Layout)
			{
				this.optTotalHeight = num2;
			}
			GUI.EndGroup();
		}

		public void GotoNode(DiaNode node)
		{
			foreach (DiaOption current in node.options)
			{
				current.dialog = this;
			}
			this.curNode = node;
		}
	}
}
