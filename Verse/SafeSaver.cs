using System;
using System.IO;
using System.Threading;

namespace Verse
{
	public static class SafeSaver
	{
		private static readonly string NewFileSuffix = ".new";

		private static readonly string OldFileSuffix = ".old";

		private static string GetFileFullPath(string path)
		{
			return Path.GetFullPath(path);
		}

		private static string GetNewFileFullPath(string path)
		{
			return Path.GetFullPath(path + SafeSaver.NewFileSuffix);
		}

		private static string GetOldFileFullPath(string path)
		{
			return Path.GetFullPath(path + SafeSaver.OldFileSuffix);
		}

		public static void Save(string path, string documentElementName, Action saveAction)
		{
			try
			{
				SafeSaver.CleanSafeSaverFiles(path);
				if (!File.Exists(SafeSaver.GetFileFullPath(path)))
				{
					SafeSaver.DoSave(SafeSaver.GetFileFullPath(path), documentElementName, saveAction);
				}
				else
				{
					SafeSaver.DoSave(SafeSaver.GetNewFileFullPath(path), documentElementName, saveAction);
					try
					{
						SafeSaver.SafeMove(SafeSaver.GetFileFullPath(path), SafeSaver.GetOldFileFullPath(path));
					}
					catch (Exception ex)
					{
						Log.Warning(string.Concat(new object[]
						{
							"Could not move file from \"",
							SafeSaver.GetFileFullPath(path),
							"\" to \"",
							SafeSaver.GetOldFileFullPath(path),
							"\": ",
							ex
						}));
						throw;
					}
					try
					{
						SafeSaver.SafeMove(SafeSaver.GetNewFileFullPath(path), SafeSaver.GetFileFullPath(path));
					}
					catch (Exception ex2)
					{
						Log.Warning(string.Concat(new object[]
						{
							"Could not move file from \"",
							SafeSaver.GetNewFileFullPath(path),
							"\" to \"",
							SafeSaver.GetFileFullPath(path),
							"\": ",
							ex2
						}));
						SafeSaver.RemoveFileIfExists(SafeSaver.GetFileFullPath(path), false);
						SafeSaver.RemoveFileIfExists(SafeSaver.GetNewFileFullPath(path), false);
						try
						{
							SafeSaver.SafeMove(SafeSaver.GetOldFileFullPath(path), SafeSaver.GetFileFullPath(path));
						}
						catch (Exception ex3)
						{
							Log.Warning(string.Concat(new object[]
							{
								"Could not move file from \"",
								SafeSaver.GetOldFileFullPath(path),
								"\" back to \"",
								SafeSaver.GetFileFullPath(path),
								"\": ",
								ex3
							}));
						}
						throw;
					}
					SafeSaver.RemoveFileIfExists(SafeSaver.GetOldFileFullPath(path), true);
				}
			}
			catch (Exception ex4)
			{
				GenUI.ErrorDialog("ProblemSavingFile".Translate(new object[]
				{
					SafeSaver.GetFileFullPath(path),
					ex4.ToString()
				}));
				throw;
			}
		}

		private static void CleanSafeSaverFiles(string path)
		{
			SafeSaver.RemoveFileIfExists(SafeSaver.GetOldFileFullPath(path), true);
			SafeSaver.RemoveFileIfExists(SafeSaver.GetNewFileFullPath(path), true);
		}

		private static void DoSave(string fullPath, string documentElementName, Action saveAction)
		{
			try
			{
				Scribe.InitWriting(fullPath, documentElementName);
				saveAction();
			}
			catch (Exception ex)
			{
				Log.Warning(string.Concat(new object[]
				{
					"An exception was thrown during saving to \"",
					fullPath,
					"\": ",
					ex
				}));
				Scribe.FinalizeWriting();
				SafeSaver.RemoveFileIfExists(fullPath, false);
				throw;
			}
			Scribe.FinalizeWriting();
		}

		private static void RemoveFileIfExists(string path, bool rethrow)
		{
			try
			{
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch (Exception ex)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Could not remove file \"",
					path,
					"\": ",
					ex
				}));
				if (rethrow)
				{
					throw;
				}
			}
		}

		private static void SafeMove(string from, string to)
		{
			Exception ex = null;
			for (int i = 0; i < 50; i++)
			{
				try
				{
					File.Move(from, to);
					return;
				}
				catch (Exception ex2)
				{
					if (ex == null)
					{
						ex = ex2;
					}
				}
				Thread.Sleep(1);
			}
			throw ex;
		}
	}
}
