using System;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace RimWorld
{
	public static class VersionControl
	{
		private static Version version;

		private static string versionString;

		private static string versionStringWithRev;

		private static DateTime buildDate;

		public static Version CurrentVersion
		{
			get
			{
				return VersionControl.version;
			}
		}

		public static string CurrentVersionString
		{
			get
			{
				return VersionControl.versionString;
			}
		}

		public static string CurrentVersionStringWithRev
		{
			get
			{
				return VersionControl.versionStringWithRev;
			}
		}

		public static int CurrentMajor
		{
			get
			{
				return VersionControl.version.Major;
			}
		}

		public static int CurrentMinor
		{
			get
			{
				return VersionControl.version.Minor;
			}
		}

		public static int CurrentBuild
		{
			get
			{
				return VersionControl.version.Build;
			}
		}

		public static int CurrentRevision
		{
			get
			{
				return VersionControl.version.Revision;
			}
		}

		public static DateTime CurrentBuildDate
		{
			get
			{
				return VersionControl.buildDate;
			}
		}

		static VersionControl()
		{
			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			DateTime dateTime = new DateTime(2000, 1, 1);
			VersionControl.buildDate = dateTime.AddDays((double)version.Build);
			int build = version.Build - 4805;
			int revision = version.Revision * 2 / 60;
			VersionControl.version = new Version(version.Major, version.Minor, build, revision);
			VersionControl.versionStringWithRev = string.Concat(new object[]
			{
				VersionControl.version.Major,
				".",
				VersionControl.version.Minor,
				".",
				VersionControl.version.Build,
				" rev",
				VersionControl.version.Revision
			});
			VersionControl.versionString = string.Concat(new object[]
			{
				VersionControl.version.Major,
				".",
				VersionControl.version.Minor,
				".",
				VersionControl.version.Build
			});
		}

		public static void DrawInfoInCorner()
		{
			Text.Font = GameFont.Small;
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			string text = "VersionIndicator".Translate(new object[]
			{
				VersionControl.versionString
			});
			if (UnityData.isDebugBuild)
			{
				text = text + " (" + "DevelopmentBuildLower".Translate() + ")";
			}
			text = text + "\n" + "CompiledOn".Translate(new object[]
			{
				VersionControl.buildDate.ToString("MMM d yyyy")
			});
			if (SteamManager.Initialized)
			{
				text = text + "\n" + "LoggedIntoSteamAs".Translate(new object[]
				{
					SteamUtility.SteamPersonaName
				});
			}
			Rect rect = new Rect(10f, 10f, 330f, Text.CalcHeight(text, 330f));
			Widgets.Label(rect, text);
			GUI.color = Color.white;
			LatestVersionGetter component = Current.Root.gameObject.GetComponent<LatestVersionGetter>();
			Rect rect2 = new Rect(10f, rect.yMax - 5f, 330f, 999f);
			component.DrawAt(rect2);
		}

		public static void LogVersionNumber()
		{
			Log.Message("RimWorld " + VersionControl.versionStringWithRev);
		}

		public static bool IsWellFormattedVersionString(string str)
		{
			string[] array = str.Split(new char[]
			{
				'.'
			});
			if (array.Length != 3)
			{
				return false;
			}
			for (int i = 0; i < 3; i++)
			{
				int num;
				if (!int.TryParse(array[i], out num))
				{
					return false;
				}
				if (num < 0)
				{
					return false;
				}
			}
			return true;
		}

		public static int BuildFromVersionString(string str)
		{
			str = VersionControl.VersionStringWithoutRev(str);
			int result = 0;
			string[] array = str.Split(new char[]
			{
				'.'
			});
			if (array.Length < 3 || !int.TryParse(array[2], out result))
			{
				Log.Warning("Could not get build from version string " + str);
			}
			return result;
		}

		public static int MinorFromVersionString(string str)
		{
			str = VersionControl.VersionStringWithoutRev(str);
			int result = 0;
			string[] array = str.Split(new char[]
			{
				'.'
			});
			if (array.Length < 2 || !int.TryParse(array[1], out result))
			{
				Log.Warning("Could not get minor version from version string " + str);
			}
			return result;
		}

		public static int MajorFromVersionString(string str)
		{
			str = VersionControl.VersionStringWithoutRev(str);
			int result = 0;
			if (!int.TryParse(str.Split(new char[]
			{
				'.'
			})[0], out result))
			{
				Log.Warning("Could not get major version from version string " + str);
			}
			return result;
		}

		public static string VersionStringWithoutRev(string str)
		{
			return str.Split(new char[]
			{
				' '
			})[0];
		}

		public static Version VersionFromString(string str)
		{
			if (str.NullOrEmpty())
			{
				throw new ArgumentException("str");
			}
			string[] array = str.Split(new char[]
			{
				'.'
			});
			if (array.Length > 3)
			{
				throw new ArgumentException("str");
			}
			int major = 0;
			int minor = 0;
			int build = 0;
			for (int i = 0; i < 3; i++)
			{
				int num;
				if (!int.TryParse(array[i], out num))
				{
					throw new ArgumentException("str");
				}
				if (num < 0)
				{
					throw new ArgumentException("str");
				}
				switch (i)
				{
				case 0:
					major = num;
					break;
				case 1:
					minor = num;
					break;
				case 2:
					build = num;
					break;
				}
			}
			return new Version(major, minor, build);
		}
	}
}
