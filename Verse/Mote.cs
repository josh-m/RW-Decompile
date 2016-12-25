using System;
using UnityEngine;

namespace Verse
{
	public abstract class Mote : Thing
	{
		protected const float MinSpeed = 0.02f;

		public Vector3 exactPosition;

		public float exactRotation;

		public Vector3 exactScale = new Vector3(1f, 1f, 1f);

		public float rotationRate;

		public Color instanceColor = Color.white;

		private int lastMaintainTick;

		public int spawnTick;

		public float spawnRealTime;

		public MoteAttachLink link1 = MoteAttachLink.Invalid;

		protected float skidSpeedMultiplierPerTick = Rand.Range(0.3f, 0.95f);

		public float Scale
		{
			set
			{
				this.exactScale = new Vector3(value, 1f, value);
			}
		}

		public float AgeSecs
		{
			get
			{
				if (this.def.mote.realTime)
				{
					return Time.realtimeSinceStartup - this.spawnRealTime;
				}
				return (float)(Find.TickManager.TicksGame - this.spawnTick) / 60f;
			}
		}

		public override Vector3 DrawPos
		{
			get
			{
				return this.exactPosition;
			}
		}

		protected virtual float LifespanSecs
		{
			get
			{
				return this.def.mote.fadeInTime + this.def.mote.solidTime + this.def.mote.fadeOutTime;
			}
		}

		public override void SpawnSetup(Map map)
		{
			base.SpawnSetup(map);
			this.spawnTick = Find.TickManager.TicksGame;
			this.spawnRealTime = Time.realtimeSinceStartup;
			RealTime.moteList.MoteSpawned(this);
			base.Map.moteCounter.Notify_MoteSpawned();
			this.exactPosition.y = Altitudes.AltitudeFor(this.def.altitudeLayer);
		}

		public override void DeSpawn()
		{
			Map map = base.Map;
			base.DeSpawn();
			RealTime.moteList.MoteDespawned(this);
			map.moteCounter.Notify_MoteDespawned();
		}

		public override void Tick()
		{
			if (!this.def.mote.realTime)
			{
				this.TimeInterval(0.0166666675f);
			}
		}

		public void RealtimeUpdate()
		{
			if (this.def.mote.realTime)
			{
				this.TimeInterval(Time.deltaTime);
			}
		}

		protected virtual void TimeInterval(float deltaTime)
		{
			if (this.AgeSecs >= this.LifespanSecs && !base.Destroyed)
			{
				this.Destroy(DestroyMode.Vanish);
				return;
			}
			if (this.def.mote.needsMaintenance && Find.TickManager.TicksGame - 1 > this.lastMaintainTick)
			{
				this.Destroy(DestroyMode.Vanish);
				return;
			}
			if (this.def.mote.growthRate > 0f)
			{
				this.exactScale = new Vector3(this.exactScale.x + this.def.mote.growthRate * deltaTime, this.exactScale.y, this.exactScale.z + this.def.mote.growthRate * deltaTime);
			}
		}

		public override void Draw()
		{
			this.exactPosition.y = Altitudes.AltitudeFor(this.def.altitudeLayer);
			base.Draw();
		}

		public void Maintain()
		{
			this.lastMaintainTick = Find.TickManager.TicksGame;
		}

		public void Attach(TargetInfo a)
		{
			this.link1 = new MoteAttachLink(a);
		}
	}
}
