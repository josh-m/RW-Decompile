using System;
using Verse;

namespace RimWorld
{
	public class UniqueIDsManager : IExposable
	{
		private int nextThingID;

		private int nextBillID;

		private int nextFactionID;

		private int nextLordID;

		private int nextTaleID;

		private int nextPassingShipID;

		private int nextWorldObjectID;

		private int nextMapID;

		private int nextAreaID;

		private int nextTransporterGroupID;

		private int nextAncientCryptosleepCasketGroupID;

		private int nextJobID;

		private int nextSignalTagID;

		private int nextWorldFeatureID;

		private int nextHediffID;

		private int nextBattleID;

		public UniqueIDsManager()
		{
			this.nextThingID = Rand.Range(0, 1000);
		}

		public int GetNextThingID()
		{
			return UniqueIDsManager.GetNextID(ref this.nextThingID);
		}

		public int GetNextBillID()
		{
			return UniqueIDsManager.GetNextID(ref this.nextBillID);
		}

		public int GetNextFactionID()
		{
			return UniqueIDsManager.GetNextID(ref this.nextFactionID);
		}

		public int GetNextLordID()
		{
			return UniqueIDsManager.GetNextID(ref this.nextLordID);
		}

		public int GetNextTaleID()
		{
			return UniqueIDsManager.GetNextID(ref this.nextTaleID);
		}

		public int GetNextPassingShipID()
		{
			return UniqueIDsManager.GetNextID(ref this.nextPassingShipID);
		}

		public int GetNextWorldObjectID()
		{
			return UniqueIDsManager.GetNextID(ref this.nextWorldObjectID);
		}

		public int GetNextMapID()
		{
			return UniqueIDsManager.GetNextID(ref this.nextMapID);
		}

		public int GetNextAreaID()
		{
			return UniqueIDsManager.GetNextID(ref this.nextAreaID);
		}

		public int GetNextTransporterGroupID()
		{
			return UniqueIDsManager.GetNextID(ref this.nextTransporterGroupID);
		}

		public int GetNextAncientCryptosleepCasketGroupID()
		{
			return UniqueIDsManager.GetNextID(ref this.nextAncientCryptosleepCasketGroupID);
		}

		public int GetNextJobID()
		{
			return UniqueIDsManager.GetNextID(ref this.nextJobID);
		}

		public int GetNextSignalTagID()
		{
			return UniqueIDsManager.GetNextID(ref this.nextSignalTagID);
		}

		public int GetNextWorldFeatureID()
		{
			return UniqueIDsManager.GetNextID(ref this.nextWorldFeatureID);
		}

		public int GetNextHediffID()
		{
			return UniqueIDsManager.GetNextID(ref this.nextHediffID);
		}

		public int GetNextBattleID()
		{
			return UniqueIDsManager.GetNextID(ref this.nextBattleID);
		}

		private static int GetNextID(ref int nextID)
		{
			if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
			{
				Log.Warning("Getting next unique ID during saving or loading. This may cause bugs.");
			}
			int result = nextID;
			nextID++;
			if (nextID == 2147483647)
			{
				Log.Warning("Next ID is at max value. Resetting to 0. This may cause bugs.");
				nextID = 0;
			}
			return result;
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.nextThingID, "nextThingID", 0, false);
			Scribe_Values.Look<int>(ref this.nextBillID, "nextBillID", 0, false);
			Scribe_Values.Look<int>(ref this.nextFactionID, "nextFactionID", 0, false);
			Scribe_Values.Look<int>(ref this.nextLordID, "nextLordID", 0, false);
			Scribe_Values.Look<int>(ref this.nextTaleID, "nextTaleID", 0, false);
			Scribe_Values.Look<int>(ref this.nextPassingShipID, "nextPassingShipID", 0, false);
			Scribe_Values.Look<int>(ref this.nextWorldObjectID, "nextWorldObjectID", 0, false);
			Scribe_Values.Look<int>(ref this.nextMapID, "nextMapID", 0, false);
			Scribe_Values.Look<int>(ref this.nextAreaID, "nextAreaID", 0, false);
			Scribe_Values.Look<int>(ref this.nextTransporterGroupID, "nextTransporterGroupID", 0, false);
			Scribe_Values.Look<int>(ref this.nextAncientCryptosleepCasketGroupID, "nextAncientCryptosleepCasketGroupID", 0, false);
			Scribe_Values.Look<int>(ref this.nextJobID, "nextJobID", 0, false);
			Scribe_Values.Look<int>(ref this.nextSignalTagID, "nextSignalTagID", 0, false);
			Scribe_Values.Look<int>(ref this.nextWorldFeatureID, "nextWorldFeatureID", 0, false);
			Scribe_Values.Look<int>(ref this.nextHediffID, "nextHediffID", 0, false);
			Scribe_Values.Look<int>(ref this.nextBattleID, "nextBattleID", 0, false);
		}
	}
}
