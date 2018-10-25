using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Pawn_TrainingTracker : IExposable
	{
		private Pawn pawn;

		private DefMap<TrainableDef, bool> wantedTrainables = new DefMap<TrainableDef, bool>();

		private DefMap<TrainableDef, int> steps = new DefMap<TrainableDef, int>();

		private DefMap<TrainableDef, bool> learned = new DefMap<TrainableDef, bool>();

		private int countDecayFrom;

		public Pawn_TrainingTracker(Pawn pawn)
		{
			this.pawn = pawn;
			this.countDecayFrom = Find.TickManager.TicksGame;
		}

		public void ExposeData()
		{
			Scribe_Deep.Look<DefMap<TrainableDef, bool>>(ref this.wantedTrainables, "wantedTrainables", new object[0]);
			Scribe_Deep.Look<DefMap<TrainableDef, int>>(ref this.steps, "steps", new object[0]);
			Scribe_Deep.Look<DefMap<TrainableDef, bool>>(ref this.learned, "learned", new object[0]);
			Scribe_Values.Look<int>(ref this.countDecayFrom, "countDecayFrom", 0, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.PawnTrainingTrackerPostLoadInit(this, ref this.wantedTrainables, ref this.steps, ref this.learned);
			}
		}

		public bool GetWanted(TrainableDef td)
		{
			return this.wantedTrainables[td];
		}

		private void SetWanted(TrainableDef td, bool wanted)
		{
			this.wantedTrainables[td] = wanted;
		}

		internal int GetSteps(TrainableDef td)
		{
			return this.steps[td];
		}

		public bool CanBeTrained(TrainableDef td)
		{
			if (this.steps[td] >= td.steps)
			{
				return false;
			}
			List<TrainableDef> prerequisites = td.prerequisites;
			if (!prerequisites.NullOrEmpty<TrainableDef>())
			{
				for (int i = 0; i < prerequisites.Count; i++)
				{
					if (!this.HasLearned(prerequisites[i]) || this.CanBeTrained(prerequisites[i]))
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool HasLearned(TrainableDef td)
		{
			return this.learned[td];
		}

		public AcceptanceReport CanAssignToTrain(TrainableDef td)
		{
			bool flag;
			return this.CanAssignToTrain(td, out flag);
		}

		public AcceptanceReport CanAssignToTrain(TrainableDef td, out bool visible)
		{
			if (this.pawn.RaceProps.untrainableTags != null)
			{
				for (int i = 0; i < this.pawn.RaceProps.untrainableTags.Count; i++)
				{
					if (td.MatchesTag(this.pawn.RaceProps.untrainableTags[i]))
					{
						visible = false;
						return false;
					}
				}
			}
			if (this.pawn.RaceProps.trainableTags != null)
			{
				int j = 0;
				while (j < this.pawn.RaceProps.trainableTags.Count)
				{
					if (td.MatchesTag(this.pawn.RaceProps.trainableTags[j]))
					{
						if (this.pawn.BodySize < td.minBodySize)
						{
							visible = true;
							return new AcceptanceReport("CannotTrainTooSmall".Translate(this.pawn.LabelCapNoCount, this.pawn));
						}
						visible = true;
						return true;
					}
					else
					{
						j++;
					}
				}
			}
			if (!td.defaultTrainable)
			{
				visible = false;
				return false;
			}
			if (this.pawn.BodySize < td.minBodySize)
			{
				visible = true;
				return new AcceptanceReport("CannotTrainTooSmall".Translate(this.pawn.LabelCapNoCount, this.pawn));
			}
			if (this.pawn.RaceProps.trainability.intelligenceOrder < td.requiredTrainability.intelligenceOrder)
			{
				visible = true;
				return new AcceptanceReport("CannotTrainNotSmartEnough".Translate(td.requiredTrainability));
			}
			visible = true;
			return true;
		}

		public TrainableDef NextTrainableToTrain()
		{
			List<TrainableDef> trainableDefsInListOrder = TrainableUtility.TrainableDefsInListOrder;
			for (int i = 0; i < trainableDefsInListOrder.Count; i++)
			{
				if (this.GetWanted(trainableDefsInListOrder[i]) && this.CanBeTrained(trainableDefsInListOrder[i]))
				{
					return trainableDefsInListOrder[i];
				}
			}
			return null;
		}

		public void Train(TrainableDef td, Pawn trainer, bool complete = false)
		{
			if (complete)
			{
				this.steps[td] = td.steps;
			}
			else
			{
				DefMap<TrainableDef, int> defMap;
				(defMap = this.steps)[td] = defMap[td] + 1;
			}
			if (this.steps[td] >= td.steps)
			{
				this.learned[td] = true;
				if (td == TrainableDefOf.Obedience && trainer != null && this.pawn.playerSettings != null && this.pawn.playerSettings.Master == null)
				{
					this.pawn.playerSettings.Master = trainer;
				}
			}
		}

		public void SetWantedRecursive(TrainableDef td, bool checkOn)
		{
			this.SetWanted(td, checkOn);
			if (checkOn)
			{
				if (td.prerequisites != null)
				{
					for (int i = 0; i < td.prerequisites.Count; i++)
					{
						this.SetWantedRecursive(td.prerequisites[i], true);
					}
				}
			}
			else
			{
				IEnumerable<TrainableDef> enumerable = from t in DefDatabase<TrainableDef>.AllDefsListForReading
				where t.prerequisites != null && t.prerequisites.Contains(td)
				select t;
				foreach (TrainableDef current in enumerable)
				{
					this.SetWantedRecursive(current, false);
				}
			}
		}

		public void TrainingTrackerTickRare()
		{
			if (this.pawn.Suspended)
			{
				this.countDecayFrom += 250;
				return;
			}
			if (!this.pawn.Spawned)
			{
				this.countDecayFrom += 250;
				return;
			}
			if (this.steps[TrainableDefOf.Tameness] == 0)
			{
				this.countDecayFrom = Find.TickManager.TicksGame;
				return;
			}
			if (Find.TickManager.TicksGame < this.countDecayFrom + TrainableUtility.DegradationPeriodTicks(this.pawn.def))
			{
				return;
			}
			TrainableDef trainableDef = (from kvp in this.steps
			where kvp.Value > 0
			select kvp.Key).Except((from kvp in this.steps
			where kvp.Value > 0 && kvp.Key.prerequisites != null
			select kvp).SelectMany((KeyValuePair<TrainableDef, int> kvp) => kvp.Key.prerequisites)).RandomElement<TrainableDef>();
			if (trainableDef == TrainableDefOf.Tameness && !TrainableUtility.TamenessCanDecay(this.pawn.def))
			{
				this.countDecayFrom = Find.TickManager.TicksGame;
				return;
			}
			this.countDecayFrom = Find.TickManager.TicksGame;
			DefMap<TrainableDef, int> defMap;
			TrainableDef def;
			(defMap = this.steps)[def = trainableDef] = defMap[def] - 1;
			if (this.steps[trainableDef] <= 0 && this.learned[trainableDef])
			{
				this.learned[trainableDef] = false;
				if (this.pawn.Faction == Faction.OfPlayer)
				{
					if (trainableDef == TrainableDefOf.Tameness)
					{
						this.pawn.SetFaction(null, null);
						Messages.Message("MessageAnimalReturnedWild".Translate(this.pawn.LabelShort, this.pawn), this.pawn, MessageTypeDefOf.NegativeEvent, true);
					}
					else
					{
						Messages.Message("MessageAnimalLostSkill".Translate(this.pawn.LabelShort, trainableDef.LabelCap, this.pawn.Named("ANIMAL")), this.pawn, MessageTypeDefOf.NegativeEvent, true);
					}
				}
			}
		}

		public void Debug_MakeDegradeHappenSoon()
		{
			this.countDecayFrom = Find.TickManager.TicksGame - TrainableUtility.DegradationPeriodTicks(this.pawn.def) - 500;
		}
	}
}
