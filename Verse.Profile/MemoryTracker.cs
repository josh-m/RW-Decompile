using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Verse.Profile
{
	[HasDebugOutput]
	public static class MemoryTracker
	{
		private class ReferenceData
		{
			public struct Link
			{
				public object target;

				public MemoryTracker.ReferenceData targetRef;

				public string path;
			}

			public List<MemoryTracker.ReferenceData.Link> refers = new List<MemoryTracker.ReferenceData.Link>();

			public List<MemoryTracker.ReferenceData.Link> referredBy = new List<MemoryTracker.ReferenceData.Link>();

			public string path;

			public int pathCost;
		}

		private struct ChildReference
		{
			public object child;

			public string path;
		}

		public class MarkupComplete : Attribute
		{
		}

		private static Dictionary<Type, HashSet<WeakReference>> tracked = new Dictionary<Type, HashSet<WeakReference>>();

		private static List<WeakReference> foundCollections = new List<WeakReference>();

		private static bool trackedLocked = false;

		private static List<object> trackedQueue = new List<object>();

		private static List<RuntimeTypeHandle> trackedTypeQueue = new List<RuntimeTypeHandle>();

		private const int updatesPerCull = 10;

		private static int updatesSinceLastCull = 0;

		private static int cullTargetIndex = 0;

		public static bool AnythingTracked
		{
			get
			{
				return MemoryTracker.tracked.Count > 0;
			}
		}

		public static IEnumerable<WeakReference> FoundCollections
		{
			get
			{
				if (MemoryTracker.foundCollections.Count == 0)
				{
					MemoryTracker.LogObjectHoldPathsFor(null, null);
				}
				return MemoryTracker.foundCollections;
			}
		}

		public static void RegisterObject(object obj)
		{
			if (MemoryTracker.trackedLocked)
			{
				MemoryTracker.trackedQueue.Add(obj);
				return;
			}
			Type type = obj.GetType();
			HashSet<WeakReference> hashSet = null;
			if (!MemoryTracker.tracked.TryGetValue(type, out hashSet))
			{
				hashSet = new HashSet<WeakReference>();
				MemoryTracker.tracked[type] = hashSet;
			}
			hashSet.Add(new WeakReference(obj));
		}

		public static void RegisterType(RuntimeTypeHandle typeHandle)
		{
			if (MemoryTracker.trackedLocked)
			{
				MemoryTracker.trackedTypeQueue.Add(typeHandle);
				return;
			}
			Type typeFromHandle = Type.GetTypeFromHandle(typeHandle);
			if (!MemoryTracker.tracked.ContainsKey(typeFromHandle))
			{
				MemoryTracker.tracked[typeFromHandle] = new HashSet<WeakReference>();
			}
		}

		private static void LockTracking()
		{
			if (MemoryTracker.trackedLocked)
			{
				throw new NotImplementedException();
			}
			MemoryTracker.trackedLocked = true;
		}

		private static void UnlockTracking()
		{
			if (!MemoryTracker.trackedLocked)
			{
				throw new NotImplementedException();
			}
			MemoryTracker.trackedLocked = false;
			foreach (object current in MemoryTracker.trackedQueue)
			{
				MemoryTracker.RegisterObject(current);
			}
			MemoryTracker.trackedQueue.Clear();
			foreach (RuntimeTypeHandle current2 in MemoryTracker.trackedTypeQueue)
			{
				MemoryTracker.RegisterType(current2);
			}
			MemoryTracker.trackedTypeQueue.Clear();
		}

		[Category("System"), DebugOutput]
		private static void ObjectsLoaded()
		{
			if (MemoryTracker.tracked.Count == 0)
			{
				Log.Message("No objects tracked, memory tracker markup may not be applied.", false);
				return;
			}
			GC.Collect();
			MemoryTracker.LockTracking();
			try
			{
				foreach (HashSet<WeakReference> current in MemoryTracker.tracked.Values)
				{
					MemoryTracker.CullNulls(current);
				}
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<Type, HashSet<WeakReference>> current2 in from kvp in MemoryTracker.tracked
				orderby -kvp.Value.Count
				select kvp)
				{
					stringBuilder.AppendLine(string.Format("{0,6} {1}", current2.Value.Count, current2.Key));
				}
				Log.Message(stringBuilder.ToString(), false);
			}
			finally
			{
				MemoryTracker.UnlockTracking();
			}
		}

		[Category("System"), DebugOutput]
		private static void ObjectHoldPaths()
		{
			if (MemoryTracker.tracked.Count == 0)
			{
				Log.Message("No objects tracked, memory tracker markup may not be applied.", false);
				return;
			}
			GC.Collect();
			MemoryTracker.LockTracking();
			try
			{
				foreach (HashSet<WeakReference> current in MemoryTracker.tracked.Values)
				{
					MemoryTracker.CullNulls(current);
				}
				List<Type> list = new List<Type>();
				list.Add(typeof(Map));
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				foreach (Type current2 in list.Concat(from kvp in MemoryTracker.tracked
				orderby -kvp.Value.Count
				select kvp.Key).Take(30))
				{
					Type type = current2;
					HashSet<WeakReference> trackedBatch = MemoryTracker.tracked.TryGetValue(type, null);
					if (trackedBatch == null)
					{
						trackedBatch = new HashSet<WeakReference>();
					}
					list2.Add(new FloatMenuOption(string.Format("{0} ({1})", type, trackedBatch.Count), delegate
					{
						MemoryTracker.LogObjectHoldPathsFor(trackedBatch, (WeakReference _) => 1);
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
					if (list2.Count == 30)
					{
						break;
					}
				}
				Find.WindowStack.Add(new FloatMenu(list2));
			}
			finally
			{
				MemoryTracker.UnlockTracking();
			}
		}

		public static void LogObjectHoldPathsFor(IEnumerable<WeakReference> elements, Func<WeakReference, int> weight)
		{
			GC.Collect();
			MemoryTracker.LockTracking();
			try
			{
				Dictionary<object, MemoryTracker.ReferenceData> dictionary = new Dictionary<object, MemoryTracker.ReferenceData>();
				HashSet<object> hashSet = new HashSet<object>();
				MemoryTracker.foundCollections.Clear();
				Queue<object> queue = new Queue<object>();
				foreach (object current in from weakref in MemoryTracker.tracked.SelectMany((KeyValuePair<Type, HashSet<WeakReference>> kvp) => kvp.Value)
				where weakref.IsAlive
				select weakref.Target)
				{
					if (!hashSet.Contains(current))
					{
						hashSet.Add(current);
						queue.Enqueue(current);
					}
				}
				foreach (Type current2 in GenTypes.AllTypes.Union(MemoryTracker.tracked.Keys))
				{
					if (!current2.FullName.Contains("MemoryTracker") && !current2.FullName.Contains("CollectionsTracker"))
					{
						if (!current2.ContainsGenericParameters)
						{
							MemoryTracker.AccumulateStaticMembers(current2, dictionary, hashSet, queue);
						}
					}
				}
				int num = 0;
				while (queue.Count > 0)
				{
					if (num % 10000 == 0)
					{
						UnityEngine.Debug.LogFormat("{0} / {1} (to process: {2})", new object[]
						{
							num,
							num + queue.Count,
							queue.Count
						});
					}
					num++;
					MemoryTracker.AccumulateReferences(queue.Dequeue(), dictionary, hashSet, queue);
				}
				if (elements != null && weight != null)
				{
					int num2 = 0;
					MemoryTracker.CalculateReferencePaths(dictionary, from kvp in dictionary
					where !kvp.Value.path.NullOrEmpty()
					select kvp.Key, num2);
					num2 += 1000;
					MemoryTracker.CalculateReferencePaths(dictionary, from kvp in dictionary
					where kvp.Value.path.NullOrEmpty() && kvp.Value.referredBy.Count == 0
					select kvp.Key, num2);
					foreach (object current3 in from kvp in dictionary
					where kvp.Value.path.NullOrEmpty()
					select kvp.Key)
					{
						num2 += 1000;
						MemoryTracker.CalculateReferencePaths(dictionary, new object[]
						{
							current3
						}, num2);
					}
					Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
					foreach (WeakReference current4 in elements)
					{
						object target = current4.Target;
						if (target != null)
						{
							string path = dictionary[target].path;
							if (!dictionary2.ContainsKey(path))
							{
								dictionary2[path] = 0;
							}
							Dictionary<string, int> dictionary3;
							string key;
							(dictionary3 = dictionary2)[key = path] = dictionary3[key] + weight(current4);
						}
					}
					StringBuilder stringBuilder = new StringBuilder();
					foreach (KeyValuePair<string, int> current5 in from kvp in dictionary2
					orderby -kvp.Value
					select kvp)
					{
						stringBuilder.AppendLine(string.Format("{0}: {1}", current5.Value, current5.Key));
					}
					Log.Message(stringBuilder.ToString(), false);
				}
			}
			finally
			{
				MemoryTracker.UnlockTracking();
			}
		}

		private static void AccumulateReferences(object obj, Dictionary<object, MemoryTracker.ReferenceData> references, HashSet<object> seen, Queue<object> toProcess)
		{
			MemoryTracker.ReferenceData referenceData = null;
			if (!references.TryGetValue(obj, out referenceData))
			{
				referenceData = new MemoryTracker.ReferenceData();
				references[obj] = referenceData;
			}
			foreach (MemoryTracker.ChildReference current in MemoryTracker.GetAllReferencedClassesFromClassOrStruct(obj, MemoryTracker.GetFieldsFromHierarchy(obj.GetType(), BindingFlags.Instance), obj, string.Empty))
			{
				if (!current.child.GetType().IsClass)
				{
					throw new ApplicationException();
				}
				MemoryTracker.ReferenceData referenceData2 = null;
				if (!references.TryGetValue(current.child, out referenceData2))
				{
					referenceData2 = new MemoryTracker.ReferenceData();
					references[current.child] = referenceData2;
				}
				referenceData2.referredBy.Add(new MemoryTracker.ReferenceData.Link
				{
					target = obj,
					targetRef = referenceData,
					path = current.path
				});
				referenceData.refers.Add(new MemoryTracker.ReferenceData.Link
				{
					target = current.child,
					targetRef = referenceData2,
					path = current.path
				});
				if (!seen.Contains(current.child))
				{
					seen.Add(current.child);
					toProcess.Enqueue(current.child);
				}
			}
		}

		private static void AccumulateStaticMembers(Type type, Dictionary<object, MemoryTracker.ReferenceData> references, HashSet<object> seen, Queue<object> toProcess)
		{
			foreach (MemoryTracker.ChildReference current in MemoryTracker.GetAllReferencedClassesFromClassOrStruct(null, MemoryTracker.GetFields(type, BindingFlags.Static), null, type.ToString() + "."))
			{
				if (!current.child.GetType().IsClass)
				{
					throw new ApplicationException();
				}
				MemoryTracker.ReferenceData referenceData = null;
				if (!references.TryGetValue(current.child, out referenceData))
				{
					referenceData = new MemoryTracker.ReferenceData();
					referenceData.path = current.path;
					referenceData.pathCost = 0;
					references[current.child] = referenceData;
				}
				if (!seen.Contains(current.child))
				{
					seen.Add(current.child);
					toProcess.Enqueue(current.child);
				}
			}
		}

		[DebuggerHidden]
		private static IEnumerable<MemoryTracker.ChildReference> GetAllReferencedClassesFromClassOrStruct(object current, IEnumerable<FieldInfo> fields, object parent, string currentPath)
		{
			foreach (FieldInfo field in fields)
			{
				if (!field.FieldType.IsPrimitive)
				{
					object referenced = null;
					referenced = field.GetValue(current);
					if (referenced != null)
					{
						foreach (MemoryTracker.ChildReference child in MemoryTracker.DistillChildReferencesFromObject(referenced, parent, currentPath + field.Name))
						{
							yield return child;
						}
					}
				}
			}
			if (current != null && current is ICollection)
			{
				MemoryTracker.foundCollections.Add(new WeakReference(current));
				foreach (object entry in (current as IEnumerable))
				{
					if (entry != null && !entry.GetType().IsPrimitive)
					{
						foreach (MemoryTracker.ChildReference child2 in MemoryTracker.DistillChildReferencesFromObject(entry, parent, currentPath + "[]"))
						{
							yield return child2;
						}
					}
				}
			}
		}

		[DebuggerHidden]
		private static IEnumerable<MemoryTracker.ChildReference> DistillChildReferencesFromObject(object current, object parent, string currentPath)
		{
			Type type = current.GetType();
			if (type.IsClass)
			{
				yield return new MemoryTracker.ChildReference
				{
					child = current,
					path = currentPath
				};
			}
			else if (!type.IsPrimitive)
			{
				if (!type.IsValueType)
				{
					throw new NotImplementedException();
				}
				string structPath = currentPath + ".";
				foreach (MemoryTracker.ChildReference childReference in MemoryTracker.GetAllReferencedClassesFromClassOrStruct(current, MemoryTracker.GetFieldsFromHierarchy(type, BindingFlags.Instance), parent, structPath))
				{
					yield return childReference;
				}
			}
		}

		[DebuggerHidden]
		private static IEnumerable<FieldInfo> GetFieldsFromHierarchy(Type type, BindingFlags bindingFlags)
		{
			while (type != null)
			{
				foreach (FieldInfo field in MemoryTracker.GetFields(type, bindingFlags))
				{
					yield return field;
				}
				type = type.BaseType;
			}
		}

		[DebuggerHidden]
		private static IEnumerable<FieldInfo> GetFields(Type type, BindingFlags bindingFlags)
		{
			FieldInfo[] fields = type.GetFields(bindingFlags | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				FieldInfo field = fields[i];
				yield return field;
			}
		}

		private static void CalculateReferencePaths(Dictionary<object, MemoryTracker.ReferenceData> references, IEnumerable<object> objects, int pathCost)
		{
			Queue<object> queue = new Queue<object>(objects);
			while (queue.Count > 0)
			{
				object obj = queue.Dequeue();
				if (references[obj].path.NullOrEmpty())
				{
					references[obj].path = string.Format("???.{0}", obj.GetType());
					references[obj].pathCost = pathCost;
				}
				MemoryTracker.CalculateObjectReferencePath(obj, references, queue);
			}
		}

		private static void CalculateObjectReferencePath(object obj, Dictionary<object, MemoryTracker.ReferenceData> references, Queue<object> queue)
		{
			MemoryTracker.ReferenceData referenceData = references[obj];
			foreach (MemoryTracker.ReferenceData.Link current in referenceData.refers)
			{
				MemoryTracker.ReferenceData referenceData2 = references[current.target];
				string text = referenceData.path + "." + current.path;
				int num = referenceData.pathCost + 1;
				if (referenceData2.path.NullOrEmpty())
				{
					queue.Enqueue(current.target);
					referenceData2.path = text;
					referenceData2.pathCost = num;
				}
				else if (referenceData2.pathCost == num && referenceData2.path.CompareTo(text) < 0)
				{
					referenceData2.path = text;
				}
				else if (referenceData2.pathCost > num)
				{
					throw new ApplicationException();
				}
			}
		}

		public static void Update()
		{
			if (MemoryTracker.tracked.Count == 0)
			{
				return;
			}
			if (MemoryTracker.updatesSinceLastCull++ >= 10)
			{
				MemoryTracker.updatesSinceLastCull = 0;
				HashSet<WeakReference> value = MemoryTracker.tracked.ElementAtOrDefault(MemoryTracker.cullTargetIndex++).Value;
				if (value == null)
				{
					MemoryTracker.cullTargetIndex = 0;
				}
				else
				{
					MemoryTracker.CullNulls(value);
				}
			}
		}

		private static void CullNulls(HashSet<WeakReference> table)
		{
			table.RemoveWhere((WeakReference element) => !element.IsAlive);
		}
	}
}
