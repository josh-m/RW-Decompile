using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class NeedDef : Def
	{
		public Type needClass;

		public Intelligence minIntelligence;

		public bool colonistAndPrisonersOnly;

		public bool colonistsOnly;

		public bool onlyIfCausedByHediff;

		public bool neverOnPrisoner;

		public bool showOnNeedList = true;

		public float baseLevel = 0.5f;

		public bool major;

		public int listPriority;

		public string tutorHighlightTag;

		public bool showForCaravanMembers;

		public float fallPerDay = 0.5f;

		public float seekerRisePerHour;

		public float seekerFallPerHour;

		public bool freezeWhileSleeping;

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string e in base.ConfigErrors())
			{
				yield return e;
			}
			if (this.description.NullOrEmpty())
			{
				yield return "no description";
			}
			if (this.needClass == null)
			{
				yield return "needClass is null";
			}
			if (this.needClass == typeof(Need_Seeker) && (this.seekerRisePerHour == 0f || this.seekerFallPerHour == 0f))
			{
				yield return "seeker rise/fall rates not set";
			}
		}
	}
}
