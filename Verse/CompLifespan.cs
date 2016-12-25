using RimWorld;
using System;

namespace Verse
{
	public class CompLifespan : ThingComp
	{
		public int age = -1;

		public CompProperties_Lifespan Props
		{
			get
			{
				return (CompProperties_Lifespan)this.props;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.LookValue<int>(ref this.age, "age", 0, false);
		}

		public override void CompTick()
		{
			this.age++;
			if (this.age >= this.Props.lifespanTicks)
			{
				this.parent.Destroy(DestroyMode.Vanish);
			}
		}

		public override void CompTickRare()
		{
			this.age += 250;
			if (this.age >= this.Props.lifespanTicks)
			{
				this.parent.Destroy(DestroyMode.Vanish);
			}
		}

		public override string CompInspectStringExtra()
		{
			string text = base.CompInspectStringExtra();
			string result = string.Empty;
			int num = this.Props.lifespanTicks - this.age;
			if (num > 0)
			{
				result = "LifespanExpiry".Translate() + " " + num.ToStringTicksToPeriod(true);
				if (!text.NullOrEmpty())
				{
					result = "\n" + text;
				}
			}
			return result;
		}
	}
}
