using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace RimWorld
{
	public class Page_ScenarioEditor : Page
	{
		private Scenario curScen;

		private Vector2 infoScrollPosition = Vector2.zero;

		private string seed;

		private bool seedIsValid = true;

		private bool editMode;

		public override string PageTitle
		{
			get
			{
				return "ScenarioEditor".Translate();
			}
		}

		public Scenario EditingScenario
		{
			get
			{
				return this.curScen;
			}
		}

		public Page_ScenarioEditor(Scenario scen)
		{
			if (scen != null)
			{
				this.curScen = scen;
				this.seedIsValid = false;
			}
			else
			{
				this.RandomizeSeedAndScenario();
			}
		}

		public override void PreOpen()
		{
			base.PreOpen();
			this.infoScrollPosition = Vector2.zero;
		}

		public override void DoWindowContents(Rect rect)
		{
			base.DrawPageTitle(rect);
			Rect mainRect = base.GetMainRect(rect, 0f, false);
			GUI.BeginGroup(mainRect);
			Rect rect2 = new Rect(0f, 0f, mainRect.width * 0.35f, mainRect.height).Rounded();
			this.DoConfigControls(rect2);
			Rect rect3 = new Rect(rect2.xMax + 17f, 0f, mainRect.width - rect2.width - 17f, mainRect.height).Rounded();
			if (!this.editMode)
			{
				ScenarioUI.DrawScenarioInfo(rect3, this.curScen, ref this.infoScrollPosition);
			}
			else
			{
				ScenarioUI.DrawScenarioEditInterface(rect3, this.curScen, ref this.infoScrollPosition);
			}
			GUI.EndGroup();
			base.DoBottomButtons(rect, null, null, null, true);
		}

		private void RandomizeSeedAndScenario()
		{
			this.seed = GenText.RandomSeedString();
			this.curScen = ScenarioMaker.GenerateNewRandomScenario(this.seed);
		}

		private void DoConfigControls(Rect rect)
		{
			Listing_Standard listing_Standard = new Listing_Standard(rect);
			listing_Standard.ColumnWidth = 200f;
			if (listing_Standard.ButtonText("Load".Translate(), null))
			{
				Find.WindowStack.Add(new Dialog_ScenarioList_Load(delegate(Scenario loadedScen)
				{
					this.curScen = loadedScen;
					this.seedIsValid = false;
				}));
			}
			if (listing_Standard.ButtonText("Save".Translate(), null) && Page_ScenarioEditor.CheckAllPartsCompatible(this.curScen))
			{
				Find.WindowStack.Add(new Dialog_ScenarioList_Save(this.curScen));
			}
			if (listing_Standard.ButtonText("RandomizeSeed".Translate(), null))
			{
				SoundDefOf.TickTiny.PlayOneShotOnCamera();
				this.RandomizeSeedAndScenario();
				this.seedIsValid = true;
			}
			if (this.seedIsValid)
			{
				listing_Standard.Label("Seed".Translate().CapitalizeFirst());
				string a = listing_Standard.TextEntry(this.seed, 1);
				if (a != this.seed)
				{
					this.seed = a;
					this.curScen = ScenarioMaker.GenerateNewRandomScenario(this.seed);
				}
			}
			else
			{
				listing_Standard.Gap(Text.LineHeight + Text.LineHeight + 2f);
			}
			listing_Standard.CheckboxLabeled("EditMode".Translate().CapitalizeFirst(), ref this.editMode, null);
			if (this.editMode)
			{
				this.seedIsValid = false;
				if (listing_Standard.ButtonText("AddPart".Translate(), null))
				{
					this.OpenAddScenPartMenu();
				}
				if (SteamManager.Initialized && (this.curScen.Category == ScenarioCategory.CustomLocal || this.curScen.Category == ScenarioCategory.SteamWorkshop) && listing_Standard.ButtonText(Workshop.UploadButtonLabel(this.curScen.GetPublishedFileId()), null) && Page_ScenarioEditor.CheckAllPartsCompatible(this.curScen))
				{
					AcceptanceReport acceptanceReport = this.curScen.TryUploadReport();
					if (!acceptanceReport.Accepted)
					{
						Messages.Message(acceptanceReport.Reason, MessageSound.RejectInput);
					}
					else
					{
						SoundDefOf.TickHigh.PlayOneShotOnCamera();
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSteamWorkshopUpload".Translate(), delegate
						{
							SoundDefOf.TickHigh.PlayOneShotOnCamera();
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmContentAuthor".Translate(), delegate
							{
								SoundDefOf.TickHigh.PlayOneShotOnCamera();
								Workshop.Upload(this.curScen);
							}, true, null));
						}, true, null));
					}
				}
			}
			listing_Standard.End();
		}

		private static bool CheckAllPartsCompatible(Scenario scen)
		{
			foreach (ScenPart current in scen.AllParts)
			{
				int num = 0;
				foreach (ScenPart current2 in scen.AllParts)
				{
					if (current2.def == current.def)
					{
						num++;
					}
					if (num > current.def.maxUses)
					{
						Messages.Message("TooMany".Translate(new object[]
						{
							current.def.maxUses
						}) + ": " + current.Label, MessageSound.RejectInput);
						bool result = false;
						return result;
					}
					if (current != current2 && !current.CanCoexistWith(current2))
					{
						Messages.Message(string.Concat(new string[]
						{
							"Incompatible".Translate(),
							": ",
							current.Label,
							", ",
							current2.Label
						}), MessageSound.RejectInput);
						bool result = false;
						return result;
					}
				}
			}
			return true;
		}

		private void OpenAddScenPartMenu()
		{
			FloatMenuUtility.MakeMenu<ScenPartDef>(from p in ScenarioMaker.AddableParts(this.curScen)
			where p.category != ScenPartCategory.Fixed
			orderby p.label
			select p, (ScenPartDef p) => p.LabelCap, (ScenPartDef p) => delegate
			{
				this.AddScenPart(p);
			});
		}

		private void AddScenPart(ScenPartDef def)
		{
			ScenPart scenPart = ScenarioMaker.MakeScenPart(def);
			scenPart.Randomize();
			this.curScen.parts.Add(scenPart);
		}

		protected override bool CanDoNext()
		{
			if (!base.CanDoNext())
			{
				return false;
			}
			if (this.curScen == null)
			{
				return false;
			}
			if (!Page_ScenarioEditor.CheckAllPartsCompatible(this.curScen))
			{
				return false;
			}
			Page_SelectScenario.BeginScenarioConfiguration(this.curScen, this);
			return true;
		}
	}
}
