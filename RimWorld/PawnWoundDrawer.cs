using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class PawnWoundDrawer
	{
		private class Wound
		{
			private List<Vector2> locsPerSide = new List<Vector2>();

			private Material mat;

			private Quaternion quat;

			private static readonly Vector2 WoundSpan = new Vector2(0.18f, 0.3f);

			public Wound(Pawn pawn)
			{
				switch (pawn.RaceProps.fleshType)
				{
				case FleshType.Normal:
					this.mat = PawnWoundDrawer.WoundOverlays_Flesh.RandomElement<Material>();
					break;
				case FleshType.Mechanoid:
					this.mat = PawnWoundDrawer.WoundOverlays_Mech.RandomElement<Material>();
					break;
				case FleshType.Insectoid:
					this.mat = PawnWoundDrawer.WoundOverlays_Insect.RandomElement<Material>();
					break;
				default:
					throw new NotImplementedException();
				}
				this.quat = Quaternion.AngleAxis((float)Rand.Range(0, 360), Vector3.up);
				for (int i = 0; i < 4; i++)
				{
					this.locsPerSide.Add(new Vector2(Rand.Value, Rand.Value));
				}
			}

			public void DrawWound(Vector3 drawLoc, Quaternion bodyQuat, Rot4 bodyRot, bool forPortrait)
			{
				Vector2 vector = this.locsPerSide[bodyRot.AsInt];
				drawLoc += new Vector3((vector.x - 0.5f) * PawnWoundDrawer.Wound.WoundSpan.x, 0f, (vector.y - 0.5f) * PawnWoundDrawer.Wound.WoundSpan.y);
				drawLoc.z -= 0.3f;
				GenDraw.DrawMeshNowOrLater(MeshPool.plane025, drawLoc, this.quat, this.mat, forPortrait);
			}
		}

		protected Pawn pawn;

		private List<PawnWoundDrawer.Wound> wounds = new List<PawnWoundDrawer.Wound>();

		private int MaxDisplayWounds = 3;

		private static readonly List<Material> WoundOverlays_Flesh = new List<Material>
		{
			MaterialPool.MatFrom("Things/Pawn/Wounds/WoundFleshA"),
			MaterialPool.MatFrom("Things/Pawn/Wounds/WoundFleshB"),
			MaterialPool.MatFrom("Things/Pawn/Wounds/WoundFleshC")
		};

		private static readonly List<Material> WoundOverlays_Mech = new List<Material>
		{
			MaterialPool.MatFrom("Things/Pawn/Wounds/WoundMechA"),
			MaterialPool.MatFrom("Things/Pawn/Wounds/WoundMechB"),
			MaterialPool.MatFrom("Things/Pawn/Wounds/WoundMechC")
		};

		private static readonly Color InsectWoundColor;

		private static readonly List<Material> WoundOverlays_Insect;

		public PawnWoundDrawer(Pawn pawn)
		{
			this.pawn = pawn;
		}

		static PawnWoundDrawer()
		{
			// Note: this type is marked as 'beforefieldinit'.
			ColorInt colorInt = new ColorInt(60, 50, 40);
			PawnWoundDrawer.InsectWoundColor = colorInt.ToColor;
			PawnWoundDrawer.WoundOverlays_Insect = new List<Material>
			{
				MaterialPool.MatFrom("Things/Pawn/Wounds/WoundA", ShaderDatabase.Cutout, PawnWoundDrawer.InsectWoundColor),
				MaterialPool.MatFrom("Things/Pawn/Wounds/WoundB", ShaderDatabase.Cutout, PawnWoundDrawer.InsectWoundColor),
				MaterialPool.MatFrom("Things/Pawn/Wounds/WoundC", ShaderDatabase.Cutout, PawnWoundDrawer.InsectWoundColor)
			};
		}

		public void RenderOverBody(Vector3 drawLoc, Mesh bodyMesh, Quaternion quat, bool forPortrait)
		{
			int num = 0;
			List<Hediff> hediffs = this.pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].def.displayWound)
				{
					Hediff_Injury hediff_Injury = hediffs[i] as Hediff_Injury;
					if (hediff_Injury == null || !hediff_Injury.IsOld())
					{
						num++;
					}
				}
			}
			int num2 = Mathf.CeilToInt((float)num / 2f);
			if (num2 > this.MaxDisplayWounds)
			{
				num2 = this.MaxDisplayWounds;
			}
			while (this.wounds.Count < num2)
			{
				this.wounds.Add(new PawnWoundDrawer.Wound(this.pawn));
				PortraitsCache.SetDirty(this.pawn);
			}
			while (this.wounds.Count > num2)
			{
				this.wounds.Remove(this.wounds.RandomElement<PawnWoundDrawer.Wound>());
				PortraitsCache.SetDirty(this.pawn);
			}
			for (int j = 0; j < this.wounds.Count; j++)
			{
				this.wounds[j].DrawWound(drawLoc, quat, this.pawn.Rotation, forPortrait);
			}
		}
	}
}
