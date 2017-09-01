using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class StatDef : Def
	{
		public StatCategoryDef category;

		public Type workerClass = typeof(StatWorker);

		public float hideAtValue = -2.14748365E+09f;

		public bool showNonAbstract = true;

		public bool showIfUndefined = true;

		public bool showOnPawns = true;

		public bool showOnHumanlikes = true;

		public bool showOnAnimals = true;

		public bool showOnMechanoids = true;

		public int displayPriorityInCategory;

		public ToStringNumberSense toStringNumberSense = ToStringNumberSense.Absolute;

		public ToStringStyle toStringStyle;

		public string formatString;

		public float defaultBaseValue = 1f;

		public float minValue;

		public float maxValue = 9999999f;

		public bool roundValue;

		public float roundToFiveOver = 3.40282347E+38f;

		public List<StatDef> statFactors;

		public bool applyFactorsIfNegative = true;

		public float noSkillFactor = 1f;

		public List<SkillNeed> skillNeedFactors;

		public List<PawnCapacityFactor> capacityFactors;

		public SimpleCurve postProcessCurve;

		public List<StatPart> parts;

		private StatWorker workerInt;

		public StatWorker Worker
		{
			get
			{
				if (this.workerInt == null)
				{
					if (this.parts != null)
					{
						for (int i = 0; i < this.parts.Count; i++)
						{
							this.parts[i].parentStat = this;
						}
					}
					this.workerInt = (StatWorker)Activator.CreateInstance(this.workerClass);
					this.workerInt.InitSetStat(this);
				}
				return this.workerInt;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string err in base.ConfigErrors())
			{
				yield return err;
			}
			if (this.capacityFactors != null)
			{
				foreach (PawnCapacityFactor afac in this.capacityFactors)
				{
					if (afac.weight > 1f)
					{
						yield return this.defName + " has activity factor with weight > 1";
					}
				}
			}
			if (this.parts != null)
			{
				for (int i = 0; i < this.parts.Count; i++)
				{
					foreach (string err2 in this.parts[i].ConfigErrors())
					{
						yield return string.Concat(new string[]
						{
							this.defName,
							" has error in StatPart ",
							this.parts[i].ToString(),
							": ",
							err2
						});
					}
				}
			}
		}

		public string ValueToString(float val, ToStringNumberSense numberSense = ToStringNumberSense.Absolute)
		{
			string text = val.ToStringByStyle(this.toStringStyle, numberSense);
			if (!this.formatString.NullOrEmpty())
			{
				text = string.Format(this.formatString, text);
			}
			return text;
		}

		public static StatDef Named(string defName)
		{
			return DefDatabase<StatDef>.GetNamed(defName, true);
		}
	}
}
