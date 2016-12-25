using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public sealed class MemoryThoughtHandler : IExposable
	{
		public Pawn pawn;

		private List<Thought_Memory> memories = new List<Thought_Memory>();

		public List<Thought_Memory> Memories
		{
			get
			{
				return this.memories;
			}
		}

		public MemoryThoughtHandler(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<Thought_Memory>(ref this.memories, "memories", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				for (int i = this.memories.Count - 1; i > 0; i--)
				{
					if (this.memories[i].def == null)
					{
						this.memories.RemoveAt(i);
					}
					else
					{
						this.memories[i].pawn = this.pawn;
					}
				}
			}
		}

		public void MemoryThoughtInterval()
		{
			for (int i = 0; i < this.memories.Count; i++)
			{
				this.memories[i].ThoughtInterval();
			}
			this.RemoveExpiredMemoryThoughts();
		}

		private void RemoveExpiredMemoryThoughts()
		{
			for (int i = this.memories.Count - 1; i >= 0; i--)
			{
				Thought_Memory thought_Memory = this.memories[i];
				if (thought_Memory.ShouldDiscard)
				{
					this.RemoveMemoryThought(thought_Memory);
					if (thought_Memory.def.nextThought != null)
					{
						this.TryGainMemoryThought(thought_Memory.def.nextThought, null);
					}
				}
			}
		}

		public void TryGainMemoryThought(ThoughtDef def, Pawn otherPawn = null)
		{
			if (!def.IsMemory)
			{
				Log.Error(def + " is not a memory thought.");
				return;
			}
			this.TryGainMemoryThought((Thought_Memory)ThoughtMaker.MakeThought(def), otherPawn);
		}

		public void TryGainMemoryThought(Thought_Memory newThought, Pawn otherPawn = null)
		{
			if (!this.pawn.needs.mood.thoughts.CanGetThought(newThought.def))
			{
				return;
			}
			if (newThought is Thought_MemorySocial && newThought.otherPawn == null && otherPawn == null)
			{
				Log.Error("Can't gain social thought " + newThought.def + " because its otherPawn is null and otherPawn passed to this method is also null. Social thoughts must have otherPawn.");
				return;
			}
			newThought.pawn = this.pawn;
			newThought.otherPawn = otherPawn;
			bool flag;
			if (!newThought.TryMergeWithExistingThought(out flag))
			{
				this.memories.Add(newThought);
			}
			if (newThought.def.stackLimitPerPawn >= 0)
			{
				while (this.NumMemoryThoughtsInGroup(newThought) > newThought.def.stackLimitPerPawn)
				{
					this.RemoveMemoryThought(this.OldestMemoryThoughtInGroup(newThought));
				}
			}
			if (newThought.def.stackLimit >= 0)
			{
				while (this.NumMemoryThoughtsOfDef(newThought.def) > newThought.def.stackLimit)
				{
					this.RemoveMemoryThought(this.OldestMemoryThoughtOfDef(newThought.def));
				}
			}
			if (newThought.def.thoughtToMake != null)
			{
				this.TryGainMemoryThought(newThought.def.thoughtToMake, newThought.otherPawn);
			}
			if (flag && newThought.def.showBubble && this.pawn.Spawned)
			{
				MoteMaker.MakeMoodThoughtBubble(this.pawn, newThought);
			}
		}

		public Thought_Memory OldestMemoryThoughtInGroup(Thought_Memory group)
		{
			Thought_Memory result = null;
			int num = -9999;
			for (int i = 0; i < this.memories.Count; i++)
			{
				Thought_Memory thought_Memory = this.memories[i];
				if (thought_Memory.GroupsWith(group))
				{
					if (thought_Memory.age > num)
					{
						result = thought_Memory;
						num = thought_Memory.age;
					}
				}
			}
			return result;
		}

		public Thought_Memory OldestMemoryThoughtOfDef(ThoughtDef def)
		{
			Thought_Memory result = null;
			int num = -9999;
			for (int i = 0; i < this.memories.Count; i++)
			{
				Thought_Memory thought_Memory = this.memories[i];
				if (thought_Memory.def == def)
				{
					if (thought_Memory.age > num)
					{
						result = thought_Memory;
						num = thought_Memory.age;
					}
				}
			}
			return result;
		}

		public void RemoveMemoryThought(Thought_Memory th)
		{
			if (!this.memories.Remove(th))
			{
				Log.Warning("Tried to remove memory thought of def " + th.def.defName + " but it's not here.");
				return;
			}
			if (th.otherPawn != null && th.otherPawn.IsWorldPawn())
			{
				Find.WorldPawns.DiscardIfUnimportant(th.otherPawn);
			}
		}

		public int NumMemoryThoughtsInGroup(Thought_Memory group)
		{
			int num = 0;
			for (int i = 0; i < this.memories.Count; i++)
			{
				if (this.memories[i].GroupsWith(group))
				{
					num++;
				}
			}
			return num;
		}

		public int NumMemoryThoughtsOfDef(ThoughtDef def)
		{
			int num = 0;
			for (int i = 0; i < this.memories.Count; i++)
			{
				if (this.memories[i].def == def)
				{
					num++;
				}
			}
			return num;
		}

		public void RemoveMemoryThoughtsOfDefWhereOtherPawnIs(ThoughtDef def, Pawn otherPawn)
		{
			while (true)
			{
				Thought_Memory thought_Memory = this.memories.Find((Thought_Memory x) => x.def == def && x.otherPawn == otherPawn);
				if (thought_Memory == null)
				{
					break;
				}
				this.RemoveMemoryThought(thought_Memory);
			}
		}

		public void RemoveMemoryThoughtsOfDef(ThoughtDef def)
		{
			if (!def.IsMemory)
			{
				Log.Warning(def + " is not a memory thought.");
				return;
			}
			while (true)
			{
				Thought_Memory thought_Memory = this.memories.Find((Thought_Memory x) => x.def == def);
				if (thought_Memory == null)
				{
					break;
				}
				this.RemoveMemoryThought(thought_Memory);
			}
		}

		public bool AnyMemoryThoughtConcerns(Pawn pawn)
		{
			for (int i = 0; i < this.memories.Count; i++)
			{
				if (this.memories[i].otherPawn == pawn)
				{
					return true;
				}
			}
			return false;
		}
	}
}
