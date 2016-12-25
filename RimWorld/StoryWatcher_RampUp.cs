using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StoryWatcher_RampUp : IExposable
	{
		private const int UpdateInterval = 5000;

		private const float ShortFactor_GameStartGraceDays = 21f;

		private const float ShortFactor_DaysToDouble = 162f;

		private const float LongFactor_GameStartGraceDays = 42f;

		private const float LongFactor_DaysToDouble = 360f;

		private float shortTermFactor = 1f;

		private float longTermFactor = 1f;

		public float TotalThreatPointsFactor
		{
			get
			{
				return this.shortTermFactor * this.longTermFactor;
			}
		}

		public float ShortTermFactor
		{
			get
			{
				return this.shortTermFactor;
			}
		}

		public float LongTermFactor
		{
			get
			{
				return this.longTermFactor;
			}
		}

		public void Notify_PlayerPawnIncappedOrKilled(Pawn p)
		{
			if (!p.RaceProps.Humanlike)
			{
				return;
			}
			float num = this.shortTermFactor - 1f;
			float num2 = this.longTermFactor - 1f;
			switch (PawnsFinder.AllMapsCaravansAndTravelingTransportPods_FreeColonists.Count<Pawn>())
			{
			case 0:
				num *= 0f;
				num2 *= 0f;
				break;
			case 1:
				num *= 0f;
				num2 *= 0f;
				break;
			case 2:
				num *= 0f;
				num2 *= 0f;
				break;
			case 3:
				num *= 0f;
				num2 *= 0.2f;
				break;
			case 4:
				num *= 0.15f;
				num2 *= 0.4f;
				break;
			case 5:
				num *= 0.25f;
				num2 *= 0.6f;
				break;
			case 6:
				num *= 0.3f;
				num2 *= 0.7f;
				break;
			case 7:
				num *= 0.35f;
				num2 *= 0.75f;
				break;
			case 8:
				num *= 0.4f;
				num2 *= 0.8f;
				break;
			case 9:
				num *= 0.45f;
				num2 *= 0.85f;
				break;
			default:
				num *= 0.5f;
				num2 *= 0.9f;
				break;
			}
			this.shortTermFactor = 1f + num;
			this.longTermFactor = 1f + num2;
		}

		public void RampUpWatcherTick()
		{
			if (Find.TickManager.TicksGame % 5000 == 0)
			{
				if ((float)GenDate.DaysPassed >= 21f)
				{
					this.shortTermFactor += 0.000514403335f;
				}
				if ((float)GenDate.DaysPassed >= 42f)
				{
					this.longTermFactor += 0.000231481492f;
				}
			}
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<float>(ref this.shortTermFactor, "shortTermFactor", 0f, false);
			Scribe_Values.LookValue<float>(ref this.longTermFactor, "longTermFactor", 0f, false);
		}
	}
}
