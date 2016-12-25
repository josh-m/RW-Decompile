using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public sealed class LetterStack : IExposable
	{
		private const float LettersBottomY = 350f;

		public const float LetterSpacing = 12f;

		private List<Letter> letters = new List<Letter>();

		private int mouseoverLetterIndex = -1;

		private float lastTopYInt;

		public float LastTopY
		{
			get
			{
				return this.lastTopYInt;
			}
		}

		public void ReceiveLetter(string label, string text, LetterType type, GlobalTargetInfo letterLookTarget, string debugText = null)
		{
			Letter let = new Letter(label, text, type, letterLookTarget);
			this.ReceiveLetter(let, debugText);
		}

		public void ReceiveLetter(string label, string text, LetterType type, string debugText = null)
		{
			Letter let = new Letter(label, text, type, GlobalTargetInfo.Invalid);
			this.ReceiveLetter(let, debugText);
		}

		public void ReceiveLetter(Letter let, string debugText = null)
		{
			SoundDef soundDef;
			if (let.LetterType == LetterType.BadUrgent)
			{
				soundDef = SoundDefOf.LetterArriveBadUrgent;
			}
			else
			{
				soundDef = SoundDefOf.LetterArrive;
			}
			soundDef.PlayOneShotOnCamera();
			if (let.LetterType == LetterType.BadUrgent && Prefs.PauseOnUrgentLetter && !Find.TickManager.Paused)
			{
				Find.TickManager.TogglePaused();
			}
			this.letters.Add(let);
			let.arrivalTime = Time.time;
		}

		public void RemoveLetter(Letter let)
		{
			this.letters.Remove(let);
		}

		public void LettersOnGUI(float baseY)
		{
			float num = baseY - 30f;
			for (int i = this.letters.Count - 1; i >= 0; i--)
			{
				this.letters[i].DrawButtonAt(num);
				num -= 42f;
			}
			this.lastTopYInt = num;
			num = baseY - 30f;
			for (int j = this.letters.Count - 1; j >= 0; j--)
			{
				this.letters[j].CheckForMouseOverTextAt(num);
				num -= 42f;
			}
		}

		public void LettersUpdate()
		{
			if (this.mouseoverLetterIndex >= 0 && this.letters.Count >= this.mouseoverLetterIndex + 1)
			{
				GlobalTargetInfo lookTarget = this.letters[this.mouseoverLetterIndex].lookTarget;
				if (lookTarget.IsValid && lookTarget.IsMapTarget && lookTarget.Map == Find.VisibleMap)
				{
					GenDraw.DrawArrowPointingAt(((TargetInfo)lookTarget).CenterVector3, false);
				}
			}
			this.mouseoverLetterIndex = -1;
			this.letters.RemoveAll((Letter l) => !l.Valid);
		}

		public void Notify_LetterMouseover(Letter let)
		{
			this.mouseoverLetterIndex = this.letters.IndexOf(let);
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<Letter>(ref this.letters, "letters", LookMode.Deep, new object[0]);
		}
	}
}
