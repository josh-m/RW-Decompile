using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public sealed class TickManager : IExposable
	{
		private int ticksGameInt;

		public int gameStartAbsTick;

		private float realTimeToTickThrough;

		private TimeSpeed curTimeSpeed = TimeSpeed.Normal;

		public TimeSpeed prePauseTimeSpeed;

		private int startingYearInt = 5500;

		private TickList tickListNormal = new TickList(TickerType.Normal);

		private TickList tickListRare = new TickList(TickerType.Rare);

		private TickList tickListLong = new TickList(TickerType.Long);

		public TimeSlower slower = new TimeSlower();

		private int lastNothingHappeningCheckTick = -1;

		private bool nothingHappeningCached;

		public int TicksGame
		{
			get
			{
				return this.ticksGameInt;
			}
		}

		public int TicksAbs
		{
			get
			{
				if (this.gameStartAbsTick == 0)
				{
					Log.ErrorOnce("Accessing TicksAbs but gameStartAbsTick is not set yet.", 1049580013);
					return this.ticksGameInt;
				}
				return this.ticksGameInt + this.gameStartAbsTick;
			}
		}

		public int StartingYear
		{
			get
			{
				return this.startingYearInt;
			}
		}

		public float TickRateMultiplier
		{
			get
			{
				if (this.slower.ForcedNormalSpeed)
				{
					if (this.curTimeSpeed == TimeSpeed.Paused)
					{
						return 0f;
					}
					return 1f;
				}
				else
				{
					switch (this.curTimeSpeed)
					{
					case TimeSpeed.Paused:
						return 0f;
					case TimeSpeed.Normal:
						return 1f;
					case TimeSpeed.Fast:
						return 3f;
					case TimeSpeed.Superfast:
						if (Find.VisibleMap == null)
						{
							return 150f;
						}
						if (this.NothingHappeningInGame())
						{
							return 12f;
						}
						return 6f;
					case TimeSpeed.Ultrafast:
						if (Find.VisibleMap == null)
						{
							return 250f;
						}
						return 15f;
					default:
						return -1f;
					}
				}
			}
		}

		private float CurTimePerTick
		{
			get
			{
				if (this.TickRateMultiplier == 0f)
				{
					return 0f;
				}
				return 1f / (60f * this.TickRateMultiplier);
			}
		}

		public bool Paused
		{
			get
			{
				return this.curTimeSpeed == TimeSpeed.Paused || Find.WindowStack.WindowsForcePause;
			}
		}

		public bool NotPlaying
		{
			get
			{
				return Find.MainTabsRoot.OpenTab == MainTabDefOf.Menu;
			}
		}

		public TimeSpeed CurTimeSpeed
		{
			get
			{
				return this.curTimeSpeed;
			}
			set
			{
				this.curTimeSpeed = value;
			}
		}

		public void TogglePaused()
		{
			if (this.curTimeSpeed != TimeSpeed.Paused)
			{
				this.prePauseTimeSpeed = this.curTimeSpeed;
				this.curTimeSpeed = TimeSpeed.Paused;
			}
			else if (this.prePauseTimeSpeed != this.curTimeSpeed)
			{
				this.curTimeSpeed = this.prePauseTimeSpeed;
			}
			else
			{
				this.curTimeSpeed = TimeSpeed.Normal;
			}
		}

		private bool NothingHappeningInGame()
		{
			if (this.lastNothingHappeningCheckTick != this.TicksGame)
			{
				this.nothingHappeningCached = true;
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					List<Pawn> list = maps[i].mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
					for (int j = 0; j < list.Count; j++)
					{
						Pawn pawn = list[j];
						if (pawn.HostFaction == null && pawn.RaceProps.Humanlike && pawn.Awake())
						{
							this.nothingHappeningCached = false;
							break;
						}
					}
					if (!this.nothingHappeningCached)
					{
						break;
					}
				}
				if (this.nothingHappeningCached)
				{
					for (int k = 0; k < maps.Count; k++)
					{
						if (maps[k].IsPlayerHome && maps[k].dangerWatcher.DangerRating >= StoryDanger.Low)
						{
							this.nothingHappeningCached = false;
							break;
						}
					}
				}
				this.lastNothingHappeningCheckTick = this.TicksGame;
			}
			return this.nothingHappeningCached;
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.ticksGameInt, "ticksGame", 0, false);
			Scribe_Values.LookValue<int>(ref this.gameStartAbsTick, "gameStartAbsTick", 0, false);
			Scribe_Values.LookValue<int>(ref this.startingYearInt, "startingYear", 0, false);
		}

		public void RegisterAllTickabilityFor(Thing t)
		{
			TickList tickList = this.TickListFor(t);
			if (tickList != null)
			{
				tickList.RegisterThing(t);
			}
		}

		public void DeRegisterAllTickabilityFor(Thing t)
		{
			TickList tickList = this.TickListFor(t);
			if (tickList != null)
			{
				tickList.DeregisterThing(t);
			}
		}

		private TickList TickListFor(Thing t)
		{
			switch (t.def.tickerType)
			{
			case TickerType.Never:
				return null;
			case TickerType.Normal:
				return this.tickListNormal;
			case TickerType.Rare:
				return this.tickListRare;
			case TickerType.Long:
				return this.tickListLong;
			default:
				throw new InvalidOperationException();
			}
		}

		public void TickManagerUpdate()
		{
			if (!this.Paused)
			{
				this.realTimeToTickThrough += Time.deltaTime;
				int num = 0;
				while (this.realTimeToTickThrough > 0f && (float)num < this.TickRateMultiplier * 2f)
				{
					this.DoSingleTick();
					this.realTimeToTickThrough -= this.CurTimePerTick;
					num++;
				}
				if (this.realTimeToTickThrough > 0f)
				{
					this.realTimeToTickThrough = 0f;
				}
			}
		}

		public void DoSingleTick()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				maps[i].MapPreTick();
			}
			if (!DebugSettings.fastEcology)
			{
				this.ticksGameInt++;
			}
			else
			{
				this.ticksGameInt += 250;
			}
			this.tickListNormal.Tick();
			this.tickListRare.Tick();
			this.tickListLong.Tick();
			try
			{
				DateUtility.DatesTick();
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			try
			{
				Find.Scenario.TickScenario();
			}
			catch (Exception ex2)
			{
				Log.Error(ex2.ToString());
			}
			try
			{
				Find.World.WorldTick();
			}
			catch (Exception ex3)
			{
				Log.Error(ex3.ToString());
			}
			try
			{
				Find.StoryWatcher.StoryWatcherTick();
			}
			catch (Exception ex4)
			{
				Log.Error(ex4.ToString());
			}
			try
			{
				Find.GameEnder.GameEndTick();
			}
			catch (Exception ex5)
			{
				Log.Error(ex5.ToString());
			}
			try
			{
				Find.Storyteller.StorytellerTick();
			}
			catch (Exception ex6)
			{
				Log.Error(ex6.ToString());
			}
			try
			{
				Current.Game.taleManager.TaleManagerTick();
			}
			catch (Exception ex7)
			{
				Log.Error(ex7.ToString());
			}
			for (int j = 0; j < maps.Count; j++)
			{
				maps[j].MapPostTick();
			}
			try
			{
				Find.History.HistoryTick();
			}
			catch (Exception ex8)
			{
				Log.Error(ex8.ToString());
			}
			try
			{
				Find.Autosaver.AutosaverTick();
			}
			catch (Exception ex9)
			{
				Log.Error(ex9.ToString());
			}
			Debug.developerConsoleVisible = false;
		}

		public void RemoveAllFromMap(Map map)
		{
			this.tickListNormal.RemoveWhere((Thing x) => x.Map == map);
			this.tickListRare.RemoveWhere((Thing x) => x.Map == map);
			this.tickListLong.RemoveWhere((Thing x) => x.Map == map);
		}

		public void DebugSetTicksGame(int newTicksGame)
		{
			this.ticksGameInt = newTicksGame;
		}
	}
}
