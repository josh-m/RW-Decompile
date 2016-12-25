using System;
using UnityEngine;

namespace Verse
{
	public class CompColorable : ThingComp
	{
		private Color color = Color.white;

		private bool active;

		public Color Color
		{
			get
			{
				if (!this.active)
				{
					return this.parent.def.graphicData.color;
				}
				return this.color;
			}
			set
			{
				if (value == this.color)
				{
					return;
				}
				this.active = true;
				this.color = value;
				this.parent.Notify_ColorChanged();
			}
		}

		public bool Active
		{
			get
			{
				return this.active;
			}
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			if (this.parent.def.colorGenerator != null && (this.parent.Stuff == null || this.parent.Stuff.stuffProps.allowColorGenerators))
			{
				this.Color = this.parent.def.colorGenerator.NewRandomizedColor();
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			if (Scribe.mode == LoadSaveMode.Saving && !this.active)
			{
				return;
			}
			Scribe_Values.LookValue<Color>(ref this.color, "color", default(Color), false);
			Scribe_Values.LookValue<bool>(ref this.active, "colorActive", false, false);
		}

		public override void PostSplitOff(Thing piece)
		{
			base.PostSplitOff(piece);
			if (this.active)
			{
				piece.SetColor(this.color, true);
			}
		}
	}
}
