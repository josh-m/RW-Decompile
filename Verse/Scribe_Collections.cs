using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Verse
{
	public static class Scribe_Collections
	{
		public static void Look<T>(ref List<T> list, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
		{
			Scribe_Collections.Look<T>(ref list, false, label, lookMode, ctorArgs);
		}

		public static void Look<T>(ref List<T> list, bool saveDestroyedThings, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
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
				else if (typeof(T) == typeof(BodyPartRecord))
				{
					lookMode = LookMode.BodyPart;
				}
				else
				{
					if (!typeof(IExposable).IsAssignableFrom(typeof(T)) || typeof(ILoadReferenceable).IsAssignableFrom(typeof(T)))
					{
						Log.Error("LookList call with a list of " + typeof(T) + " must have lookMode set explicitly.", false);
						return;
					}
					lookMode = LookMode.Deep;
				}
			}
			if (Scribe.EnterNode(label))
			{
				try
				{
					if (Scribe.mode == LoadSaveMode.Saving)
					{
						if (list == null)
						{
							Scribe.saver.WriteAttribute("IsNull", "True");
						}
						else
						{
							foreach (T current in list)
							{
								if (lookMode == LookMode.Value)
								{
									T t = current;
									Scribe_Values.Look<T>(ref t, "li", default(T), true);
								}
								else if (lookMode == LookMode.LocalTargetInfo)
								{
									LocalTargetInfo localTargetInfo = (LocalTargetInfo)((object)current);
									Scribe_TargetInfo.Look(ref localTargetInfo, saveDestroyedThings, "li");
								}
								else if (lookMode == LookMode.TargetInfo)
								{
									TargetInfo targetInfo = (TargetInfo)((object)current);
									Scribe_TargetInfo.Look(ref targetInfo, saveDestroyedThings, "li");
								}
								else if (lookMode == LookMode.GlobalTargetInfo)
								{
									GlobalTargetInfo globalTargetInfo = (GlobalTargetInfo)((object)current);
									Scribe_TargetInfo.Look(ref globalTargetInfo, saveDestroyedThings, "li");
								}
								else if (lookMode == LookMode.Def)
								{
									Def def = (Def)((object)current);
									Scribe_Defs.Look<Def>(ref def, "li");
								}
								else if (lookMode == LookMode.BodyPart)
								{
									BodyPartRecord bodyPartRecord = (BodyPartRecord)((object)current);
									Scribe_BodyParts.Look(ref bodyPartRecord, "li", null);
								}
								else if (lookMode == LookMode.Deep)
								{
									T t2 = current;
									Scribe_Deep.Look<T>(ref t2, saveDestroyedThings, "li", ctorArgs);
								}
								else if (lookMode == LookMode.Reference)
								{
									ILoadReferenceable loadReferenceable = (ILoadReferenceable)((object)current);
									Scribe_References.Look<ILoadReferenceable>(ref loadReferenceable, "li", saveDestroyedThings);
								}
							}
						}
					}
					else if (Scribe.mode == LoadSaveMode.LoadingVars)
					{
						XmlNode curXmlParent = Scribe.loader.curXmlParent;
						XmlAttribute xmlAttribute = curXmlParent.Attributes["IsNull"];
						if (xmlAttribute != null && xmlAttribute.Value.ToLower() == "true")
						{
							list = null;
						}
						else if (lookMode == LookMode.Value)
						{
							list = new List<T>(curXmlParent.ChildNodes.Count);
							foreach (XmlNode subNode in curXmlParent.ChildNodes)
							{
								T item = ScribeExtractor.ValueFromNode<T>(subNode, default(T));
								list.Add(item);
							}
						}
						else if (lookMode == LookMode.Deep)
						{
							list = new List<T>(curXmlParent.ChildNodes.Count);
							foreach (XmlNode subNode2 in curXmlParent.ChildNodes)
							{
								T item2 = ScribeExtractor.SaveableFromNode<T>(subNode2, ctorArgs);
								list.Add(item2);
							}
						}
						else if (lookMode == LookMode.Def)
						{
							list = new List<T>(curXmlParent.ChildNodes.Count);
							foreach (XmlNode subNode3 in curXmlParent.ChildNodes)
							{
								T item3 = ScribeExtractor.DefFromNodeUnsafe<T>(subNode3);
								list.Add(item3);
							}
						}
						else if (lookMode == LookMode.BodyPart)
						{
							list = new List<T>(curXmlParent.ChildNodes.Count);
							int num = 0;
							foreach (XmlNode node in curXmlParent.ChildNodes)
							{
								T item4 = (T)((object)ScribeExtractor.BodyPartFromNode(node, num.ToString(), null));
								list.Add(item4);
								num++;
							}
						}
						else if (lookMode == LookMode.LocalTargetInfo)
						{
							list = new List<T>(curXmlParent.ChildNodes.Count);
							int num2 = 0;
							foreach (XmlNode node2 in curXmlParent.ChildNodes)
							{
								LocalTargetInfo localTargetInfo2 = ScribeExtractor.LocalTargetInfoFromNode(node2, num2.ToString(), LocalTargetInfo.Invalid);
								T item5 = (T)((object)localTargetInfo2);
								list.Add(item5);
								num2++;
							}
						}
						else if (lookMode == LookMode.TargetInfo)
						{
							list = new List<T>(curXmlParent.ChildNodes.Count);
							int num3 = 0;
							foreach (XmlNode node3 in curXmlParent.ChildNodes)
							{
								TargetInfo targetInfo2 = ScribeExtractor.TargetInfoFromNode(node3, num3.ToString(), TargetInfo.Invalid);
								T item6 = (T)((object)targetInfo2);
								list.Add(item6);
								num3++;
							}
						}
						else if (lookMode == LookMode.GlobalTargetInfo)
						{
							list = new List<T>(curXmlParent.ChildNodes.Count);
							int num4 = 0;
							foreach (XmlNode node4 in curXmlParent.ChildNodes)
							{
								GlobalTargetInfo globalTargetInfo2 = ScribeExtractor.GlobalTargetInfoFromNode(node4, num4.ToString(), GlobalTargetInfo.Invalid);
								T item7 = (T)((object)globalTargetInfo2);
								list.Add(item7);
								num4++;
							}
						}
						else if (lookMode == LookMode.Reference)
						{
							List<string> list2 = new List<string>(curXmlParent.ChildNodes.Count);
							foreach (XmlNode xmlNode in curXmlParent.ChildNodes)
							{
								list2.Add(xmlNode.InnerText);
							}
							Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(list2, string.Empty);
						}
					}
					else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
					{
						if (lookMode == LookMode.Reference)
						{
							list = Scribe.loader.crossRefs.TakeResolvedRefList<T>(string.Empty);
						}
						else if (lookMode == LookMode.LocalTargetInfo)
						{
							if (list != null)
							{
								for (int i = 0; i < list.Count; i++)
								{
									list[i] = (T)((object)ScribeExtractor.ResolveLocalTargetInfo((LocalTargetInfo)((object)list[i]), i.ToString()));
								}
							}
						}
						else if (lookMode == LookMode.TargetInfo)
						{
							if (list != null)
							{
								for (int j = 0; j < list.Count; j++)
								{
									list[j] = (T)((object)ScribeExtractor.ResolveTargetInfo((TargetInfo)((object)list[j]), j.ToString()));
								}
							}
						}
						else if (lookMode == LookMode.GlobalTargetInfo && list != null)
						{
							for (int k = 0; k < list.Count; k++)
							{
								list[k] = (T)((object)ScribeExtractor.ResolveGlobalTargetInfo((GlobalTargetInfo)((object)list[k]), k.ToString()));
							}
						}
					}
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				if (lookMode == LookMode.Reference)
				{
					Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(null, label);
				}
				list = null;
			}
		}

		public static void Look<K, V>(ref Dictionary<K, V> dict, string label, LookMode keyLookMode = LookMode.Undefined, LookMode valueLookMode = LookMode.Undefined)
		{
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				bool flag = keyLookMode == LookMode.Reference;
				bool flag2 = valueLookMode == LookMode.Reference;
				if (flag != flag2)
				{
					Log.Error("You need to provide working lists for the keys and values in order to be able to load such dictionary. label=" + label, false);
				}
			}
			List<K> list = null;
			List<V> list2 = null;
			Scribe_Collections.Look<K, V>(ref dict, label, keyLookMode, valueLookMode, ref list, ref list2);
		}

		public static void Look<K, V>(ref Dictionary<K, V> dict, string label, LookMode keyLookMode, LookMode valueLookMode, ref List<K> keysWorkingList, ref List<V> valuesWorkingList)
		{
			if (Scribe.EnterNode(label))
			{
				try
				{
					if (Scribe.mode == LoadSaveMode.Saving && dict == null)
					{
						Scribe.saver.WriteAttribute("IsNull", "True");
					}
					else
					{
						if (Scribe.mode == LoadSaveMode.LoadingVars)
						{
							XmlAttribute xmlAttribute = Scribe.loader.curXmlParent.Attributes["IsNull"];
							if (xmlAttribute != null && xmlAttribute.Value.ToLower() == "true")
							{
								dict = null;
							}
							else
							{
								dict = new Dictionary<K, V>();
							}
						}
						if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
						{
							keysWorkingList = new List<K>();
							valuesWorkingList = new List<V>();
							if (Scribe.mode == LoadSaveMode.Saving && dict != null)
							{
								foreach (KeyValuePair<K, V> current in dict)
								{
									keysWorkingList.Add(current.Key);
									valuesWorkingList.Add(current.Value);
								}
							}
						}
						Scribe_Collections.Look<K>(ref keysWorkingList, "keys", keyLookMode, new object[0]);
						Scribe_Collections.Look<V>(ref valuesWorkingList, "values", valueLookMode, new object[0]);
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
						if (((flag && Scribe.mode == LoadSaveMode.ResolvingCrossRefs) || (!flag && Scribe.mode == LoadSaveMode.LoadingVars)) && dict != null)
						{
							if (keysWorkingList == null)
							{
								Log.Error("Cannot fill dictionary because there are no keys. label=" + label, false);
							}
							else if (valuesWorkingList == null)
							{
								Log.Error("Cannot fill dictionary because there are no values. label=" + label, false);
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
										valuesWorkingList.Count,
										", label=",
										label
									}), false);
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
											". label=",
											label
										}), false);
									}
									else
									{
										try
										{
											dict.Add(keysWorkingList[i], valuesWorkingList[i]);
										}
										catch (OutOfMemoryException)
										{
											throw;
										}
										catch (Exception ex)
										{
											Log.Error(string.Concat(new object[]
											{
												"Exception in LookDictionary(label=",
												label,
												"): ",
												ex
											}), false);
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
					}
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				dict = null;
			}
		}

		public static void Look<T>(ref HashSet<T> valueHashSet, string label, LookMode lookMode = LookMode.Undefined)
		{
			Scribe_Collections.Look<T>(ref valueHashSet, false, label, lookMode);
		}

		public static void Look<T>(ref HashSet<T> valueHashSet, bool saveDestroyedThings, string label, LookMode lookMode = LookMode.Undefined)
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
			Scribe_Collections.Look<T>(ref list, saveDestroyedThings, label, lookMode, new object[0]);
			if ((lookMode == LookMode.Reference && Scribe.mode == LoadSaveMode.ResolvingCrossRefs) || (lookMode != LookMode.Reference && Scribe.mode == LoadSaveMode.LoadingVars))
			{
				if (list == null)
				{
					valueHashSet = null;
				}
				else
				{
					valueHashSet = new HashSet<T>();
					for (int i = 0; i < list.Count; i++)
					{
						valueHashSet.Add(list[i]);
					}
				}
			}
		}

		public static void Look<T>(ref Stack<T> valueStack, string label, LookMode lookMode = LookMode.Undefined)
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
			Scribe_Collections.Look<T>(ref list, label, lookMode, new object[0]);
			if ((lookMode == LookMode.Reference && Scribe.mode == LoadSaveMode.ResolvingCrossRefs) || (lookMode != LookMode.Reference && Scribe.mode == LoadSaveMode.LoadingVars))
			{
				if (list == null)
				{
					valueStack = null;
				}
				else
				{
					valueStack = new Stack<T>();
					for (int i = 0; i < list.Count; i++)
					{
						valueStack.Push(list[i]);
					}
				}
			}
		}
	}
}
