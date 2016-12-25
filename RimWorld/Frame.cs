using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Frame : Building, IConstructible, IThingContainerOwner
	{
		protected const float UnderfieldOverdrawFactor = 1.15f;

		protected const float CenterOverdrawFactor = 0.5f;

		public ThingContainer resourceContainer;

		public float workDone;

		private Material cachedCornerMat;

		private Material cachedTileMat;

		private static readonly Material UnderfieldMat = MaterialPool.MatFrom("Things/Building/BuildingFrame/Underfield", ShaderDatabase.Transparent);

		private static readonly Texture2D CornerTex = ContentFinder<Texture2D>.Get("Things/Building/BuildingFrame/Corner", true);

		private static readonly Texture2D TileTex = ContentFinder<Texture2D>.Get("Things/Building/BuildingFrame/Tile", true);

		private List<ThingCount> cachedMaterialsNeeded = new List<ThingCount>();

		public float WorkToMake
		{
			get
			{
				return this.def.entityDefToBuild.GetStatValueAbstract(StatDefOf.WorkToMake, base.Stuff);
			}
		}

		public float WorkLeft
		{
			get
			{
				return this.WorkToMake - this.workDone;
			}
		}

		public float PercentComplete
		{
			get
			{
				return this.workDone / this.WorkToMake;
			}
		}

		public override string Label
		{
			get
			{
				string text = this.def.entityDefToBuild.label + "FrameLabelExtra".Translate();
				if (base.Stuff != null)
				{
					return base.Stuff.label + " " + text;
				}
				return text;
			}
		}

		public override Color DrawColor
		{
			get
			{
				if (!this.def.MadeFromStuff)
				{
					List<ThingCount> costList = this.def.entityDefToBuild.costList;
					if (costList != null)
					{
						for (int i = 0; i < costList.Count; i++)
						{
							ThingDef thingDef = costList[i].thingDef;
							if (thingDef.IsStuff && thingDef.stuffProps.color != Color.white)
							{
								return thingDef.stuffProps.color;
							}
						}
					}
					return new Color(0.6f, 0.6f, 0.6f);
				}
				return base.DrawColor;
			}
		}

		public EffecterDef ConstructionEffect
		{
			get
			{
				if (base.Stuff != null && base.Stuff.stuffProps.constructEffect != null)
				{
					return base.Stuff.stuffProps.constructEffect;
				}
				if (this.def.entityDefToBuild.constructEffect != null)
				{
					return this.def.entityDefToBuild.constructEffect;
				}
				return EffecterDefOf.ConstructMetal;
			}
		}

		private Material CornerMat
		{
			get
			{
				if (this.cachedCornerMat == null)
				{
					this.cachedCornerMat = MaterialPool.MatFrom(Frame.CornerTex, ShaderDatabase.Cutout, this.DrawColor);
				}
				return this.cachedCornerMat;
			}
		}

		private Material TileMat
		{
			get
			{
				if (this.cachedTileMat == null)
				{
					this.cachedTileMat = MaterialPool.MatFrom(Frame.TileTex, ShaderDatabase.Cutout, this.DrawColor);
				}
				return this.cachedTileMat;
			}
		}

		public Frame()
		{
			this.resourceContainer = new ThingContainer(this, false);
		}

		public ThingContainer GetContainer()
		{
			return this.resourceContainer;
		}

		public IntVec3 GetPosition()
		{
			return base.Position;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<float>(ref this.workDone, "workDone", 0f, false);
			Scribe_Deep.LookDeep<ThingContainer>(ref this.resourceContainer, "resourceContainer", new object[]
			{
				this
			});
		}

		public ThingDef UIStuff()
		{
			return base.Stuff;
		}

		public List<ThingCount> MaterialsNeeded()
		{
			this.cachedMaterialsNeeded.Clear();
			List<ThingCount> list = this.def.entityDefToBuild.CostListAdjusted(base.Stuff, true);
			for (int i = 0; i < list.Count; i++)
			{
				ThingCount thingCount = list[i];
				int num = this.resourceContainer.NumContained(thingCount.thingDef);
				int num2 = thingCount.count - num;
				if (num2 > 0)
				{
					this.cachedMaterialsNeeded.Add(new ThingCount(thingCount.thingDef, num2));
				}
			}
			return this.cachedMaterialsNeeded;
		}

		public void CompleteConstruction(Pawn worker)
		{
			this.resourceContainer.Clear();
			this.Destroy(DestroyMode.Vanish);
			if (this.GetStatValue(StatDefOf.WorkToMake, true) > 150f && this.def.entityDefToBuild is ThingDef && ((ThingDef)this.def.entityDefToBuild).category == ThingCategory.Building)
			{
				SoundDefOf.BuildingComplete.PlayOneShot(base.Position);
			}
			ThingDef thingDef = this.def.entityDefToBuild as ThingDef;
			if (thingDef != null)
			{
				Thing thing = ThingMaker.MakeThing(thingDef, base.Stuff);
				thing.SetFactionDirect(base.Faction);
				CompQuality compQuality = thing.TryGetComp<CompQuality>();
				if (compQuality != null)
				{
					int level = worker.skills.GetSkill(SkillDefOf.Construction).level;
					compQuality.SetQuality(QualityUtility.RandomCreationQuality(level), ArtGenerationContext.Colony);
				}
				CompArt compArt = thing.TryGetComp<CompArt>();
				if (compArt != null)
				{
					if (compQuality == null)
					{
						compArt.InitializeArt(ArtGenerationContext.Colony);
					}
					compArt.JustCreatedBy(worker);
				}
				thing.HitPoints = Mathf.CeilToInt((float)this.HitPoints / (float)base.MaxHitPoints * (float)thing.MaxHitPoints);
				GenSpawn.Spawn(thing, base.Position, base.Rotation);
			}
			else
			{
				Find.TerrainGrid.SetTerrain(base.Position, (TerrainDef)this.def.entityDefToBuild);
			}
			worker.records.Increment(RecordDefOf.ThingsConstructed);
		}

		public void FailConstruction(Pawn worker)
		{
			this.Destroy(DestroyMode.Cancel);
			if (this.def.entityDefToBuild.blueprintDef != null)
			{
				Blueprint_Build blueprint_Build = (Blueprint_Build)ThingMaker.MakeThing(this.def.entityDefToBuild.blueprintDef, null);
				blueprint_Build.stuffToUse = base.Stuff;
				blueprint_Build.SetFactionDirect(base.Faction);
				GenSpawn.Spawn(blueprint_Build, base.Position, base.Rotation);
			}
			MoteMaker.ThrowText(this.DrawPos, "TextMote_ConstructionFail".Translate(), 6f);
			if (base.Faction == Faction.OfPlayer && this.WorkToMake > 1400f)
			{
				Messages.Message("MessageConstructionFailed".Translate(new object[]
				{
					this.Label,
					worker.LabelShort
				}), base.Position, MessageSound.Negative);
			}
		}

		public override void Draw()
		{
			Vector2 vector = new Vector2((float)this.def.size.x, (float)this.def.size.z);
			vector.x *= 1.15f;
			vector.y *= 1.15f;
			Vector3 s = new Vector3(vector.x, 1f, vector.y);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(this.DrawPos, base.Rotation.AsQuat, s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, Frame.UnderfieldMat, 0);
			int num = 4;
			for (int i = 0; i < num; i++)
			{
				float num2 = (float)Mathf.Min(base.RotatedSize.x, base.RotatedSize.z);
				float num3 = num2 * 0.38f;
				IntVec3 intVec = default(IntVec3);
				if (i == 0)
				{
					intVec = new IntVec3(-1, 0, -1);
				}
				else if (i == 1)
				{
					intVec = new IntVec3(-1, 0, 1);
				}
				else if (i == 2)
				{
					intVec = new IntVec3(1, 0, 1);
				}
				else if (i == 3)
				{
					intVec = new IntVec3(1, 0, -1);
				}
				Vector3 b = default(Vector3);
				b.x = (float)intVec.x * ((float)base.RotatedSize.x / 2f - num3 / 2f);
				b.z = (float)intVec.z * ((float)base.RotatedSize.z / 2f - num3 / 2f);
				Vector3 s2 = new Vector3(num3, 1f, num3);
				Matrix4x4 matrix2 = default(Matrix4x4);
				Vector3 arg_1EB_1 = this.DrawPos + Vector3.up * 0.03f + b;
				Rot4 rot = new Rot4(i);
				matrix2.SetTRS(arg_1EB_1, rot.AsQuat, s2);
				Graphics.DrawMesh(MeshPool.plane10, matrix2, this.CornerMat, 0);
			}
			float num4 = this.PercentComplete / 1f;
			int num5 = Mathf.CeilToInt(num4 * (float)base.RotatedSize.x * (float)base.RotatedSize.z * 4f);
			IntVec2 intVec2 = base.RotatedSize * 2;
			for (int j = 0; j < num5; j++)
			{
				IntVec2 intVec3 = default(IntVec2);
				intVec3.z = j / intVec2.x;
				intVec3.x = j - intVec3.z * intVec2.x;
				Vector3 a = new Vector3((float)intVec3.x * 0.5f, 0f, (float)intVec3.z * 0.5f) + this.DrawPos;
				a.x -= (float)base.RotatedSize.x * 0.5f - 0.25f;
				a.z -= (float)base.RotatedSize.z * 0.5f - 0.25f;
				Vector3 s3 = new Vector3(0.5f, 1f, 0.5f);
				Matrix4x4 matrix3 = default(Matrix4x4);
				matrix3.SetTRS(a + Vector3.up * 0.02f, Quaternion.identity, s3);
				Graphics.DrawMesh(MeshPool.plane10, matrix3, this.TileMat, 0);
			}
			base.Comps_PostDraw();
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo c in base.GetGizmos())
			{
				yield return c;
			}
			Command buildCopy = BuildCopyCommandUtility.BuildCopyCommand(this.def.entityDefToBuild, base.Stuff);
			if (buildCopy != null)
			{
				yield return buildCopy;
			}
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			stringBuilder.AppendLine("ContainedResources".Translate() + ":");
			List<ThingCount> list = this.def.entityDefToBuild.CostListAdjusted(base.Stuff, true);
			for (int i = 0; i < list.Count; i++)
			{
				ThingCount need = list[i];
				int num = need.count;
				foreach (ThingCount current in from needed in this.MaterialsNeeded()
				where needed.thingDef == need.thingDef
				select needed)
				{
					num -= current.count;
				}
				stringBuilder.AppendLine(string.Concat(new object[]
				{
					need.thingDef.LabelCap,
					": ",
					num,
					" / ",
					need.count
				}));
			}
			stringBuilder.Append("WorkLeft".Translate() + ": " + this.WorkLeft.ToStringWorkAmount());
			return stringBuilder.ToString();
		}

		virtual bool get_Spawned()
		{
			return base.Spawned;
		}
	}
}
