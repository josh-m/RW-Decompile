using System;

namespace Verse
{
	public static class PreLoadUtility
	{
		public static void CheckVersionAndLoad(string path, ScribeMetaHeaderUtility.ScribeHeaderMode mode, Action loadAct)
		{
			bool flag = false;
			try
			{
				try
				{
					Scribe.InitLoadingMetaHeaderOnly(path);
				}
				catch (Exception ex)
				{
					Log.Warning(string.Concat(new object[]
					{
						"Exception loading ",
						path,
						": ",
						ex
					}));
				}
				ScribeMetaHeaderUtility.LoadGameDataHeader(mode, false);
				flag = ScribeMetaHeaderUtility.TryCreateDialogsForVersionMismatchWarnings(loadAct);
				CrossRefResolver.ResolveAllCrossReferences();
				PostLoadInitter.DoAllPostLoadInits();
			}
			catch
			{
				CrossRefResolver.Clear();
				PostLoadInitter.Clear();
				throw;
			}
			if (!flag)
			{
				loadAct();
			}
		}
	}
}
