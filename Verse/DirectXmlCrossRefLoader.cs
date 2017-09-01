using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace Verse
{
	public static class DirectXmlCrossRefLoader
	{
		private abstract class WantedRef
		{
			public object wanter;

			public abstract bool TryResolve(FailMode failReportMode);
		}

		private class WantedRefForObject : DirectXmlCrossRefLoader.WantedRef
		{
			public FieldInfo fi;

			public string defName;

			public WantedRefForObject(object wanter, FieldInfo fi, string targetDefName)
			{
				this.wanter = wanter;
				this.fi = fi;
				this.defName = targetDefName;
			}

			public override bool TryResolve(FailMode failReportMode)
			{
				if (this.fi == null)
				{
					Log.Error("Trying to resolve null field for def named " + this.defName);
					return false;
				}
				Def defSilentFail = GenDefDatabase.GetDefSilentFail(this.fi.FieldType, this.defName);
				if (defSilentFail == null)
				{
					if (failReportMode == FailMode.LogErrors)
					{
						Log.Error(string.Concat(new object[]
						{
							"Could not resolve cross-reference: No ",
							this.fi.FieldType,
							" named ",
							this.defName,
							" found to give to ",
							this.wanter.GetType(),
							" ",
							this.wanter
						}));
					}
					return false;
				}
				SoundDef soundDef = defSilentFail as SoundDef;
				if (soundDef != null && soundDef.isUndefined)
				{
					Log.Warning(string.Concat(new object[]
					{
						"Could not resolve cross-reference: No ",
						this.fi.FieldType,
						" named ",
						this.defName,
						" found to give to ",
						this.wanter.GetType(),
						" ",
						this.wanter,
						" (using undefined sound instead)"
					}));
				}
				this.fi.SetValue(this.wanter, defSilentFail);
				return true;
			}
		}

		private class WantedRefForList<T> : DirectXmlCrossRefLoader.WantedRef where T : new()
		{
			private List<string> defNames = new List<string>();

			public WantedRefForList(object wanter)
			{
				this.wanter = wanter;
			}

			public void AddWantedListEntry(string newTargetDefName)
			{
				this.defNames.Add(newTargetDefName);
			}

			public override bool TryResolve(FailMode failReportMode)
			{
				bool flag = false;
				for (int i = 0; i < this.defNames.Count; i++)
				{
					T t = DirectXmlCrossRefLoader.TryResolveDef<T>(this.defNames[i], failReportMode);
					if (t != null)
					{
						((List<T>)this.wanter).Add(t);
						this.defNames.RemoveAt(i);
						i--;
					}
					else
					{
						flag = true;
					}
				}
				return !flag;
			}
		}

		private class WantedRefForDictionary<K, V> : DirectXmlCrossRefLoader.WantedRef where K : new() where V : new()
		{
			private List<XmlNode> wantedDictRefs = new List<XmlNode>();

			public WantedRefForDictionary(object wanter)
			{
				this.wanter = wanter;
			}

			public void AddWantedDictEntry(XmlNode entryNode)
			{
				this.wantedDictRefs.Add(entryNode);
			}

			public override bool TryResolve(FailMode failReportMode)
			{
				failReportMode = FailMode.LogErrors;
				bool flag = typeof(Def).IsAssignableFrom(typeof(K));
				bool flag2 = typeof(Def).IsAssignableFrom(typeof(V));
				List<Pair<K, V>> list = new List<Pair<K, V>>();
				foreach (XmlNode current in this.wantedDictRefs)
				{
					XmlNode xmlNode = current["key"];
					XmlNode xmlNode2 = current["value"];
					K first;
					if (flag)
					{
						first = DirectXmlCrossRefLoader.TryResolveDef<K>(xmlNode.InnerText, failReportMode);
					}
					else
					{
						first = DirectXmlToObject.ObjectFromXml<K>(xmlNode, true);
					}
					V second;
					if (flag2)
					{
						second = DirectXmlCrossRefLoader.TryResolveDef<V>(xmlNode2.InnerText, failReportMode);
					}
					else
					{
						second = DirectXmlToObject.ObjectFromXml<V>(xmlNode2, true);
					}
					list.Add(new Pair<K, V>(first, second));
				}
				Dictionary<K, V> dictionary = (Dictionary<K, V>)this.wanter;
				dictionary.Clear();
				foreach (Pair<K, V> current2 in list)
				{
					try
					{
						dictionary.Add(current2.First, current2.Second);
					}
					catch
					{
						Log.Error(string.Concat(new object[]
						{
							"Failed to load key/value pair: ",
							current2.First,
							", ",
							current2.Second
						}));
					}
				}
				return true;
			}
		}

		private static List<DirectXmlCrossRefLoader.WantedRef> wantedRefs = new List<DirectXmlCrossRefLoader.WantedRef>();

		public static bool LoadingInProgress
		{
			get
			{
				return DirectXmlCrossRefLoader.wantedRefs.Count > 0;
			}
		}

		public static void RegisterObjectWantsCrossRef(object wanter, FieldInfo fi, string targetDefName)
		{
			DirectXmlCrossRefLoader.WantedRefForObject item = new DirectXmlCrossRefLoader.WantedRefForObject(wanter, fi, targetDefName);
			DirectXmlCrossRefLoader.wantedRefs.Add(item);
		}

		public static void RegisterObjectWantsCrossRef(object wanter, string fieldName, string targetDefName)
		{
			DirectXmlCrossRefLoader.WantedRefForObject item = new DirectXmlCrossRefLoader.WantedRefForObject(wanter, wanter.GetType().GetField(fieldName), targetDefName);
			DirectXmlCrossRefLoader.wantedRefs.Add(item);
		}

		public static void RegisterListWantsCrossRef<T>(List<T> wanterList, string targetDefName) where T : new()
		{
			DirectXmlCrossRefLoader.WantedRefForList<T> wantedRefForList = null;
			foreach (DirectXmlCrossRefLoader.WantedRef current in DirectXmlCrossRefLoader.wantedRefs)
			{
				if (current.wanter == wanterList)
				{
					wantedRefForList = (DirectXmlCrossRefLoader.WantedRefForList<T>)current;
					break;
				}
			}
			if (wantedRefForList == null)
			{
				wantedRefForList = new DirectXmlCrossRefLoader.WantedRefForList<T>(wanterList);
				DirectXmlCrossRefLoader.wantedRefs.Add(wantedRefForList);
			}
			wantedRefForList.AddWantedListEntry(targetDefName);
		}

		public static void RegisterDictionaryWantsCrossRef<K, V>(Dictionary<K, V> wanterDict, XmlNode entryNode) where K : new() where V : new()
		{
			DirectXmlCrossRefLoader.WantedRefForDictionary<K, V> wantedRefForDictionary = null;
			foreach (DirectXmlCrossRefLoader.WantedRef current in DirectXmlCrossRefLoader.wantedRefs)
			{
				if (current.wanter == wanterDict)
				{
					wantedRefForDictionary = (DirectXmlCrossRefLoader.WantedRefForDictionary<K, V>)current;
					break;
				}
			}
			if (wantedRefForDictionary == null)
			{
				wantedRefForDictionary = new DirectXmlCrossRefLoader.WantedRefForDictionary<K, V>(wanterDict);
				DirectXmlCrossRefLoader.wantedRefs.Add(wantedRefForDictionary);
			}
			wantedRefForDictionary.AddWantedDictEntry(entryNode);
		}

		public static T TryResolveDef<T>(string defName, FailMode failReportMode)
		{
			T t = (T)((object)GenDefDatabase.GetDefSilentFail(typeof(T), defName));
			if (t != null)
			{
				return t;
			}
			if (failReportMode == FailMode.LogErrors)
			{
				Log.Error(string.Concat(new object[]
				{
					"Could not resolve cross-reference to ",
					typeof(T),
					" named ",
					defName
				}));
			}
			return default(T);
		}

		public static void Clear()
		{
			DirectXmlCrossRefLoader.wantedRefs.Clear();
		}

		public static void ResolveAllWantedCrossReferences(FailMode failReportMode)
		{
			foreach (DirectXmlCrossRefLoader.WantedRef current in DirectXmlCrossRefLoader.wantedRefs.ListFullCopy<DirectXmlCrossRefLoader.WantedRef>())
			{
				if (current.TryResolve(failReportMode))
				{
					DirectXmlCrossRefLoader.wantedRefs.Remove(current);
				}
			}
		}
	}
}
