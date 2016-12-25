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

		public static TargetInfo TargetInfoFromNode(XmlNode subNode, TargetInfo defaultValue)
		{
			if (subNode == null)
			{
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(Thing));
				return defaultValue;
			}
			if (subNode.InnerText[0] == '(')
			{
				LoadIDsWantedBank.RegisterLoadIDReadFromXml(null, typeof(Thing));
				return IntVec3.FromString(subNode.InnerText);
			}
			LoadIDsWantedBank.RegisterLoadIDReadFromXml(subNode.InnerText, typeof(Thing));
			return Scribe_TargetInfo.PackShouldLoadThingRefValue;
		}
	}
}
