using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public sealed class SituationalThoughtHandler
	{
		private class CachedSituationalSocialThoughts
		{
			private const int ExpireAfterTicks = 300;

			public List<Thought_SituationalSocial> thoughts = new List<Thought_SituationalSocial>();

			public List<Thought_SituationalSocial> activeThoughts = new List<Thought_SituationalSocial>();

			public int lastStateRecalculationTick = -99999;

			public int lastQueryTick = -99999;

			public bool Expired
			{
				get
				{
					return Find.TickManager.TicksGame - this.lastQueryTick >= 300;
				}
			}

			public bool ShouldRecalculateState
			{
				get
				{
					return Find.TickManager.TicksGame - this.lastStateRecalculationTick >= 100;
				}
			}
		}

		private const int RecalculateStateEveryTicks = 100;

		public Pawn pawn;

		private List<Thought_Situational> cachedSituationalThoughts = new List<Thought_Situational>();

		private int lastStateRecalculation = -99999;

		private Dictionary<Pawn, SituationalThoughtHandler.CachedSituationalSocialThoughts> cachedSituationalSocialThoughts = new Dictionary<Pawn, SituationalThoughtHandler.CachedSituationalSocialThoughts>();

		private Dictionary<Pawn, SituationalThoughtHandler.CachedSituationalSocialThoughts> cachedSituationalSocialThoughtsAffectingMood = new Dictionary<Pawn, SituationalThoughtHandler.CachedSituationalSocialThoughts>();

		private List<Thought_Situational> activeSituationalThoughts = new List<Thought_Situational>();

		private static List<Thought_SituationalSocial> emptySituationalSocial = new List<Thought_SituationalSocial>();

		private HashSet<ThoughtDef> tmpCachedThoughts = new HashSet<ThoughtDef>();

		private HashSet<Pair<ThoughtDef, Pawn>> tmpToAdd = new HashSet<Pair<ThoughtDef, Pawn>>();

		private HashSet<ThoughtDef> tmpCachedSocialThoughts = new HashSet<ThoughtDef>();

		public SituationalThoughtHandler(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public List<Thought_Situational> GetSituationalThoughtsAffectingMood()
		{
			this.activeSituationalThoughts.Clear();
			if (Current.ProgramState != ProgramState.Playing)
			{
				return this.activeSituationalThoughts;
			}
			this.CheckRecalculateSituationalThoughtsAffectingMoodState();
			for (int i = 0; i < this.cachedSituationalThoughts.Count; i++)
			{
				if (this.cachedSituationalThoughts[i].Active)
				{
					this.activeSituationalThoughts.Add(this.cachedSituationalThoughts[i]);
				}
			}
			foreach (KeyValuePair<Pawn, SituationalThoughtHandler.CachedSituationalSocialThoughts> current in this.cachedSituationalSocialThoughtsAffectingMood)
			{
				current.Value.lastQueryTick = Find.TickManager.TicksGame;
				for (int j = 0; j < current.Value.activeThoughts.Count; j++)
				{
					this.activeSituationalThoughts.Add(current.Value.activeThoughts[j]);
				}
			}
			return this.activeSituationalThoughts;
		}

		public List<Thought_SituationalSocial> SocialSituationalThoughts(Pawn otherPawn)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return SituationalThoughtHandler.emptySituationalSocial;
			}
			this.CheckRecalculateSocialSituationalThoughtsState(otherPawn);
			SituationalThoughtHandler.CachedSituationalSocialThoughts cachedSituationalSocialThoughts = this.cachedSituationalSocialThoughts[otherPawn];
			cachedSituationalSocialThoughts.lastQueryTick = Find.TickManager.TicksGame;
			return cachedSituationalSocialThoughts.activeThoughts;
		}

		private void CheckRecalculateSituationalThoughtsAffectingMoodState()
		{
			if (Find.TickManager.TicksGame - this.lastStateRecalculation < 100)
			{
				return;
			}
			this.lastStateRecalculation = Find.TickManager.TicksGame;
			ProfilerThreadCheck.BeginSample("recalculating situational thoughts");
			try
			{
				this.tmpCachedThoughts.Clear();
				for (int i = 0; i < this.cachedSituationalThoughts.Count; i++)
				{
					this.cachedSituationalThoughts[i].RecalculateState();
					this.tmpCachedThoughts.Add(this.cachedSituationalThoughts[i].def);
				}
				List<ThoughtDef> allDefsListForReading = DefDatabase<ThoughtDef>.AllDefsListForReading;
				int j = 0;
				int count = allDefsListForReading.Count;
				while (j < count)
				{
					if (allDefsListForReading[j].IsSituational)
					{
						if (!allDefsListForReading[j].IsSocial)
						{
							if (!this.tmpCachedThoughts.Contains(allDefsListForReading[j]))
							{
								Thought_Situational thought_Situational = this.TryCreateSituationalThought(allDefsListForReading[j]);
								if (thought_Situational != null)
								{
									this.cachedSituationalThoughts.Add(thought_Situational);
								}
							}
						}
					}
					j++;
				}
				this.RecalculateSocialSituationalThoughtsAffectingMood();
			}
			finally
			{
				ProfilerThreadCheck.EndSample();
			}
		}

		private void RecalculateSocialSituationalThoughtsAffectingMood()
		{
			try
			{
				this.tmpToAdd.Clear();
				List<ThoughtDef> allDefsListForReading = DefDatabase<ThoughtDef>.AllDefsListForReading;
				int i = 0;
				int count = allDefsListForReading.Count;
				while (i < count)
				{
					if (allDefsListForReading[i].IsSituational)
					{
						if (allDefsListForReading[i].IsSocial)
						{
							if (allDefsListForReading[i].socialThoughtAffectingMood)
							{
								foreach (Pawn current in allDefsListForReading[i].Worker.PotentialPawnCandidates(this.pawn))
								{
									if (current != this.pawn)
									{
										this.tmpToAdd.Add(new Pair<ThoughtDef, Pawn>(allDefsListForReading[i], current));
									}
								}
							}
						}
					}
					i++;
				}
				foreach (KeyValuePair<Pawn, SituationalThoughtHandler.CachedSituationalSocialThoughts> current2 in this.cachedSituationalSocialThoughtsAffectingMood)
				{
					for (int j = current2.Value.thoughts.Count - 1; j >= 0; j--)
					{
						if (!this.tmpToAdd.Contains(new Pair<ThoughtDef, Pawn>(current2.Value.thoughts[j].def, current2.Key)))
						{
							current2.Value.thoughts.RemoveAt(j);
						}
					}
				}
				foreach (Pair<ThoughtDef, Pawn> elem in this.tmpToAdd)
				{
					SituationalThoughtHandler.CachedSituationalSocialThoughts cachedSituationalSocialThoughts;
					bool flag = this.cachedSituationalSocialThoughtsAffectingMood.TryGetValue(elem.Second, out cachedSituationalSocialThoughts);
					if (!flag || cachedSituationalSocialThoughts.thoughts.Find((Thought_SituationalSocial x) => x.def == elem.First) == null)
					{
						Thought_SituationalSocial thought_SituationalSocial = this.TryCreateSituationalSocialThought(elem.First, elem.Second);
						if (thought_SituationalSocial != null)
						{
							if (!flag)
							{
								cachedSituationalSocialThoughts = new SituationalThoughtHandler.CachedSituationalSocialThoughts();
								this.cachedSituationalSocialThoughtsAffectingMood.Add(elem.Second, cachedSituationalSocialThoughts);
							}
							cachedSituationalSocialThoughts.thoughts.Add(thought_SituationalSocial);
						}
					}
				}
				this.cachedSituationalSocialThoughtsAffectingMood.RemoveAll((KeyValuePair<Pawn, SituationalThoughtHandler.CachedSituationalSocialThoughts> x) => x.Value.thoughts.Count == 0);
				foreach (KeyValuePair<Pawn, SituationalThoughtHandler.CachedSituationalSocialThoughts> current3 in this.cachedSituationalSocialThoughtsAffectingMood)
				{
					current3.Value.activeThoughts.Clear();
					for (int k = 0; k < current3.Value.thoughts.Count; k++)
					{
						current3.Value.thoughts[k].RecalculateState();
						current3.Value.lastStateRecalculationTick = Find.TickManager.TicksGame;
						if (current3.Value.thoughts[k].Active)
						{
							current3.Value.activeThoughts.Add(current3.Value.thoughts[k]);
						}
					}
				}
			}
			finally
			{
				ProfilerThreadCheck.EndSample();
			}
		}

		public void CheckRecalculateSocialSituationalThoughtsState(Pawn otherPawn)
		{
			ProfilerThreadCheck.BeginSample("recalculating situational social thoughts");
			try
			{
				SituationalThoughtHandler.CachedSituationalSocialThoughts cachedSituationalSocialThoughts;
				if (!this.cachedSituationalSocialThoughts.TryGetValue(otherPawn, out cachedSituationalSocialThoughts))
				{
					cachedSituationalSocialThoughts = new SituationalThoughtHandler.CachedSituationalSocialThoughts();
					this.cachedSituationalSocialThoughts.Add(otherPawn, cachedSituationalSocialThoughts);
				}
				if (cachedSituationalSocialThoughts.ShouldRecalculateState)
				{
					cachedSituationalSocialThoughts.lastStateRecalculationTick = Find.TickManager.TicksGame;
					this.tmpCachedSocialThoughts.Clear();
					for (int i = 0; i < cachedSituationalSocialThoughts.thoughts.Count; i++)
					{
						cachedSituationalSocialThoughts.thoughts[i].RecalculateState();
						this.tmpCachedSocialThoughts.Add(cachedSituationalSocialThoughts.thoughts[i].def);
					}
					List<ThoughtDef> allDefsListForReading = DefDatabase<ThoughtDef>.AllDefsListForReading;
					int j = 0;
					int count = allDefsListForReading.Count;
					while (j < count)
					{
						if (allDefsListForReading[j].IsSituational)
						{
							if (allDefsListForReading[j].IsSocial)
							{
								if (!this.tmpCachedSocialThoughts.Contains(allDefsListForReading[j]))
								{
									Thought_SituationalSocial thought_SituationalSocial = this.TryCreateSituationalSocialThought(allDefsListForReading[j], otherPawn);
									if (thought_SituationalSocial != null)
									{
										cachedSituationalSocialThoughts.thoughts.Add(thought_SituationalSocial);
									}
								}
							}
						}
						j++;
					}
					cachedSituationalSocialThoughts.activeThoughts.Clear();
					for (int k = 0; k < cachedSituationalSocialThoughts.thoughts.Count; k++)
					{
						if (cachedSituationalSocialThoughts.thoughts[k].Active)
						{
							cachedSituationalSocialThoughts.activeThoughts.Add(cachedSituationalSocialThoughts.thoughts[k]);
						}
					}
				}
			}
			finally
			{
				ProfilerThreadCheck.EndSample();
			}
		}

		private Thought_Situational TryCreateSituationalThought(ThoughtDef def)
		{
			Thought_Situational thought_Situational = null;
			try
			{
				if (!this.pawn.needs.mood.thoughts.CanGetThought(def))
				{
					Thought_Situational result = null;
					return result;
				}
				if (!def.Worker.CurrentState(this.pawn).Active)
				{
					Thought_Situational result = null;
					return result;
				}
				thought_Situational = (Thought_Situational)ThoughtMaker.MakeThought(def);
				thought_Situational.pawn = this.pawn;
				thought_Situational.RecalculateState();
			}
			catch (Exception ex)
			{
				Log.Error(string.Concat(new object[]
				{
					"Exception while recalculating ",
					def,
					" thought state for pawn ",
					this.pawn,
					": ",
					ex
				}));
			}
			return thought_Situational;
		}

		private Thought_SituationalSocial TryCreateSituationalSocialThought(ThoughtDef def, Pawn otherPawn)
		{
			if (!this.pawn.needs.mood.thoughts.CanGetThought(def))
			{
				return null;
			}
			if (!def.Worker.CurrentSocialState(this.pawn, otherPawn).Active)
			{
				return null;
			}
			Thought_SituationalSocial thought_SituationalSocial = (Thought_SituationalSocial)ThoughtMaker.MakeThought(def);
			thought_SituationalSocial.pawn = this.pawn;
			thought_SituationalSocial.otherPawn = otherPawn;
			thought_SituationalSocial.RecalculateState();
			return thought_SituationalSocial;
		}

		public void SituationalThoughtInterval()
		{
			this.RemoveExpiredSituationalThoughtsFromCache();
		}

		public void Notify_SituationalThoughtsDirty()
		{
			this.cachedSituationalThoughts.Clear();
			this.cachedSituationalSocialThoughts.Clear();
		}

		private void RemoveExpiredSituationalThoughtsFromCache()
		{
			this.cachedSituationalSocialThoughts.RemoveAll((KeyValuePair<Pawn, SituationalThoughtHandler.CachedSituationalSocialThoughts> x) => x.Value.Expired || x.Key.Discarded);
			this.cachedSituationalSocialThoughtsAffectingMood.RemoveAll((KeyValuePair<Pawn, SituationalThoughtHandler.CachedSituationalSocialThoughts> x) => x.Value.Expired || x.Key.Discarded);
		}
	}
}
