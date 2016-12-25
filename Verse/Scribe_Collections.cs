using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Verse
{
	public static class Scribe_Collections
	{
		public static void LookList<T>(ref List<T> list, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
		{
			Scribe_Collections.LookList<T>(ref list, false, label, lookMode, ctorArgs);
		}

		public static void LookList<T>(ref List<T> list, bool saveDestroyedThings, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
		{
			if (lookMode == LookMode.Undefined)
			{
				if (ParseHelper.HandlesType(typeof(T)))
				{
					lookMode = LookMode.Value;
				}
				else if (typeof(T) == typeof(LocalTargetInfo))
				{
					lookMode = LookMode.LocalTargetInfo;
				}
				else if (typeof(T) == typeof(TargetInfo))
				{
					lookMode = LookMode.TargetInfo;
				}
				else if (typeof(T) == typeof(GlobalTargetInfo))
				{
					lookMode = LookMode.GlobalTargetInfo;
				}
				else if (typeof(Def).IsAssignableFrom(typeof(T)))
				{
					lookMode = LookMode.Def;
				}
				else
				{
					if (!typeof(IExposable).IsAssignableFrom(typeof(T)) || typeof(ILoadReferenceable).IsAssignableFrom(typeof(T)))
					{
						Log.Error("LookList call with a list of " + typeof(T) + " must have lookMode set explicitly.");
						return;
					}
					lookMode = LookMode.Deep;
				}
			}
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (list == null && lookMode == LookMode.Reference)
				{
					Log.Warning(string.Concat(new object[]
					{
						"Saving null list \"",
						label,
						"\" with look mode ",
						lookMode,
						". This will cause bugs because null lists are not registered during loading so CrossRefResolver will break."
					}));
				}
				Scribe.EnterNode(label);
				if (list == null)
				{
					Scribe.WriteAttribute("IsNull", "True");
				}
				else
				{
					foreach (T current in list)
					{
						if (lookMode == LookMode.Value)
						{
							T t = current;
							Scribe_Values.LookValue<T>(ref t, "li", default(T), true);
						}
						else if (lookMode == LookMode.LocalTargetInfo)
						{
							LocalTargetInfo localTargetInfo = (LocalTargetInfo)((object)current);
							Scribe_TargetInfo.LookTargetInfo(ref localTargetInfo, saveDestroyedThings, "li");
						}
						else if (lookMode == LookMode.TargetInfo)
						{
							TargetInfo targetInfo = (TargetInfo)((object)current);
							Scribe_TargetInfo.LookTargetInfo(ref targetInfo, saveDestroyedThings, "li");
						}
						else if (lookMode == LookMode.GlobalTargetInfo)
						{
							GlobalTargetInfo globalTargetInfo = (GlobalTargetInfo)((object)current);
							Scribe_TargetInfo.LookTargetInfo(ref globalTargetInfo, saveDestroyedThings, "li");
						}
						else if (lookMode == LookMode.Def)
						{
							Def def = (Def)((object)current);
							Scribe_Defs.LookDef<Def>(ref def, "li");
						}
						else if (lookMode == LookMode.Deep)
						{
							T t2 = current;
							Scribe_Deep.LookDeep<T>(ref t2, saveDestroyedThings, "li", ctorArgs);
						}
						else if (lookMode == LookMode.Reference)
						{
							ILoadReferenceable loadReferenceable = (ILoadReferenceable)((object)current);
							Scribe_References.LookReference<ILoadReferenceable>(ref loadReferenceable, "li", saveDestroyedThings);
						}
					}
				}
				Scribe.ExitNode();
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				if (Scribe.curParent == null)
				{
					Log.Error("Scribe.curParent is null. I'm not sure why.");
					list = null;
					return;
				}
				XmlNode xmlNode = Scribe.curParent[label];
				if (xmlNode == null)
				{
					list = null;
					return;
				}
				XmlAttribute xmlAttribute = xmlNode.Attributes["IsNull"];
				if (xmlAttribute != null && xmlAttribute.Value.ToLower() == "true")
				{
					list = null;
					return;
				}
				if (lookMode == LookMode.Value)
				{
					list = new List<T>(xmlNode.ChildNodes.Count);
					foreach (XmlNode subNode in xmlNode.ChildNodes)
					{
						T item = ScribeExtractor.ValueFromNode<T>(subNode, default(T));
						list.Add(item);
					}
				}
				else if (lookMode == LookMode.Deep)
				{
					list = new List<T>(xmlNode.ChildNodes.Count);
					foreach (XmlNode subNode2 in xmlNode.ChildNodes)
					{
						T item2 = ScribeExtractor.SaveableFromNode<T>(subNode2, ctorArgs);
						list.Add(item2);
					}
				}
				else if (lookMode == LookMode.Def)
				{
					list = new List<T>(xmlNode.ChildNodes.Count);
					foreach (XmlNode subNode3 in xmlNode.ChildNodes)
					{
						T item3 = ScribeExtractor.DefFromNodeUnsafe<T>(subNode3);
						list.Add(item3);
					}
				}
				else if (lookMode == LookMode.LocalTargetInfo)
				{
					list = new List<T>(xmlNode.ChildNodes.Count);
					foreach (XmlNode subNode4 in xmlNode.ChildNodes)
					{
						LocalTargetInfo localTargetInfo2 = ScribeExtractor.LocalTargetInfoFromNode(subNode4, LocalTargetInfo.Invalid);
						T item4 = (T)((object)localTargetInfo2);
						list.Add(item4);
					}
				}
				else if (lookMode == LookMode.TargetInfo)
				{
					list = new List<T>(xmlNode.ChildNodes.Count);
					foreach (XmlNode subNode5 in xmlNode.ChildNodes)
					{
						TargetInfo targetInfo2 = ScribeExtractor.TargetInfoFromNode(subNode5, TargetInfo.Invalid);
						T item5 = (T)((object)targetInfo2);
						list.Add(item5);
					}
				}
				else if (lookMode == LookMode.GlobalTargetInfo)
				{
					list = new List<T>(xmlNode.ChildNodes.Count);
					foreach (XmlNode subNode6 in xmlNode.ChildNodes)
					{
						GlobalTargetInfo globalTargetInfo2 = ScribeExtractor.GlobalTargetInfoFromNode(subNode6, GlobalTargetInfo.Invalid);
						T item6 = (T)((object)globalTargetInfo2);
						list.Add(item6);
					}
				}
				else if (lookMode == LookMode.Reference)
				{
					List<string> list2 = new List<string>(xmlNode.ChildNodes.Count);
					foreach (XmlNode xmlNode2 in xmlNode.ChildNodes)
					{
						list2.Add(xmlNode2.InnerText);
					}
					LoadIDsWantedBank.RegisterLoadIDListReadFromXml(list2);
				}
			}
			else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				if (lookMode == LookMode.Reference)
				{
					list = CrossRefResolver.NextResolvedRefList<T>();
				}
				else if (lookMode == LookMode.LocalTargetInfo)
				{
					if (list != null)
					{
						for (int i = 0; i < list.Count; i++)
						{
							list[i] = (T)((object)ScribeExtractor.ResolveLocalTargetInfo((LocalTargetInfo)((object)list[i])));
						}
					}
				}
				else if (lookMode == LookMode.TargetInfo)
				{
					if (list != null)
					{
						for (int j = 0; j < list.Count; j++)
						{
							list[j] = (T)((object)ScribeExtractor.ResolveTargetInfo((TargetInfo)((object)list[j])));
						}
					}
				}
				else if (lookMode == LookMode.GlobalTargetInfo && list != null)
				{
					for (int k = 0; k < list.Count; k++)
					{
						list[k] = (T)((object)ScribeExtractor.ResolveGlobalTargetInfo((GlobalTargetInfo)((object)list[k])));
					}
				}
			}
		}

		public static void LookDictionary<K, V>(ref Dictionary<K, V> dict, string label, LookMode keyLookMode = LookMode.Undefined, LookMode valueLookMode = LookMode.Undefined) where K : new()
		{
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				bool flag = keyLookMode == LookMode.Reference;
				bool flag2 = valueLookMode == LookMode.Reference;
				if (flag != flag2)
				{
					Log.Error("You need to provide working lists for the keys and values in order to be able to load such dictionary.");
					return;
				}
			}
			List<K> list = null;
			List<V> list2 = null;
			Scribe_Collections.LookDictionary<K, V>(ref dict, label, keyLookMode, valueLookMode, ref list, ref list2);
		}

		public static void LookDictionary<K, V>(ref Dictionary<K, V> dict, string label, LookMode keyLookMode, LookMode valueLookMode, ref List<K> keysWorkingList, ref List<V> valuesWorkingList) where K : new()
		{
			Scribe.EnterNode(label);
			if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
			{
				keysWorkingList = new List<K>();
				valuesWorkingList = new List<V>();
			}
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				foreach (KeyValuePair<K, V> current in dict)
				{
					keysWorkingList.Add(current.Key);
					valuesWorkingList.Add(current.Value);
				}
			}
			Scribe_Collections.LookList<K>(ref keysWorkingList, "keys", keyLookMode, new object[0]);
			Scribe_Collections.LookList<V>(ref valuesWorkingList, "values", valueLookMode, new object[0]);
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (keysWorkingList != null)
				{
					keysWorkingList.Clear();
					keysWorkingList = null;
				}
				if (valuesWorkingList != null)
				{
					valuesWorkingList.Clear();
					valuesWorkingList = null;
				}
			}
			bool flag = keyLookMode == LookMode.Reference || valueLookMode == LookMode.Reference;
			if ((flag && Scribe.mode == LoadSaveMode.ResolvingCrossRefs) || (!flag && Scribe.mode == LoadSaveMode.LoadingVars))
			{
				dict.Clear();
				if (keysWorkingList == null)
				{
					Log.Error("Cannot fill dictionary because there are no keys.");
				}
				else if (valuesWorkingList == null)
				{
					Log.Error("Cannot fill dictionary because there are no values.");
				}
				else
				{
					if (keysWorkingList.Count != valuesWorkingList.Count)
					{
						Log.Error(string.Concat(new object[]
						{
							"Keys count does not match the values count while loading a dictionary (maybe keys and values were resolved during different passes?). Some elements will be skipped. keys=",
							keysWorkingList.Count,
							", values=",
							valuesWorkingList.Count
						}));
					}
					int num = Math.Min(keysWorkingList.Count, valuesWorkingList.Count);
					for (int i = 0; i < num; i++)
					{
						if (keysWorkingList[i] == null)
						{
							Log.Error(string.Concat(new object[]
							{
								"Null key while loading dictionary of ",
								typeof(K),
								" and ",
								typeof(V),
								"."
							}));
						}
						else
						{
							try
							{
								dict.Add(keysWorkingList[i], valuesWorkingList[i]);
							}
							catch (Exception ex)
							{
								Log.Error(string.Concat(new object[]
								{
									"Exception in LookDictionary(node=",
									label,
									"): ",
									ex
								}));
							}
						}
					}
				}
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (keysWorkingList != null)
				{
					keysWorkingList.Clear();
					keysWorkingList = null;
				}
				if (valuesWorkingList != null)
				{
					valuesWorkingList.Clear();
					valuesWorkingList = null;
				}
			}
			Scribe.ExitNode();
		}

		public static void LookHashSet<T>(ref HashSet<T> valueHashSet, string label, LookMode lookMode = LookMode.Undefined) where T : new()
		{
			Scribe_Collections.LookHashSet<T>(ref valueHashSet, false, label, lookMode);
		}

		public static void LookHashSet<T>(ref HashSet<T> valueHashSet, bool saveDestroyedThings, string label, LookMode lookMode = LookMode.Undefined) where T : new()
		{
			List<T> list = null;
			if (Scribe.mode == LoadSaveMode.Saving && valueHashSet != null)
			{
				list = new List<T>();
				foreach (T current in valueHashSet)
				{
					list.Add(current);
				}
			}
			Scribe_Collections.LookList<T>(ref list, saveDestroyedThings, label, lookMode, new object[0]);
			if ((lookMode == LookMode.Reference && Scribe.mode == LoadSaveMode.ResolvingCrossRefs) || (lookMode != LookMode.Reference && Scribe.mode == LoadSaveMode.LoadingVars))
			{
				if (list == null)
				{
					valueHashSet = null;
				}
				else
				{
					if (valueHashSet == null)
					{
						valueHashSet = new HashSet<T>();
					}
					else
					{
						valueHashSet.Clear();
					}
					for (int i = 0; i < list.Count; i++)
					{
						valueHashSet.Add(list[i]);
					}
				}
			}
		}

		public static void LookStack<T>(ref Stack<T> valueStack, string label, LookMode lookMode = LookMode.Undefined) where T : new()
		{
			List<T> list = null;
			if (Scribe.mode == LoadSaveMode.Saving && valueStack != null)
			{
				list = new List<T>();
				foreach (T current in valueStack)
				{
					list.Add(current);
				}
			}
			Scribe_Collections.LookList<T>(ref list, label, lookMode, new object[0]);
			if ((lookMode == LookMode.Reference && Scribe.mode == LoadSaveMode.ResolvingCrossRefs) || (lookMode != LookMode.Reference && Scribe.mode == LoadSaveMode.LoadingVars))
			{
				if (list == null)
				{
					valueStack = null;
				}
				else
				{
					if (valueStack == null)
					{
						valueStack = new Stack<T>();
					}
					else
					{
						valueStack.Clear();
					}
					for (int i = 0; i < list.Count; i++)
					{
						valueStack.Push(list[i]);
					}
				}
			}
		}
	}
}
