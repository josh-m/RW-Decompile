using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ItemCollectionGeneratorDef : Def
	{
		public Type workerClass = typeof(ItemCollectionGenerator);

		public List<ThingDef> allowedDefs = new List<ThingDef>();

		[Unsaved]
		private ItemCollectionGenerator workerInt;

		public ItemCollectionGenerator Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					this.workerInt = (ItemCollectionGenerator)Activator.CreateInstance(this.workerClass);
					this.workerInt.def = this;
				}
				return this.workerInt;
			}
		}
	}
}
