using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public sealed class TaleManager : IExposable
	{
		private const int MaxUnusedVolatileTales = 350;

		private List<Tale> tales = new List<Tale>();

		public IEnumerable<Tale> AllTales
		{
			get
			{
				return this.tales;
			}
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<Tale>(ref this.tales, "tales", LookMode.Deep, new object[0]);
		}

		public void TaleManagerTick()
		{
			this.RemoveExpiredTales();
		}

		public void Add(Tale tale)
		{
			this.tales.Add(tale);
			this.CheckCullTales(tale);
		}

		private void RemoveTale(Tale tale)
		{
			if (!tale.Unused)
			{
				Log.Warning("Tried to remove used tale " + tale);
				return;
			}
			this.tales.Remove(tale);
			tale.PostRemove();
		}

		private void CheckCullTales(Tale addedTale)
		{
			this.CheckCullUnusedVolatileTales();
			this.CheckCullUnusedTalesWithMaxPerPawnLimit(addedTale);
		}

		private void CheckCullUnusedVolatileTales()
		{
			int i = 0;
			for (int j = 0; j < this.tales.Count; j++)
			{
				if (this.tales[j].def.type == TaleType.Volatile && this.tales[j].Unused)
				{
					i++;
				}
			}
			while (i > 350)
			{
				Tale tale = null;
				float num = 3.40282347E+38f;
				for (int k = 0; k < this.tales.Count; k++)
				{
					if (this.tales[k].def.type == TaleType.Volatile && this.tales[k].Unused && this.tales[k].InterestLevel < num)
					{
						tale = this.tales[k];
						num = this.tales[k].InterestLevel;
					}
				}
				this.RemoveTale(tale);
				i--;
			}
		}

		private void CheckCullUnusedTalesWithMaxPerPawnLimit(Tale addedTale)
		{
			if (addedTale.def.maxPerPawn < 0)
			{
				return;
			}
			if (addedTale.DominantPawn == null)
			{
				return;
			}
			int i = 0;
			for (int j = 0; j < this.tales.Count; j++)
			{
				if (this.tales[j].Unused && this.tales[j].def == addedTale.def && this.tales[j].DominantPawn == addedTale.DominantPawn)
				{
					i++;
				}
			}
			while (i > addedTale.def.maxPerPawn)
			{
				Tale tale = null;
				int num = -1;
				for (int k = 0; k < this.tales.Count; k++)
				{
					if (this.tales[k].Unused && this.tales[k].def == addedTale.def && this.tales[k].DominantPawn == addedTale.DominantPawn && this.tales[k].AgeTicks > num)
					{
						tale = this.tales[k];
						num = this.tales[k].AgeTicks;
					}
				}
				this.RemoveTale(tale);
				i--;
			}
		}

		private void RemoveExpiredTales()
		{
			for (int i = this.tales.Count - 1; i >= 0; i--)
			{
				if (this.tales[i].Expired)
				{
					this.RemoveTale(this.tales[i]);
				}
			}
		}

		public TaleReference GetRandomTaleReferenceForArt(ArtGenerationContext source)
		{
			if (source == ArtGenerationContext.Outsider)
			{
				return TaleReference.Taleless;
			}
			if (this.tales.Count == 0)
			{
				return TaleReference.Taleless;
			}
			if (Rand.Value < 0.25f)
			{
				return TaleReference.Taleless;
			}
			Tale tale;
			if (!(from x in this.tales
			where x.def.usableForArt
			select x).TryRandomElementByWeight((Tale ta) => ta.InterestLevel, out tale))
			{
				return TaleReference.Taleless;
			}
			tale.Notify_NewlyUsed();
			return new TaleReference(tale);
		}

		public TaleReference GetRandomTaleReferenceForArtConcerning(Thing th)
		{
			if (this.tales.Count == 0)
			{
				return TaleReference.Taleless;
			}
			Tale tale;
			if (!(from x in this.tales
			where x.def.usableForArt && x.Concerns(th)
			select x).TryRandomElementByWeight((Tale x) => x.InterestLevel, out tale))
			{
				return TaleReference.Taleless;
			}
			tale.Notify_NewlyUsed();
			return new TaleReference(tale);
		}

		public Tale GetLatestTale(TaleDef def, Pawn pawn)
		{
			Tale tale = null;
			int num = 0;
			for (int i = 0; i < this.tales.Count; i++)
			{
				if (this.tales[i].def == def && this.tales[i].DominantPawn == pawn && (tale == null || this.tales[i].AgeTicks < num))
				{
					tale = this.tales[i];
					num = this.tales[i].AgeTicks;
				}
			}
			return tale;
		}

		public void Notify_PawnDestroyed(Pawn pawn)
		{
			for (int i = this.tales.Count - 1; i >= 0; i--)
			{
				if (this.tales[i].Unused)
				{
					if (!this.tales[i].def.usableForArt)
					{
						if (this.tales[i].def.type != TaleType.PermanentHistorical)
						{
							if (this.tales[i].DominantPawn == pawn)
							{
								this.RemoveTale(this.tales[i]);
							}
						}
					}
				}
			}
		}

		public void Notify_PawnDiscarded(Pawn p)
		{
			for (int i = this.tales.Count - 1; i >= 0; i--)
			{
				if (this.tales[i].Concerns(p))
				{
					Log.Warning(string.Concat(new object[]
					{
						"Discarding pawn ",
						p,
						", but he is referenced by a tale ",
						this.tales[i],
						"."
					}));
					this.RemoveTale(this.tales[i]);
				}
			}
		}

		public bool AnyTaleConcerns(Pawn p)
		{
			for (int i = 0; i < this.tales.Count; i++)
			{
				if (this.tales[i].Concerns(p))
				{
					return true;
				}
			}
			return false;
		}

		public float GetMaxHistoricalTaleDay()
		{
			float num = 0f;
			for (int i = 0; i < this.tales.Count; i++)
			{
				Tale tale = this.tales[i];
				if (tale.def.type == TaleType.PermanentHistorical)
				{
					float num2 = (float)GenDate.TickAbsToGame(tale.date) / 60000f;
					if (num2 > num)
					{
						num = num2;
					}
				}
			}
			return num;
		}

		public void LogTales()
		{
			StringBuilder stringBuilder = new StringBuilder();
			IEnumerable<Tale> enumerable = from x in this.tales
			where !x.Unused
			select x;
			IEnumerable<Tale> enumerable2 = from x in this.tales
			where x.def.type == TaleType.Volatile && x.Unused
			select x;
			IEnumerable<Tale> enumerable3 = from x in this.tales
			where x.def.type == TaleType.PermanentHistorical && x.Unused
			select x;
			IEnumerable<Tale> enumerable4 = from x in this.tales
			where x.def.type == TaleType.Expirable && x.Unused
			select x;
			stringBuilder.AppendLine("All tales count: " + this.tales.Count);
			stringBuilder.AppendLine("Used count: " + enumerable.Count<Tale>());
			stringBuilder.AppendLine(string.Concat(new object[]
			{
				"Unused volatile count: ",
				enumerable2.Count<Tale>(),
				" (max: ",
				350,
				")"
			}));
			stringBuilder.AppendLine("Unused permanent count: " + enumerable3.Count<Tale>());
			stringBuilder.AppendLine("Unused expirable count: " + enumerable4.Count<Tale>());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("-------Used-------");
			foreach (Tale current in enumerable)
			{
				stringBuilder.AppendLine(current.ToString());
			}
			stringBuilder.AppendLine("-------Unused volatile-------");
			foreach (Tale current2 in enumerable2)
			{
				stringBuilder.AppendLine(current2.ToString());
			}
			stringBuilder.AppendLine("-------Unused permanent-------");
			foreach (Tale current3 in enumerable3)
			{
				stringBuilder.AppendLine(current3.ToString());
			}
			stringBuilder.AppendLine("-------Unused expirable-------");
			foreach (Tale current4 in enumerable4)
			{
				stringBuilder.AppendLine(current4.ToString());
			}
			Log.Message(stringBuilder.ToString());
		}

		public void LogTaleInterestSummary()
		{
			StringBuilder stringBuilder = new StringBuilder();
			float num = (from t in this.tales
			where t.def.usableForArt
			select t).Sum((Tale t) => t.InterestLevel);
			TaleDef def;
			Func<TaleDef, float> defInterest = (TaleDef def) => (from t in this.tales
			where t.def == def
			select t).Sum((Tale t) => t.InterestLevel);
			using (IEnumerator<TaleDef> enumerator = (from def in DefDatabase<TaleDef>.AllDefs
			where def.usableForArt
			orderby defInterest(def) descending
			select def).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					def = enumerator.Current;
					stringBuilder.AppendLine(string.Concat(new object[]
					{
						def.label,
						":   [",
						(from t in this.tales
						where t.def == def
						select t).Count<Tale>(),
						"]   ",
						(defInterest(def) / num).ToStringPercent("F2")
					}));
				}
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
