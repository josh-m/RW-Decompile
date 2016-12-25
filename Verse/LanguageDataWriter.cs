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
				Messages.Message("Cannot write: File already exists at " + GenFilePaths.BackstoryOutputFilePath, MessageSound.RejectInput);
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
					xmlWriter.WriteElementString("title", value.Title);
					xmlWriter.WriteElementString("titleShort", value.TitleShort);
					xmlWriter.WriteElementString("desc", value.baseDesc);
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndDocument();
			}
			Messages.Message("Fresh backstory translation file saved to " + GenFilePaths.BackstoryOutputFilePath, MessageSound.Standard);
		}
	}
}
