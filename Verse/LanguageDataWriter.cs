using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Verse
{
	public static class LanguageDataWriter
	{
		public static void WriteBackstoryFile()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(GenFilePaths.DevOutputFolderPath);
			if (!directoryInfo.Exists)
			{
				directoryInfo.Create();
			}
			FileInfo fileInfo = new FileInfo(GenFilePaths.BackstoryOutputFilePath);
			if (fileInfo.Exists)
			{
				Find.WindowStack.Add(new Dialog_MessageBox("Cannot write: File already exists at " + GenFilePaths.BackstoryOutputFilePath, null, null, null, null, null, false, null, null));
				return;
			}
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = true;
			xmlWriterSettings.IndentChars = "\t";
			using (XmlWriter xmlWriter = XmlWriter.Create(GenFilePaths.BackstoryOutputFilePath, xmlWriterSettings))
			{
				xmlWriter.WriteStartDocument();
				xmlWriter.WriteStartElement("BackstoryTranslations");
				foreach (KeyValuePair<string, Backstory> current in BackstoryDatabase.allBackstories)
				{
					Backstory value = current.Value;
					xmlWriter.WriteStartElement(value.identifier);
					xmlWriter.WriteElementString("title", value.title);
					if (!value.titleFemale.NullOrEmpty())
					{
						xmlWriter.WriteElementString("titleFemale", value.titleFemale);
					}
					xmlWriter.WriteElementString("titleShort", value.titleShort);
					if (!value.titleShortFemale.NullOrEmpty())
					{
						xmlWriter.WriteElementString("titleShortFemale", value.titleShortFemale);
					}
					xmlWriter.WriteElementString("desc", value.baseDesc);
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndDocument();
			}
			Messages.Message("Fresh backstory translation file saved to " + GenFilePaths.BackstoryOutputFilePath, MessageTypeDefOf.NeutralEvent, false);
		}
	}
}
