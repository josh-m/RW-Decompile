using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class WorldFeatures : IExposable
	{
		public List<WorldFeature> features = new List<WorldFeature>();

		public bool textsCreated;

		private static List<TextMeshPro> texts = new List<TextMeshPro>();

		private const float BaseAlpha = 0.3f;

		private const float AlphaChangeSpeed = 5f;

		private static readonly GameObject WorldTextPrefab = Resources.Load<GameObject>("Prefabs/WorldText");

		public void ExposeData()
		{
			Scribe_Collections.Look<WorldFeature>(ref this.features, "features", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				WorldGrid grid = Find.WorldGrid;
				if (grid.tileFeature != null && grid.tileFeature.Length != 0)
				{
					DataSerializeUtility.LoadUshort(grid.tileFeature, grid.TilesCount, delegate(int i, ushort data)
					{
						grid[i].feature = ((data != 65535) ? this.GetFeatureWithID((int)data) : null);
					});
				}
				this.textsCreated = false;
			}
		}

		public void UpdateFeatures()
		{
			if (!this.textsCreated)
			{
				this.textsCreated = true;
				this.CreateTextsAndSetPosition();
			}
			bool showWorldFeatures = Find.PlaySettings.showWorldFeatures;
			for (int i = 0; i < this.features.Count; i++)
			{
				Vector3 position = WorldFeatures.texts[i].transform.position;
				bool flag = showWorldFeatures && !WorldRendererUtility.HiddenBehindTerrainNow(position);
				if (flag != WorldFeatures.texts[i].gameObject.activeInHierarchy)
				{
					WorldFeatures.texts[i].gameObject.SetActive(flag);
					this.WrapAroundPlanetSurface(WorldFeatures.texts[i]);
				}
				if (flag)
				{
					this.UpdateAlpha(WorldFeatures.texts[i], this.features[i]);
				}
			}
		}

		public WorldFeature GetFeatureWithID(int uniqueID)
		{
			for (int i = 0; i < this.features.Count; i++)
			{
				if (this.features[i].uniqueID == uniqueID)
				{
					return this.features[i];
				}
			}
			return null;
		}

		private void UpdateAlpha(TextMeshPro text, WorldFeature feature)
		{
			float num = 0.3f * feature.alpha;
			if (text.color.a != num)
			{
				text.color = new Color(1f, 1f, 1f, num);
				this.WrapAroundPlanetSurface(text);
			}
			float num2 = Time.deltaTime * 5f;
			if (this.GoodCameraAltitudeFor(text))
			{
				feature.alpha += num2;
			}
			else
			{
				feature.alpha -= num2;
			}
			feature.alpha = Mathf.Clamp01(feature.alpha);
		}

		private bool GoodCameraAltitudeFor(TextMeshPro text)
		{
			float num = text.renderedHeight;
			float altitude = Find.WorldCameraDriver.altitude;
			float num2 = 1f / (altitude / 125f * (altitude / 125f));
			num *= num2;
			if (Find.WorldCameraDriver.CurrentZoom <= WorldCameraZoomRange.VeryClose && num >= 0.56f)
			{
				return false;
			}
			if (num < 0.038f)
			{
				return Find.WorldCameraDriver.AltitudePercent <= 0.07f;
			}
			return num <= 0.82f || Find.WorldCameraDriver.AltitudePercent >= 0.35f;
		}

		private void CreateTextsAndSetPosition()
		{
			this.CreateOrDestroyTexts();
			WorldGrid worldGrid = Find.WorldGrid;
			float averageTileSize = worldGrid.averageTileSize;
			for (int i = 0; i < this.features.Count; i++)
			{
				WorldFeatures.texts[i].text = this.features[i].name;
				WorldFeatures.texts[i].rectTransform.sizeDelta = new Vector2(this.features[i].maxDrawSizeInTiles.x * averageTileSize, this.features[i].maxDrawSizeInTiles.y * averageTileSize);
				Vector3 normalized = this.features[i].drawCenter.normalized;
				Quaternion quaternion = Quaternion.LookRotation(Vector3.Cross(normalized, Vector3.up), normalized);
				quaternion *= Quaternion.Euler(Vector3.right * 90f);
				quaternion *= Quaternion.Euler(Vector3.forward * (90f - this.features[i].drawAngle));
				WorldFeatures.texts[i].transform.rotation = quaternion;
				WorldFeatures.texts[i].transform.localPosition = this.features[i].drawCenter;
				this.WrapAroundPlanetSurface(WorldFeatures.texts[i]);
				WorldFeatures.texts[i].gameObject.SetActive(false);
			}
		}

		private void CreateOrDestroyTexts()
		{
			while (WorldFeatures.texts.Count > this.features.Count)
			{
				UnityEngine.Object.Destroy(WorldFeatures.texts[WorldFeatures.texts.Count - 1]);
				WorldFeatures.texts.RemoveLast<TextMeshPro>();
			}
			while (WorldFeatures.texts.Count < this.features.Count)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(WorldFeatures.WorldTextPrefab);
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				TextMeshPro component = gameObject.GetComponent<TextMeshPro>();
				component.color = new Color(1f, 1f, 1f, 0f);
				Material[] sharedMaterials = component.GetComponent<MeshRenderer>().sharedMaterials;
				for (int i = 0; i < sharedMaterials.Length; i++)
				{
					sharedMaterials[i].renderQueue = WorldMaterials.FeatureNameRenderQueue;
				}
				WorldFeatures.texts.Add(component);
			}
		}

		private void WrapAroundPlanetSurface(TextMeshPro textMesh)
		{
			TMP_TextInfo textInfo = textMesh.textInfo;
			int characterCount = textInfo.characterCount;
			if (characterCount == 0)
			{
				return;
			}
			textMesh.ForceMeshUpdate();
			Vector3[] vertices = textMesh.mesh.vertices;
			float num = textMesh.bounds.extents.x * 2f;
			float num2 = Find.WorldGrid.DistOnSurfaceToAngle(num);
			for (int i = 0; i < characterCount; i++)
			{
				TMP_CharacterInfo tMP_CharacterInfo = textInfo.characterInfo[i];
				if (tMP_CharacterInfo.isVisible)
				{
					int vertexIndex = tMP_CharacterInfo.vertexIndex;
					Vector3 vector = vertices[vertexIndex] + vertices[vertexIndex + 1] + vertices[vertexIndex + 2] + vertices[vertexIndex + 3];
					vector /= 4f;
					float num3 = vector.x / (num / 2f);
					bool flag = num3 >= 0f;
					num3 = Mathf.Abs(num3);
					float num4 = num2 / 2f * num3;
					float num5 = (180f - num4) / 2f;
					float num6 = 200f * Mathf.Tan(num4 / 2f * 0.0174532924f);
					Vector3 vector2 = new Vector3(Mathf.Sin(num5 * 0.0174532924f) * num6 * ((!flag) ? -1f : 1f), vector.y, Mathf.Cos(num5 * 0.0174532924f) * num6);
					vector2 += new Vector3(Mathf.Sin(num4 * 0.0174532924f) * ((!flag) ? -1f : 1f), 0f, -Mathf.Cos(num4 * 0.0174532924f)) * 0.06f;
					Vector3 b = vector2 - vector;
					Vector3 vector3 = vertices[vertexIndex] + b;
					Vector3 vector4 = vertices[vertexIndex + 1] + b;
					Vector3 vector5 = vertices[vertexIndex + 2] + b;
					Vector3 vector6 = vertices[vertexIndex + 3] + b;
					Quaternion rotation = Quaternion.Euler(0f, num4 * ((!flag) ? 1f : -1f), 0f);
					vector3 = rotation * (vector3 - vector2) + vector2;
					vector4 = rotation * (vector4 - vector2) + vector2;
					vector5 = rotation * (vector5 - vector2) + vector2;
					vector6 = rotation * (vector6 - vector2) + vector2;
					vertices[vertexIndex] = vector3;
					vertices[vertexIndex + 1] = vector4;
					vertices[vertexIndex + 2] = vector5;
					vertices[vertexIndex + 3] = vector6;
				}
			}
			textMesh.mesh.vertices = vertices;
		}
	}
}
