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
				else if (typeof(T) == typeof(TargetInfo))
				{
					lookMode = LookMode.TargetInfo;
				}
				else if (typeof(Def).IsAssignableFrom(typeof(T)))
				{
					lookMode = LookMode.DefReference;
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
				if (list == null && lookMode == LookMode.MapReference)
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
						else if (lookMode == LookMode.TargetInfo)
						{
							TargetInfo targetInfo = (TargetInfo)((object)current);
							Scribe_TargetInfo.LookTargetInfo(ref targetInfo, saveDestroyedThings, "li");
						}
						else if (lookMode == LookMode.DefReference)
						{
							Def def = (Def)((object)current);
							Scribe_Defs.LookDef<Def>(ref def, "li");
						}
						else if (lookMode == LookMode.Deep)
						{
							T t2 = current;
							Scribe_Deep.LookDeep<T>(ref t2, saveDestroyedThings, "li", ctorArgs);
						}
						else if (lookMode == LookMode.MapReference)
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
				else if (lookMode == LookMode.DefReference)
				{
					list = new List<T>(xmlNode.ChildNodes.Count);
					foreach (XmlNode subNode3 in xmlNode.ChildNodes)
					{
						T item3 = ScribeExtractor.DefFromNodeUnsafe<T>(subNode3);
						list.Add(item3);
					}
				}
				else if (lookMode == LookMode.TargetInfo)
				{
					list = new List<T>(xmlNode.ChildNodes.Count);
					foreach (XmlNode subNode4 in xmlNode.ChildNodes)
					{
						TargetInfo targetInfo2 = ScribeExtractor.TargetInfoFromNode(subNode4, TargetInfo.Invalid);
						T item4 = (T)((object)targetInfo2);
						list.Add(item4);
					}
				}
				else if (lookMode == LookMode.MapReference)
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
				if (lookMode == LookMode.MapReference)
				{
					list = CrossRefResolver.NextResolvedRefList<T>();
				}
				else if (lookMode == LookMode.TargetInfo && list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						Thing thing = CrossRefResolver.NextResolvedRef<Thing>();
						IntVec3 cell = ((TargetInfo)((object)list[i])).Cell;
						if (thing != null)
						{
							list[i] = (T)((object)new TargetInfo(thing));
						}
						else
						{
							list[i] = (T)((object)new TargetInfo(cell));
						}
					}
				}
			}
		}

		public static void LookDictionary<K, V>(ref Dictionary<K, V> dict, string label, LookMode keyLookMode = LookMode.Undefined, LookMode valueLookMode = LookMode.Undefined) where K : new()
		{
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				return;
			}
			Scribe.EnterNode(label);
			List<K> list = new List<K>();
			List<V> list2 = new List<V>();
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				foreach (KeyValuePair<K, V> current in dict)
				{
					list.Add(current.Key);
					list2.Add(current.Value);
				}
			}
			Scribe_Collections.LookList<K>(ref list, "keys", keyLookMode, new object[0]);
			Scribe_Collections.LookList<V>(ref list2, "values", valueLookMode, new object[0]);
			bool flag = keyLookMode == LookMode.MapReference || valueLookMode == LookMode.MapReference;
			if ((flag && Scribe.mode == LoadSaveMode.ResolvingCrossRefs) || (!flag && Scribe.mode == LoadSaveMode.LoadingVars))
			{
				dict.Clear();
				if (list == null)
				{
					Log.Error("Cannot fill dictionary because there are no keys.");
				}
				else if (list2 == null)
				{
					Log.Error("Cannot fill dictionary because there are no values.");
				}
				else
				{
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i] == null)
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
								dict.Add(list[i], list2[i]);
							}
							catch (Exception arg)
							{
								Log.Error("Exception in LookDictionary: " + arg);
							}
						}
					}
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
			if ((lookMode == LookMode.MapReference && Scribe.mode == LoadSaveMode.ResolvingCrossRefs) || (lookMode != LookMode.MapReference && Scribe.mode == LoadSaveMode.LoadingVars))
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
			if ((lookMode == LookMode.MapReference && Scribe.mode == LoadSaveMode.ResolvingCrossRefs) || (lookMode != LookMode.MapReference && Scribe.mode == LoadSaveMode.LoadingVars))
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
