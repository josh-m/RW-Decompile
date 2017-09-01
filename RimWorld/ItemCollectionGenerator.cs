using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class ItemCollectionGenerator
	{
		public ItemCollectionGeneratorDef def;

		public static List<Thing> thingsBeingGeneratedNow;

		protected virtual ItemCollectionGeneratorParams RandomTestParams
		{
			get
			{
				return default(ItemCollectionGeneratorParams);
			}
		}

		public List<Thing> Generate(ItemCollectionGeneratorParams parms)
		{
			if (ItemCollectionGenerator.thingsBeingGeneratedNow != null)
			{
				Log.Error("thingsBeingGeneratedNow is not null. Nested calls are not allowed.");
			}
			List<Thing> list = new List<Thing>();
			ItemCollectionGenerator.thingsBeingGeneratedNow = list;
			try
			{
				this.Generate(parms, list);
				this.PostProcess(list);
			}
			finally
			{
				ItemCollectionGenerator.thingsBeingGeneratedNow = null;
			}
			return list;
		}

		public List<Thing> GenerateRandomTestItems()
		{
			return this.Generate(this.RandomTestParams);
		}

		protected abstract void Generate(ItemCollectionGeneratorParams parms, List<Thing> outThings);

		private void PostProcess(List<Thing> things)
		{
			if (things.RemoveAll((Thing x) => x == null) != 0)
			{
				Log.Error(this.def + " generated null things.");
			}
			this.Minify(things);
			this.ChangeDeadPawnsToTheirCorpses(things);
		}

		private void Minify(List<Thing> things)
		{
			for (int i = 0; i < things.Count; i++)
			{
				if (things[i].def.Minifiable)
				{
					int stackCount = things[i].stackCount;
					things[i].stackCount = 1;
					MinifiedThing minifiedThing = things[i].MakeMinified();
					minifiedThing.stackCount = stackCount;
					things[i] = minifiedThing;
				}
			}
		}

		private void ChangeDeadPawnsToTheirCorpses(List<Thing> things)
		{
			for (int i = 0; i < things.Count; i++)
			{
				if (things[i].ParentHolder is Corpse)
				{
					things[i] = (Corpse)things[i].ParentHolder;
				}
			}
		}
	}
}
