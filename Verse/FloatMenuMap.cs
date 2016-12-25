using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class FloatMenuMap : FloatMenu
	{
		private Vector3 clickPos;

		public FloatMenuMap(List<FloatMenuOption> options, string title, Vector3 clickPos) : base(options, title, false)
		{
			this.clickPos = clickPos;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
			if (pawn == null)
			{
				Find.WindowStack.TryRemove(this, true);
				return;
			}
			List<FloatMenuOption> curOpts = FloatMenuMakerMap.ChoicesAtFor(this.clickPos, pawn);
			for (int i = 0; i < this.options.Count; i++)
			{
				if (!this.options[i].Disabled && !FloatMenuMap.StillValid(this.options[i], curOpts))
				{
					this.options[i].Disabled = true;
				}
			}
			base.DoWindowContents(inRect);
		}

		private static bool StillValid(FloatMenuOption opt, List<FloatMenuOption> curOpts)
		{
			if (opt.revalidateClickTarget == null)
			{
				for (int i = 0; i < curOpts.Count; i++)
				{
					if (FloatMenuMap.OptionsMatch(opt, curOpts[i]))
					{
						return true;
					}
				}
			}
			else
			{
				if (!opt.revalidateClickTarget.Spawned)
				{
					return false;
				}
				List<FloatMenuOption> list = FloatMenuMakerMap.ChoicesAtFor(opt.revalidateClickTarget.Position.ToVector3Shifted(), Find.Selector.SingleSelectedThing as Pawn);
				for (int j = 0; j < list.Count; j++)
				{
					if (FloatMenuMap.OptionsMatch(opt, list[j]))
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
