using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public sealed class ThoughtHandler : IExposable
	{
		public Pawn pawn;

		public MemoryThoughtHandler memories;

		public SituationalThoughtHandler situational;

		private static List<Thought> tmpThoughts = new List<Thought>();

		private static List<Thought> tmpTotalMoodOffsetThoughts = new List<Thought>();

		private static List<ISocialThought> tmpSocialThoughts = new List<ISocialThought>();

		private static List<ISocialThought> tmpTotalOpinionOffsetThoughts = new List<ISocialThought>();

		public ThoughtHandler(Pawn pawn)
		{
			this.pawn = pawn;
			this.memories = new MemoryThoughtHandler(pawn);
			this.situational = new SituationalThoughtHandler(pawn);
		}

		public void ExposeData()
		{
			Scribe_Deep.Look<MemoryThoughtHandler>(ref this.memories, "memories", new object[]
			{
				this.pawn
			});
		}

		public void ThoughtInterval()
		{
			this.situational.SituationalThoughtInterval();
			this.memories.MemoryThoughtInterval();
		}

		public void GetAllMoodThoughts(List<Thought> outThoughts)
		{
			outThoughts.Clear();
			List<Thought_Memory> list = this.memories.Memories;
			for (int i = 0; i < list.Count; i++)
			{
				Thought_Memory thought_Memory = list[i];
				if (thought_Memory.MoodOffset() != 0f)
				{
					outThoughts.Add(thought_Memory);
				}
			}
			this.situational.AppendMoodThoughts(outThoughts);
		}

		public void GetMoodThoughts(Thought group, List<Thought> outThoughts)
		{
			this.GetAllMoodThoughts(outThoughts);
			for (int i = outThoughts.Count - 1; i >= 0; i--)
			{
				if (!outThoughts[i].GroupsWith(group))
				{
					outThoughts.RemoveAt(i);
				}
			}
		}

		public float MoodOffsetOfGroup(Thought group)
		{
			this.GetMoodThoughts(group, ThoughtHandler.tmpThoughts);
			if (!ThoughtHandler.tmpThoughts.Any<Thought>())
			{
				return 0f;
			}
			float num = 0f;
			float num2 = 1f;
			float num3 = 0f;
			for (int i = 0; i < ThoughtHandler.tmpThoughts.Count; i++)
			{
				Thought thought = ThoughtHandler.tmpThoughts[i];
				num += thought.MoodOffset();
				num3 += num2;
				num2 *= thought.def.stackedEffectMultiplier;
			}
			float num4 = num / (float)ThoughtHandler.tmpThoughts.Count;
			ThoughtHandler.tmpThoughts.Clear();
			return num4 * num3;
		}

		public void GetDistinctMoodThoughtGroups(List<Thought> outThoughts)
		{
			this.GetAllMoodThoughts(outThoughts);
			for (int i = outThoughts.Count - 1; i >= 0; i--)
			{
				Thought other = outThoughts[i];
				for (int j = 0; j < i; j++)
				{
					if (outThoughts[j].GroupsWith(other))
					{
						outThoughts.RemoveAt(i);
						break;
					}
				}
			}
		}

		public float TotalMoodOffset()
		{
			this.GetDistinctMoodThoughtGroups(ThoughtHandler.tmpTotalMoodOffsetThoughts);
			float num = 0f;
			for (int i = 0; i < ThoughtHandler.tmpTotalMoodOffsetThoughts.Count; i++)
			{
				num += this.MoodOffsetOfGroup(ThoughtHandler.tmpTotalMoodOffsetThoughts[i]);
			}
			ThoughtHandler.tmpTotalMoodOffsetThoughts.Clear();
			return num;
		}

		public void GetSocialThoughts(Pawn otherPawn, List<ISocialThought> outThoughts)
		{
			outThoughts.Clear();
			List<Thought_Memory> list = this.memories.Memories;
			for (int i = 0; i < list.Count; i++)
			{
				ISocialThought socialThought = list[i] as ISocialThought;
				if (socialThought != null && socialThought.OtherPawn() == otherPawn)
				{
					outThoughts.Add(socialThought);
				}
			}
			this.situational.AppendSocialThoughts(otherPawn, outThoughts);
		}

		public void GetSocialThoughts(Pawn otherPawn, ISocialThought group, List<ISocialThought> outThoughts)
		{
			this.GetSocialThoughts(otherPawn, outThoughts);
			for (int i = outThoughts.Count - 1; i >= 0; i--)
			{
				if (!((Thought)outThoughts[i]).GroupsWith((Thought)group))
				{
					outThoughts.RemoveAt(i);
				}
			}
		}

		public int OpinionOffsetOfGroup(ISocialThought group, Pawn otherPawn)
		{
			this.GetSocialThoughts(otherPawn, group, ThoughtHandler.tmpSocialThoughts);
			for (int i = ThoughtHandler.tmpSocialThoughts.Count - 1; i >= 0; i--)
			{
				if (ThoughtHandler.tmpSocialThoughts[i].OpinionOffset() == 0f)
				{
					ThoughtHandler.tmpSocialThoughts.RemoveAt(i);
				}
			}
			if (!ThoughtHandler.tmpSocialThoughts.Any<ISocialThought>())
			{
				return 0;
			}
			ThoughtDef def = ((Thought)group).def;
			if (def.IsMemory && def.stackedEffectMultiplier != 1f)
			{
				ThoughtHandler.tmpSocialThoughts.Sort((ISocialThought a, ISocialThought b) => ((Thought_Memory)a).age.CompareTo(((Thought_Memory)b).age));
			}
			float num = 0f;
			float num2 = 1f;
			for (int j = 0; j < ThoughtHandler.tmpSocialThoughts.Count; j++)
			{
				num += ThoughtHandler.tmpSocialThoughts[j].OpinionOffset() * num2;
				num2 *= ((Thought)ThoughtHandler.tmpSocialThoughts[j]).def.stackedEffectMultiplier;
			}
			ThoughtHandler.tmpSocialThoughts.Clear();
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

		public void GetDistinctSocialThoughtGroups(Pawn otherPawn, List<ISocialThought> outThoughts)
		{
			this.GetSocialThoughts(otherPawn, outThoughts);
			for (int i = outThoughts.Count - 1; i >= 0; i--)
			{
				ISocialThought socialThought = outThoughts[i];
				for (int j = 0; j < i; j++)
				{
					if (((Thought)outThoughts[j]).GroupsWith((Thought)socialThought))
					{
						outThoughts.RemoveAt(i);
						break;
					}
				}
			}
		}

		public int TotalOpinionOffset(Pawn otherPawn)
		{
			this.GetDistinctSocialThoughtGroups(otherPawn, ThoughtHandler.tmpTotalOpinionOffsetThoughts);
			int num = 0;
			for (int i = 0; i < ThoughtHandler.tmpTotalOpinionOffsetThoughts.Count; i++)
			{
				num += this.OpinionOffsetOfGroup(ThoughtHandler.tmpTotalOpinionOffsetThoughts[i], otherPawn);
			}
			ThoughtHandler.tmpTotalOpinionOffsetThoughts.Clear();
			return num;
		}
	}
}
