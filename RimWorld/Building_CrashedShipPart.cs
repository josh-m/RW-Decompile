using System;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Building_CrashedShipPart : Building
	{
		protected int age;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.age, "age", 0, false);
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (stringBuilder.Length != 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append("AwokeDaysAgo".Translate(this.age.TicksToDays().ToString("F1")));
			return stringBuilder.ToString();
		}

		public override void Tick()
		{
			base.Tick();
			this.age++;
		}
	}
}
