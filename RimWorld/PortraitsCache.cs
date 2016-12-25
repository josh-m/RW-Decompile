using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class PortraitsCache
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct CachedPortrait
		{
			private const float CacheDuration = 1f;

			public RenderTexture RenderTexture
			{
				get;
				private set;
			}

			public bool Dirty
			{
				get;
				private set;
			}

			public float LastUseTime
			{
				get;
				private set;
			}

			public bool Expired
			{
				get
				{
					return Time.time - this.LastUseTime > 1f;
				}
			}

			public CachedPortrait(RenderTexture renderTexture, bool dirty, float lastUseTime)
			{
				this.RenderTexture = renderTexture;
				this.Dirty = dirty;
				this.LastUseTime = lastUseTime;
			}
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct CachedPortraitsWithParams
		{
			public Dictionary<Pawn, PortraitsCache.CachedPortrait> CachedPortraits
			{
				get;
				private set;
			}

			public Vector2 Size
			{
				get;
				private set;
			}

			public Vector3 CameraOffset
			{
				get;
				private set;
			}

			public float CameraZoom
			{
				get;
				private set;
			}

			public CachedPortraitsWithParams(Vector2 size, Vector3 cameraOffset, float cameraZoom)
			{
				this.CachedPortraits = new Dictionary<Pawn, PortraitsCache.CachedPortrait>();
				this.Size = size;
				this.CameraOffset = cameraOffset;
				this.CameraZoom = cameraZoom;
			}
		}

		private static List<RenderTexture> renderTexturesPool = new List<RenderTexture>();

		private static List<PortraitsCache.CachedPortraitsWithParams> cachedPortraits = new List<PortraitsCache.CachedPortraitsWithParams>();

		private static List<Pawn> toRemove = new List<Pawn>();

		private static List<Pawn> toSetDirty = new List<Pawn>();

		public static RenderTexture Get(Pawn pawn, Vector2 size, Vector3 cameraOffset = default(Vector3), float cameraZoom = 1f)
		{
			Dictionary<Pawn, PortraitsCache.CachedPortrait> dictionary = PortraitsCache.GetOrCreateCachedPortraitsWithParams(size, cameraOffset, cameraZoom).CachedPortraits;
			PortraitsCache.CachedPortrait cachedPortrait;
			if (dictionary.TryGetValue(pawn, out cachedPortrait))
			{
				if (!cachedPortrait.RenderTexture.IsCreated())
				{
					cachedPortrait.RenderTexture.Create();
					PortraitsCache.RenderPortrait(pawn, cachedPortrait.RenderTexture, cameraOffset, cameraZoom);
				}
				else if (cachedPortrait.Dirty)
				{
					PortraitsCache.RenderPortrait(pawn, cachedPortrait.RenderTexture, cameraOffset, cameraZoom);
				}
				dictionary.Remove(pawn);
				dictionary.Add(pawn, new PortraitsCache.CachedPortrait(cachedPortrait.RenderTexture, false, Time.time));
				return cachedPortrait.RenderTexture;
			}
			RenderTexture renderTexture = PortraitsCache.NewRenderTexture(size);
			PortraitsCache.RenderPortrait(pawn, renderTexture, cameraOffset, cameraZoom);
			dictionary.Add(pawn, new PortraitsCache.CachedPortrait(renderTexture, false, Time.time));
			return renderTexture;
		}

		public static void SetDirty(Pawn pawn)
		{
			for (int i = 0; i < PortraitsCache.cachedPortraits.Count; i++)
			{
				Dictionary<Pawn, PortraitsCache.CachedPortrait> dictionary = PortraitsCache.cachedPortraits[i].CachedPortraits;
				PortraitsCache.CachedPortrait cachedPortrait;
				if (dictionary.TryGetValue(pawn, out cachedPortrait) && !cachedPortrait.Dirty)
				{
					dictionary.Remove(pawn);
					dictionary.Add(pawn, new PortraitsCache.CachedPortrait(cachedPortrait.RenderTexture, true, cachedPortrait.LastUseTime));
				}
			}
		}

		public static void PortraitsCacheUpdate()
		{
			PortraitsCache.RemoveExpiredCachedPortraits();
			PortraitsCache.SetAnimatedPortraitsDirty();
		}

		public static void Clear()
		{
			for (int i = 0; i < PortraitsCache.cachedPortraits.Count; i++)
			{
				foreach (KeyValuePair<Pawn, PortraitsCache.CachedPortrait> current in PortraitsCache.cachedPortraits[i].CachedPortraits)
				{
					PortraitsCache.DestroyRenderTexture(current.Value.RenderTexture);
				}
			}
			PortraitsCache.cachedPortraits.Clear();
			for (int j = 0; j < PortraitsCache.renderTexturesPool.Count; j++)
			{
				PortraitsCache.DestroyRenderTexture(PortraitsCache.renderTexturesPool[j]);
			}
			PortraitsCache.renderTexturesPool.Clear();
		}

		private static PortraitsCache.CachedPortraitsWithParams GetOrCreateCachedPortraitsWithParams(Vector2 size, Vector3 cameraOffset, float cameraZoom)
		{
			for (int i = 0; i < PortraitsCache.cachedPortraits.Count; i++)
			{
				if (PortraitsCache.cachedPortraits[i].Size == size && PortraitsCache.cachedPortraits[i].CameraOffset == cameraOffset && PortraitsCache.cachedPortraits[i].CameraZoom == cameraZoom)
				{
					return PortraitsCache.cachedPortraits[i];
				}
			}
			PortraitsCache.CachedPortraitsWithParams cachedPortraitsWithParams = new PortraitsCache.CachedPortraitsWithParams(size, cameraOffset, cameraZoom);
			PortraitsCache.cachedPortraits.Add(cachedPortraitsWithParams);
			return cachedPortraitsWithParams;
		}

		private static void DestroyRenderTexture(RenderTexture rt)
		{
			rt.DiscardContents();
			UnityEngine.Object.Destroy(rt);
		}

		private static void RemoveExpiredCachedPortraits()
		{
			for (int i = 0; i < PortraitsCache.cachedPortraits.Count; i++)
			{
				Dictionary<Pawn, PortraitsCache.CachedPortrait> dictionary = PortraitsCache.cachedPortraits[i].CachedPortraits;
				PortraitsCache.toRemove.Clear();
				foreach (KeyValuePair<Pawn, PortraitsCache.CachedPortrait> current in dictionary)
				{
					if (current.Value.Expired)
					{
						PortraitsCache.toRemove.Add(current.Key);
						PortraitsCache.renderTexturesPool.Add(current.Value.RenderTexture);
					}
				}
				for (int j = 0; j < PortraitsCache.toRemove.Count; j++)
				{
					dictionary.Remove(PortraitsCache.toRemove[j]);
				}
				PortraitsCache.toRemove.Clear();
			}
		}

		private static void SetAnimatedPortraitsDirty()
		{
			for (int i = 0; i < PortraitsCache.cachedPortraits.Count; i++)
			{
				Dictionary<Pawn, PortraitsCache.CachedPortrait> dictionary = PortraitsCache.cachedPortraits[i].CachedPortraits;
				PortraitsCache.toSetDirty.Clear();
				foreach (KeyValuePair<Pawn, PortraitsCache.CachedPortrait> current in dictionary)
				{
					if (PortraitsCache.IsAnimated(current.Key) && !current.Value.Dirty)
					{
						PortraitsCache.toSetDirty.Add(current.Key);
					}
				}
				for (int j = 0; j < PortraitsCache.toSetDirty.Count; j++)
				{
					PortraitsCache.CachedPortrait cachedPortrait = dictionary[PortraitsCache.toSetDirty[j]];
					dictionary.Remove(PortraitsCache.toSetDirty[j]);
					dictionary.Add(PortraitsCache.toSetDirty[j], new PortraitsCache.CachedPortrait(cachedPortrait.RenderTexture, true, cachedPortrait.LastUseTime));
				}
				PortraitsCache.toSetDirty.Clear();
			}
		}

		private static RenderTexture NewRenderTexture(Vector2 size)
		{
			int num = PortraitsCache.renderTexturesPool.FindLastIndex((RenderTexture x) => x.width == (int)size.x && x.height == (int)size.y);
			if (num != -1)
			{
				RenderTexture result = PortraitsCache.renderTexturesPool[num];
				PortraitsCache.renderTexturesPool.RemoveAt(num);
				return result;
			}
			return new RenderTexture((int)size.x, (int)size.y, 24);
		}

		private static void RenderPortrait(Pawn pawn, RenderTexture renderTexture, Vector3 cameraOffset, float cameraZoom)
		{
			Find.PortraitRenderer.RenderPortrait(pawn, renderTexture, cameraOffset, cameraZoom);
		}

		private static bool IsAnimated(Pawn pawn)
		{
			return Current.ProgramState == ProgramState.Playing && pawn.Drawer.renderer.graphics.flasher.FlashingNowOrRecently;
		}
	}
}
