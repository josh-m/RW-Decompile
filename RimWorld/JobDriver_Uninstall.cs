using System;
using Verse;

namespace RimWorld
{
	public class JobDriver_Uninstall : JobDriver_RemoveBuilding
	{
		public const int UninstallWork = 90;

		protected override DesignationDef Designation
		{
			get
			{
				return DesignationDefOf.Uninstall;
			}
		}

		protected override int TotalNeededWork
		{
			get
			{
				return 90;
			}
		}

		protected override void FinishedRemoving()
		{
			base.Building.Uninstall();
			this.pawn.records.Increment(RecordDefOf.ThingsUninstalled);
		}
	}
}
