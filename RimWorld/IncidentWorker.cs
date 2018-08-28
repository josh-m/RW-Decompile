using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
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

		public bool CanFireNow(IncidentParms parms, bool forced = false)
		{
			if (!parms.forced)
			{
				if (!this.def.TargetAllowed(parms.target))
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
				if (this.def.allowedBiomes != null)
				{
					BiomeDef biome = Find.WorldGrid[parms.target.Tile].biome;
					if (!this.def.allowedBiomes.Contains(biome))
					{
						return false;
					}
				}
				Scenario scenario = Find.Scenario;
				for (int i = 0; i < scenario.parts.Count; i++)
				{
					ScenPart_DisableIncident scenPart_DisableIncident = scenario.parts[i] as ScenPart_DisableIncident;
					if (scenPart_DisableIncident != null && scenPart_DisableIncident.Incident == this.def)
					{
						return false;
					}
				}
				if (this.def.minPopulation > 0 && PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.Count<Pawn>() < this.def.minPopulation)
				{
					return false;
				}
				Dictionary<IncidentDef, int> lastFireTicks = parms.target.StoryState.lastFireTicks;
				int ticksGame = Find.TickManager.TicksGame;
				int num;
				if (lastFireTicks.TryGetValue(this.def, out num))
				{
					float num2 = (float)(ticksGame - num) / 60000f;
					if (num2 < this.def.minRefireDays)
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
							if (num3 < this.def.minRefireDays)
							{
								return false;
							}
						}
					}
				}
				if (this.def.minGreatestPopulation > 0 && Find.StoryWatcher.statsRecord.greatestPopulation < this.def.minGreatestPopulation)
				{
					return false;
				}
			}
			return this.CanFireNowSub(parms);
		}

		protected virtual bool CanFireNowSub(IncidentParms parms)
		{
			return true;
		}

		public bool TryExecute(IncidentParms parms)
		{
			bool flag = this.TryExecuteWorker(parms);
			if (flag)
			{
				if (this.def.tale != null)
				{
					Pawn pawn = null;
					if (parms.target is Caravan)
					{
						pawn = ((Caravan)parms.target).RandomOwner();
					}
					else if (parms.target is Map)
					{
						pawn = ((Map)parms.target).mapPawns.FreeColonistsSpawned.RandomElementWithFallback(null);
					}
					else if (parms.target is World)
					{
						pawn = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.RandomElementWithFallback(null);
					}
					if (pawn != null)
					{
						TaleRecorder.RecordTale(this.def.tale, new object[]
						{
							pawn
						});
					}
				}
				if (this.def.category.tale != null)
				{
					Tale tale = TaleRecorder.RecordTale(this.def.category.tale, new object[0]);
					if (tale != null)
					{
						tale.customLabel = this.def.label;
					}
				}
			}
			return flag;
		}

		protected virtual bool TryExecuteWorker(IncidentParms parms)
		{
			Log.Error("Unimplemented incident " + this, false);
			return false;
		}

		protected void SendStandardLetter()
		{
			if (this.def.letterLabel.NullOrEmpty() || this.def.letterText.NullOrEmpty())
			{
				Log.Error("Sending standard incident letter with no label or text.", false);
			}
			Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterDef, null);
		}

		protected void SendStandardLetter(LookTargets lookTargets, Faction relatedFaction = null, params string[] textArgs)
		{
			if (this.def.letterLabel.NullOrEmpty() || this.def.letterText.NullOrEmpty())
			{
				Log.Error("Sending standard incident letter with no label or text.", false);
			}
			string text = string.Format(this.def.letterText, textArgs).CapitalizeFirst();
			Find.LetterStack.ReceiveLetter(this.def.letterLabel, text, this.def.letterDef, lookTargets, relatedFaction, null);
		}
	}
}
