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

		public List<Letter> LettersListForReading
		{
			get
			{
				return this.letters;
			}
		}

		public float LastTopY
		{
			get
			{
				return this.lastTopYInt;
			}
		}

		public void ReceiveLetter(string label, string text, LetterDef textLetterDef, GlobalTargetInfo lookTarget, string debugInfo = null)
		{
			ChoiceLetter let = LetterMaker.MakeLetter(label, text, textLetterDef, lookTarget);
			this.ReceiveLetter(let, debugInfo);
		}

		public void ReceiveLetter(string label, string text, LetterDef textLetterDef, string debugInfo = null)
		{
			ChoiceLetter let = LetterMaker.MakeLetter(label, text, textLetterDef);
			this.ReceiveLetter(let, debugInfo);
		}

		public void ReceiveLetter(Letter let, string debugInfo = null)
		{
			let.def.arriveSound.PlayOneShotOnCamera(null);
			if (let.def == LetterDefOf.BadUrgent && Prefs.PauseOnUrgentLetter && !Find.TickManager.Paused)
			{
				Find.TickManager.TogglePaused();
			}
			this.letters.Add(let);
			let.arrivalTime = Time.time;
			let.debugInfo = debugInfo;
			let.Received();
		}

		public void RemoveLetter(Letter let)
		{
			this.letters.Remove(let);
			let.Removed();
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

		public void LetterStackTick()
		{
			int num = Find.TickManager.TicksGame + 1;
			for (int i = 0; i < this.letters.Count; i++)
			{
				LetterWithTimeout letterWithTimeout = this.letters[i] as LetterWithTimeout;
				if (letterWithTimeout != null && letterWithTimeout.TimeoutActive && letterWithTimeout.disappearAtTick == num)
				{
					letterWithTimeout.OpenLetter();
					break;
				}
			}
		}

		public void LetterStackUpdate()
		{
			if (this.mouseoverLetterIndex >= 0 && this.mouseoverLetterIndex < this.letters.Count)
			{
				GlobalTargetInfo lookTarget = this.letters[this.mouseoverLetterIndex].lookTarget;
				if (lookTarget.IsValid && lookTarget.IsMapTarget && lookTarget.Map == Find.VisibleMap)
				{
					GenDraw.DrawArrowPointingAt(((TargetInfo)lookTarget).CenterVector3, false);
				}
			}
			this.mouseoverLetterIndex = -1;
			for (int i = this.letters.Count - 1; i >= 0; i--)
			{
				if (!this.letters[i].StillValid)
				{
					this.RemoveLetter(this.letters[i]);
				}
			}
		}

		public void Notify_LetterMouseover(Letter let)
		{
			this.mouseoverLetterIndex = this.letters.IndexOf(let);
		}

		public void ExposeData()
		{
			Scribe_Collections.Look<Letter>(ref this.letters, "letters", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (this.letters.RemoveAll((Letter x) => x == null) != 0)
				{
					Log.Error("Some letters were null.");
				}
			}
		}
	}
}
