using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class WaterInfo : MapComponent
	{
		public byte[] riverOffsetMap;

		public Texture2D riverOffsetTexture;

		public List<Vector3> riverDebugData = new List<Vector3>();

		public float[] riverFlowMap;

		public CellRect riverFlowMapBounds;

		public const int RiverOffsetMapBorder = 2;

		public WaterInfo(Map map) : base(map)
		{
		}

		public override void MapRemoved()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				UnityEngine.Object.Destroy(this.riverOffsetTexture);
			});
		}

		public void SetTextures()
		{
			Camera subcamera = Current.SubcameraDriver.GetSubcamera(SubcameraDefOf.WaterDepth);
			Shader.SetGlobalTexture(ShaderPropertyIDs.WaterOutputTex, subcamera.targetTexture);
			if (this.riverOffsetTexture == null && this.riverOffsetMap != null && this.riverOffsetMap.Length > 0)
			{
				this.riverOffsetTexture = new Texture2D(this.map.Size.x + 4, this.map.Size.z + 4, TextureFormat.RGFloat, false);
				this.riverOffsetTexture.LoadRawTextureData(this.riverOffsetMap);
				this.riverOffsetTexture.wrapMode = TextureWrapMode.Clamp;
				this.riverOffsetTexture.Apply();
			}
			Shader.SetGlobalTexture(ShaderPropertyIDs.WaterOffsetTex, this.riverOffsetTexture);
		}

		public Vector3 GetWaterMovement(Vector3 position)
		{
			if (this.riverOffsetMap == null)
			{
				return Vector3.zero;
			}
			if (this.riverFlowMap == null)
			{
				this.GenerateRiverFlowMap();
			}
			IntVec3 intVec = new IntVec3(Mathf.FloorToInt(position.x), 0, Mathf.FloorToInt(position.z));
			IntVec3 c = new IntVec3(Mathf.FloorToInt(position.x) + 1, 0, Mathf.FloorToInt(position.z) + 1);
			if (!this.riverFlowMapBounds.Contains(intVec) || !this.riverFlowMapBounds.Contains(c))
			{
				return Vector3.zero;
			}
			int num = this.riverFlowMapBounds.IndexOf(intVec);
			int num2 = num + 1;
			int num3 = num + this.riverFlowMapBounds.Width;
			int num4 = num3 + 1;
			Vector3 a = Vector3.Lerp(new Vector3(this.riverFlowMap[num * 2], 0f, this.riverFlowMap[num * 2 + 1]), new Vector3(this.riverFlowMap[num2 * 2], 0f, this.riverFlowMap[num2 * 2 + 1]), position.x - Mathf.Floor(position.x));
			Vector3 b = Vector3.Lerp(new Vector3(this.riverFlowMap[num3 * 2], 0f, this.riverFlowMap[num3 * 2 + 1]), new Vector3(this.riverFlowMap[num4 * 2], 0f, this.riverFlowMap[num4 * 2 + 1]), position.x - Mathf.Floor(position.x));
			return Vector3.Lerp(a, b, position.z - (float)Mathf.FloorToInt(position.z));
		}

		public void GenerateRiverFlowMap()
		{
			if (this.riverOffsetMap == null)
			{
				return;
			}
			this.riverFlowMapBounds = new CellRect(-2, -2, this.map.Size.x + 4, this.map.Size.z + 4);
			this.riverFlowMap = new float[this.riverFlowMapBounds.Area * 2];
			float[] array = new float[this.riverFlowMapBounds.Area * 2];
			Buffer.BlockCopy(this.riverOffsetMap, 0, array, 0, array.Length * 4);
			for (int i = this.riverFlowMapBounds.minZ; i <= this.riverFlowMapBounds.maxZ; i++)
			{
				int newZ = (i != this.riverFlowMapBounds.minZ) ? (i - 1) : i;
				int newZ2 = (i != this.riverFlowMapBounds.maxZ) ? (i + 1) : i;
				float num = (float)((i != this.riverFlowMapBounds.minZ && i != this.riverFlowMapBounds.maxZ) ? 2 : 1);
				for (int j = this.riverFlowMapBounds.minX; j <= this.riverFlowMapBounds.maxX; j++)
				{
					int newX = (j != this.riverFlowMapBounds.minX) ? (j - 1) : j;
					int newX2 = (j != this.riverFlowMapBounds.maxX) ? (j + 1) : j;
					float num2 = (float)((j != this.riverFlowMapBounds.minX && j != this.riverFlowMapBounds.maxX) ? 2 : 1);
					float x = (array[this.riverFlowMapBounds.IndexOf(new IntVec3(newX2, 0, i)) * 2 + 1] - array[this.riverFlowMapBounds.IndexOf(new IntVec3(newX, 0, i)) * 2 + 1]) / num2;
					float z = (array[this.riverFlowMapBounds.IndexOf(new IntVec3(j, 0, newZ2)) * 2 + 1] - array[this.riverFlowMapBounds.IndexOf(new IntVec3(j, 0, newZ)) * 2 + 1]) / num;
					Vector3 vector = new Vector3(x, 0f, z);
					if (vector.magnitude > 0.0001f)
					{
						vector = vector.normalized / vector.magnitude;
						int num3 = this.riverFlowMapBounds.IndexOf(new IntVec3(j, 0, i)) * 2;
						this.riverFlowMap[num3] = vector.x;
						this.riverFlowMap[num3 + 1] = vector.z;
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			DataExposeUtility.ByteArray(ref this.riverOffsetMap, "riverOffsetMap");
			this.GenerateRiverFlowMap();
		}

		public void DebugDrawRiver()
		{
			for (int i = 0; i < this.riverDebugData.Count; i += 2)
			{
				GenDraw.DrawLineBetween(this.riverDebugData[i], this.riverDebugData[i + 1], SimpleColor.Magenta);
			}
		}
	}
}
