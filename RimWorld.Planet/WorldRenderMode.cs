using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public abstract class WorldRenderMode
	{
		private const int WorldTextureSize = 4096;

		private Texture2D savedTex;

		public abstract string Label
		{
			get;
		}

		public Texture WorldTex
		{
			get
			{
				if (this.savedTex == null)
				{
					this.RegenerateTexture();
				}
				return this.savedTex;
			}
		}

		public abstract void DrawWorldMeshes();

		public void RegenerateTexture()
		{
			Rand.PushSeed();
			Rand.Seed = Find.World.info.Seed;
			Camera current = Camera.current;
			Camera camera = null;
			GameObject gameObject = new GameObject("WorldRenderCamera_" + this.Label, new Type[]
			{
				typeof(Camera)
			});
			try
			{
				camera = gameObject.GetComponent<Camera>();
				WorldRenderCam worldRenderCam = camera.gameObject.AddComponent<WorldRenderCam>();
				worldRenderCam.renderMode = this;
				camera.backgroundColor = Color.blue;
				gameObject.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
				camera.orthographic = true;
				camera.aspect = 1f;
				float num = (float)Mathf.Max(Find.World.Size.x, Find.World.Size.z);
				camera.gameObject.transform.position = new Vector3(num / 2f, 50f, num / 2f);
				camera.orthographicSize = num / 2f;
				if (current != null)
				{
					current.enabled = false;
				}
				camera.enabled = true;
				this.RenderWorldTexture(camera);
			}
			finally
			{
				if (camera != null)
				{
					camera.enabled = false;
				}
				UnityEngine.Object.Destroy(gameObject);
				gameObject = null;
				if (current != null)
				{
					current.enabled = true;
				}
			}
			Rand.PopSeed();
		}

		public void CleanUp()
		{
			this.DestroyWorldTexture();
		}

		private void RenderWorldTexture(Camera renderCam)
		{
			this.DestroyWorldTexture();
			RenderTexture renderTexture = new RenderTexture(4096, 4096, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			try
			{
				renderCam.targetTexture = renderTexture;
				renderCam.Render();
				renderCam.targetTexture = null;
				RenderTexture.active = renderTexture;
				this.savedTex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
				try
				{
					this.savedTex.ReadPixels(new Rect(0f, 0f, (float)renderTexture.width, (float)renderTexture.height), 0, 0);
					this.savedTex.Apply();
				}
				catch
				{
					this.DestroyWorldTexture();
					throw;
				}
			}
			finally
			{
				RenderTexture.active = null;
				renderTexture.Release();
				UnityEngine.Object.Destroy(renderTexture);
				renderTexture = null;
			}
		}

		public void Notify_WorldChanged()
		{
			this.DestroyWorldTexture();
		}

		private void DestroyWorldTexture()
		{
			Texture2D localCopy = this.savedTex;
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				if (localCopy != null)
				{
					UnityEngine.Object.Destroy(localCopy);
				}
			});
			this.savedTex = null;
		}
	}
}
