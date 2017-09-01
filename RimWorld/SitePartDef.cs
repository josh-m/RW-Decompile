using System;
using Verse;

namespace RimWorld
{
	public class SitePartDef : SiteDefBase
	{
		public Type workerClass = typeof(SitePartWorker);

		[Unsaved]
		private SitePartWorker workerInt;

		public SitePartWorker Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					this.workerInt = (SitePartWorker)Activator.CreateInstance(this.workerClass);
					this.workerInt.def = this;
				}
				return this.workerInt;
			}
		}

		public override bool FactionCanOwn(Faction faction)
		{
			return base.FactionCanOwn(faction) && this.Worker.FactionCanOwn(faction);
		}
	}
}
