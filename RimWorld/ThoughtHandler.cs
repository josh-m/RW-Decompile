using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public sealed class ThoughtHandler : IExposable
	{
		public Pawn pawn;

		public MemoryThoughtHandler memories;

		public SituationalThoughtHandler situational;

		private List<Thought> tmpThoughts = new List<Thought>();

		private static List<ISocialThought> tmpSocialThoughts = new List<ISocialThought>();

		private List<ISocialThought> distinctSocialThoughtGroups = new List<ISocialThought>();

		private List<Thought> distinctThoughtGroups = new List<Thought>();

		public List<Thought> Thoughts
		{
			get
			{
				this.tmpThoughts.Clear();
				List<Thought_Memory> list = this.memories.Memories;
				for (int i = 0; i < list.Count; i++)
				{
					this.tmpThoughts.Add(list[i]);
				}
				List<Thought_Situational> situationalThoughtsAffectingMood = this.situational.SituationalThoughtsAffectingMood;
				for (int j = 0; j < situationalThoughtsAffectingMood.Count; j++)
				{
					this.tmpThoughts.Add(situationalThoughtsAffectingMood[j]);
				}
				return this.tmpThoughts;
			}
		}

		public ThoughtHandler(Pawn pawn)
		{
			this.pawn = pawn;
			this.memories = new MemoryThoughtHandler(pawn);
			this.situational = new SituationalThoughtHandler(pawn);
		}

		public void ExposeData()
		{
			Scribe_Deep.LookDeep<MemoryThoughtHandler>(ref this.memories, "memories", new object[]
			{
				this.pawn
			});
		}

		public IEnumerable<Thought> ThoughtsInGroup(Thought group)
		{
			return from t in this.Thoughts
			where t.GroupsWith(@group)
			select t;
		}

		public void ThoughtInterval()
		{
			this.situational.SituationalThoughtInterval();
			this.memories.MemoryThoughtInterval();
		}

		public bool CanGetThought(ThoughtDef def)
		{
			ProfilerThreadCheck.BeginSample("CanGetThought()");
			try
			{
				if (this.pawn.MentalState != null && this.pawn.MentalStateDef.blockNormalThoughts && !def.validWhileInMentalState && !def.IsSocial)
				{
					bool result = false;
					return result;
				}
				if (def.nullifyingTraits != null)
				{
					for (int i = 0; i < def.nullifyingTraits.Count; i++)
					{
						if (this.pawn.story.traits.HasTrait(def.nullifyingTraits[i]))
						{
							bool result = false;
							return result;
						}
					}
				}
				if (def.requiredTraits != null)
				{
					for (int j = 0; j < def.requiredTraits.Count; j++)
					{
						if (!this.pawn.story.traits.HasTrait(def.requiredTraits[j]))
						{
							bool result = false;
							return result;
						}
						if (def.RequiresSpecificTraitsDegree && def.requiredTraitsDegree != this.pawn.story.traits.DegreeOfTrait(def.requiredTraits[j]))
						{
							bool result = false;
							return result;
						}
					}
				}
				if (def.nullifiedIfNotColonist && !this.pawn.IsColonist)
				{
					bool result = false;
					return result;
				}
				if (ThoughtUtility.IsSituationalThoughtNullifiedByHediffs(def, this.pawn))
				{
					bool result = false;
					return result;
				}
				if (ThoughtUtility.IsThoughtNullifiedByOwnTales(def, this.pawn))
				{
					bool result = false;
					return result;
				}
			}
			finally
			{
				ProfilerThreadCheck.EndSample();
			}
			return true;
		}

		public float MoodOffsetOfThoughtGroup(Thought group)
		{
			float num = 0f;
			float num2 = 1f;
			float num3 = 0f;
			int num4 = 0;
			List<Thought> thoughts = this.Thoughts;
			for (int i = 0; i < thoughts.Count; i++)
			{
				Thought thought = thoughts[i];
				if (thought.GroupsWith(group))
				{
					num += thought.MoodOffset();
					num3 += num2;
					num2 *= thought.def.stackedEffectMultiplier;
					num4++;
				}
			}
			float num5 = num / (float)num4;
			return num5 * num3;
		}

		public int OpinionOffsetOfThoughtGroup(ISocialThought group, Pawn otherPawn)
		{
			ProfilerThreadCheck.BeginSample("OpinionOffsetOfThoughtGroup()");
			ThoughtHandler.tmpSocialThoughts.Clear();
			List<Thought> thoughts = this.Thoughts;
			List<Thought_SituationalSocial> list = this.situational.SocialSituationalThoughts(otherPawn);
			for (int i = 0; i < list.Count; i++)
			{
				thoughts.Add(list[i]);
			}
			for (int j = 0; j < thoughts.Count; j++)
			{
				if (thoughts[j].GroupsWith((Thought)group))
				{
					ISocialThought socialThought = (ISocialThought)thoughts[j];
					if (socialThought.OtherPawnID() == otherPawn.thingIDNumber && socialThought.OpinionOffset() != 0f)
					{
						ThoughtHandler.tmpSocialThoughts.Add(socialThought);
					}
				}
			}
			ThoughtDef def = ((Thought)group).def;
			if (def.IsMemory && def.stackedEffectMultiplier != 1f)
			{
				ThoughtHandler.tmpSocialThoughts.Sort((ISocialThought a, ISocialThought b) => ((Thought_Memory)a).age.CompareTo(((Thought_Memory)b).age));
			}
			float num = 0f;
			float num2 = 1f;
			for (int k = 0; k < ThoughtHandler.tmpSocialThoughts.Count; k++)
			{
				num += ThoughtHandler.tmpSocialThoughts[k].OpinionOffset() * num2;
				num2 *= ((Thought)ThoughtHandler.tmpSocialThoughts[k]).def.stackedEffectMultiplier;
			}
			ThoughtHandler.tmpSocialThoughts.Clear();
			ProfilerThreadCheck.EndSample();
			if (num == 0f)
			{
				return 0;
			}
			if (num > 0f)
			{
				return Mathf.Max(Mathf.RoundToInt(num), 1);
			}
			return Mathf.Min(Mathf.RoundToInt(num), -1);
		}

		public List<ISocialThought> DistinctSocialThoughtGroups(Pawn otherPawn)
		{
			this.distinctSocialThoughtGroups.Clear();
			List<Thought> thoughts = this.Thoughts;
			List<Thought_SituationalSocial> list = this.situational.SocialSituationalThoughts(otherPawn);
			for (int j = 0; j < list.Count; j++)
			{
				thoughts.Add(list[j]);
			}
			int i;
			for (i = 0; i < thoughts.Count; i++)
			{
				ISocialThought socialThought = thoughts[i] as ISocialThought;
				if (socialThought != null && socialThought.OtherPawnID() == otherPawn.thingIDNumber && socialThought.OpinionOffset() != 0f)
				{
					if (!this.distinctSocialThoughtGroups.Any((ISocialThought x) => ((Thought)x).GroupsWith(thoughts[i])))
					{
						this.distinctSocialThoughtGroups.Add(socialThought);
					}
				}
			}
			return this.distinctSocialThoughtGroups;
		}

		public List<Thought> DistinctThoughtGroups()
		{
			this.distinctThoughtGroups.Clear();
			List<Thought> thoughts = this.Thoughts;
			int i;
			for (i = 0; i < thoughts.Count; i++)
			{
				if (!this.distinctThoughtGroups.Any((Thought x) => x.GroupsWith(thoughts[i])))
				{
					this.distinctThoughtGroups.Add(thoughts[i]);
				}
			}
			return this.distinctThoughtGroups;
		}

		public float TotalMood()
		{
			List<Thought> list = this.DistinctThoughtGroups();
			float num = 0f;
			for (int i = 0; i < list.Count; i++)
			{
				num += this.MoodOffsetOfThoughtGroup(list[i]);
			}
			return num;
		}
	}
}
