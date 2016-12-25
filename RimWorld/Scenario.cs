using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Verse;
using Verse.Steam;

namespace RimWorld
{
	public class Scenario : IExposable, WorkshopUploadable
	{
		public const int NameMaxLength = 55;

		public const int SummaryMaxLength = 300;

		public const int DescriptionMaxLength = 1000;

		public string name;

		public string summary;

		public string description;

		internal ScenPart_PlayerFaction playerFaction;

		internal List<ScenPart> parts = new List<ScenPart>();

		private PublishedFileId_t publishedFileIdInt = PublishedFileId_t.Invalid;

		private ScenarioCategory categoryInt;

		public string fileName;

		private WorkshopItemHook workshopHookInt;

		private string tempUploadDir;

		public bool enabled = true;

		public FileInfo File
		{
			get
			{
				return new FileInfo(GenFilePaths.AbsPathForScenario(this.fileName));
			}
		}

		public IEnumerable<ScenPart> AllParts
		{
			get
			{
				yield return this.playerFaction;
				for (int i = 0; i < this.parts.Count; i++)
				{
					yield return this.parts[i];
				}
			}
		}

		public ScenarioCategory Category
		{
			get
			{
				if (this.categoryInt == ScenarioCategory.Undefined)
				{
					Log.Error("Category is Undefined on Scenario " + this);
				}
				return this.categoryInt;
			}
			set
			{
				this.categoryInt = value;
			}
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<string>(ref this.name, "name", null, false);
			Scribe_Values.LookValue<string>(ref this.summary, "summary", null, false);
			Scribe_Values.LookValue<string>(ref this.description, "description", null, false);
			Scribe_Values.LookValue<PublishedFileId_t>(ref this.publishedFileIdInt, "publishedFileId", PublishedFileId_t.Invalid, false);
			Scribe_Deep.LookDeep<ScenPart_PlayerFaction>(ref this.playerFaction, "playerFaction", new object[0]);
			Scribe_Collections.LookList<ScenPart>(ref this.parts, "parts", LookMode.Deep, new object[0]);
		}

		[DebuggerHidden]
		public IEnumerable<string> ConfigErrors()
		{
			if (this.name.NullOrEmpty())
			{
				yield return "no title";
			}
			if (this.parts.NullOrEmpty<ScenPart>())
			{
				yield return "no parts";
			}
			if (this.playerFaction == null)
			{
				yield return "no playerFaction";
			}
			foreach (ScenPart part in this.AllParts)
			{
				foreach (string e in part.ConfigErrors())
				{
					yield return e;
				}
			}
		}

