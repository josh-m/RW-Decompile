using System;
using System.Collections.Generic;

namespace Verse
{
	internal static class LoadIDsWantedBank
	{
		private struct IdRecord
		{
			public string id;

			public Type refeeType;

			public IdRecord(string id, Type wanter)
			{
				this.id = id;
				this.refeeType = wanter;
			}
		}

		private static Queue<LoadIDsWantedBank.IdRecord> idsRead = new Queue<LoadIDsWantedBank.IdRecord>();

		private static List<List<string>> idListsRead = new List<List<string>>();

		private static int readMax = 0;

		public static int IDCount
		{
			get
			{
				return LoadIDsWantedBank.idsRead.Count;
			}
		}

		public static void ConfirmClear()
		{
			if (LoadIDsWantedBank.idListsRead.Count > 0 || LoadIDsWantedBank.idListsRead.Count > 0)
			{
				Log.Error(string.Concat(new object[]
				{
					"Not all loadIDs which were read were consumed. ",
					LoadIDsWantedBank.idListsRead.Count,
					" singles and ",
					LoadIDsWantedBank.idListsRead.Count,
					" lists remain."
				}));
				LoadIDsWantedBank.idsRead.Clear();
				LoadIDsWantedBank.idListsRead.Clear();
			}
		}

		public static void RegisterLoadIDReadFromXml(string targetLoadID, Type refeeType)
		{
			LoadIDsWantedBank.idsRead.Enqueue(new LoadIDsWantedBank.IdRecord(targetLoadID, refeeType));
			if (LoadIDsWantedBank.idsRead.Count > LoadIDsWantedBank.readMax)
			{
				LoadIDsWantedBank.readMax = LoadIDsWantedBank.idsRead.Count;
			}
		}

		public static void RegisterLoadIDListReadFromXml(List<string> targetLoadIDList)
		{
			LoadIDsWantedBank.idListsRead.Add(targetLoadIDList);
		}

		public static string GetNext(Type wantedType)
		{
			LoadIDsWantedBank.IdRecord idRecord = LoadIDsWantedBank.idsRead.Dequeue();
			if (idRecord.refeeType != wantedType)
			{
				Log.Error(string.Concat(new object[]
				{
					"Misaligned load ID request. Requests didn't match between the LoadingVars and ResolvingCrossRefs stage. Id ",
					(idRecord.id == null) ? "null" : idRecord.id,
					" was entered as a ",
					idRecord.refeeType,
					" but is being read as a ",
					wantedType,
					"."
				}));
			}
			return idRecord.id;
		}

		public static List<string> GetNextList()
		{
			if (!LoadIDsWantedBank.idListsRead.Any<List<string>>())
			{
				Log.Error("LoadIDsWantedBank: Could not get next list because there is none.");
				return new List<string>();
			}
			List<string> result = LoadIDsWantedBank.idListsRead[0];
			LoadIDsWantedBank.idListsRead.RemoveAt(0);
			return result;
		}
	}
}
