using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;

namespace Verse
{
	public class CameraDriver : MonoBehaviour
	{
		public const float MaxDeltaTime = 0.025f;

		private const float ScreenDollyEdgeWidth = 20f;

		private const float ScreenDollyEdgeWidth_BottomFullscreen = 6f;

		private const float MinDurationForMouseToTouchScreenBottomEdgeToDolly = 0.28f;

		private const float MapEdgeClampMarginCells = -2f;

		public const float StartingSize = 24f;

		private const float MinSize = 11f;

		private const float MaxSize = 60f;

		private const float ZoomSpeed = 2.6f;

		private const float ZoomTightness = 0.4f;

		private const float ZoomScaleFromAltDenominator = 35f;

		private const float PageKeyZoomRate = 4f;

		private const float ScrollWheelZoomRate = 0.35f;

		public const float MinAltitude = 15f;

		private const float MaxAltitude = 65f;

		private const float ReverbDummyAltitude = 65f;

		public CameraShaker shaker = new CameraShaker();

		private Camera cachedCamera;

		private GameObject reverbDummy;

		public CameraMapConfig config = new CameraMapConfig_Normal();

		private Vector3 velocity;

		private Vector3 rootPos;

		private float rootSize;

		private float desiredSize;

		private Vector2 desiredDolly = Vector2.zero;

		private Vector2 mouseDragVect = Vector2.zero;

		private bool mouseCoveredByUI;

		private float mouseTouchingScreenBottomEdgeStartTime = -1f;

		private static int lastViewRectGetFrame = -1;

		private static CellRect lastViewRect;

		private Camera MyCamera
		{
			get
			{
				if (this.cachedCamera == null)
				{
					this.cachedCamera = base.GetComponent<Camera>();
				}
				return this.cachedCamera;
			}
		}

		private float ScreenDollyEdgeWidthBottom
		{
			get
			{
				if (Screen.fullScreen)
				{
					return 6f;
				}
				return 20f;
			}
		}

		public CameraZoomRange CurrentZoom
		{
			get
			{
				if (this.rootSize < 12f)
				{
					return CameraZoomRange.Closest;
				}
				if (this.rootSize < 13.8f)
				{
					return CameraZoomRange.Close;
				}
				if (this.rootSize < 42f)
				{
					return CameraZoomRange.Middle;
				}
				if (this.rootSize < 57f)
				{
					return CameraZoomRange.Far;
				}
				return CameraZoomRange.Furthest;
			}
		}

		private Vector3 CurrentRealPosition
		{
			get
			{
				return this.MyCamera.transform.position;
			}
		}

		public IntVec3 MapPosition
		{
			get
			{
				IntVec3 result = this.CurrentRealPosition.ToIntVec3();
				result.y = 0;
				return result;
			}
		}

		public CellRect CurrentViewRect
		{
			get
			{
				if (Time.frameCount != CameraDriver.lastViewRectGetFrame)
				{
					CameraDriver.lastViewRect = default(CellRect);
					float num = (float)UI.screenWidth / (float)UI.screenHeight;
					CameraDriver.lastViewRect.minX = Mathf.FloorToInt(this.CurrentRealPosition.x - this.rootSize * num - 1f);
					CameraDriver.lastViewRect.maxX = Mathf.CeilToInt(this.CurrentRealPosition.x + this.rootSize * num);
					CameraDriver.lastViewRect.minZ = Mathf.FloorToInt(this.CurrentRealPosition.z - this.rootSize - 1f);
					CameraDriver.lastViewRect.maxZ = Mathf.CeilToInt(this.CurrentRealPosition.z + this.rootSize);
					CameraDriver.lastViewRectGetFrame = Time.frameCount;
				}
				return CameraDriver.lastViewRect;
			}
		}

		public static float HitchReduceFactor
		{
			get
			{
				float result = 1f;
				if (Time.deltaTime > 0.025f)
				{
					result = 0.025f / Time.deltaTime;
				}
				return result;
			}
		}

		public float CellSizePixels
		{
			get
			{
				return (float)UI.screenHeight / (this.rootSize * 2f);
			}
		}

		public void Awake()
		{
			this.ResetSize();
			this.reverbDummy = GameObject.Find("ReverbZoneDummy");
			this.ApplyPositionToGameObject();
			this.MyCamera.farClipPlane = 71.5f;
		}

		public void OnPreCull()
		{
			if (LongEventHandler.ShouldWaitForEvent)
			{
				return;
			}
			if (Find.VisibleMap == null)
			{
				return;
			}
			if (!WorldRendererUtility.WorldRenderedNow)
			{
				Find.VisibleMap.weatherManager.DrawAllWeather();
			}
		}

