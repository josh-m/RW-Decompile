using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class Site : MapParent
	{
		public string customLabel;

		public SiteCore core;

		public List<SitePart> parts = new List<SitePart>();

		public bool sitePartsKnown;

		public bool factionMustRemainHostile;

		public float desiredThreatPoints;

		private bool startedCountdown;

		private bool anyEnemiesInitially;

		private Material cachedMat;

		private static List<string> tmpSitePartsLabels = new List<string>();

		public override string Label
		{
			get
			{
				if (!this.customLabel.NullOrEmpty())
				{
					return this.customLabel;
				}
				if (this.MainSiteDef == SiteCoreDefOf.PreciousLump && this.core.parms.preciousLumpResources != null)
				{
					return "PreciousLumpLabel".Translate(this.core.parms.preciousLumpResources.label);
				}
				return this.MainSiteDef.label;
			}
		}

		public override Texture2D ExpandingIcon
		{
			get
			{
				return this.MainSiteDef.ExpandingIconTexture;
			}
		}

		public override Material Material
		{
			get
			{
				if (this.cachedMat == null)
				{
					Color color;
					if (this.MainSiteDef.applyFactionColorToSiteTexture && base.Faction != null)
					{
						color = base.Faction.Color;
					}
					else
					{
						color = Color.white;
					}
					this.cachedMat = MaterialPool.MatFrom(this.MainSiteDef.siteTexture, ShaderDatabase.WorldOverlayTransparentLit, color, WorldMaterials.WorldObjectRenderQueue);
				}
				return this.cachedMat;
			}
		}

		public override bool AppendFactionToInspectString
		{
			get
			{
				return this.MainSiteDef.applyFactionColorToSiteTexture || this.MainSiteDef.showFactionInInspectString;
			}
		}

		private SiteCoreOrPartBase MainSiteCoreOrPart
		{
			get
			{
				if (this.core.def == SiteCoreDefOf.Nothing && this.parts.Any<SitePart>())
				{
					return this.parts[0];
				}
				return this.core;
			}
		}

		private SiteCoreOrPartDefBase MainSiteDef
		{
			get
			{
				return this.MainSiteCoreOrPart.Def;
			}
		}

		public override IEnumerable<GenStepWithParams> ExtraGenStepDefs
		{
			get
			{
				foreach (GenStepWithParams g in base.ExtraGenStepDefs)
				{
					yield return g;
				}
				GenStepParams coreGenStepParms = default(GenStepParams);
				coreGenStepParms.siteCoreOrPart = this.core;
				List<GenStepDef> coreGenStepDefs = this.core.def.ExtraGenSteps;
				for (int i = 0; i < coreGenStepDefs.Count; i++)
				{
					yield return new GenStepWithParams(coreGenStepDefs[i], coreGenStepParms);
				}
				for (int j = 0; j < this.parts.Count; j++)
				{
					GenStepParams partGenStepParams = default(GenStepParams);
					partGenStepParams.siteCoreOrPart = this.parts[j];
					List<GenStepDef> partGenStepDefs = this.parts[j].def.ExtraGenSteps;
					for (int k = 0; k < partGenStepDefs.Count; k++)
					{
						yield return new GenStepWithParams(partGenStepDefs[k], partGenStepParams);
					}
				}
			}
		}

		public string ApproachOrderString
		{
			get
			{
				return (!this.MainSiteDef.approachOrderString.NullOrEmpty()) ? string.Format(this.MainSiteDef.approachOrderString, this.Label) : "ApproachSite".Translate(this.Label);
			}
		}

		public string ApproachingReportString
		{
			get
			{
				return (!this.MainSiteDef.approachingReportString.NullOrEmpty()) ? string.Format(this.MainSiteDef.approachingReportString, this.Label) : "ApproachingSite".Translate(this.Label);
			}
		}

		public float ActualThreatPoints
		{
			get
			{
				float num = this.core.parms.threatPoints;
				for (int i = 0; i < this.parts.Count; i++)
				{
					num += this.parts[i].parms.threatPoints;
				}
				return num;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<string>(ref this.customLabel, "customLabel", null, false);
			Scribe_Deep.Look<SiteCore>(ref this.core, "core", new object[0]);
			Scribe_Collections.Look<SitePart>(ref this.parts, "parts", LookMode.Deep, new object[0]);
			Scribe_Values.Look<bool>(ref this.startedCountdown, "startedCountdown", false, false);
			Scribe_Values.Look<bool>(ref this.anyEnemiesInitially, "anyEnemiesInitially", false, false);
			Scribe_Values.Look<bool>(ref this.sitePartsKnown, "sitePartsKnown", false, false);
			Scribe_Values.Look<bool>(ref this.factionMustRemainHostile, "factionMustRemainHostile", false, false);
			Scribe_Values.Look<float>(ref this.desiredThreatPoints, "desiredThreatPoints", 0f, false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.SitePostLoadInit(this);
			}
		}

		public override void Tick()
		{
			base.Tick();
			this.core.def.Worker.SiteCoreWorkerTick(this);
			for (int i = 0; i < this.parts.Count; i++)
			{
				this.parts[i].def.Worker.SitePartWorkerTick(this);
			}
			if (base.HasMap)
			{
				this.CheckStartForceExitAndRemoveMapCountdown();
			}
		}

		public override void PostMapGenerate()
		{
			base.PostMapGenerate();
			Map map = base.Map;
			this.core.def.Worker.PostMapGenerate(map);
			for (int i = 0; i < this.parts.Count; i++)
			{
				this.parts[i].def.Worker.PostMapGenerate(map);
			}
			this.anyEnemiesInitially = GenHostility.AnyHostileActiveThreatToPlayer(base.Map);
		}

		public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			alsoRemoveWorldObject = true;
			return !base.Map.mapPawns.AnyPawnBlockingMapRemoval;
		}

		[DebuggerHidden]
		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			foreach (FloatMenuOption f in base.GetFloatMenuOptions(caravan))
			{
				yield return f;
			}
			foreach (FloatMenuOption f2 in this.core.def.Worker.GetFloatMenuOptions(caravan, this))
			{
				yield return f2;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative)
		{
			foreach (FloatMenuOption o in base.GetTransportPodsFloatMenuOptions(pods, representative))
			{
				yield return o;
			}
			foreach (FloatMenuOption o2 in this.core.def.Worker.GetTransportPodsFloatMenuOptions(pods, representative, this))
			{
				yield return o2;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			if (base.HasMap && Find.WorldSelector.SingleSelectedObject == this)
			{
				yield return SettleInExistingMapUtility.SettleCommand(base.Map, true);
			}
		}

		private void CheckStartForceExitAndRemoveMapCountdown()
		{
			if (this.startedCountdown)
			{
				return;
			}
			if (GenHostility.AnyHostileActiveThreatToPlayer(base.Map))
			{
				return;
			}
			this.startedCountdown = true;
			int num = Mathf.RoundToInt(this.core.def.forceExitAndRemoveMapCountdownDurationDays * 60000f);
			string text = (!this.anyEnemiesInitially) ? "MessageSiteCountdownBecauseNoEnemiesInitially".Translate(TimedForcedExit.GetForceExitAndRemoveMapCountdownTimeLeftString(num)) : "MessageSiteCountdownBecauseNoMoreEnemies".Translate(TimedForcedExit.GetForceExitAndRemoveMapCountdownTimeLeftString(num));
			Messages.Message(text, this, MessageTypeDefOf.PositiveEvent, true);
			base.GetComponent<TimedForcedExit>().StartForceExitAndRemoveMapCountdown(num);
			TaleRecorder.RecordTale(TaleDefOf.CaravanAssaultSuccessful, new object[]
			{
				base.Map.mapPawns.FreeColonists.RandomElement<Pawn>()
			});
		}

		public override bool AllMatchingObjectsOnScreenMatchesWith(WorldObject other)
		{
			Site site = other as Site;
			return site != null && site.MainSiteDef == this.MainSiteDef;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (this.sitePartsKnown)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendLine();
				}
				Site.tmpSitePartsLabels.Clear();
				for (int i = 0; i < this.parts.Count; i++)
				{
					if (!this.parts[i].def.alwaysHidden)
					{
						Site.tmpSitePartsLabels.Add(this.parts[i].def.Worker.GetPostProcessedThreatLabel(this, this.parts[i]));
					}
				}
				if (Site.tmpSitePartsLabels.Count == 0)
				{
					stringBuilder.Append("KnownSiteThreatsNone".Translate());
				}
				else if (Site.tmpSitePartsLabels.Count == 1)
				{
					stringBuilder.Append("KnownSiteThreat".Translate(Site.tmpSitePartsLabels[0].CapitalizeFirst()));
				}
				else
				{
					stringBuilder.Append("KnownSiteThreats".Translate(Site.tmpSitePartsLabels.ToCommaList(true).CapitalizeFirst()));
				}
			}
			return stringBuilder.ToString();
		}

		public override string GetDescription()
		{
			string text = this.MainSiteDef.description;
			string description = base.GetDescription();
			if (!description.NullOrEmpty())
			{
				if (!text.NullOrEmpty())
				{
					text += "\n\n";
				}
				text += description;
			}
			return text;
		}
	}
}