		public string GetFullInformationText()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(this.description);
			stringBuilder.AppendLine();
			foreach (ScenPart current in this.AllParts)
			{
				current.summarized = false;
			}
			foreach (ScenPart current2 in from p in this.AllParts
			orderby p.def.summaryPriority descending, p.def.defName
			where p.visible
			select p)
			{
				string text = current2.Summary(this);
				if (!text.NullOrEmpty())
				{
					stringBuilder.AppendLine(text);
				}
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public string GetSummary()
		{
			return this.summary;
		}

		public Scenario CopyForEditing()
		{
			Scenario scenario = new Scenario();
			scenario.name = this.name;
			scenario.summary = this.summary;
			scenario.description = this.description;
			scenario.playerFaction = (ScenPart_PlayerFaction)this.playerFaction.CopyForEditing();
			scenario.parts.AddRange(from p in this.parts
			select p.CopyForEditing());
			scenario.categoryInt = ScenarioCategory.CustomLocal;
			return scenario;
		}

		public void PreConfigure()
		{
			foreach (ScenPart current in this.AllParts)
			{
				current.PreConfigure();
			}
		}

		public Page GetFirstConfigPage()
		{
			List<Page> list = new List<Page>();
			list.Add(new Page_SelectStoryteller());
			list.Add(new Page_CreateWorldParams());
			list.Add(new Page_SelectLandingSite());
			foreach (Page current in this.parts.SelectMany((ScenPart p) => p.GetConfigPages()))
			{
				list.Add(current);
			}
			Page page = PageUtility.StitchedPages(list);
			if (page != null)
			{
				Page page2 = page;
				while (page2.next != null)
				{
					page2 = page2.next;
				}
				page2.nextAct = delegate
				{
					PageUtility.InitGameStart();
				};
			}
			return page;
		}

		public bool AllowPlayerStartingPawn(Pawn pawn)
		{
			foreach (ScenPart current in this.AllParts)
			{
				if (!current.AllowPlayerStartingPawn(pawn))
				{
					return false;
				}
			}
			return true;
		}

		public void Notify_PawnGenerated(Pawn pawn, PawnGenerationContext context)
		{
			foreach (ScenPart current in this.AllParts)
			{
				current.Notify_PawnGenerated(pawn, context);
			}
		}

		public void Notify_PawnDied(Corpse corpse)
		{
			for (int i = 0; i < this.parts.Count; i++)
			{
				this.parts[i].Notify_PawnDied(corpse);
			}
		}

		public void PostWorldLoad()
		{
			foreach (ScenPart current in this.AllParts)
			{
				current.PostWorldLoad();
			}
		}

		public void PreMapGenerate()
		{
			foreach (ScenPart current in this.AllParts)
			{
				current.PreMapGenerate();
			}
		}

		public void GenerateIntoMap()
		{
			foreach (ScenPart current in this.AllParts)
			{
				current.GenerateIntoMap();
			}
		}

		public void PostMapGenerate()
		{
			foreach (ScenPart current in this.AllParts)
			{
				current.PostMapGenerate();
			}
		}

		public void PostGameStart()
		{
			foreach (ScenPart current in this.AllParts)
			{
				current.PostGameStart();
			}
		}

		public float GetStatFactor(StatDef stat)
		{
			float num = 1f;
			for (int i = 0; i < this.parts.Count; i++)
			{
				ScenPart_StatFactor scenPart_StatFactor = this.parts[i] as ScenPart_StatFactor;
				if (scenPart_StatFactor != null)
				{
					num *= scenPart_StatFactor.GetStatFactor(stat);
				}
			}
			return num;
		}

		public void TickScenario()
		{
			for (int i = 0; i < this.parts.Count; i++)
			{
				this.parts[i].Tick();
			}
		}

		public void RemovePart(ScenPart part)
		{
			if (!this.parts.Contains(part))
			{
				Log.Error("Cannot remove: " + part);
			}
			this.parts.Remove(part);
		}

		public bool CanReorder(ScenPart part, ReorderDirection dir)
		{
			if (!part.def.PlayerAddRemovable)
			{
				return false;
			}
			int num = this.parts.IndexOf(part);
			if (dir == ReorderDirection.Up)
			{
				return num != 0 && (num <= 0 || this.parts[num - 1].def.PlayerAddRemovable);
			}
			if (dir == ReorderDirection.Down)
			{
				return num != this.parts.Count - 1;
			}
			throw new NotImplementedException();
		}

		public void Reorder(ScenPart part, ReorderDirection dir)
		{
			int num = this.parts.IndexOf(part);
			this.parts.RemoveAt(num);
			if (dir == ReorderDirection.Up)
			{
				this.parts.Insert(num - 1, part);
			}
			if (dir == ReorderDirection.Down)
			{
				this.parts.Insert(num + 1, part);
			}
		}

		public bool CanToUploadToWorkshop()
		{
			return this.Category != ScenarioCategory.FromDef && this.TryUploadReport().Accepted && !this.GetWorkshopItemHook().MayHaveAuthorNotCurrentUser;
		}

		public void PrepareForWorkshopUpload()
		{
			string path = this.name + Rand.RangeInclusive(100, 999).ToString();
			this.tempUploadDir = Path.Combine(GenFilePaths.TempFolderPath, path);
			DirectoryInfo directoryInfo = new DirectoryInfo(this.tempUploadDir);
			if (directoryInfo.Exists)
			{
				directoryInfo.Delete();
			}
			directoryInfo.Create();
			string text = Path.Combine(this.tempUploadDir, this.name);
			text += ".rsc";
			GameDataSaveLoader.SaveScenario(this, text);
		}

		public AcceptanceReport TryUploadReport()
		{
			if (this.name == null || this.name.Length < 3 || this.summary == null || this.summary.Length < 3 || this.description == null || this.description.Length < 3)
			{
				return "TextFieldsMustBeFilled".Translate();
			}
			return AcceptanceReport.WasAccepted;
		}

		public PublishedFileId_t GetPublishedFileId()
		{
			return this.publishedFileIdInt;
		}

		public void SetPublishedFileId(PublishedFileId_t newPfid)
		{
			this.publishedFileIdInt = newPfid;
			if (this.Category == ScenarioCategory.CustomLocal && !this.fileName.NullOrEmpty())
			{
				GameDataSaveLoader.SaveScenario(this, GenFilePaths.AbsPathForScenario(this.fileName));
			}
		}

		public string GetWorkshopName()
		{
			return this.name;
		}

		public string GetWorkshopDescription()
		{
			return this.GetFullInformationText();
		}

		public string GetWorkshopPreviewImagePath()
		{
			return GenFilePaths.ScenarioPreviewImagePath;
		}

		public IList<string> GetWorkshopTags()
		{
			return new List<string>
			{
				"Scenario"
			};
		}

		public DirectoryInfo GetWorkshopUploadDirectory()
		{
			return new DirectoryInfo(this.tempUploadDir);
		}

		public WorkshopItemHook GetWorkshopItemHook()
		{
			if (this.workshopHookInt == null)
			{
				this.workshopHookInt = new WorkshopItemHook(this);
			}
			return this.workshopHookInt;
		}

		public override string ToString()
		{
			return this.name.NullOrEmpty() ? "LabellessScenario" : this.name;
		}

		public override int GetHashCode()
		{
			int num = 6126121;
			if (this.name != null)
			{
				num ^= this.name.GetHashCode();
			}
			if (this.summary != null)
			{
				num ^= this.summary.GetHashCode();
			}
			if (this.description != null)
			{
				num ^= this.description.GetHashCode();
			}
			return num ^ this.publishedFileIdInt.GetHashCode();
		}
	}
}
