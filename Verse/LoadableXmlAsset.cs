using System;
using System.Xml;

namespace Verse
{
	public class LoadableXmlAsset
	{
		public string name;

		public string fullFolderPath;

		public XmlDocument xmlDoc;

		public LoadableXmlAsset(string name, string fullFolderPath, string contents)
		{
			this.name = name;
			this.fullFolderPath = fullFolderPath;
			try
			{
				this.xmlDoc = new XmlDocument();
				this.xmlDoc.LoadXml(contents);
			}
			catch (Exception ex)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Exception reading ",
					name,
					" as XML: ",
					ex
				}));
				this.xmlDoc = null;
			}
		}

		public override string ToString()
		{
			return this.name;
		}
	}
}
