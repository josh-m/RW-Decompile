using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Tale : IExposable, ILoadReferenceable
	{
		public TaleDef def;

		public int id;

		private int uses;

		public int date = -1;

		public TaleData_Surroundings surroundings;

		public int AgeTicks
		{
			get
			{
				return Find.TickManager.TicksAbs - this.date;
			}
		}

		public int Uses
		{
			get
			{
				return this.uses;
			}
		}

		public bool Unused
		{
			get
			{
				return this.uses == 0;
			}
		}

		public virtual Pawn DominantPawn
		{
			get
			{
				return null;
			}
		}

		public float InterestLevel
		{
			get
			{
				float num = this.def.baseInterest;
				num /= (float)(1 + this.uses * 3);
				float a = 0f;
				switch (this.def.type)
				{
				case TaleType.Volatile:
					a = 50f;
					break;
				case TaleType.Expirable:
					a = this.def.expireDays;
					break;
				case TaleType.PermanentHistorical:
					a = 50f;
					break;
				}
				float value = (float)(this.AgeTicks / 60000);
				num *= Mathf.InverseLerp(a, 0f, value);
				if (num < 0.01f)
				{
					num = 0.01f;
				}
				return num;
			}
		}

		public bool Expired
		{
			get
			{
				return this.Unused && this.def.type == TaleType.Expirable && (float)this.AgeTicks > this.def.expireDays * 60000f;
			}
		}

		public virtual string ShortSummary
		{
			get
			{
				return this.def.LabelCap;
			}
		}

		public virtual void GenerateTestData()
		{
			if (Find.VisibleMap == null)
			{
				Log.Error("Can't generate test data because there is no map.");
			}
			this.date = Rand.Range(-108000000, -7200000);
			this.surroundings = TaleData_Surroundings.GenerateRandom(Find.VisibleMap);
		}

		public virtual bool Concerns(Thing th)
		{
			return false;
		}

		public virtual void PostRemove()
		{
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.Look<TaleDef>(ref this.def, "def");
			Scribe_Values.Look<int>(ref this.id, "id", 0, false);
			Scribe_Values.Look<int>(ref this.uses, "uses", 0, false);
			Scribe_Values.Look<int>(ref this.date, "date", 0, false);
			Scribe_Deep.Look<TaleData_Surroundings>(ref this.surroundings, "surroundings", new object[0]);
		}

		public void Notify_NewlyUsed()
		{
			this.uses++;
		}

		public void Notify_ReferenceDestroyed()
		{
			if (this.uses == 0)
			{
				Log.Warning("Called reference destroyed method on tale " + this + " but uses count is 0.");
				return;
			}
			this.uses--;
		}

		[DebuggerHidden]
		public IEnumerable<Rule> GetTextGenerationRules()
		{
			if (this.def.rulePack != null)
			{
				for (int i = 0; i < this.def.rulePack.Rules.Count; i++)
				{
					yield return this.def.rulePack.Rules[i];
				}
			}
			Vector2 location = Vector2.zero;
			if (this.surroundings != null && this.surroundings.tile >= 0)
			{
				location = Find.WorldGrid.LongLatOf(this.surroundings.tile);
			}
			yield return new Rule_String("date", GenDate.DateFullStringAt((long)this.date, location));
			if (this.surroundings != null)
			{
				foreach (Rule r in this.surroundings.GetRules())
				{
					yield return r;
				}
			}
			foreach (Rule r2 in this.SpecialTextGenerationRules())
			{
				yield return r2;
			}
		}

		[DebuggerHidden]
		protected virtual IEnumerable<Rule> SpecialTextGenerationRules()
		{
		}

		public string GetUniqueLoadID()
		{
			return "Tale_" + this.id;
		}

		public override int GetHashCode()
		{
			return this.id;
		}

		public override string ToString()
		{
			string str = string.Concat(new object[]
			{
				"(#",
				this.id,
				": ",
				this.ShortSummary,
				"(age=",
				((float)this.AgeTicks / 60000f).ToString("F2"),
				" interest=",
				this.InterestLevel
			});
			if (this.Unused && this.def.type == TaleType.Expirable)
			{
				str = str + ", expireDays=" + this.def.expireDays.ToString("F2");
			}
			return str + ")";
		}
	}
}
