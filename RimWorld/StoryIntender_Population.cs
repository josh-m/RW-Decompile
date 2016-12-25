using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StoryIntender_Population : IExposable
	{
		public Storyteller teller;

		private int lastPopGainTime = 100000;

		private StorytellerDef Def
		{
			get
			{
				return this.teller.def;
			}
		}

		private int TimeSinceLastGain
		{
			get
			{
				return Find.TickManager.TicksGame - this.lastPopGainTime;
			}
		}

		public virtual float PopulationIntent
		{
			get
			{
				return StoryIntender_Population.CalculatePopulationIntent(this.Def, this.AdjustedPopulation, this.TimeSinceLastGain);
			}
		}

		public float AdjustedPopulation
		{
			get
			{
				float num = 0f;
				num += (float)PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Colonists.Count<Pawn>();
				return num + (float)PawnsFinder.AllMapsCaravansAndTravelingTransportPods.Count((Pawn x) => x.IsPrisonerOfColony) * 0.5f;
			}
		}

		public string DebugReadout
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("IntenderPopulation");
				stringBuilder.AppendLine("   adjusted population: " + this.AdjustedPopulation);
				stringBuilder.AppendLine("   intent : " + this.PopulationIntent);
				stringBuilder.AppendLine("   last gain time : " + this.lastPopGainTime);
				stringBuilder.AppendLine("   time since last gain: " + this.TimeSinceLastGain);
				return stringBuilder.ToString();
			}
		}

		public StoryIntender_Population()
		{
		}

		public StoryIntender_Population(Storyteller teller)
		{
			this.teller = teller;
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.lastPopGainTime, "lastPopGainTime", 0, false);
		}

		public void Notify_PopulationGained()
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				this.lastPopGainTime = Find.TickManager.TicksGame;
			}
		}

		private static float CalculatePopulationIntent(StorytellerDef def, float curPop, int ticksSinceGain)
		{
			if (curPop <= def.desiredPopulationMin)
			{
				return GenMath.LerpDouble(1f, def.desiredPopulationMin, 4f, 1f, curPop);
			}
			float num;
			if (curPop >= def.desiredPopulationCritical)
			{
				num = -1f;
			}
			else if (curPop <= def.desiredPopulationMax)
			{
				num = 1f - Mathf.InverseLerp(def.desiredPopulationMin, def.desiredPopulationMax, curPop);
			}
			else
			{
				num = -1f * Mathf.InverseLerp(def.desiredPopulationMax, def.desiredPopulationCritical, curPop);
			}
			if (num > 0f)
			{
				float value = (float)ticksSinceGain / 60000f;
				float num2 = Mathf.InverseLerp((float)def.desiredPopulationGainIntervalMinDays, (float)def.desiredPopulationGainIntervalMaxDays, value);
				num2 = Mathf.Clamp01(num2);
				num2 = Mathf.Max(num2, 0.15f);
				num *= num2;
			}
			return num;
		}

		public void DoTable_PopulationIntents()
		{
			List<float> list = new List<float>();
			for (int i = 0; i < 30; i++)
			{
				list.Add((float)i);
			}
			List<float> list2 = new List<float>();
			for (int j = 0; j < 40; j += 3)
			{
				list2.Add((float)j);
			}
			DebugTables.MakeTablesDialog<float, float>(list2, (float ds) => "day " + ds.ToString("F0"), list, (float rv) => rv.ToString("F2"), (float ds, float p) => StoryIntender_Population.CalculatePopulationIntent(this.Def, p, (int)(ds * 60000f)).ToString("F2"), "pop");
		}
	}
}
