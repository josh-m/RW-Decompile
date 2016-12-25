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
					Type type;
					if (xmlAttribute2 != null)
					{
						type = GenTypes.GetTypeInAnyAssembly(xmlAttribute2.Value);
						if (type == null)
						{
							Log.Error(string.Concat(new object[]
							{
								"Could not find class ",
								xmlAttribute2.Value,
								" while resolving node ",
								subNode.Name,
								". Trying to use ",
								typeof(T),
								" instead. Full node: ",
								subNode.OuterXml
							}));
							type = typeof(T);
						}
					}
					else
					{
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
						CrossRefResolver.RegisterForCrossRefResolve(exposable);
					}
					XmlNode curParent = Scribe.curParent;
					Scribe.curParent = subNode;
					exposable.ExposeData();
					Scribe.curParent = curParent;
					if (!flag)
					{
						PostLoadInitter.RegisterForPostLoadInit(exposable);
					}
					result = (T)((object)exposable);
				}
				catch (Exception ex)
				{
					result = default(T);
					throw new InvalidOperationException(string.Concat(new object[]
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

		public static LocalTargetInfo LocalTargetInfoFromNode(XmlNode subNode, LocalTargetInfo defaultValue)
		{
			if (subNode == null)
			{
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(Thing));
				return defaultValue;
			}
			if (subNode.InnerText[0] == '(')
			{
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(Thing));
				return new LocalTargetInfo(IntVec3.FromString(subNode.InnerText));
			}
			LoadIDsWantedBank.RegisterLoadIDReadFromXml(subNode.InnerText, typeof(Thing));
			return LocalTargetInfo.Invalid;
		}

		public static TargetInfo TargetInfoFromNode(XmlNode subNode, TargetInfo defaultValue)
		{
			if (subNode == null)
			{
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(Thing));
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(Map));
				return defaultValue;
			}
			if (subNode.InnerText[0] == '(')
			{
				string str;
				string targetLoadID;
				ScribeExtractor.ExtractCellAndMapPairFromTargetInfo(subNode.InnerText, out str, out targetLoadID);
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(Thing));
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(targetLoadID, typeof(Map));
				return new TargetInfo(IntVec3.FromString(str), null, true);
			}
			LoadIDsWantedBank.RegisterLoadIDReadFromXml(subNode.InnerText, typeof(Thing));
			LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(Map));
			return TargetInfo.Invalid;
		}

		public static GlobalTargetInfo GlobalTargetInfoFromNode(XmlNode subNode, GlobalTargetInfo defaultValue)
		{
			if (subNode == null)
			{
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(Thing));
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(Map));
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(WorldObject));
				return defaultValue;
			}
			if (subNode.InnerText[0] == '(')
			{
				string str;
				string targetLoadID;
				ScribeExtractor.ExtractCellAndMapPairFromTargetInfo(subNode.InnerText, out str, out targetLoadID);
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(Thing));
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(targetLoadID, typeof(Map));
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(WorldObject));
				return new GlobalTargetInfo(IntVec3.FromString(str), null, true);
			}
			int tile;
			if (int.TryParse(subNode.InnerText, out tile))
			{
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(Thing));
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(Map));
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(WorldObject));
				return new GlobalTargetInfo(tile);
			}
			if (!subNode.InnerText.NullOrEmpty() && subNode.InnerText[0] == '@')
			{
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(Thing));
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(Map));
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(subNode.InnerText.Substring(1), typeof(WorldObject));
				return GlobalTargetInfo.Invalid;
			}
			LoadIDsWantedBank.RegisterLoadIDReadFromXml(subNode.InnerText, typeof(Thing));
			LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(Map));
			LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(WorldObject));
			return GlobalTargetInfo.Invalid;
		}

		public static LocalTargetInfo ResolveLocalTargetInfo(LocalTargetInfo loaded)
		{
			Thing thing = CrossRefResolver.NextResolvedRef<Thing>();
			IntVec3 cell = loaded.Cell;
			if (thing != null)
			{
				return new LocalTargetInfo(thing);
			}
			return new LocalTargetInfo(cell);
		}

		public static TargetInfo ResolveTargetInfo(TargetInfo loaded)
		{
			Thing thing = CrossRefResolver.NextResolvedRef<Thing>();
			Map map = CrossRefResolver.NextResolvedRef<Map>();
			IntVec3 cell = loaded.Cell;
			if (thing != null)
			{
				return new TargetInfo(thing);
			}
			if (cell.IsValid && map != null)
			{
				return new TargetInfo(cell, map, false);
			}
			return TargetInfo.Invalid;
		}

		public static GlobalTargetInfo ResolveGlobalTargetInfo(GlobalTargetInfo loaded)
		{
			Thing thing = CrossRefResolver.NextResolvedRef<Thing>();
			Map map = CrossRefResolver.NextResolvedRef<Map>();
			WorldObject worldObject = CrossRefResolver.NextResolvedRef<WorldObject>();
			IntVec3 cell = loaded.Cell;
			int tile = loaded.Tile;
			if (thing != null)
			{
				return new GlobalTargetInfo(thing);
			}
			if (worldObject != null)
			{
				return new GlobalTargetInfo(worldObject);
			}
			if (cell.IsValid)
			{
				if (map != null)
				{
					return new GlobalTargetInfo(cell, map, false);
				}
				return GlobalTargetInfo.Invalid;
			}
			else
			{
				if (tile >= 0)
				{
					return new GlobalTargetInfo(tile);
				}
				return GlobalTargetInfo.Invalid;
			}
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
