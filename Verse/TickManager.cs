using RimWorld;
using System;
using System.Linq;
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
						if (this.NothingHappeningInGame())
						{
							return 12f;
						}
						return 6f;
					case TimeSpeed.Ultrafast:
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
			return !Find.MapPawns.FreeColonistsSpawned.Any((Pawn p) => p.Awake()) && Find.StoryWatcher.watcherDanger.DangerRating < StoryDanger.Low;
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
			ItemAvailabilityUtility.Tick();
			ListerHaulables.ListerHaulablesTick();
			try
			{
				AutoBuildRoofZoneSetter.AutoBuildRoofZoneSetterTick_First();
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			RoofCollapseBufferResolver.CollapseRoofsMarkedToCollapse();
			WindManager.WindManagerTick();
			try
			{
				GenTemperature.GenTemperatureTick();
			}
			catch (Exception ex2)
			{
				Log.Error(ex2.ToString());
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
				Find.Scenario.TickScenario();
			}
			catch (Exception ex3)
			{
				Log.Error(ex3.ToString());
			}
			try
			{
				Find.World.WorldTick();
			}
			catch (Exception ex4)
			{
				Log.Error(ex4.ToString());
			}
			try
			{
				Find.StoryWatcher.StoryWatcherTick();
			}
			catch (Exception ex5)
			{
				Log.Error(ex5.ToString());
			}
			try
			{
				Find.GameEnder.GameEndTick();
			}
			catch (Exception ex6)
			{
				Log.Error(ex6.ToString());
			}
			try
			{
				Find.Storyteller.StorytellerTick();
			}
			catch (Exception ex7)
			{
				Log.Error(ex7.ToString());
			}
			try
			{
				WildSpawner.WildSpawnerTick();
			}
			catch (Exception ex8)
			{
				Log.Error(ex8.ToString());
			}
			try
			{
				PowerNetManager.PowerNetsTick();
			}
			catch (Exception ex9)
			{
				Log.Error(ex9.ToString());
			}
			try
			{
				SteadyAtmosphereEffects.SteadyAtmosphereEffectsTick();
			}
			catch (Exception ex10)
			{
				Log.Error(ex10.ToString());
			}
			try
			{
				Find.LordManager.LordManagerTick();
			}
			catch (Exception ex11)
			{
				Log.Error(ex11.ToString());
			}
			try
			{
				Find.PassingShipManager.PassingShipManagerTick();
			}
			catch (Exception ex12)
			{
				Log.Error(ex12.ToString());
			}
			try
			{
				Find.DebugDrawer.DebugDrawerTick();
			}
			catch (Exception ex13)
			{
				Log.Error(ex13.ToString());
			}
			try
			{
				Find.Map.autosaver.AutosaverTick();
			}
			catch (Exception ex14)
			{
				Log.Error(ex14.ToString());
			}
			try
			{
				Current.Game.taleManager.TaleManagerTick();
			}
			catch (Exception ex15)
			{
				Log.Error(ex15.ToString());
			}
			try
			{
				Find.VoluntarilyJoinableLordsStarter.VoluntarilyJoinableLordsStarterTick();
			}
			catch (Exception ex16)
			{
				Log.Error(ex16.ToString());
			}
			try
			{
				Find.MapConditionManager.MapConditionManagerTick();
			}
			catch (Exception ex17)
			{
				Log.Error(ex17.ToString());
			}
			try
			{
				Find.WeatherManager.WeatherManagerTick();
			}
			catch (Exception ex18)
			{
				Log.Error(ex18.ToString());
			}
			try
			{
				Find.ResourceCounter.ResourceCounterTick();
			}
			catch (Exception ex19)
			{
				Log.Error(ex19.ToString());
			}
			try
			{
				Find.History.HistoryTick();
			}
			catch (Exception ex20)
			{
				Log.Error(ex20.ToString());
			}
			try
			{
				DateUtility.DatesTick();
			}
			catch (Exception ex21)
			{
				Log.Error(ex21.ToString());
			}
			for (int i = 0; i < Find.Map.components.Count; i++)
			{
				try
				{
					Find.Map.components[i].MapComponentTick();
				}
				catch (Exception ex22)
				{
					Log.Error(ex22.ToString());
				}
			}
			Debug.developerConsoleVisible = false;
		}

		public void DebugSetTicksGame(int newTicksGame)
		{
			this.ticksGameInt = newTicksGame;
		}
	}
}
