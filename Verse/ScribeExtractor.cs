using RimWorld.Planet;
using System;
using System.Xml;

namespace Verse
{
	public static class ScribeExtractor
	{
		public static T ValueFromNode<T>(XmlNode subNode, T defaultValue)
		{
			if (subNode == null)
			{
				return defaultValue;
			}
			T result;
			try
			{
				try
				{
					result = (T)((object)ParseHelper.FromString(subNode.InnerText, typeof(T)));
					return result;
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat(new object[]
					{
						"Exception parsing node ",
						subNode.OuterXml,
						" into a ",
						typeof(T),
						":\n",
						ex.ToString()
					}));
				}
				result = default(T);
			}
			catch (Exception arg)
			{
				Log.Error("Exception loading XML: " + arg);
				result = defaultValue;
			}
			return result;
		}

		public static T DefFromNode<T>(XmlNode subNode) where T : Def, new()
		{
			if (subNode == null || subNode.InnerText == null || subNode.InnerText == "null")
			{
				return (T)((object)null);
			}
			string defName = BackCompatibility.BackCompatibleDefName(typeof(T), subNode.InnerText);
			T namedSilentFail = DefDatabase<T>.GetNamedSilentFail(defName);
			if (namedSilentFail == null)
			{
				Log.Error(string.Concat(new object[]
				{
					"Could not load reference to ",
					typeof(T),
					" named ",
					subNode.InnerText
				}));
			}
			return namedSilentFail;
		}

		public static T DefFromNodeUnsafe<T>(XmlNode subNode)
		{
			return (T)((object)GenGeneric.InvokeStaticGenericMethod(typeof(ScribeExtractor), typeof(T), "DefFromNode", new object[]
			{
				subNode
			}));
		}

		public static T SaveableFromNode<T>(XmlNode subNode, object[] ctorArgs)
		{
			if (Scribe.mode != LoadSaveMode.LoadingVars)
			{
				Log.Error("Called SaveableFromNode(), but mode is " + Scribe.mode);
				return default(T);
			}
			if (subNode == null)
			{
				return default(T);
			}
			XmlAttribute xmlAttribute = subNode.Attributes["IsNull"];
			T result;
			if (xmlAttribute != null && xmlAttribute.Value == "True")
			{
				result = default(T);
			}
			else
			{
				try
				{
					XmlAttribute xmlAttribute2 = subNode.Attributes["Class"];
					string text = (xmlAttribute2 == null) ? typeof(T).FullName : xmlAttribute2.Value;
					Type type = BackCompatibility.GetBackCompatibleType(typeof(T), text, subNode);
					if (type == null)
					{
						Log.Error(string.Concat(new object[]
						{
							"Could not find class ",
							text,
							" while resolving node ",
							subNode.Name,
							". Trying to use ",
							typeof(T),
							" instead. Full node: ",
							subNode.OuterXml
						}));
						type = typeof(T);
					}
					if (type.IsAbstract)
					{
						throw new ArgumentException("Can't load abstract class " + type);
					}
					IExposable exposable = (IExposable)Activator.CreateInstance(type, ctorArgs);
					bool flag = typeof(T).IsValueType || typeof(Name).IsAssignableFrom(typeof(T));
					if (!flag)
					{
						Scribe.loader.crossRefs.RegisterForCrossRefResolve(exposable);
					}
					XmlNode curXmlParent = Scribe.loader.curXmlParent;
					IExposable curParent = Scribe.loader.curParent;
					string curPathRelToParent = Scribe.loader.curPathRelToParent;
					Scribe.loader.curXmlParent = subNode;
					Scribe.loader.curParent = exposable;
					Scribe.loader.curPathRelToParent = null;
					try
					{
						exposable.ExposeData();
					}
					finally
					{
						Scribe.loader.curXmlParent = curXmlParent;
						Scribe.loader.curParent = curParent;
						Scribe.loader.curPathRelToParent = curPathRelToParent;
					}
					if (!flag)
					{
						Scribe.loader.initer.RegisterForPostLoadInit(exposable);
					}
					result = (T)((object)exposable);
				}
				catch (Exception ex)
				{
					result = default(T);
					Log.Error(string.Concat(new object[]
					{
						"SaveableFromNode exception: ",
						ex,
						"\nSubnode:\n",
						subNode.OuterXml
					}));
				}
			}
			return result;
		}

