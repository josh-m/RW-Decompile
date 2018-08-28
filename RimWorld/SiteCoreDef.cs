using System;

namespace RimWorld
{
	public class SiteCoreDef : SiteCoreOrPartDefBase
	{
		public bool transportPodsCanLandAndGenerateMap = true;

		public float forceExitAndRemoveMapCountdownDurationDays = 3f;

		public new SiteCoreWorker Worker
		{
			get
			{
				return (SiteCoreWorker)base.Worker;
			}
		}

		public SiteCoreDef()
		{
			this.workerClass = typeof(SiteCoreWorker);
		}

		public override bool FactionCanOwn(Faction faction)
		{
			return base.FactionCanOwn(faction) && this.Worker.FactionCanOwn(faction);
		}

		protected override SiteCoreOrPartWorkerBase CreateWorker()
		{
			return (SiteCoreWorker)Activator.CreateInstance(this.workerClass);
		}
	}
}
