using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public static class PawnApparelGenerator
	{
		private struct PossibleApparelSet
		{
			private const float StartingMinTemperature = 12f;

			private const float TargetMinTemperature = -40f;

			private List<ThingStuffPair> aps;

			public int Count
			{
				get
				{
					return this.aps.Count;
				}
			}

			public float TotalPrice
			{
				get
				{
					return this.aps.Sum((ThingStuffPair pa) => pa.Price);
				}
			}

			public float TotalInsulationCold
			{
				get
				{
					return this.aps.Sum((ThingStuffPair a) => a.InsulationCold);
				}
			}

			public void Reset()
			{
				if (this.aps != null)
				{
					this.aps.Clear();
				}
				else
				{
					this.aps = new List<ThingStuffPair>();
				}
			}

			public void Add(ThingStuffPair pair)
			{
				this.aps.Add(pair);
			}

			public bool PairOverlapsAnything(ThingStuffPair pair)
			{
				for (int i = 0; i < this.aps.Count; i++)
				{
					if (!ApparelUtility.CanWearTogether(this.aps[i].thing, pair.thing))
					{
						return true;
					}
				}
				return false;
			}

			public bool CoatButNoShirt()
			{
				bool flag = false;
				bool flag2 = false;
				for (int i = 0; i < this.aps.Count; i++)
				{
					if (this.aps[i].thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
					{
						for (int j = 0; j < this.aps[i].thing.apparel.layers.Count; j++)
						{
							ApparelLayer apparelLayer = this.aps[i].thing.apparel.layers[j];
							if (apparelLayer == ApparelLayer.OnSkin)
							{
								flag2 = true;
							}
							if (apparelLayer == ApparelLayer.Shell || apparelLayer == ApparelLayer.Middle)
							{
								flag = true;
							}
						}
					}
				}
				return flag && !flag2;
			}

			public bool Covers(BodyPartGroupDef bp)
			{
				for (int i = 0; i < this.aps.Count; i++)
				{
					if (this.aps[i].thing.apparel.bodyPartGroups.Contains(bp))
					{
						return true;
					}
				}
				return false;
			}

			public bool IsNaked(Gender gender)
			{
				switch (gender)
				{
				case Gender.None:
					return false;
				case Gender.Male:
					return !this.Covers(BodyPartGroupDefOf.Legs);
				case Gender.Female:
					return !this.Covers(BodyPartGroupDefOf.Legs) || !this.Covers(BodyPartGroupDefOf.Torso);
				default:
					return false;
				}
			}

			public bool SatisfiesNeededWarmth(NeededWarmth warmth)
			{
				if (warmth == NeededWarmth.Any)
				{
					return true;
				}
				if (warmth == NeededWarmth.Cool)
				{
					return true;
				}
				if (warmth == NeededWarmth.Warm)
				{
					float num = this.aps.Sum((ThingStuffPair a) => a.InsulationCold);
					return num <= -52f;
				}
				throw new NotImplementedException();
			}

			public void AddFreeWarmthAsNeeded(NeededWarmth warmth)
			{
				if (warmth == NeededWarmth.Any)
				{
					return;
				}
				if (warmth == NeededWarmth.Cool)
				{
					return;
				}
				if (DebugViewSettings.logApparelGeneration)
				{
					PawnApparelGenerator.debugSb.AppendLine();
					PawnApparelGenerator.debugSb.AppendLine("Trying to give free warm layer.");
				}
				if (!this.SatisfiesNeededWarmth(warmth))
				{
					if (DebugViewSettings.logApparelGeneration)
					{
						PawnApparelGenerator.debugSb.AppendLine("Checking to give free torso-cover at max price " + PawnApparelGenerator.freeWarmParkaMaxPrice);
					}
					Predicate<ThingStuffPair> parkaPairValidator = (ThingStuffPair pa) => pa.Price <= PawnApparelGenerator.freeWarmParkaMaxPrice && pa.InsulationCold <= -40f;
					ThingStuffPair parkaPair;
					if ((from pa in PawnApparelGenerator.potentialApparel
					where parkaPairValidator(pa)
					select pa).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality / (pa.Price * pa.Price), out parkaPair))
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							PawnApparelGenerator.debugSb.AppendLine(string.Concat(new object[]
							{
								"Giving free torso-cover: ",
								parkaPair,
								" insulation=",
								parkaPair.InsulationCold
							}));
							foreach (ThingStuffPair current in from a in this.aps
							where !ApparelUtility.CanWearTogether(a.thing, parkaPair.thing)
							select a)
							{
								PawnApparelGenerator.debugSb.AppendLine(string.Concat(new object[]
								{
									"    -replaces ",
									current.ToString(),
									" InsulationCold=",
									current.InsulationCold
								}));
							}
						}
						this.aps.RemoveAll((ThingStuffPair pa) => !ApparelUtility.CanWearTogether(pa.thing, parkaPair.thing));
						this.aps.Add(parkaPair);
					}
				}
				if (!this.SatisfiesNeededWarmth(warmth))
				{
					if (DebugViewSettings.logApparelGeneration)
					{
						PawnApparelGenerator.debugSb.AppendLine("Checking to give free hat at max price " + PawnApparelGenerator.freeWarmHatMaxPrice);
					}
					Predicate<ThingStuffPair> hatPairValidator = (ThingStuffPair pa) => pa.Price <= PawnApparelGenerator.freeWarmHatMaxPrice && pa.InsulationCold <= -7f && (pa.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) || pa.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead));
					ThingStuffPair hatPair;
					if ((from pa in PawnApparelGenerator.potentialApparel
					where hatPairValidator(pa)
					select pa).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality / (pa.Price * pa.Price), out hatPair))
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							PawnApparelGenerator.debugSb.AppendLine(string.Concat(new object[]
							{
								"Giving free hat: ",
								hatPair,
								" insulation=",
								hatPair.InsulationCold
							}));
							foreach (ThingStuffPair current2 in from a in this.aps
							where !ApparelUtility.CanWearTogether(a.thing, hatPair.thing)
							select a)
							{
								PawnApparelGenerator.debugSb.AppendLine(string.Concat(new object[]
								{
									"    -replaces ",
									current2.ToString(),
									" InsulationCold=",
									current2.InsulationCold
								}));
							}
						}
						this.aps.RemoveAll((ThingStuffPair pa) => !ApparelUtility.CanWearTogether(pa.thing, hatPair.thing));
						this.aps.Add(hatPair);
					}
				}
				if (DebugViewSettings.logApparelGeneration)
				{
					PawnApparelGenerator.debugSb.AppendLine("New TotalInsulationCold: " + this.TotalInsulationCold);
				}
			}

			public void GiveToPawn(Pawn pawn)
			{
				for (int i = 0; i < this.aps.Count; i++)
				{
					Apparel apparel = (Apparel)ThingMaker.MakeThing(this.aps[i].thing, this.aps[i].stuff);
					PawnGenerator.PostProcessGeneratedGear(apparel, pawn);
					if (ApparelUtility.HasPartsToWear(pawn, apparel.def))
					{
						pawn.apparel.Wear(apparel, false);
					}
				}
				List<Apparel> wornApparel = pawn.apparel.WornApparel;
				if (wornApparel.Count > 4)
				{
					for (int j = 0; j < wornApparel.Count; j++)
					{
						for (int k = 0; k < wornApparel.Count; k++)
						{
							if (j != k && !ApparelUtility.CanWearTogether(wornApparel[j].def, wornApparel[k].def))
							{
								Log.Error(string.Concat(new object[]
								{
									pawn,
									" generated with apparel that cannot be worn together: ",
									wornApparel[j],
									", ",
									wornApparel[k]
								}));
								return;
							}
						}
					}
				}
			}

			public override string ToString()
			{
				string str = "[";
				for (int i = 0; i < this.aps.Count; i++)
				{
					str = str + this.aps[i].ToString() + ", ";
				}
				return str + "]";
			}
		}

		private static List<ThingStuffPair> potentialApparel;

		private static float freeWarmParkaMaxPrice;

		private static float freeWarmHatMaxPrice;

		private static PawnApparelGenerator.PossibleApparelSet workingSet;

		private static StringBuilder debugSb;

		static PawnApparelGenerator()
		{
			PawnApparelGenerator.potentialApparel = new List<ThingStuffPair>();
			PawnApparelGenerator.debugSb = null;
			PawnApparelGenerator.Reset();
		}

		public static void Reset()
		{
			PawnApparelGenerator.potentialApparel = ThingStuffPair.AllWith((ThingDef td) => td.IsApparel);
			PawnApparelGenerator.freeWarmParkaMaxPrice = (float)((int)(StatDefOf.MarketValue.Worker.GetValueAbstract(ThingDefOf.Apparel_Parka, ThingDefOf.Cloth) * 1.3f));
			PawnApparelGenerator.freeWarmHatMaxPrice = (float)((int)(StatDefOf.MarketValue.Worker.GetValueAbstract(ThingDefOf.Apparel_Tuque, ThingDefOf.Cloth) * 1.3f));
		}

		public static void GenerateStartingApparelFor(Pawn pawn, PawnGenerationRequest request)
		{
			if (!pawn.RaceProps.ToolUser)
			{
				return;
			}
			if (pawn.Faction == null)
			{
				Log.Error("Cannot generate apparel for faction-less pawn " + pawn);
				return;
			}
			pawn.apparel.DestroyAll(DestroyMode.Vanish);
			PawnApparelGenerator.workingSet.Reset();
			float randomInRange = pawn.kindDef.apparelMoney.RandomInRange;
			NeededWarmth neededWarmth = PawnApparelGenerator.ApparelWarmthNeededNow(pawn, request);
			bool flag = Rand.Value < pawn.kindDef.apparelAllowHeadwearChance;
			PawnApparelGenerator.debugSb = null;
			if (DebugViewSettings.logApparelGeneration)
			{
				PawnApparelGenerator.debugSb = new StringBuilder();
				PawnApparelGenerator.debugSb.AppendLine("Generating apparel for " + pawn);
				PawnApparelGenerator.debugSb.AppendLine("Money: " + randomInRange.ToString("F0"));
				PawnApparelGenerator.debugSb.AppendLine("Needed warmth: " + neededWarmth);
				PawnApparelGenerator.debugSb.AppendLine("Headwear allowed: " + flag);
			}
			if (randomInRange >= 0.001f)
			{
				int num = 0;
				while (true)
				{
					PawnApparelGenerator.GenerateWorkingPossibleApparelSetFor(pawn, randomInRange, flag);
					if (DebugViewSettings.logApparelGeneration)
					{
						PawnApparelGenerator.debugSb.Append(num.ToString().PadRight(5) + "Trying: " + PawnApparelGenerator.workingSet.ToString());
					}
					if (num >= 10 || Rand.Value >= 0.85f)
					{
						goto IL_1EA;
					}
					float num2 = Rand.Range(0.45f, 0.8f);
					float totalPrice = PawnApparelGenerator.workingSet.TotalPrice;
					if (totalPrice >= randomInRange * num2)
					{
						goto IL_1EA;
					}
					if (DebugViewSettings.logApparelGeneration)
					{
						PawnApparelGenerator.debugSb.AppendLine(string.Concat(new string[]
						{
							" -- Failed: Spent $",
							totalPrice.ToString("F0"),
							", < ",
							(num2 * 100f).ToString("F0"),
							"% of money."
						}));
					}
					IL_34E:
					num++;
					continue;
					IL_1EA:
					if (num < 20 && Rand.Value < 0.97f && !PawnApparelGenerator.workingSet.Covers(BodyPartGroupDefOf.Torso))
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							PawnApparelGenerator.debugSb.AppendLine(" -- Failed: Does not cover torso.");
						}
						goto IL_34E;
					}
					if (num < 30 && Rand.Value < 0.8f && PawnApparelGenerator.workingSet.CoatButNoShirt())
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							PawnApparelGenerator.debugSb.AppendLine(" -- Failed: Coat but no shirt.");
						}
						goto IL_34E;
					}
					if (num < 50 && !PawnApparelGenerator.workingSet.SatisfiesNeededWarmth(neededWarmth))
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							PawnApparelGenerator.debugSb.AppendLine(" -- Failed: Wrong warmth.");
						}
						goto IL_34E;
					}
					if (num < 80 && PawnApparelGenerator.workingSet.IsNaked(pawn.gender))
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							PawnApparelGenerator.debugSb.AppendLine(" -- Failed: Naked.");
						}
						goto IL_34E;
					}
					break;
				}
				if (DebugViewSettings.logApparelGeneration)
				{
					PawnApparelGenerator.debugSb.Append(string.Concat(new object[]
					{
						" -- Approved! Total price: $",
						PawnApparelGenerator.workingSet.TotalPrice.ToString("F0"),
						", TotalInsulationCold: ",
						PawnApparelGenerator.workingSet.TotalInsulationCold
					}));
				}
			}
			if ((!pawn.kindDef.apparelIgnoreSeasons || request.ForceAddFreeWarmLayerIfNeeded) && !PawnApparelGenerator.workingSet.SatisfiesNeededWarmth(neededWarmth))
			{
				PawnApparelGenerator.workingSet.AddFreeWarmthAsNeeded(neededWarmth);
			}
			if (DebugViewSettings.logApparelGeneration)
			{
				Log.Message(PawnApparelGenerator.debugSb.ToString());
			}
			PawnApparelGenerator.workingSet.GiveToPawn(pawn);
		}

		private static void GenerateWorkingPossibleApparelSetFor(Pawn pawn, float money, bool headwearAllowed)
		{
			PawnApparelGenerator.workingSet.Reset();
			List<ThingDef> reqApparel = pawn.kindDef.apparelRequired;
			if (reqApparel != null)
			{
				int i;
				for (i = 0; i < reqApparel.Count; i++)
				{
					ThingStuffPair pair = (from pa in PawnApparelGenerator.potentialApparel
					where pa.thing == reqApparel[i]
					select pa).RandomElementByWeight((ThingStuffPair pa) => pa.Commonality);
					PawnApparelGenerator.workingSet.Add(pair);
					money -= pair.Price;
				}
			}
			while (true)
			{
				if (Rand.Value < 0.1f)
				{
					break;
				}
				Predicate<ThingStuffPair> pairValidator = delegate(ThingStuffPair pa)
				{
					if (pa.Price > money)
					{
						return false;
					}
					if (!headwearAllowed && (pa.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) || pa.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead)))
					{
						return false;
					}
					if (pa.stuff != null && !pawn.Faction.def.CanUseStuffForApparel(pa.stuff))
					{
						return false;
					}
					if (PawnApparelGenerator.workingSet.PairOverlapsAnything(pa))
					{
						return false;
					}
					if (!pawn.kindDef.apparelTags.NullOrEmpty<string>())
					{
						bool flag = false;
						for (int i = 0; i < pawn.kindDef.apparelTags.Count; i++)
						{
							for (int j = 0; j < pa.thing.apparel.tags.Count; j++)
							{
								if (pawn.kindDef.apparelTags[i] == pa.thing.apparel.tags[j])
								{
									flag = true;
									break;
								}
							}
							if (flag)
							{
								break;
							}
						}
						if (!flag)
						{
							return false;
						}
					}
					return true;
				};
				IEnumerable<ThingStuffPair> source = from pa in PawnApparelGenerator.potentialApparel
				where pairValidator(pa)
				select pa;
				if (!source.Any<ThingStuffPair>())
				{
					break;
				}
				ThingStuffPair pair2 = source.RandomElementByWeight((ThingStuffPair pa) => pa.Commonality);
				PawnApparelGenerator.workingSet.Add(pair2);
				money -= pair2.Price;
			}
		}

		private static NeededWarmth ApparelWarmthNeededNow(Pawn pawn, PawnGenerationRequest request)
		{
			if (Current.ProgramState != ProgramState.MapPlaying)
			{
				return NeededWarmth.Any;
			}
			NeededWarmth neededWarmth = NeededWarmth.Any;
			Month month = GenDate.CurrentMonth;
			for (int i = 0; i < 2; i++)
			{
				NeededWarmth neededWarmth2 = PawnApparelGenerator.CalculateNeededWarmth(pawn, month);
				if (neededWarmth2 != NeededWarmth.Any)
				{
					neededWarmth = neededWarmth2;
					break;
				}
				if (month == Month.Dec)
				{
					month = Month.Jan;
				}
				else
				{
					month += 1;
				}
			}
			if (!pawn.kindDef.apparelIgnoreSeasons)
			{
				return neededWarmth;
			}
			if (request.ForceAddFreeWarmLayerIfNeeded && neededWarmth == NeededWarmth.Warm)
			{
				return neededWarmth;
			}
			return NeededWarmth.Any;
		}

		public static NeededWarmth CalculateNeededWarmth(Pawn pawn, Month month)
		{
			float num = GenTemperature.AverageTemperatureAtWorldCoordsForMonth(Find.Map.WorldCoords, month);
			if (num < pawn.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null) - 4f)
			{
				return NeededWarmth.Warm;
			}
			if (num > pawn.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null) + 4f)
			{
				return NeededWarmth.Cool;
			}
			return NeededWarmth.Any;
		}

		internal static void LogGenerationData()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("All potential apparel:");
			foreach (ThingStuffPair current in PawnApparelGenerator.potentialApparel)
			{
				stringBuilder.AppendLine(current.ToString());
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
