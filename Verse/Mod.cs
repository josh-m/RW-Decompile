using System;
using UnityEngine;

namespace Verse
{
	public abstract class Mod
	{
		private ModSettings modSettings;

		private ModContentPack intContent;

		public ModContentPack Content
		{
			get
			{
				return this.intContent;
			}
		}

		public Mod(ModContentPack content)
		{
			this.intContent = content;
		}

		public T GetSettings<T>() where T : ModSettings, new()
		{
			if (this.modSettings != null && this.modSettings.GetType() != typeof(T))
			{
				Log.Error(string.Format("Mod {0} attempted to read two different settings classes (was {1}, is now {2})", this.Content.Name, this.modSettings.GetType(), typeof(T)));
				return (T)((object)null);
			}
			if (this.modSettings != null)
			{
				return (T)((object)this.modSettings);
			}
			this.modSettings = LoadedModManager.ReadModSettings<T>(this.intContent.Identifier, base.GetType().Name);
			this.modSettings.Mod = this;
			return this.modSettings as T;
		}

		public virtual void WriteSettings()
		{
			if (this.modSettings != null)
			{
				this.modSettings.Write();
			}
		}

		public virtual void DoSettingsWindowContents(Rect inRect)
		{
		}

		public virtual string SettingsCategory()
		{
			return string.Empty;
		}
	}
}
