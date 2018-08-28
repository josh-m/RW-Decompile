using System;

namespace Verse
{
	public static class Scribe_BodyParts
	{
		public static void Look(ref BodyPartRecord part, string label, BodyPartRecord defaultValue = null)
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (part != defaultValue && Scribe.EnterNode(label))
				{
					try
					{
						if (part == null)
						{
							Scribe.saver.WriteAttribute("IsNull", "True");
						}
						else
						{
							string defName = part.body.defName;
							Scribe_Values.Look<string>(ref defName, "body", null, false);
							int index = part.Index;
							Scribe_Values.Look<int>(ref index, "index", 0, true);
						}
					}
					finally
					{
						Scribe.ExitNode();
					}
				}
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				part = ScribeExtractor.BodyPartFromNode(Scribe.loader.curXmlParent[label], label, defaultValue);
			}
		}
	}
}
