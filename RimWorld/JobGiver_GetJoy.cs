using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_GetJoy : ThinkNode_JobGiver
	{
		private DefMap<JoyGiverDef, float> joyGiverChances;

		protected virtual bool CanDoDuringMedicalRest
		{
			get
			{
				return false;
			}
		}

		protected virtual bool JoyGiverAllowed(JoyGiverDef def)
		{
			return true;
		}

		protected virtual Job TryGiveJobFromJoyGiverDefDirect(JoyGiverDef def, Pawn pawn)
		{
			return def.Worker.TryGiveJob(pawn);
		}

		public override void ResolveReferences()
		{
			this.joyGiverChances = new DefMap<JoyGiverDef, float>();
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!this.CanDoDuringMedicalRest && pawn.InBed() && HealthAIUtility.ShouldSeekMedicalRest(pawn))
			{
				return null;
			}
			List<JoyGiverDef> allDefsListForReading = DefDatabase<JoyGiverDef>.AllDefsListForReading;
			JoyToleranceSet tolerances = pawn.needs.joy.tolerances;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				JoyGiverDef joyGiverDef = allDefsListForReading[i];
				this.joyGiverChances[joyGiverDef] = 0f;
				if (this.JoyGiverAllowed(joyGiverDef))
				{
					if (joyGiverDef.Worker.MissingRequiredCapacity(pawn) == null)
					{
						if (joyGiverDef.pctPawnsEverDo < 1f)
						{
							Rand.PushSeed();
							Rand.Seed = (pawn.thingIDNumber ^ 63216713);
							if (Rand.Value >= joyGiverDef.pctPawnsEverDo)
							{
								Rand.PopSeed();
								goto IL_100;
							}
							Rand.PopSeed();
						}
						float num = joyGiverDef.Worker.GetChance(pawn);
						float num2 = 1f - tolerances[joyGiverDef.joyKind];
						num *= num2 * num2;
						this.joyGiverChances[joyGiverDef] = num;
					}
				}
				IL_100:;
			}
			for (int j = 0; j < this.joyGiverChances.Count; j++)
			{
				JoyGiverDef def;
				if (!allDefsListForReading.TryRandomElementByWeight((JoyGiverDef d) => this.joyGiverChances[d], out def))
				{
					break;
				}
				Job job = this.TryGiveJobFromJoyGiverDefDirect(def, pawn);
				if (job != null)
				{
					return job;
				}
				this.joyGiverChances[def] = 0f;
			}
			return null;
		}
	}
}
