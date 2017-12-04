using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Verse
{
	public static class GenTypes
	{
		public static readonly List<string> IgnoredNamespaceNames = new List<string>
		{
			"RimWorld",
			"Verse",
			"Verse.AI",
			"Verse.Sound",
			"Verse.Grammar",
			"RimWorld.Planet",
			"RimWorld.BaseGen"
		};

		private static IEnumerable<Assembly> AllActiveAssemblies
		{
			get
			{
				yield return Assembly.GetExecutingAssembly();
				foreach (ModContentPack mod in LoadedModManager.RunningMods)
				{
					int i = 0;
					while (i < mod.assemblies.loadedAssemblies.Count)
					{
						yield return mod.assemblies.loadedAssemblies[i];
					}
				}
			}
		}

		public static IEnumerable<Type> AllTypes
		{
			get
			{
				foreach (Assembly assembly in GenTypes.AllActiveAssemblies)
				{
					Type[] assemblyTypes = null;
					try
					{
						assemblyTypes = assembly.GetTypes();
					}
					catch (ReflectionTypeLoadException)
					{
						Log.Error("Exception getting types in assembly " + assembly.ToString());
					}
					if (assemblyTypes != null)
					{
						Type[] array = assemblyTypes;
						int i = 0;
						while (i < array.Length)
						{
							Type type = array[i];
							yield return type;
						}
					}
				}
			}
		}

		public static IEnumerable<Type> AllTypesWithAttribute<TAttr>() where TAttr : Attribute
		{
			return GenTypes.AllTypes.Where(new Func<Type, bool>(GenAttribute.HasAttribute<TAttr>));
		}

		public static IEnumerable<Type> AllSubclasses(this Type baseType)
		{
			return from x in GenTypes.AllTypes
			where x.IsSubclassOf(baseType)
			select x;
		}

		public static IEnumerable<Type> AllSubclassesNonAbstract(this Type baseType)
		{
			return from x in GenTypes.AllTypes
			where x.IsSubclassOf(baseType) && !x.IsAbstract
			select x;
		}

		public static IEnumerable<Type> AllLeafSubclasses(this Type baseType)
		{
			return from type in baseType.AllSubclasses()
			where !type.AllSubclasses().Any<Type>()
			select type;
		}

		[DebuggerHidden]
		public static IEnumerable<Type> InstantiableDescendantsAndSelf(this Type baseType)
		{
			if (!baseType.IsAbstract)
			{
				yield return baseType;
			}
			foreach (Type descendant in baseType.AllSubclasses())
			{
				if (!descendant.IsAbstract)
				{
					yield return descendant;
				}
			}
		}

		public static Type GetTypeInAnyAssembly(string typeName)
		{
			Type typeInAnyAssemblyRaw = GenTypes.GetTypeInAnyAssemblyRaw(typeName);
			if (typeInAnyAssemblyRaw != null)
			{
				return typeInAnyAssemblyRaw;
			}
			for (int i = 0; i < GenTypes.IgnoredNamespaceNames.Count; i++)
			{
				string typeName2 = GenTypes.IgnoredNamespaceNames[i] + "." + typeName;
				typeInAnyAssemblyRaw = GenTypes.GetTypeInAnyAssemblyRaw(typeName2);
				if (typeInAnyAssemblyRaw != null)
				{
					return typeInAnyAssemblyRaw;
				}
			}
			return null;
		}

		private static Type GetTypeInAnyAssemblyRaw(string typeName)
		{
			foreach (Assembly current in GenTypes.AllActiveAssemblies)
			{
				Type type = current.GetType(typeName, false, true);
				if (type != null)
				{
					return type;
				}
			}
			return null;
		}

		public static string GetTypeNameWithoutIgnoredNamespaces(Type type)
		{
			if (type.IsGenericType)
			{
				return type.ToString();
			}
			for (int i = 0; i < GenTypes.IgnoredNamespaceNames.Count; i++)
			{
				if (type.Namespace == GenTypes.IgnoredNamespaceNames[i])
				{
					return type.Name;
				}
			}
			return type.FullName;
		}
	}
}
