using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Pawn_TrainingTracker : IExposable
	{
		private Pawn pawn;

		private DefMap<TrainableDef, bool> wantedTrainables = new DefMap<TrainableDef, bool>();

		private DefMap<TrainableDef, int> steps = new DefMap<TrainableDef, int>();

		public Pawn_TrainingTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Deep.LookDeep<DefMap<TrainableDef, bool>>(ref this.wantedTrainables, "wantedTrainables", new object[0]);
			Scribe_Deep.LookDeep<DefMap<TrainableDef, int>>(ref this.steps, "steps", new object[0]);
		}

		public bool GetWanted(TrainableDef td)
		{
			return this.wantedTrainables[td];
		}

		public void SetWanted(TrainableDef td, bool wanted)
		{
			this.wantedTrainables[td] = wanted;
		}

		internal int GetSteps(TrainableDef td)
		{
			return this.steps[td];
		}

		public bool IsCompleted(TrainableDef td)
		{
			return this.steps[td] >= td.steps;
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
							return new AcceptanceReport("CannotTrainTooSmall".Translate(new object[]
							{
								this.pawn.LabelCapNoCount
							}));
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
				return new AcceptanceReport("CannotTrainTooSmall".Translate(new object[]
				{
					this.pawn.LabelCapNoCount
				}));
			}
			if (this.pawn.RaceProps.trainableIntelligence < td.requiredTrainableIntelligence)
			{
				visible = true;
				return new AcceptanceReport("CannotTrainNotSmartEnough".Translate(new object[]
				{
					td.requiredTrainableIntelligence
				}));
			}
			visible = true;
			return true;
		}

		public TrainableDef NextTrainableToTrain()
		{
			List<TrainableDef> trainableDefsInListOrder = TrainableUtility.TrainableDefsInListOrder;
			for (int i = 0; i < trainableDefsInListOrder.Count; i++)
			{
				if (this.GetWanted(trainableDefsInListOrder[i]) && !this.IsCompleted(trainableDefsInListOrder[i]))
				{
					return trainableDefsInListOrder[i];
				}
			}
			return null;
		}

		public void Train(TrainableDef td, Pawn trainer)
		{
			DefMap<TrainableDef, int> defMap;
			DefMap<TrainableDef, int> expr_06 = defMap = this.steps;
			int num = defMap[td];
			expr_06[td] = num + 1;
			if (this.IsCompleted(td) && td == TrainableDefOf.Obedience)
			{
				this.pawn.playerSettings.master = trainer;
			}
		}
	}
}
