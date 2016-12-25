using RimWorld;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using Verse.Steam;

namespace Verse
{
	public class ModMetaData : WorkshopUploadable
	{
		private class ModMetaDataInternal
		{
			public string name = string.Empty;

			public string author = "Anonymous";

			public string url = string.Empty;

			public string targetVersion = "Unknown";

			public string description = "No description provided.";
		}

		private const string AboutFolderName = "About";

		private DirectoryInfo rootDirInt;

		private ContentSource source;

		public Texture2D previewImage;

		public bool enabled = true;

		private ModMetaData.ModMetaDataInternal meta = new ModMetaData.ModMetaDataInternal();

		private WorkshopItemHook workshopHookInt;

		private PublishedFileId_t publishedFileIdInt = PublishedFileId_t.Invalid;

		public string Identifier
		{
			get
			{
				return this.RootDir.Name;
			}
		}

		public DirectoryInfo RootDir
		{
			get
			{
				return this.rootDirInt;
			}
		}

		public bool IsCoreMod
		{
			get
			{
				return this.Identifier == ModContentPack.CoreModIdentifier;
			}
		}

		public bool Active
		{
			get
			{
				return ModsConfig.IsActive(this);
			}
			set
			{
				ModsConfig.SetActive(this, value);
			}
		}

		public bool VersionCompatible
		{
			get
			{
				return this.IsCoreMod || (VersionControl.IsWellFormattedVersionString(this.TargetVersion) && VersionControl.MinorFromVersionString(this.TargetVersion) == VersionControl.CurrentMinor);
			}
		}

		public string Name
		{
			get
			{
				return this.meta.name;
			}
			set
			{
				this.meta.name = value;
			}
		}

		public string Author
		{
			get
			{
				return this.meta.author;
			}
		}

		public string Url
		{
			get
			{
				return this.meta.url;
			}
		}

		public string TargetVersion
		{
			get
			{
				return this.meta.targetVersion;
			}
		}

		public string Description
		{
			get
			{
				return this.meta.description;
			}
		}

		public string PreviewImagePath
		{
			get
			{
				return string.Concat(new object[]
				{
					this.rootDirInt.FullName,
					Path.DirectorySeparatorChar,
					"About",
					Path.DirectorySeparatorChar,
					"Preview.png"
				});
			}
		}

		public ContentSource Source
		{
			get
			{
				return this.source;
			}
		}

		public bool OnSteamWorkshop
		{
			get
			{
				return this.source == ContentSource.SteamWorkshop;
			}
		}

		private string PublishedFileIdPath
		{
			get
			{
				return string.Concat(new object[]
				{
					this.rootDirInt.FullName,
					Path.DirectorySeparatorChar,
					"About",
					Path.DirectorySeparatorChar,
					"PublishedFileId.txt"
				});
			}
		}

		public ModMetaData(string localAbsPath)
		{
			this.rootDirInt = new DirectoryInfo(localAbsPath);
			this.source = ContentSource.LocalFolder;
			this.Init();
		}

		public ModMetaData(WorkshopItem workshopItem)
		{
			this.rootDirInt = workshopItem.Directory;
			this.source = ContentSource.SteamWorkshop;
			this.Init();
		}

		private void Init()
		{
			this.meta = XmlLoader.ItemFromXmlFile<ModMetaData.ModMetaDataInternal>(string.Concat(new object[]
			{
				this.RootDir.FullName,
				Path.DirectorySeparatorChar,
				"About",
				Path.DirectorySeparatorChar,
				"About.xml"
			}), true);
			if (this.meta.name.NullOrEmpty())
			{
				if (this.OnSteamWorkshop)
				{
					this.meta.name = "Workshop mod " + this.Identifier;
				}
				else
				{
					this.meta.name = this.Identifier;
				}
			}
			if (!this.IsCoreMod && !this.OnSteamWorkshop && !VersionControl.IsWellFormattedVersionString(this.meta.targetVersion))
			{
				Log.ErrorOnce(string.Concat(new string[]
				{
					"Mod ",
					this.meta.name,
					" has incorrectly formatted target version '",
					this.meta.targetVersion,
					"'. For the current version, write: <targetVersion>",
					VersionControl.CurrentVersionString,
					"</targetVersion>"
				}), this.Identifier.GetHashCode());
			}
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				string url = GenFilePaths.SafeURIForUnityWWWFromPath(this.PreviewImagePath);
				WWW wWW = new WWW(url);
				wWW.threadPriority = UnityEngine.ThreadPriority.High;
				while (!wWW.isDone)
				{
					Thread.Sleep(1);
				}
				if (wWW.error == null)
				{
					this.previewImage = wWW.texture;
				}
			});
			string publishedFileIdPath = this.PublishedFileIdPath;
			if (File.Exists(this.PublishedFileIdPath))
			{
				string s = File.ReadAllText(publishedFileIdPath);
				this.publishedFileIdInt = new PublishedFileId_t(ulong.Parse(s));
			}
		}

		internal void DeleteContent()
		{
			this.rootDirInt.Delete(true);
			ModLister.RebuildModList();
		}

		public void PrepareForWorkshopUpload()
		{
		}

		public bool CanToUploadToWorkshop()
		{
			return !this.IsCoreMod && this.Source == ContentSource.LocalFolder && !this.GetWorkshopItemHook().MayHaveAuthorNotCurrentUser;
		}

		public PublishedFileId_t GetPublishedFileId()
		{
			return this.publishedFileIdInt;
		}

		public void SetPublishedFileId(PublishedFileId_t newPfid)
		{
			if (this.publishedFileIdInt == newPfid)
			{
				return;
			}
			this.publishedFileIdInt = newPfid;
			File.WriteAllText(this.PublishedFileIdPath, newPfid.ToString());
		}

		public string GetWorkshopName()
		{
			return this.Name;
		}

		public string GetWorkshopDescription()
		{
			return this.Description;
		}

		public string GetWorkshopPreviewImagePath()
		{
			return this.PreviewImagePath;
		}

		public IList<string> GetWorkshopTags()
		{
			return new List<string>
			{
				"Mod"
			};
		}

		public DirectoryInfo GetWorkshopUploadDirectory()
		{
			return this.RootDir;
		}

		public WorkshopItemHook GetWorkshopItemHook()
		{
			if (this.workshopHookInt == null)
			{
				this.workshopHookInt = new WorkshopItemHook(this);
			}
			return this.workshopHookInt;
		}

		public override int GetHashCode()
		{
			return this.Identifier.GetHashCode();
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"[",
				this.Identifier,
				"|",
				this.Name,
				"]"
			});
		}

		public string ToStringLong()
		{
			return this.Identifier + "(" + this.RootDir.ToString() + ")";
		}
	}
}
