using System;
using System.IO;
using System.Xml;

namespace Verse
{
	public static class Scribe
	{
		public static LoadSaveMode mode = LoadSaveMode.Inactive;

		public static XmlNode curParent = null;

		public static bool writingForDebug = false;

		private static Stream saveStream = null;

		private static XmlWriter writer = null;

		private static LoadSaveMode oldMode = Scribe.mode;

		private static XmlNode oldParent = Scribe.curParent;

		public static void SaveState()
		{
			Scribe.oldMode = Scribe.mode;
			Scribe.oldParent = Scribe.curParent;
		}

		public static void RestoreState()
		{
			Scribe.curParent = Scribe.oldParent;
			Scribe.mode = Scribe.oldMode;
		}

		public static void ForceStop()
		{
			Scribe.mode = LoadSaveMode.Inactive;
			Scribe.curParent = null;
			Scribe.writingForDebug = false;
			Scribe.saveStream = null;
			Scribe.writer = null;
		}

		public static bool EnterNode(string elementName)
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				Scribe.writer.WriteStartElement(elementName);
			}
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				XmlElement xmlElement = Scribe.curParent[elementName];
				if (xmlElement == null)
				{
					return false;
				}
				Scribe.curParent = xmlElement;
			}
			return true;
		}

		public static void ExitNode()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				Scribe.writer.WriteEndElement();
			}
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				Scribe.curParent = Scribe.curParent.ParentNode;
			}
		}

		public static void InitLoading(string filePath)
		{
			using (StreamReader streamReader = new StreamReader(filePath))
			{
				using (XmlTextReader xmlTextReader = new XmlTextReader(streamReader))
				{
					try
					{
						XmlDocument xmlDocument = new XmlDocument();
						xmlDocument.Load(xmlTextReader);
						Scribe.curParent = xmlDocument.DocumentElement;
					}
					catch (Exception ex)
					{
						Log.Error("Exception while init loading file: " + filePath + "\n" + ex.ToString());
						throw;
					}
				}
			}
			Scribe.mode = LoadSaveMode.LoadingVars;
		}

		public static void InitLoadingMetaHeaderOnly(string filePath)
		{
			using (StreamReader streamReader = new StreamReader(filePath))
			{
				using (XmlTextReader xmlTextReader = new XmlTextReader(streamReader))
				{
					if (!ScribeMetaHeaderUtility.ReadToMetaElement(xmlTextReader))
					{
						Scribe.mode = LoadSaveMode.Inactive;
						return;
					}
					using (XmlReader xmlReader = xmlTextReader.ReadSubtree())
					{
						XmlDocument xmlDocument = new XmlDocument();
						xmlDocument.Load(xmlReader);
						XmlElement xmlElement = xmlDocument.CreateElement("root");
						xmlElement.AppendChild(xmlDocument.DocumentElement);
						Scribe.curParent = xmlElement;
					}
				}
			}
			Scribe.mode = LoadSaveMode.LoadingVars;
		}

		public static void FinalizeLoading()
		{
			Scribe.ExitNode();
		}

		public static void InitWriting(string filePath, string documentElementName)
		{
			DebugLoadIDsSavingErrorsChecker.Clear();
			Scribe.mode = LoadSaveMode.Saving;
			Scribe.saveStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = true;
			xmlWriterSettings.IndentChars = "\t";
			Scribe.writer = XmlWriter.Create(Scribe.saveStream, xmlWriterSettings);
			Scribe.writer.WriteStartDocument();
			Scribe.EnterNode(documentElementName);
		}

		public static void FinalizeWriting()
		{
			if (Scribe.writer != null)
			{
				Scribe.ExitNode();
				Scribe.writer.WriteEndDocument();
				Scribe.writer.Flush();
				Scribe.writer.Close();
				Scribe.writer = null;
			}
			if (Scribe.saveStream != null)
			{
				Scribe.saveStream.Flush();
				Scribe.saveStream.Close();
				Scribe.saveStream = null;
			}
			Scribe.mode = LoadSaveMode.Inactive;
			DebugLoadIDsSavingErrorsChecker.CheckForErrorsAndClear();
		}

		public static void WriteElement(string elementName, string value)
		{
			Scribe.writer.WriteElementString(elementName, value);
		}

		public static void WriteAttribute(string attributeName, string value)
		{
			Scribe.writer.WriteAttributeString(attributeName, value);
		}

		public static string DebugOutputFor(IExposable saveable)
		{
			string result;
			using (StringWriter stringWriter = new StringWriter())
			{
				using (Scribe.writer = XmlWriter.Create(stringWriter, new XmlWriterSettings
				{
					Indent = true,
					IndentChars = "  ",
					OmitXmlDeclaration = true
				}))
				{
					LoadSaveMode loadSaveMode = Scribe.mode;
					DebugLoadIDsSavingErrorsChecker.Clear();
					Scribe.mode = LoadSaveMode.Saving;
					Scribe.writingForDebug = true;
					Scribe_Deep.LookDeep<IExposable>(ref saveable, "Saveable", new object[0]);
					Scribe.mode = loadSaveMode;
					DebugLoadIDsSavingErrorsChecker.CheckForErrorsAndClear();
					Scribe.writingForDebug = false;
					result = stringWriter.ToString();
				}
			}
			return result;
		}
	}
}
