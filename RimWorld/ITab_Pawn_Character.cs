using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ITab_Pawn_Character : ITab
	{
		private Pawn PawnToShowInfoAbout
		{
			get
			{
				Pawn pawn = null;
				if (base.SelPawn != null)
				{
					pawn = base.SelPawn;
				}
				else
				{
					Corpse corpse = base.SelThing as Corpse;
					if (corpse != null)
					{
						pawn = corpse.InnerPawn;
					}
				}
				if (pawn == null)
				{
					Log.Error("Character tab found no selected pawn to display.");
					return null;
				}
				return pawn;
			}
		}

		public override bool IsVisible
		{
			get
			{
				return this.PawnToShowInfoAbout.story != null;
			}
		}

		public ITab_Pawn_Character()
		{
			this.size = CharacterCardUtility.PawnCardSize + new Vector2(17f, 17f) * 2f;
			this.labelKey = "TabCharacter";
			this.tutorTag = "Character";
		}

		protected override void FillTab()
		{
			Rect rect = new Rect(17f, 17f, CharacterCardUtility.PawnCardSize.x, CharacterCardUtility.PawnCardSize.y);
			CharacterCardUtility.DrawCharacterCard(rect, this.PawnToShowInfoAbout);
		}
	}
}
