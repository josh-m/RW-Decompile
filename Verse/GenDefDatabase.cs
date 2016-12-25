using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Verse
{
	public static class GenDefDatabase
	{
		public static Def GetDef(Type defType, string defName, bool errorOnFail = true)
		{
			return (Def)GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), defType, "GetNamed", new object[]
			{
				defName,
				errorOnFail
			});
		}

		public static Def GetDefSilentFail(Type type, string targetDefName)
		{
			if (type == typeof(SoundDef))
			{
				return SoundDef.Named(targetDefName);
			}
			return (Def)GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), type, "GetNamedSilentFail", new object[]
			{
				targetDefName
			});
		}

		[DebuggerHidden]
		public static IEnumerable<Type> AllDefTypesWithDatabases()
		{
			foreach (Type defType in typeof(Def).AllSubclasses())
			{
				if (!defType.IsAbstract)
				{
					if (defType != typeof(Def))
					{
						bool foundNonAbstractAncestor = false;
						Type parent = defType.BaseType;
						while (parent != null && parent != typeof(Def))
						{
							if (!parent.IsAbstract)
							{
								foundNonAbstractAncestor = true;
								break;
							}
							parent = parent.BaseType;
						}
						if (!foundNonAbstractAncestor)
						{
							yield return defType;
						}
					}
				}
			}
		}

		public static IEnumerable<T> DefsToGoInDatabase<T>(ModContentPack mod)
		{
			return mod.AllDefs.OfType<T>();
		}
	}
}