		public void OnGUI()
		{
			if (LongEventHandler.ShouldWaitForEvent)
			{
				return;
			}
			if (Find.VisibleMap == null)
			{
				return;
			}
			this.mouseCoveredByUI = false;
			if (Find.WindowStack.GetWindowAt(UI.MousePositionOnUIInverted) != null)
			{
				this.mouseCoveredByUI = true;
			}
			if (!Find.WindowStack.WindowsPreventCameraMotion)
			{
				if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
				{
					this.mouseDragVect = Event.current.delta;
					Event.current.Use();
				}
				float num = 0f;
				if (Event.current.type == EventType.ScrollWheel)
				{
					num -= Event.current.delta.y * 0.35f;
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraZoom, KnowledgeAmount.TinyInteraction);
				}
				if (KeyBindingDefOf.MapZoomIn.KeyDownEvent)
				{
					num += 4f;
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraZoom, KnowledgeAmount.SmallInteraction);
				}
				if (KeyBindingDefOf.MapZoomOut.KeyDownEvent)
				{
					num -= 4f;
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraZoom, KnowledgeAmount.SmallInteraction);
				}
				this.desiredSize -= num * 2.6f * this.rootSize / 35f;
				if (this.desiredSize < 11f)
				{
					this.desiredSize = 11f;
				}
				if (this.desiredSize > 60f)
				{
					this.desiredSize = 60f;
				}
				this.desiredDolly = Vector3.zero;
				if (KeyBindingDefOf.MapDollyLeft.IsDown)
				{
					this.desiredDolly.x = -this.config.dollyRateKeys;
				}
				if (KeyBindingDefOf.MapDollyRight.IsDown)
				{
					this.desiredDolly.x = this.config.dollyRateKeys;
				}
				if (KeyBindingDefOf.MapDollyUp.IsDown)
				{
					this.desiredDolly.y = this.config.dollyRateKeys;
				}
				if (KeyBindingDefOf.MapDollyDown.IsDown)
				{
					this.desiredDolly.y = -this.config.dollyRateKeys;
				}
				if (this.mouseDragVect != Vector2.zero)
				{
					this.mouseDragVect *= CameraDriver.HitchReduceFactor;
					this.mouseDragVect.x = this.mouseDragVect.x * -1f;
					this.desiredDolly += this.mouseDragVect * this.config.dollyRateMouseDrag;
					this.mouseDragVect = Vector2.zero;
				}
			}
		}

		public void Update()
		{
			if (LongEventHandler.ShouldWaitForEvent)
			{
				return;
			}
			if (Find.VisibleMap == null)
			{
				return;
			}
			Vector2 lhs = this.CalculateCurInputDollyVect();
			if (lhs != Vector2.zero)
			{
				float d = (this.rootSize - 11f) / 49f * 0.7f + 0.3f;
				this.velocity = new Vector3(lhs.x, 0f, lhs.y) * d;
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.CameraDolly, KnowledgeAmount.FrameInteraction);
			}
			if (!Find.WindowStack.WindowsPreventCameraMotion)
			{
				float d2 = Time.deltaTime * CameraDriver.HitchReduceFactor;
				this.rootPos += this.velocity * d2 * this.config.moveSpeedScale;
				if (this.rootPos.x > (float)Find.VisibleMap.Size.x + -2f)
				{
					this.rootPos.x = (float)Find.VisibleMap.Size.x + -2f;
				}
				if (this.rootPos.z > (float)Find.VisibleMap.Size.z + -2f)
				{
					this.rootPos.z = (float)Find.VisibleMap.Size.z + -2f;
				}
				if (this.rootPos.x < 2f)
				{
					this.rootPos.x = 2f;
				}
				if (this.rootPos.z < 2f)
				{
					this.rootPos.z = 2f;
				}
			}
			if (this.velocity != Vector3.zero)
			{
				this.velocity *= this.config.camSpeedDecayFactor;
				if (this.velocity.magnitude < 0.1f)
				{
					this.velocity = Vector3.zero;
				}
			}
			float num = this.desiredSize - this.rootSize;
			this.rootSize += num * 0.4f;
			this.shaker.Update();
			this.ApplyPositionToGameObject();
			if (Find.VisibleMap != null)
			{
				RememberedCameraPos rememberedCameraPos = Find.VisibleMap.rememberedCameraPos;
				rememberedCameraPos.rootPos = this.rootPos;
				rememberedCameraPos.rootSize = this.rootSize;
			}
		}

		private void ApplyPositionToGameObject()
		{
			this.rootPos.y = 15f + (this.rootSize - 11f) / 49f * 50f;
			this.MyCamera.orthographicSize = this.rootSize;
			this.MyCamera.transform.position = this.rootPos + this.shaker.ShakeOffset;
			Vector3 position = base.transform.position;
			position.y = 65f;
			this.reverbDummy.transform.position = position;
		}

		private Vector2 CalculateCurInputDollyVect()
		{
			Vector2 vector = this.desiredDolly;
			bool flag = false;
			if ((UnityData.isEditor || Screen.fullScreen) && Prefs.EdgeScreenScroll && !this.mouseCoveredByUI)
			{
				Vector2 mousePositionOnUI = UI.MousePositionOnUI;
				Vector2 point = mousePositionOnUI;
				point.y = (float)UI.screenHeight - point.y;
				Rect rect = new Rect(0f, 0f, 200f, 200f);
				Rect rect2 = new Rect((float)(UI.screenWidth - 250), 0f, 255f, 255f);
				Rect rect3 = new Rect(0f, (float)(UI.screenHeight - 250), 225f, 255f);
				Rect rect4 = new Rect((float)(UI.screenWidth - 250), (float)(UI.screenHeight - 250), 255f, 255f);
				MainTabWindow_Inspect mainTabWindow_Inspect = (MainTabWindow_Inspect)MainTabDefOf.Inspect.Window;
				if (Find.MainTabsRoot.OpenTab == MainTabDefOf.Inspect && mainTabWindow_Inspect.RecentHeight > rect3.height)
				{
					rect3.yMin = (float)UI.screenHeight - mainTabWindow_Inspect.RecentHeight;
				}
				if (!rect.Contains(point) && !rect3.Contains(point) && !rect2.Contains(point) && !rect4.Contains(point))
				{
					Vector2 b = new Vector2(0f, 0f);
					if (mousePositionOnUI.x >= 0f && mousePositionOnUI.x < 20f)
					{
						b.x -= this.config.dollyRateScreenEdge;
					}
					if (mousePositionOnUI.x <= (float)UI.screenWidth && mousePositionOnUI.x > (float)UI.screenWidth - 20f)
					{
						b.x += this.config.dollyRateScreenEdge;
					}
					if (mousePositionOnUI.y <= (float)UI.screenHeight && mousePositionOnUI.y > (float)UI.screenHeight - 20f)
					{
						b.y += this.config.dollyRateScreenEdge;
					}
					if (mousePositionOnUI.y >= 0f && mousePositionOnUI.y < this.ScreenDollyEdgeWidthBottom)
					{
						if (this.mouseTouchingScreenBottomEdgeStartTime < 0f)
						{
							this.mouseTouchingScreenBottomEdgeStartTime = Time.realtimeSinceStartup;
						}
						if (Time.realtimeSinceStartup - this.mouseTouchingScreenBottomEdgeStartTime >= 0.28f)
						{
							b.y -= this.config.dollyRateScreenEdge;
						}
						flag = true;
					}
					vector += b;
				}
			}
			if (!flag)
			{
				this.mouseTouchingScreenBottomEdgeStartTime = -1f;
			}
			if (Input.GetKey(KeyCode.LeftShift))
			{
				vector *= 2.4f;
			}
			return vector;
		}

		public void Expose()
		{
			Scribe.EnterNode("cameraMap");
			Scribe_Values.LookValue<Vector3>(ref this.rootPos, "camRootPos", default(Vector3), false);
			Scribe_Values.LookValue<float>(ref this.desiredSize, "desiredSize", 0f, false);
			this.rootSize = this.desiredSize;
			Scribe.ExitNode();
		}

		public void ResetSize()
		{
			this.desiredSize = 24f;
			this.rootSize = this.desiredSize;
		}

		public void JumpTo(Vector3 newLookAt)
		{
			this.rootPos = new Vector3(newLookAt.x, this.rootPos.y, newLookAt.z);
		}

		public void JumpTo(IntVec3 IntLoc)
		{
			this.JumpTo(IntLoc.ToVector3Shifted());
		}

		public void SetRootPosAndSize(Vector3 rootPos, float rootSize)
		{
			this.rootPos = rootPos;
			this.rootSize = rootSize;
			this.desiredDolly = Vector2.zero;
			this.desiredSize = rootSize;
			LongEventHandler.ExecuteWhenFinished(new Action(this.ApplyPositionToGameObject));
		}
	}
}
