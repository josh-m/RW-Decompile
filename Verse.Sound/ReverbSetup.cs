using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.Sound
{
	public class ReverbSetup
	{
		public float dryLevel;

		public float room;

		public float roomHF;

		public float roomLF;

		public float decayTime = 1f;

		public float decayHFRatio = 0.5f;

		public float reflectionsLevel = -10000f;

		public float reflectionsDelay;

		public float reverbLevel;

		public float reverbDelay = 0.04f;

		public float hfReference = 5000f;

		public float lfReference = 250f;

		public float diffusion = 100f;

		public float density = 100f;

		public void DoEditWidgets(WidgetRow widgetRow)
		{
			if (widgetRow.ButtonText("Setup from preset...", "Set up the reverb filter from a preset.", true, false))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				using (IEnumerator enumerator = Enum.GetValues(typeof(AudioReverbPreset)).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						AudioReverbPreset audioReverbPreset = (AudioReverbPreset)((int)enumerator.Current);
						if (audioReverbPreset != AudioReverbPreset.User)
						{
							AudioReverbPreset localPreset = audioReverbPreset;
							list.Add(new FloatMenuOption(audioReverbPreset.ToString(), delegate
							{
								this.SetupAs(localPreset);
							}, MenuOptionPriority.Default, null, null, 0f, null, null));
						}
					}
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}

		public void ApplyTo(AudioReverbFilter filter)
		{
			filter.dryLevel = this.dryLevel;
			filter.room = this.room;
			filter.roomHF = this.roomHF;
			filter.roomLF = this.roomLF;
			filter.decayTime = this.decayTime;
			filter.decayHFRatio = this.decayHFRatio;
			filter.reflectionsLevel = this.reflectionsLevel;
			filter.reflectionsDelay = this.reflectionsDelay;
			filter.reverbLevel = this.reverbLevel;
			filter.reverbDelay = this.reverbDelay;
			filter.hfReference = this.hfReference;
			filter.lfReference = this.lfReference;
			filter.diffusion = this.diffusion;
			filter.density = this.density;
		}

		public static ReverbSetup Lerp(ReverbSetup A, ReverbSetup B, float t)
		{
			return new ReverbSetup
			{
				dryLevel = Mathf.Lerp(A.dryLevel, B.dryLevel, t),
				room = Mathf.Lerp(A.room, B.room, t),
				roomHF = Mathf.Lerp(A.roomHF, B.roomHF, t),
				roomLF = Mathf.Lerp(A.roomLF, B.roomLF, t),
				decayTime = Mathf.Lerp(A.decayTime, B.decayTime, t),
				decayHFRatio = Mathf.Lerp(A.decayHFRatio, B.decayHFRatio, t),
				reflectionsLevel = Mathf.Lerp(A.reflectionsLevel, B.reflectionsLevel, t),
				reflectionsDelay = Mathf.Lerp(A.reflectionsDelay, B.reflectionsDelay, t),
				reverbLevel = Mathf.Lerp(A.reverbLevel, B.reverbLevel, t),
				reverbDelay = Mathf.Lerp(A.reverbDelay, B.reverbDelay, t),
				hfReference = Mathf.Lerp(A.hfReference, B.hfReference, t),
				lfReference = Mathf.Lerp(A.lfReference, B.lfReference, t),
				diffusion = Mathf.Lerp(A.diffusion, B.diffusion, t),
				density = Mathf.Lerp(A.density, B.density, t)
			};
		}
	}
}
