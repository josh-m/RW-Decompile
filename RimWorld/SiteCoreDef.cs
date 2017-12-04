using System;
using Verse;

namespace RimWorld
{
	public class SiteCoreDef : SiteDefBase
	{
		public Type workerClass = typeof(SiteCoreWorker);

		public bool transportPodsCanLandAndGenerateMap = true;

		public float forceExitAndRemoveMapCountdownDurationDays = 3f;

		[Unsaved]
		private SiteCoreWorker workerInt;

		public SiteCoreWorker Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					this.workerInt = (SiteCoreWorker)Activator.CreateInstance(this.workerClass);
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
