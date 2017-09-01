using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	public class MentalBreaker : IExposable
	{
		private const int CheckInterval = 150;

		private const float ExtremeBreakMTBDays = 0.7f;

		private const float MajorBreakMTBDays = 3f;

		private const float MinorBreakMTBDays = 10f;

		private const int MinTicksBelowToBreak = 1500;

		private const float MajorBreakMoodSpan = 0.15f;

		private const float MinorBreakMoodSpan = 0.15f;

		private Pawn pawn;

		private int ticksBelowExtreme;

		private int ticksBelowMajor;

		private int ticksBelowMinor;

		private static List<Thought> tmpThoughts = new List<Thought>();

		public float BreakThresholdExtreme
		{
			get
			{
				return this.pawn.GetStatValue(StatDefOf.MentalBreakThreshold, true);
			}
		}

		public float BreakThresholdMajor
		{
			get
			{
				return this.pawn.GetStatValue(StatDefOf.MentalBreakThreshold, true) + 0.15f;
			}
		}

		public float BreakThresholdMinor
		{
			get
			{
				return this.pawn.GetStatValue(StatDefOf.MentalBreakThreshold, true) + 0.15f + 0.15f;
			}
		}

		private bool CanDoRandomMentalBreaks
		{
			get
			{
				return this.pawn.RaceProps.Humanlike && (this.pawn.Spawned || this.pawn.IsCaravanMember());
			}
		}

		public bool BreakExtremeIsImminent
		{
			get
			{
				return this.pawn.MentalStateDef == null && this.CurMood < this.BreakThresholdExtreme;
			}
		}

		public bool BreakMajorIsImminent
		{
			get
			{
				return this.pawn.MentalStateDef == null && !this.BreakExtremeIsImminent && this.CurMood < this.BreakThresholdMajor;
			}
		}

		public bool BreakMinorIsImminent
		{
			get
			{
				return this.pawn.MentalStateDef == null && !this.BreakExtremeIsImminent && !this.BreakMajorIsImminent && this.CurMood < this.BreakThresholdMinor;
			}
		}

		public bool BreakExtremeIsApproaching
		{
			get
			{
				return this.pawn.MentalStateDef == null && !this.BreakExtremeIsImminent && this.CurMood < this.BreakThresholdExtreme + 0.1f;
			}
		}

		private float CurMood
		{
			get
			{
				if (this.pawn.needs.mood == null)
				{
					return 0.5f;
				}
				return this.pawn.needs.mood.CurLevel;
			}
		}

		private IEnumerable<MentalBreakDef> CurrentPossibleMoodBreaks
		{
			get
			{
				MentalBreakIntensity intensity = this.CurrentDesiredMoodBreakIntensity;
				while (true)
				{
					IEnumerable<MentalBreakDef> breaks = from d in DefDatabase<MentalBreakDef>.AllDefsListForReading
					where this.<>f__this.BreakCanHappenNow(d, this.<intensity>__0)
					select d;
					bool yieldedOne = false;
					foreach (MentalBreakDef b in breaks)
					{
						yield return b;
					}
					if (yieldedOne)
					{
						break;
					}
					if (intensity == MentalBreakIntensity.Minor)
					{
						goto Block_2;
					}
					intensity -= 1;
				}
				return;
				Block_2:
				Log.ErrorOnce("No mental breaks possible for " + this.pawn, 888112);
			}
		}

		private MentalBreakIntensity CurrentDesiredMoodBreakIntensity
		{
			get
			{
				if (this.ticksBelowExtreme >= 1500)
				{
					return MentalBreakIntensity.Extreme;
				}
				if (this.ticksBelowMajor >= 1500)
				{
					return MentalBreakIntensity.Major;
				}
				if (this.ticksBelowMinor >= 1500)
				{
					return MentalBreakIntensity.Minor;
				}
				Log.ErrorOnce("Got CurrentDesiredBreakIntensity for " + this.pawn + " but he don't desire any break right now.", 123126);
				return MentalBreakIntensity.Minor;
			}
		}

		public MentalBreaker()
		{
		}

		public MentalBreaker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		internal void Reset()
		{
			this.ticksBelowExtreme = 0;
			this.ticksBelowMajor = 0;
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.ticksBelowExtreme, "ticksBelowExtreme", 0, false);
			Scribe_Values.Look<int>(ref this.ticksBelowMajor, "ticksBelowMajor", 0, false);
			Scribe_Values.Look<int>(ref this.ticksBelowMinor, "ticksBelowMinor", 0, false);
		}

		public void MentalStateStarterTick()
		{
			if (this.CanDoRandomMentalBreaks && this.pawn.MentalStateDef == null && this.pawn.IsHashIntervalTick(150))
			{
				if (!DebugSettings.enableRandomMentalStates)
				{
					return;
				}
				if (this.CurMood < this.BreakThresholdExtreme)
				{
					this.ticksBelowExtreme += 150;
				}
				else
				{
					this.ticksBelowExtreme = 0;
				}
				if (this.CurMood < this.BreakThresholdMajor)
				{
					this.ticksBelowMajor += 150;
				}
				else
				{
					this.ticksBelowMajor = 0;
				}
				if (this.CurMood < this.BreakThresholdMinor)
				{
					this.ticksBelowMinor += 150;
				}
				else
				{
					this.ticksBelowMinor = 0;
				}
				if (this.TestMoodMentalBreak() && this.TryDoRandomMoodCausedMentalBreak())
				{
					return;
				}
				if (this.pawn.story != null)
				{
					List<Trait> allTraits = this.pawn.story.traits.allTraits;
					for (int i = 0; i < allTraits.Count; i++)
					{
						TraitDegreeData currentData = allTraits[i].CurrentData;
						if (currentData.randomMentalState != null)
						{
							float mtb = currentData.randomMentalStateMtbDaysMoodCurve.Evaluate(this.CurMood);
							if (Rand.MTBEventOccurs(mtb, 60000f, 150f) && currentData.randomMentalState.Worker.StateCanOccur(this.pawn) && this.pawn.mindState.mentalStateHandler.TryStartMentalState(currentData.randomMentalState, null, false, false, null))
							{
								return;
							}
						}
					}
				}
			}
		}

		private bool TestMoodMentalBreak()
		{
			if (this.ticksBelowExtreme > 1500)
			{
				return Rand.MTBEventOccurs(0.7f, 60000f, 150f);
			}
			if (this.ticksBelowMajor > 1500)
			{
				return Rand.MTBEventOccurs(3f, 60000f, 150f);
			}
			return this.ticksBelowMinor > 1500 && Rand.MTBEventOccurs(10f, 60000f, 150f);
		}

		public bool TryDoRandomMoodCausedMentalBreak()
		{
			if (!this.CanDoRandomMentalBreaks || this.pawn.Downed || !this.pawn.Awake())
			{
				return false;
			}
			if (this.pawn.Faction != Faction.OfPlayer && this.CurrentDesiredMoodBreakIntensity != MentalBreakIntensity.Extreme)
			{
				return false;
			}
			MentalBreakDef mentalBreakDef;
			if (!this.CurrentPossibleMoodBreaks.TryRandomElementByWeight((MentalBreakDef d) => d.Worker.CommonalityFor(this.pawn), out mentalBreakDef))
			{
				return false;
			}
			Thought thought = this.RandomFinalStraw();
			string reason = (thought == null) ? null : thought.LabelCap;
			this.pawn.mindState.mentalStateHandler.TryStartMentalState(mentalBreakDef.mentalState, reason, false, true, null);
			return true;
		}

		private Thought RandomFinalStraw()
		{
			this.pawn.needs.mood.thoughts.GetAllMoodThoughts(MentalBreaker.tmpThoughts);
			float num = 0f;
			for (int i = 0; i < MentalBreaker.tmpThoughts.Count; i++)
			{
				float num2 = MentalBreaker.tmpThoughts[i].MoodOffset();
				if (num2 < num)
				{
					num = num2;
				}
			}
			float maxMoodOffset = num * 0.5f;
			Thought result = null;
			(from x in MentalBreaker.tmpThoughts
			where x.MoodOffset() <= maxMoodOffset
			select x).TryRandomElementByWeight((Thought x) => -x.MoodOffset(), out result);
			MentalBreaker.tmpThoughts.Clear();
			return result;
		}

		private bool BreakCanHappenNow(MentalBreakDef def, MentalBreakIntensity intensity)
		{
			if (def.intensity != intensity)
			{
				return false;
			}
			if (def.requiredTrait != null && (this.pawn.story == null || !this.pawn.story.traits.HasTrait(def.requiredTrait)))
			{
				return false;
			}
			if (this.pawn.story.traits.allTraits.Any((Trait tr) => tr.CurrentData.disallowedMentalStates != null && tr.CurrentData.disallowedMentalStates.Contains(def.mentalState)))
			{
				return false;
			}
			if (!def.mentalState.Worker.StateCanOccur(this.pawn))
			{
				return false;
			}
			IEnumerable<MentalBreakDef> source = from bd in this.pawn.story.traits.AllowedMentalBreaks
			where bd.mentalState.Worker.StateCanOccur(this.pawn)
			select bd;
			return this.pawn.story == null || !source.Any((MentalBreakDef b) => b.intensity == def.intensity) || source.Any((MentalBreakDef b) => b == def);
		}

		public float MentalBreakThresholdFor(MentalBreakIntensity intensity)
		{
			switch (intensity)
			{
			case MentalBreakIntensity.Extreme:
				return this.BreakThresholdExtreme;
			case MentalBreakIntensity.Major:
				return this.BreakThresholdMajor;
			case MentalBreakIntensity.Minor:
				return this.BreakThresholdMinor;
			default:
				throw new NotImplementedException();
			}
		}

		internal string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(this.pawn.ToString());
			stringBuilder.AppendLine(string.Concat(new object[]
			{
				"   ticksBelowExtreme=",
				this.ticksBelowExtreme,
				"/",
				1500
			}));
			stringBuilder.AppendLine(string.Concat(new object[]
			{
				"   ticksBelowSerious=",
				this.ticksBelowMajor,
				"/",
				1500
			}));
			stringBuilder.AppendLine(string.Concat(new object[]
			{
				"   ticksBelowMinor=",
				this.ticksBelowMinor,
				"/",
				1500
			}));
			float num = (from d in this.CurrentPossibleMoodBreaks
			select d.Worker.CommonalityFor(this.pawn)).Sum();
			foreach (MentalBreakDef current in this.CurrentPossibleMoodBreaks)
			{
				float num2 = current.Worker.CommonalityFor(this.pawn);
				stringBuilder.AppendLine(string.Concat(new object[]
				{
					"   ",
					current,
					" ",
					(num2 / num).ToStringPercent("F4")
				}));
			}
			return stringBuilder.ToString();
		}

		internal void LogPossibleMentalBreaks()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(this.pawn + " current possible mood mental breaks:");
			stringBuilder.AppendLine("CurrentDesiredMoodBreakIntensity: " + this.CurrentDesiredMoodBreakIntensity);
			foreach (MentalBreakDef current in this.CurrentPossibleMoodBreaks)
			{
				stringBuilder.AppendLine("  " + current);
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
