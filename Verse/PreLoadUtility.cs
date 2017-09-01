using System;

namespace Verse
{
	public static class PreLoadUtility
	{
		public static void CheckVersionAndLoad(string path, ScribeMetaHeaderUtility.ScribeHeaderMode mode, Action loadAct)
		{
			Scribe.loader.InitLoadingMetaHeaderOnly(path);
			try
			{
				ScribeMetaHeaderUtility.LoadGameDataHeader(mode, false);
				Scribe.loader.FinalizeLoading();
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
				Scribe.ForceStop();
			}
			if (!ScribeMetaHeaderUtility.TryCreateDialogsForVersionMismatchWarnings(loadAct))
			{
				loadAct();
			}
		}
	}
}
