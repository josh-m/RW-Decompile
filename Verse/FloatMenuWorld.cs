using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class FloatMenuWorld : FloatMenu
	{
		private Vector2 clickPos;

		public FloatMenuWorld(List<FloatMenuOption> options, string title, Vector2 clickPos) : base(options, title, false)
		{
			this.clickPos = clickPos;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Caravan caravan = Find.WorldSelector.SingleSelectedObject as Caravan;
			if (caravan == null)
			{
				Find.WindowStack.TryRemove(this, true);
				return;
			}
			List<FloatMenuOption> curOpts = FloatMenuMakerWorld.ChoicesAtFor(this.clickPos, caravan);
			for (int i = 0; i < this.options.Count; i++)
			{
				if (!this.options[i].Disabled && !FloatMenuWorld.StillValid(this.options[i], curOpts))
				{
					this.options[i].Disabled = true;
				}
			}
			base.DoWindowContents(inRect);
		}

		private static bool StillValid(FloatMenuOption opt, List<FloatMenuOption> curOpts)
		{
			if (opt.revalidateWorldClickTarget == null)
			{
				for (int i = 0; i < curOpts.Count; i++)
				{
					if (FloatMenuWorld.OptionsMatch(opt, curOpts[i]))
					{
						return true;
					}
				}
			}
			else
			{
				if (!opt.revalidateWorldClickTarget.Spawned)
				{
					return false;
				}
				Vector2 mousePos = opt.revalidateWorldClickTarget.ScreenPos();
				mousePos.y = (float)UI.screenHeight - mousePos.y;
				List<FloatMenuOption> list = FloatMenuMakerWorld.ChoicesAtFor(mousePos, (Caravan)Find.WorldSelector.SingleSelectedObject);
				for (int j = 0; j < list.Count; j++)
				{
					if (FloatMenuWorld.OptionsMatch(opt, list[j]))
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool OptionsMatch(FloatMenuOption a, FloatMenuOption b)
		{
			return a.Label == b.Label;
		}
	}
}