		public static LocalTargetInfo LocalTargetInfoFromNode(XmlNode node, string label, LocalTargetInfo defaultValue)
		{
			LoadIDsWantedBank loadIDs = Scribe.loader.crossRefs.loadIDs;
			if (node != null && Scribe.EnterNode(label))
			{
				try
				{
					string innerText = node.InnerText;
					LocalTargetInfo result;
					if (innerText.Length != 0 && innerText[0] == '(')
					{
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(Thing), "thing");
						result = new LocalTargetInfo(IntVec3.FromString(innerText));
						return result;
					}
					loadIDs.RegisterLoadIDReadFromXml(innerText, typeof(Thing), "thing");
					result = LocalTargetInfo.Invalid;
					return result;
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
			loadIDs.RegisterLoadIDReadFromXml(null, typeof(Thing), label + "/thing");
			return defaultValue;
		}

		public static TargetInfo TargetInfoFromNode(XmlNode node, string label, TargetInfo defaultValue)
		{
			LoadIDsWantedBank loadIDs = Scribe.loader.crossRefs.loadIDs;
			if (node != null && Scribe.EnterNode(label))
			{
				try
				{
					string innerText = node.InnerText;
					TargetInfo result;
					if (innerText.Length != 0 && innerText[0] == '(')
					{
						string str;
						string targetLoadID;
						ScribeExtractor.ExtractCellAndMapPairFromTargetInfo(innerText, out str, out targetLoadID);
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(Thing), "thing");
						loadIDs.RegisterLoadIDReadFromXml(targetLoadID, typeof(Map), "map");
						result = new TargetInfo(IntVec3.FromString(str), null, true);
						return result;
					}
					loadIDs.RegisterLoadIDReadFromXml(innerText, typeof(Thing), "thing");
					loadIDs.RegisterLoadIDReadFromXml(null, typeof(Map), "map");
					result = TargetInfo.Invalid;
					return result;
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
			loadIDs.RegisterLoadIDReadFromXml(null, typeof(Thing), label + "/thing");
			loadIDs.RegisterLoadIDReadFromXml(null, typeof(Map), label + "/map");
			return defaultValue;
		}

		public static GlobalTargetInfo GlobalTargetInfoFromNode(XmlNode node, string label, GlobalTargetInfo defaultValue)
		{
			LoadIDsWantedBank loadIDs = Scribe.loader.crossRefs.loadIDs;
			if (node != null && Scribe.EnterNode(label))
			{
				try
				{
					string innerText = node.InnerText;
					GlobalTargetInfo result;
					if (innerText.Length != 0 && innerText[0] == '(')
					{
						string str;
						string targetLoadID;
						ScribeExtractor.ExtractCellAndMapPairFromTargetInfo(innerText, out str, out targetLoadID);
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(Thing), "thing");
						loadIDs.RegisterLoadIDReadFromXml(targetLoadID, typeof(Map), "map");
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(WorldObject), "worldObject");
						result = new GlobalTargetInfo(IntVec3.FromString(str), null, true);
						return result;
					}
					int tile;
					if (int.TryParse(innerText, out tile))
					{
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(Thing), "thing");
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(Map), "map");
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(WorldObject), "worldObject");
						result = new GlobalTargetInfo(tile);
						return result;
					}
					if (innerText.Length != 0 && innerText[0] == '@')
					{
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(Thing), "thing");
						loadIDs.RegisterLoadIDReadFromXml(null, typeof(Map), "map");
						loadIDs.RegisterLoadIDReadFromXml(innerText.Substring(1), typeof(WorldObject), "worldObject");
						result = GlobalTargetInfo.Invalid;
						return result;
					}
					loadIDs.RegisterLoadIDReadFromXml(innerText, typeof(Thing), "thing");
					loadIDs.RegisterLoadIDReadFromXml(null, typeof(Map), "map");
					loadIDs.RegisterLoadIDReadFromXml(null, typeof(WorldObject), "worldObject");
					result = GlobalTargetInfo.Invalid;
					return result;
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
			loadIDs.RegisterLoadIDReadFromXml(null, typeof(Thing), label + "/thing");
			loadIDs.RegisterLoadIDReadFromXml(null, typeof(Map), label + "/map");
			loadIDs.RegisterLoadIDReadFromXml(null, typeof(WorldObject), label + "/worldObject");
			return defaultValue;
		}

		public static LocalTargetInfo ResolveLocalTargetInfo(LocalTargetInfo loaded, string label)
		{
			if (Scribe.EnterNode(label))
			{
				try
				{
					Thing thing = Scribe.loader.crossRefs.TakeResolvedRef<Thing>("thing");
					IntVec3 cell = loaded.Cell;
					LocalTargetInfo result;
					if (thing != null)
					{
						result = new LocalTargetInfo(thing);
						return result;
					}
					result = new LocalTargetInfo(cell);
					return result;
				}
				finally
				{
					Scribe.ExitNode();
				}
				return loaded;
			}
			return loaded;
		}

		public static TargetInfo ResolveTargetInfo(TargetInfo loaded, string label)
		{
			if (Scribe.EnterNode(label))
			{
				try
				{
					Thing thing = Scribe.loader.crossRefs.TakeResolvedRef<Thing>("thing");
					Map map = Scribe.loader.crossRefs.TakeResolvedRef<Map>("map");
					IntVec3 cell = loaded.Cell;
					TargetInfo result;
					if (thing != null)
					{
						result = new TargetInfo(thing);
						return result;
					}
					if (cell.IsValid && map != null)
					{
						result = new TargetInfo(cell, map, false);
						return result;
					}
					result = TargetInfo.Invalid;
					return result;
				}
				finally
				{
					Scribe.ExitNode();
				}
				return loaded;
			}
			return loaded;
		}

		public static GlobalTargetInfo ResolveGlobalTargetInfo(GlobalTargetInfo loaded, string label)
		{
			if (Scribe.EnterNode(label))
			{
				try
				{
					Thing thing = Scribe.loader.crossRefs.TakeResolvedRef<Thing>("thing");
					Map map = Scribe.loader.crossRefs.TakeResolvedRef<Map>("map");
					WorldObject worldObject = Scribe.loader.crossRefs.TakeResolvedRef<WorldObject>("worldObject");
					IntVec3 cell = loaded.Cell;
					int tile = loaded.Tile;
					if (thing != null)
					{
						GlobalTargetInfo result = new GlobalTargetInfo(thing);
						return result;
					}
					if (worldObject != null)
					{
						GlobalTargetInfo result = new GlobalTargetInfo(worldObject);
						return result;
					}
					if (cell.IsValid)
					{
						GlobalTargetInfo result;
						if (map != null)
						{
							result = new GlobalTargetInfo(cell, map, false);
							return result;
						}
						result = GlobalTargetInfo.Invalid;
						return result;
					}
					else
					{
						GlobalTargetInfo result;
						if (tile >= 0)
						{
							result = new GlobalTargetInfo(tile);
							return result;
						}
						result = GlobalTargetInfo.Invalid;
						return result;
					}
				}
				finally
				{
					Scribe.ExitNode();
				}
				return loaded;
			}
			return loaded;
		}

		private static void ExtractCellAndMapPairFromTargetInfo(string str, out string cell, out string map)
		{
			int num = str.IndexOf(')');
			cell = str.Substring(0, num + 1);
			int num2 = str.IndexOf(',', num + 1);
			map = str.Substring(num2 + 1);
			map = map.TrimStart(new char[]
			{
				' '
			});
		}
	}
}
