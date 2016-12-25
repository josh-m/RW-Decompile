using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class IncidentWorker
	{
		public IncidentDef def;

		public virtual float AdjustedChance
		{
			get
			{
				return this.def.baseChance;
			}
		}

		public bool CanFireNow(IIncidentTarget target)
		{
			if (!this.def.TargetAllowed(target))
			{
				return false;
			}
			if (GenDate.DaysPassed < this.def.earliestDay)
			{
				return false;
			}
			if (Find.Storyteller.difficulty.difficulty < this.def.minDifficulty)
			{
				return false;
			}
			BiomeDef biome = Find.WorldGrid[target.Tile].biome;
			if (this.def.allowedBiomes != null && !this.def.allowedBiomes.Contains(biome))
			{
				return false;
			}
			for (int i = 0; i < Find.Scenario.parts.Count; i++)
			{
				ScenPart_DisableIncident scenPart_DisableIncident = Find.Scenario.parts[i] as ScenPart_DisableIncident;
				if (scenPart_DisableIncident != null && scenPart_DisableIncident.Incident == this.def)
				{
					return false;
				}
			}
			Dictionary<IncidentDef, int> lastFireTicks = Find.StoryWatcher.storyState.lastFireTicks;
			int ticksGame = Find.TickManager.TicksGame;
			int num;
			if (lastFireTicks.TryGetValue(this.def, out num))
			{
				float num2 = (float)(ticksGame - num) / 60000f;
				if (num2 < (float)this.def.minRefireDays)
				{
					return false;
				}
			}
			List<IncidentDef> refireCheckIncidents = this.def.RefireCheckIncidents;
			if (refireCheckIncidents != null)
			{
				for (int j = 0; j < refireCheckIncidents.Count; j++)
				{
					if (lastFireTicks.TryGetValue(refireCheckIncidents[j], out num))
					{
						float num3 = (float)(ticksGame - num) / 60000f;
						if (num3 < (float)this.def.minRefireDays)
						{
							return false;
						}
					}
				}
			}
			return this.CanFireNowSub(target);
		}

		protected virtual bool CanFireNowSub(IIncidentTarget target)
		{
			return true;
		}

		public virtual bool TryExecute(IncidentParms parms)
		{
			Log.Error("Unimplemented incident " + this);
			return false;
		}

		protected void SendStandardLetter()
		{
			if (this.def.letterLabel.NullOrEmpty() || this.def.letterText.NullOrEmpty())
			{
				Log.Error("Sending standard incident letter with no label or text.");
			}
			Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterType, null);
		}

		protected void SendStandardLetter(TargetInfo target, params string[] textArgs)
		{
			if (this.def.letterLabel.NullOrEmpty() || this.def.letterText.NullOrEmpty())
			{
				Log.Error("Sending standard incident letter with no label or text.");
			}
			string text = string.Format(this.def.letterText, textArgs);
			Find.LetterStack.ReceiveLetter(this.def.letterLabel, text, this.def.letterType, target, null);
		}
	}
}
