using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Verse
{
	public static class StaticConstructorOnStartupUtility
	{
		public static bool coreStaticAssetsLoaded;

		public static void CallAll()
		{
			IEnumerable<Type> enumerable = GenTypes.AllTypesWithAttribute<StaticConstructorOnStartup>();
			foreach (Type current in enumerable)
			{
				RuntimeHelpers.RunClassConstructor(current.TypeHandle);
			}
			StaticConstructorOnStartupUtility.coreStaticAssetsLoaded = true;
		}

		public static void ReportProbablyMissingAttributes()
		{
			BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			foreach (Type current in GenTypes.AllTypes)
			{
				if (!current.HasAttribute<StaticConstructorOnStartup>())
				{
					FieldInfo fieldInfo = current.GetFields(bindingAttr).FirstOrDefault(delegate(FieldInfo x)
					{
						Type type = x.FieldType;
						if (type.IsArray)
						{
							type = type.GetElementType();
						}
						return typeof(Texture).IsAssignableFrom(type) || typeof(Material).IsAssignableFrom(type) || typeof(Shader).IsAssignableFrom(type) || typeof(Graphic).IsAssignableFrom(type) || typeof(GameObject).IsAssignableFrom(type) || typeof(MaterialPropertyBlock).IsAssignableFrom(type);
					});
					if (fieldInfo != null)
					{
						Log.Warning(string.Concat(new string[]
						{
							"Type ",
							current.Name,
							" probably needs a StaticConstructorOnStartup attribute, because it has a field ",
							fieldInfo.Name,
							" of type ",
							fieldInfo.FieldType.Name,
							". All assets must be loaded in the main thread."
						}));
					}
				}
			}
		}
	}
}
