using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Area_Allowed : Area
	{
		private string labelInt;

		private Color colorInt = Color.red;

		public override string Label
		{
			get
			{
				return this.labelInt;
			}
		}

		public override Color Color
		{
			get
			{
				return this.colorInt;
			}
		}

		public override bool Mutable
		{
			get
			{
				return true;
			}
		}

		public override int ListPriority
		{
			get
			{
				return 500;
			}
		}

		public Area_Allowed()
		{
		}

		public Area_Allowed(AreaManager areaManager, string label = null) : base(areaManager)
		{
			this.areaManager = areaManager;
			if (!label.NullOrEmpty())
			{
				this.labelInt = label;
			}
			else
			{
				int num = 1;
				while (true)
				{
					this.labelInt = "AreaDefaultLabel".Translate(new object[]
					{
						num
					});
					if (areaManager.GetLabeled(this.labelInt) == null)
					{
						break;
					}
					num++;
				}
			}
			this.colorInt = new Color(Rand.Value, Rand.Value, Rand.Value);
			this.colorInt = Color.Lerp(this.colorInt, Color.gray, 0.5f);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<string>(ref this.labelInt, "label", null, false);
			Scribe_Values.Look<Color>(ref this.colorInt, "color", default(Color), false);
		}

		public override bool AssignableAsAllowed()
		{
			return true;
		}

		public override void SetLabel(string label)
		{
			this.labelInt = label;
		}

		public override string GetUniqueLoadID()
		{
			return string.Concat(new object[]
			{
				"Area_",
				this.ID,
				"_Named_",
				this.labelInt
			});
		}

		public override string ToString()
		{
			return this.labelInt;
		}
	}
}
