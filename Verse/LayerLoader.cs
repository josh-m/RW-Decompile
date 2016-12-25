using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Xml.XPath;
using UnityEngine;

namespace Verse
{
	public static class LayerLoader
	{
		public static void LoadFileIntoList(TextAsset ass, List<DiaNodeMold> NodeListToFill, List<DiaNodeList> ListListToFill, DiaNodeType NodesType)
		{
			TextReader textReader = new StringReader(ass.text);
			XPathDocument xPathDocument = new XPathDocument(textReader);
			XPathNavigator xPathNavigator = xPathDocument.CreateNavigator();
			xPathNavigator.MoveToFirst();
			xPathNavigator.MoveToFirstChild();
			foreach (XPathNavigator xPathNavigator2 in xPathNavigator.Select("Node"))
			{
				try
				{
					TextReader textReader2 = new StringReader(xPathNavigator2.OuterXml);
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(DiaNodeMold));
					DiaNodeMold diaNodeMold = (DiaNodeMold)xmlSerializer.Deserialize(textReader2);
					diaNodeMold.nodeType = NodesType;
					NodeListToFill.Add(diaNodeMold);
					textReader2.Dispose();
				}
				catch (Exception ex)
				{
					Log.Message(string.Concat(new object[]
					{
						"Exception deserializing ",
						xPathNavigator2.OuterXml,
						":\n",
						ex.InnerException
					}));
				}
			}
			foreach (XPathNavigator xPathNavigator3 in xPathNavigator.Select("NodeList"))
			{
				try
				{
					TextReader textReader3 = new StringReader(xPathNavigator3.OuterXml);
					XmlSerializer xmlSerializer2 = new XmlSerializer(typeof(DiaNodeList));
					DiaNodeList item = (DiaNodeList)xmlSerializer2.Deserialize(textReader3);
					ListListToFill.Add(item);
				}
				catch (Exception ex2)
				{
					Log.Message(string.Concat(new object[]
					{
						"Exception deserializing ",
						xPathNavigator3.OuterXml,
						":\n",
						ex2.InnerException
					}));
				}
			}
		}

		public static void MarkNonRootNodes(List<DiaNodeMold> NodeList)
		{
			foreach (DiaNodeMold current in NodeList)
			{
				LayerLoader.RecursiveSetIsRootFalse(current);
			}
			foreach (DiaNodeMold current2 in NodeList)
			{
				foreach (DiaNodeMold current3 in NodeList)
				{
					foreach (DiaOptionMold current4 in current3.optionList)
					{
						bool flag = false;
						foreach (string current5 in current4.ChildNodeNames)
						{
							if (current5 == current2.name)
							{
								flag = true;
							}
						}
						if (flag)
						{
							current2.isRoot = false;
						}
					}
				}
			}
		}

		private static void RecursiveSetIsRootFalse(DiaNodeMold d)
		{
			foreach (DiaOptionMold current in d.optionList)
			{
				foreach (DiaNodeMold current2 in current.ChildNodes)
				{
					current2.isRoot = false;
					LayerLoader.RecursiveSetIsRootFalse(current2);
				}
			}
		}
	}
}
