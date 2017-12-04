using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.Grammar;

namespace Verse
{
	public static class PlayLogEntryUtility
	{
		[DebuggerHidden]
		public static IEnumerable<Rule> RulesForOptionalWeapon(string prefix, ThingDef weaponDef, ThingDef projectileDef)
		{
			if (weaponDef != null)
			{
				foreach (Rule rule in GrammarUtility.RulesForDef(prefix, weaponDef))
				{
					yield return rule;
				}
				ThingDef projectile = projectileDef;
				if (projectile == null && !weaponDef.Verbs.NullOrEmpty<VerbProperties>())
				{
					projectile = weaponDef.Verbs[0].defaultProjectile;
				}
				if (projectile != null)
				{
					foreach (Rule rule2 in GrammarUtility.RulesForDef(prefix + "_projectile", projectile))
					{
						yield return rule2;
					}
				}
			}
		}

		[DebuggerHidden]
		public static IEnumerable<Rule> RulesForDamagedParts(string prefix, List<BodyPartDef> bodyParts, List<bool> bodyPartsDestroyed, Dictionary<string, string> constants)
		{
			if (bodyParts != null)
			{
				int destroyedIndex = 0;
				int damagedIndex = 0;
				for (int i = 0; i < bodyParts.Count; i++)
				{
					yield return new Rule_String(string.Format(prefix + "{0}_label", i), bodyParts[i].label);
					constants[string.Format(prefix + "{0}_destroyed", i)] = bodyPartsDestroyed[i].ToString();
					if (bodyPartsDestroyed[i])
					{
						string arg_12A_0 = prefix + "_destroyed{0}_label";
						int num;
						destroyedIndex = (num = destroyedIndex) + 1;
						yield return new Rule_String(string.Format(arg_12A_0, num), bodyParts[i].label);
					}
					else
					{
						string arg_18F_0 = prefix + "_damaged{0}_label";
						int num;
						damagedIndex = (num = damagedIndex) + 1;
						yield return new Rule_String(string.Format(arg_18F_0, num), bodyParts[i].label);
					}
				}
				constants[prefix + "_count"] = bodyParts.Count.ToString();
				constants[prefix + "_destroyed_count"] = destroyedIndex.ToString();
				constants[prefix + "_damaged_count"] = damagedIndex.ToString();
			}
			else
			{
				constants[prefix + "_count"] = "0";
				constants[prefix + "_destroyed_count"] = "0";
				constants[prefix + "_damaged_count"] = "0";
			}
		}
	}
}
