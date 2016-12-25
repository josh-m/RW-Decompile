using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class ScenPart_PawnModifier : ScenPart
	{
		protected float chance = 1f;

		protected PawnGenerationContext context;

		private string chanceBuf;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<float>(ref this.chance, "chance", 0f, false);
			Scribe_Values.LookValue<PawnGenerationContext>(ref this.context, "context", PawnGenerationContext.All, false);
		}

		protected void DoPawnModifierEditInterface(Rect rect)
		{
			Rect rect2 = rect.TopHalf();
			Rect rect3 = rect2.LeftPart(0.333f).Rounded();
			Rect rect4 = rect2.RightPart(0.666f).Rounded();
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect3, "chance".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.TextFieldPercent(rect4, ref this.chance, ref this.chanceBuf, 0f, 1f);
			Rect rect5 = rect.BottomHalf();
			Rect rect6 = rect5.LeftPart(0.333f).Rounded();
			Rect rect7 = rect5.RightPart(0.666f).Rounded();
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect6, "context".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			if (Widgets.ButtonText(rect7, this.context.ToStringHuman(), true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				using (IEnumerator enumerator = Enum.GetValues(typeof(PawnGenerationContext)).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						PawnGenerationContext localCont2 = (PawnGenerationContext)((int)enumerator.Current);
						PawnGenerationContext localCont = localCont2;
						list.Add(new FloatMenuOption(localCont.ToStringHuman(), delegate
						{
							this.context = localCont;
						}, MenuOptionPriority.Default, null, null, 0f, null, null));
					}
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}

		public override void Randomize()
		{
			this.chance = GenMath.RoundedHundredth(Rand.Range(0.05f, 1f));
			this.context = PawnGenerationContextUtility.GetRandom();
		}

		public override void Notify_PawnGenerated(Pawn pawn, PawnGenerationContext context)
		{
			if (pawn.RaceProps.Humanlike && this.context.Includes(context))
			{
				this.ModifyPawn(pawn);
			}
		}

		protected abstract void ModifyPawn(Pawn p);
	}
}
