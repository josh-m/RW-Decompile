using System;
using System.Collections.Generic;

namespace Verse
{
	public class ExplosionManager : MapComponent
	{
		private List<Explosion> explosions = new List<Explosion>();

		private List<Explosion> tmpToTick = new List<Explosion>();

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.LookList<Explosion>(ref this.explosions, "explosions", LookMode.Deep, new object[0]);
		}

		public override void MapComponentTick()
		{
			base.MapComponentTick();
			this.tmpToTick.Clear();
			this.tmpToTick.AddRange(this.explosions);
			for (int i = 0; i < this.tmpToTick.Count; i++)
			{
				this.tmpToTick[i].Tick();
			}
			this.explosions.RemoveAll((Explosion x) => x.finished);
		}

		public void StartExplosion(Explosion explosion, SoundDef explosionSound)
		{
			this.explosions.Add(explosion);
			explosion.StartExplosion(explosionSound);
		}
	}
}
