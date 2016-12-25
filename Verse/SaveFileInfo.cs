using RimWorld;
using System;
using System.IO;
using UnityEngine;

namespace Verse
{
	public struct SaveFileInfo
	{
		private FileInfo fileInfo;

		private string gameVersion;

		public static readonly Color UnimportantTextColor = new Color(1f, 1f, 1f, 0.5f);

		public bool Valid
		{
			get
			{
				return this.gameVersion != null;
			}
		}

		public FileInfo FileInfo
		{
			get
			{
				return this.fileInfo;
			}
		}

		public string GameVersion
		{
			get
			{
				if (!this.Valid)
				{
					return "???";
				}
				return this.gameVersion;
			}
		}

		public Color VersionColor
		{
			get
			{
				if (!this.Valid)
				{
					return Color.red;
				}
				if (VersionControl.MajorFromVersionString(this.gameVersion) != VersionControl.CurrentMajor || VersionControl.MinorFromVersionString(this.gameVersion) != VersionControl.CurrentMinor)
				{
					return Color.red;
				}
				if (VersionControl.BuildFromVersionString(this.gameVersion) != VersionControl.CurrentBuild)
				{
					return Color.yellow;
				}
				return SaveFileInfo.UnimportantTextColor;
			}
		}

		public TipSignal CompatibilityTip
		{
			get
			{
				if (!this.Valid)
				{
					return "SaveIsUnknownFormat".Translate();
				}
				if (VersionControl.MajorFromVersionString(this.gameVersion) != VersionControl.CurrentMajor || VersionControl.MinorFromVersionString(this.gameVersion) != VersionControl.CurrentMinor)
				{
					return "SaveIsFromDifferentGameVersion".Translate(new object[]
					{
						VersionControl.CurrentVersionString,
						this.gameVersion
					});
				}
				if (VersionControl.BuildFromVersionString(this.gameVersion) != VersionControl.CurrentBuild)
				{
					return "SaveIsFromDifferentGameBuild".Translate(new object[]
					{
						VersionControl.CurrentVersionString,
						this.gameVersion
					});
				}
				return "SaveIsFromThisGameBuild".Translate();
			}
		}

		public SaveFileInfo(FileInfo fileInfo)
		{
			this.fileInfo = fileInfo;
			this.gameVersion = ScribeMetaHeaderUtility.GameVersionOf(fileInfo);
		}
	}
}
