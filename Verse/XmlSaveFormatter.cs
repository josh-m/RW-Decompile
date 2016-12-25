using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Verse
{
	internal static class XmlSaveFormatter
	{
		public static void AddWhitespaceFromRoot(XElement root)
		{
			if (!root.Elements().Any<XElement>())
			{
				return;
			}
			foreach (XElement current in root.Elements().ToList<XElement>())
			{
				XText content = new XText("\n");
				current.AddAfterSelf(content);
			}
			root.Elements().First<XElement>().AddBeforeSelf(new XText("\n"));
			root.Elements().Last<XElement>().AddAfterSelf(new XText("\n"));
			foreach (XElement current2 in root.Elements().ToList<XElement>())
			{
				XmlSaveFormatter.IndentXml(current2, 1);
			}
		}

		private static void IndentXml(XElement element, int depth)
		{
			element.AddBeforeSelf(new XText(XmlSaveFormatter.IndentString(depth, true)));
			bool startWithNewline = element.NextNode == null;
			element.AddAfterSelf(new XText(XmlSaveFormatter.IndentString(depth - 1, startWithNewline)));
			foreach (XElement current in element.Elements().ToList<XElement>())
			{
				XmlSaveFormatter.IndentXml(current, depth + 1);
			}
		}

		private static string IndentString(int depth, bool startWithNewline)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (startWithNewline)
			{
				stringBuilder.Append("\n");
			}
			for (int i = 0; i < depth; i++)
			{
				stringBuilder.Append("  ");
			}
			return stringBuilder.ToString();
		}
	}
}
