using System;
using System.Collections.Generic;
using System.Text;

namespace Verse
{
	public class LoadIDsWantedBank
	{
		private struct IdRecord
		{
			public string targetLoadID;

			public Type targetType;

			public string pathRelToParent;

			public IExposable parent;

			public IdRecord(string targetLoadID, Type targetType, string pathRelToParent, IExposable parent)
			{
				this.targetLoadID = targetLoadID;
				this.targetType = targetType;
				this.pathRelToParent = pathRelToParent;
				this.parent = parent;
			}
		}

		private struct IdListRecord
		{
			public List<string> targetLoadIDs;

			public string pathRelToParent;

			public IExposable parent;

			public IdListRecord(List<string> targetLoadIDs, string pathRelToParent, IExposable parent)
			{
				this.targetLoadIDs = targetLoadIDs;
				this.pathRelToParent = pathRelToParent;
				this.parent = parent;
			}
		}

		private List<LoadIDsWantedBank.IdRecord> idsRead = new List<LoadIDsWantedBank.IdRecord>();

		private List<LoadIDsWantedBank.IdListRecord> idListsRead = new List<LoadIDsWantedBank.IdListRecord>();

		public void ConfirmClear()
		{
			if (this.idsRead.Count > 0 || this.idListsRead.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Not all loadIDs which were read were consumed.");
				if (this.idsRead.Count > 0)
				{
					stringBuilder.AppendLine("Singles:");
					for (int i = 0; i < this.idsRead.Count; i++)
					{
						stringBuilder.AppendLine(string.Concat(new object[]
						{
							"  ",
							this.idsRead[i].targetLoadID.ToStringSafe<string>(),
							" of type ",
							this.idsRead[i].targetType,
							". pathRelToParent=",
							this.idsRead[i].pathRelToParent,
							", parent=",
							this.idsRead[i].parent.ToStringSafe<IExposable>()
						}));
					}
				}
				if (this.idListsRead.Count > 0)
				{
					stringBuilder.AppendLine("Lists:");
					for (int j = 0; j < this.idListsRead.Count; j++)
					{
						stringBuilder.AppendLine(string.Concat(new object[]
						{
							"  List with ",
							(this.idListsRead[j].targetLoadIDs == null) ? 0 : this.idListsRead[j].targetLoadIDs.Count,
							" elements. pathRelToParent=",
							this.idListsRead[j].pathRelToParent,
							", parent=",
							this.idListsRead[j].parent.ToStringSafe<IExposable>()
						}));
					}
				}
				Log.Warning(stringBuilder.ToString().TrimEndNewlines());
			}
			this.Clear();
		}

		public void Clear()
		{
			this.idsRead.Clear();
			this.idListsRead.Clear();
		}

		public void RegisterLoadIDReadFromXml(string targetLoadID, Type targetType, string pathRelToParent, IExposable parent)
		{
			for (int i = 0; i < this.idsRead.Count; i++)
			{
				if (this.idsRead[i].parent == parent && this.idsRead[i].pathRelToParent == pathRelToParent)
				{
					Log.Error(string.Concat(new string[]
					{
						"Tried to register the same load ID twice: ",
						targetLoadID,
						", pathRelToParent=",
						pathRelToParent,
						", parent=",
						parent.ToStringSafe<IExposable>()
					}));
					return;
				}
			}
			this.idsRead.Add(new LoadIDsWantedBank.IdRecord(targetLoadID, targetType, pathRelToParent, parent));
		}

		public void RegisterLoadIDReadFromXml(string targetLoadID, Type targetType, string toAppendToPathRelToParent)
		{
			string text = Scribe.loader.curPathRelToParent;
			if (!toAppendToPathRelToParent.NullOrEmpty())
			{
				text = text + '/' + toAppendToPathRelToParent;
			}
			this.RegisterLoadIDReadFromXml(targetLoadID, targetType, text, Scribe.loader.curParent);
		}

		public void RegisterLoadIDListReadFromXml(List<string> targetLoadIDList, string pathRelToParent, IExposable parent)
		{
			for (int i = 0; i < this.idListsRead.Count; i++)
			{
				if (this.idListsRead[i].parent == parent && this.idListsRead[i].pathRelToParent == pathRelToParent)
				{
					Log.Error("Tried to register the same list of load IDs twice. pathRelToParent=" + pathRelToParent + ", parent=" + parent.ToStringSafe<IExposable>());
					return;
				}
			}
			this.idListsRead.Add(new LoadIDsWantedBank.IdListRecord(targetLoadIDList, pathRelToParent, parent));
		}

		public void RegisterLoadIDListReadFromXml(List<string> targetLoadIDList, string toAppendToPathRelToParent)
		{
			string text = Scribe.loader.curPathRelToParent;
			if (!toAppendToPathRelToParent.NullOrEmpty())
			{
				text = text + '/' + toAppendToPathRelToParent;
			}
			this.RegisterLoadIDListReadFromXml(targetLoadIDList, text, Scribe.loader.curParent);
		}

		public string Take<T>(string pathRelToParent, IExposable parent)
		{
			for (int i = 0; i < this.idsRead.Count; i++)
			{
				if (this.idsRead[i].parent == parent && this.idsRead[i].pathRelToParent == pathRelToParent)
				{
					string targetLoadID = this.idsRead[i].targetLoadID;
					if (typeof(T) != this.idsRead[i].targetType)
					{
						Log.Error(string.Concat(new object[]
						{
							"Trying to get load ID of object of type ",
							typeof(T),
							", but it was registered as ",
							this.idsRead[i].targetType,
							". pathRelToParent=",
							pathRelToParent,
							", parent=",
							parent.ToStringSafe<IExposable>()
						}));
					}
					this.idsRead.RemoveAt(i);
					return targetLoadID;
				}
			}
			Log.Error("Could not get load ID. We're asking for something which was never added during LoadingVars. pathRelToParent=" + pathRelToParent + ", parent=" + parent.ToStringSafe<IExposable>());
			return null;
		}

		public List<string> TakeList(string pathRelToParent, IExposable parent)
		{
			for (int i = 0; i < this.idListsRead.Count; i++)
			{
				if (this.idListsRead[i].parent == parent && this.idListsRead[i].pathRelToParent == pathRelToParent)
				{
					List<string> targetLoadIDs = this.idListsRead[i].targetLoadIDs;
					this.idListsRead.RemoveAt(i);
					return targetLoadIDs;
				}
			}
			Log.Error("Could not get load IDs list. We're asking for something which was never added during LoadingVars. pathRelToParent=" + pathRelToParent + ", parent=" + parent.ToStringSafe<IExposable>());
			return new List<string>();
		}
	}
}
