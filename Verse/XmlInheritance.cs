using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Verse
{
	public static class XmlInheritance
	{
		private class XmlInheritanceNode
		{
			public XmlNode xmlNode;

			public XmlNode resolvedXmlNode;

			public ModContentPack mod;

			public XmlInheritance.XmlInheritanceNode parent;

			public List<XmlInheritance.XmlInheritanceNode> children = new List<XmlInheritance.XmlInheritanceNode>();
		}

		private static Dictionary<XmlNode, XmlInheritance.XmlInheritanceNode> resolvedNodes = new Dictionary<XmlNode, XmlInheritance.XmlInheritanceNode>();

		private static List<XmlInheritance.XmlInheritanceNode> unresolvedNodes = new List<XmlInheritance.XmlInheritanceNode>();

		private static Dictionary<string, List<XmlInheritance.XmlInheritanceNode>> nodesByName = new Dictionary<string, List<XmlInheritance.XmlInheritanceNode>>();

		private static readonly string NameAttributeName = "Name";

		private static readonly string ParentNameAttributeName = "ParentName";

		private static HashSet<string> tempUsedNodeNames = new HashSet<string>();

		public static void TryRegisterAllFrom(LoadableXmlAsset xmlAsset, ModContentPack mod)
		{
			if (xmlAsset.xmlDoc == null)
			{
				return;
			}
			XmlNodeList childNodes = xmlAsset.xmlDoc.DocumentElement.ChildNodes;
			for (int i = 0; i < childNodes.Count; i++)
			{
				if (childNodes[i].NodeType == XmlNodeType.Element)
				{
					XmlInheritance.TryRegister(childNodes[i], mod);
				}
			}
		}

		public static void TryRegister(XmlNode node, ModContentPack mod)
		{
			XmlAttribute xmlAttribute = node.Attributes[XmlInheritance.NameAttributeName];
			XmlAttribute xmlAttribute2 = node.Attributes[XmlInheritance.ParentNameAttributeName];
			if (xmlAttribute == null && xmlAttribute2 == null)
			{
				return;
			}
			List<XmlInheritance.XmlInheritanceNode> list = null;
			if (xmlAttribute != null && XmlInheritance.nodesByName.TryGetValue(xmlAttribute.Value, out list))
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].mod == mod)
					{
						if (mod == null)
						{
							Log.Error("XML error: Could not register node named \"" + xmlAttribute.Value + "\" because this name is already used.");
						}
						else
						{
							Log.Error(string.Concat(new string[]
							{
								"XML error: Could not register node named \"",
								xmlAttribute.Value,
								"\" in mod ",
								mod.ToString(),
								" because this name is already used in this mod."
							}));
						}
						return;
					}
				}
			}
			XmlInheritance.XmlInheritanceNode xmlInheritanceNode = new XmlInheritance.XmlInheritanceNode();
			xmlInheritanceNode.xmlNode = node;
			xmlInheritanceNode.mod = mod;
			XmlInheritance.unresolvedNodes.Add(xmlInheritanceNode);
			if (xmlAttribute != null)
			{
				if (list != null)
				{
					list.Add(xmlInheritanceNode);
				}
				else
				{
					list = new List<XmlInheritance.XmlInheritanceNode>();
					list.Add(xmlInheritanceNode);
					XmlInheritance.nodesByName.Add(xmlAttribute.Value, list);
				}
			}
		}

		public static void Resolve()
		{
			XmlInheritance.ResolveParentsAndChildNodesLinks();
			XmlInheritance.ResolveXmlNodes();
		}

		public static XmlNode GetResolvedNodeFor(XmlNode originalNode)
		{
			if (originalNode.Attributes[XmlInheritance.ParentNameAttributeName] != null)
			{
				XmlInheritance.XmlInheritanceNode xmlInheritanceNode;
				if (XmlInheritance.resolvedNodes.TryGetValue(originalNode, out xmlInheritanceNode))
				{
					return xmlInheritanceNode.resolvedXmlNode;
				}
				if (XmlInheritance.unresolvedNodes.Any((XmlInheritance.XmlInheritanceNode x) => x.xmlNode == originalNode))
				{
					Log.Error("XML error: XML node \"" + originalNode.Name + "\" has not been resolved yet. There's probably a Resolve() call missing somewhere.");
				}
				else
				{
					Log.Error("XML error: Tried to get resolved node for node \"" + originalNode.Name + "\" which uses a ParentName attribute, but it is not in a resolved nodes collection, which means that it was never registered or there was an error while resolving it.");
				}
			}
			return originalNode;
		}

		public static void Clear()
		{
			XmlInheritance.resolvedNodes.Clear();
			XmlInheritance.unresolvedNodes.Clear();
			XmlInheritance.nodesByName.Clear();
		}

		private static void ResolveParentsAndChildNodesLinks()
		{
			for (int i = 0; i < XmlInheritance.unresolvedNodes.Count; i++)
			{
				XmlAttribute xmlAttribute = XmlInheritance.unresolvedNodes[i].xmlNode.Attributes[XmlInheritance.ParentNameAttributeName];
				if (xmlAttribute != null)
				{
					XmlInheritance.unresolvedNodes[i].parent = XmlInheritance.GetBestParentFor(XmlInheritance.unresolvedNodes[i], xmlAttribute.Value);
					if (XmlInheritance.unresolvedNodes[i].parent != null)
					{
						XmlInheritance.unresolvedNodes[i].parent.children.Add(XmlInheritance.unresolvedNodes[i]);
					}
				}
			}
		}

		private static void ResolveXmlNodes()
		{
			List<XmlInheritance.XmlInheritanceNode> list = (from x in XmlInheritance.unresolvedNodes
			where x.parent == null || x.parent.resolvedXmlNode != null
			select x).ToList<XmlInheritance.XmlInheritanceNode>();
			for (int i = 0; i < list.Count; i++)
			{
				XmlInheritance.ResolveXmlNodesRecursively(list[i]);
			}
			for (int j = 0; j < XmlInheritance.unresolvedNodes.Count; j++)
			{
				if (XmlInheritance.unresolvedNodes[j].resolvedXmlNode == null)
				{
					Log.Error("XML error: Cyclic inheritance hierarchy detected for node \"" + XmlInheritance.unresolvedNodes[j].xmlNode.Name + "\". Full node: " + XmlInheritance.unresolvedNodes[j].xmlNode.OuterXml);
				}
				else
				{
					XmlInheritance.resolvedNodes.Add(XmlInheritance.unresolvedNodes[j].xmlNode, XmlInheritance.unresolvedNodes[j]);
				}
			}
			XmlInheritance.unresolvedNodes.Clear();
		}

		private static void ResolveXmlNodesRecursively(XmlInheritance.XmlInheritanceNode node)
		{
			if (node.resolvedXmlNode != null)
			{
				Log.Error("XML error: Cyclic inheritance hierarchy detected for node \"" + node.xmlNode.Name + "\". Full node: " + node.xmlNode.OuterXml);
				return;
			}
			XmlInheritance.ResolveXmlNodeFor(node);
			for (int i = 0; i < node.children.Count; i++)
			{
				XmlInheritance.ResolveXmlNodesRecursively(node.children[i]);
			}
		}

		private static XmlInheritance.XmlInheritanceNode GetBestParentFor(XmlInheritance.XmlInheritanceNode node, string parentName)
		{
			XmlInheritance.XmlInheritanceNode xmlInheritanceNode = null;
			List<XmlInheritance.XmlInheritanceNode> list;
			if (XmlInheritance.nodesByName.TryGetValue(parentName, out list))
			{
				if (node.mod == null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].mod == null)
						{
							xmlInheritanceNode = list[i];
							break;
						}
					}
					if (xmlInheritanceNode == null)
					{
						for (int j = 0; j < list.Count; j++)
						{
							if (xmlInheritanceNode == null || list[j].mod.loadOrder < xmlInheritanceNode.mod.loadOrder)
							{
								xmlInheritanceNode = list[j];
							}
						}
					}
				}
				else
				{
					for (int k = 0; k < list.Count; k++)
					{
						if (list[k].mod != null)
						{
							if (list[k].mod.loadOrder <= node.mod.loadOrder && (xmlInheritanceNode == null || list[k].mod.loadOrder > xmlInheritanceNode.mod.loadOrder))
							{
								xmlInheritanceNode = list[k];
							}
						}
					}
					if (xmlInheritanceNode == null)
					{
						for (int l = 0; l < list.Count; l++)
						{
							if (list[l].mod == null)
							{
								xmlInheritanceNode = list[l];
								break;
							}
						}
					}
				}
			}
			if (xmlInheritanceNode == null)
			{
				Log.Error(string.Concat(new string[]
				{
					"XML error: Could not find parent node named \"",
					parentName,
					"\" for node \"",
					node.xmlNode.Name,
					"\". Full node: ",
					node.xmlNode.OuterXml
				}));
				return null;
			}
			return xmlInheritanceNode;
		}

		private static void ResolveXmlNodeFor(XmlInheritance.XmlInheritanceNode node)
		{
			if (node.parent == null)
			{
				node.resolvedXmlNode = node.xmlNode;
				return;
			}
			if (node.parent.resolvedXmlNode == null)
			{
				Log.Error("XML error: Internal error. Tried to resolve node whose parent has not been resolved yet. This means that this method was called in incorrect order.");
				node.resolvedXmlNode = node.xmlNode;
				return;
			}
			XmlInheritance.CheckForDuplicateNodes(node.xmlNode);
			XmlNode xmlNode = node.parent.resolvedXmlNode.CloneNode(true);
			xmlNode.Attributes.RemoveAll();
			XmlAttributeCollection attributes = node.xmlNode.Attributes;
			for (int i = 0; i < attributes.Count; i++)
			{
				if (!(attributes[i].Name == XmlInheritance.NameAttributeName) && !(attributes[i].Name == XmlInheritance.ParentNameAttributeName))
				{
					XmlAttribute node2 = (XmlAttribute)xmlNode.OwnerDocument.ImportNode(attributes[i], true);
					xmlNode.Attributes.Append(node2);
				}
			}
			XmlAttributeCollection attributes2 = node.parent.resolvedXmlNode.Attributes;
			for (int j = 0; j < attributes2.Count; j++)
			{
				if (!(attributes2[j].Name == XmlInheritance.NameAttributeName) && !(attributes2[j].Name == XmlInheritance.ParentNameAttributeName))
				{
					if (xmlNode.Attributes[attributes2[j].Name] == null)
					{
						XmlAttribute node3 = (XmlAttribute)xmlNode.OwnerDocument.ImportNode(attributes2[j], true);
						xmlNode.Attributes.Append(node3);
					}
				}
			}
			XmlInheritance.RecursiveNodeCopyOverwriteElements(node.xmlNode, xmlNode);
			node.resolvedXmlNode = xmlNode;
		}

		private static void RecursiveNodeCopyOverwriteElements(XmlNode source, XmlNode target)
		{
			if (source.ChildNodes.Count == 0)
			{
				while (target.HasChildNodes)
				{
					target.RemoveChild(target.FirstChild);
				}
			}
			if (source.ChildNodes.Count == 1 && source.FirstChild.NodeType == XmlNodeType.Text)
			{
				while (target.HasChildNodes)
				{
					target.RemoveChild(target.FirstChild);
				}
				XmlNode newChild = target.OwnerDocument.ImportNode(source.FirstChild, true);
				target.AppendChild(newChild);
			}
			else
			{
				foreach (XmlNode xmlNode in source)
				{
					if (xmlNode.Name == "li")
					{
						XmlNode newChild2 = target.OwnerDocument.ImportNode(xmlNode, true);
						target.AppendChild(newChild2);
					}
					else
					{
						XmlNode xmlNode2 = target[xmlNode.Name];
						if (xmlNode2 != null)
						{
							XmlInheritance.RecursiveNodeCopyOverwriteElements(xmlNode, xmlNode2);
						}
						else
						{
							XmlNode newChild3 = target.OwnerDocument.ImportNode(xmlNode, true);
							target.AppendChild(newChild3);
						}
					}
				}
			}
		}

		private static void CheckForDuplicateNodes(XmlNode originalNode)
		{
			foreach (XmlNode xmlNode in originalNode.ChildNodes)
			{
				if (!(xmlNode.Name == "li"))
				{
					if (XmlInheritance.tempUsedNodeNames.Contains(xmlNode.Name))
					{
						Log.Error("XML error: Duplicate XML node name " + xmlNode.Name + " in this XML block: " + originalNode.OuterXml);
					}
					else
					{
						XmlInheritance.tempUsedNodeNames.Add(xmlNode.Name);
					}
				}
			}
			XmlInheritance.tempUsedNodeNames.Clear();
		}
	}
}
