using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		private static List<Thought> tgThoughts = new List<Thought>();

		private static List<Thought> moThoughts = new List<Thought>();

		private static List<ISocialThought> tmpSocialThoughts = new List<ISocialThought>();

		private static List<Thought> oogThoughts = new List<Thought>();

		private static List<Thought> dsThoughts = new List<Thought>();

		private List<Thought> distinctThoughtGroups = new List<Thought>();

		private static List<Thought> dtThoughts = new List<Thought>();

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

		[DebuggerHidden]
		public IEnumerable<Thought> ThoughtsInGroup(Thought group)
		{
			this.GetMainThoughts(ThoughtHandler.tgThoughts);
			foreach (Thought th in from t in ThoughtHandler.tgThoughts
			where t.GroupsWith(this.@group)
			select t)
			{
				yield return th;
			}
			ThoughtHandler.tgThoughts.Clear();
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
				if (!def.validWhileDespawned && !this.pawn.Spawned && !def.IsMemory)
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

		public void GetMainThoughts(List<Thought> resultList)
		{
			if (resultList == null)
			{
				Log.Error("GetMainThoughts with null resultList.");
				resultList = new List<Thought>();
			}
			else if (resultList.Count != 0)
			{
				resultList.Clear();
			}
			List<Thought_Memory> list = this.memories.Memories;
			for (int i = 0; i < list.Count; i++)
			{
				resultList.Add(list[i]);
			}
			List<Thought_Situational> situationalThoughtsAffectingMood = this.situational.GetSituationalThoughtsAffectingMood();
			for (int j = 0; j < situationalThoughtsAffectingMood.Count; j++)
			{
				resultList.Add(situationalThoughtsAffectingMood[j]);
			}
		}

		public float MoodOffsetOfThoughtGroup(Thought groupFirst)
		{
			float num = 0f;
			float num2 = 1f;
			float num3 = 0f;
			int num4 = 0;
			this.GetMainThoughts(ThoughtHandler.moThoughts);
			for (int i = 0; i < ThoughtHandler.moThoughts.Count; i++)
			{
				Thought thought = ThoughtHandler.moThoughts[i];
				if (thought.GroupsWith(groupFirst))
				{
					num += thought.MoodOffset();
					num3 += num2;
					num2 *= thought.def.stackedEffectMultiplier;
					num4++;
				}
			}
			if (num4 == 0)
			{
				num4 = 1;
			}
			float num5 = num / (float)num4;
			ThoughtHandler.moThoughts.Clear();
			return num5 * num3;
		}

		public int OpinionOffsetOfThoughtGroup(ISocialThought group, Pawn otherPawn)
		{
			ProfilerThreadCheck.BeginSample("OpinionOffsetOfThoughtGroup()");
			ThoughtHandler.tmpSocialThoughts.Clear();
			this.GetMainThoughts(ThoughtHandler.oogThoughts);
			List<Thought_SituationalSocial> list = this.situational.SocialSituationalThoughts(otherPawn);
			for (int i = 0; i < list.Count; i++)
			{
				ThoughtHandler.oogThoughts.Add(list[i]);
			}
			for (int j = 0; j < ThoughtHandler.oogThoughts.Count; j++)
			{
				if (ThoughtHandler.oogThoughts[j].GroupsWith((Thought)group))
				{
					ISocialThought socialThought = (ISocialThought)ThoughtHandler.oogThoughts[j];
					if (socialThought.OtherPawn() == otherPawn && socialThought.OpinionOffset() != 0f)
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
			ThoughtHandler.oogThoughts.Clear();
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

		public void GetDistinctSocialThoughtGroups(Pawn otherPawn, List<ISocialThought> resultsList)
		{
			if (resultsList.Count != 0)
			{
				resultsList.Clear();
			}
			this.GetMainThoughts(ThoughtHandler.dsThoughts);
			List<Thought_SituationalSocial> list = this.situational.SocialSituationalThoughts(otherPawn);
			for (int j = 0; j < list.Count; j++)
			{
				ThoughtHandler.dsThoughts.Add(list[j]);
			}
			int i;
			for (i = 0; i < ThoughtHandler.dsThoughts.Count; i++)
			{
				ISocialThought socialThought = ThoughtHandler.dsThoughts[i] as ISocialThought;
				if (socialThought != null && socialThought.OtherPawn() == otherPawn && socialThought.OpinionOffset() != 0f)
				{
					if (!resultsList.Any((ISocialThought x) => ((Thought)x).GroupsWith(ThoughtHandler.dsThoughts[i])))
					{
						resultsList.Add(socialThought);
					}
				}
			}
			ThoughtHandler.dsThoughts.Clear();
		}

		public List<Thought> DistinctThoughtGroups()
		{
			this.distinctThoughtGroups.Clear();
			this.GetMainThoughts(ThoughtHandler.dtThoughts);
			int i;
			for (i = 0; i < ThoughtHandler.dtThoughts.Count; i++)
			{
				if (!this.distinctThoughtGroups.Any((Thought x) => x.GroupsWith(ThoughtHandler.dtThoughts[i])))
				{
					this.distinctThoughtGroups.Add(ThoughtHandler.dtThoughts[i]);
				}
			}
			ThoughtHandler.dtThoughts.Clear();
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
