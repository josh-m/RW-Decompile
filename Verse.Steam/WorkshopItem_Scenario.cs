using RimWorld;
using Steamworks;
using System;
using System.IO;
using System.Linq;

namespace Verse.Steam
{
	public class WorkshopItem_Scenario : WorkshopItem
	{
		private Scenario cachedScenario;

		public override PublishedFileId_t PublishedFileId
		{
			get
			{
				return base.PublishedFileId;
			}
			set
			{
				base.PublishedFileId = value;
				if (this.cachedScenario != null)
				{
					this.cachedScenario.SetPublishedFileId(value);
				}
			}
		}

		public Scenario GetScenario()
		{
			if (this.cachedScenario == null)
			{
				this.LoadScenario();
			}
			return this.cachedScenario;
		}

		private void LoadScenario()
		{
			FileInfo fileInfo = (from fi in base.Directory.GetFiles("*.rsc")
			where fi.Extension == ".rsc"
			select fi).First<FileInfo>();
			if (GameDataSaveLoader.TryLoadScenario(fileInfo.FullName, ScenarioCategory.SteamWorkshop, out this.cachedScenario))
			{
				this.cachedScenario.SetPublishedFileId(this.PublishedFileId);
			}
		}
	}
}
