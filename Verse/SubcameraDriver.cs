using System;
using UnityEngine;

namespace Verse
{
	public class SubcameraDriver : MonoBehaviour
	{
		private Camera[] subcameras;

		public void Init()
		{
			if (this.subcameras != null)
			{
				return;
			}
			if (!PlayDataLoader.Loaded)
			{
				return;
			}
			Camera camera = Find.Camera;
			this.subcameras = new Camera[DefDatabase<SubcameraDef>.DefCount];
			foreach (SubcameraDef current in DefDatabase<SubcameraDef>.AllDefsListForReading)
			{
				Camera camera2 = new GameObject
				{
					name = current.defName,
					transform = 
					{
						parent = base.transform,
						localPosition = Vector3.zero,
						localScale = Vector3.one,
						localRotation = Quaternion.identity
					}
				}.AddComponent<Camera>();
				camera2.orthographic = camera.orthographic;
				camera2.orthographicSize = camera.orthographicSize;
				if (current.layer.NullOrEmpty())
				{
					camera2.cullingMask = 0;
				}
				else
				{
					camera2.cullingMask = LayerMask.GetMask(new string[]
					{
						current.layer
					});
				}
				camera2.nearClipPlane = camera.nearClipPlane;
				camera2.farClipPlane = camera.farClipPlane;
				camera2.useOcclusionCulling = camera.useOcclusionCulling;
				camera2.allowHDR = camera.allowHDR;
				camera2.renderingPath = camera.renderingPath;
				camera2.clearFlags = CameraClearFlags.Color;
				camera2.backgroundColor = new Color(0f, 0f, 0f, 0f);
				camera2.depth = (float)current.depth;
				this.subcameras[(int)current.index] = camera2;
			}
		}

		public void UpdatePositions(Camera camera)
		{
			if (this.subcameras == null)
			{
				return;
			}
			for (int i = 0; i < this.subcameras.Length; i++)
			{
				this.subcameras[i].orthographicSize = camera.orthographicSize;
				RenderTexture renderTexture = this.subcameras[i].targetTexture;
				if (renderTexture != null && (renderTexture.width != Screen.width || renderTexture.height != Screen.height))
				{
					UnityEngine.Object.Destroy(renderTexture);
					renderTexture = null;
				}
				if (renderTexture == null)
				{
					renderTexture = new RenderTexture(Screen.width, Screen.height, 0, DefDatabase<SubcameraDef>.AllDefsListForReading[i].BestFormat);
				}
				if (!renderTexture.IsCreated())
				{
					renderTexture.Create();
				}
				this.subcameras[i].targetTexture = renderTexture;
			}
		}

		public Camera GetSubcamera(SubcameraDef def)
		{
			if (this.subcameras == null || def == null || this.subcameras.Length <= (int)def.index)
			{
				return null;
			}
			return this.subcameras[(int)def.index];
		}
	}
}
