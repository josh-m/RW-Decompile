using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Verse
{
	public class ModAssemblyHandler
	{
		private ModContentPack mod;

		public List<Assembly> loadedAssemblies = new List<Assembly>();

		private static bool globalResolverIsSet;

		public ModAssemblyHandler(ModContentPack mod)
		{
			this.mod = mod;
		}

		public void ReloadAll()
		{
			if (!ModAssemblyHandler.globalResolverIsSet)
			{
				ResolveEventHandler @object = (object obj, ResolveEventArgs args) => Assembly.GetExecutingAssembly();
				AppDomain currentDomain = AppDomain.CurrentDomain;
				currentDomain.AssemblyResolve += new ResolveEventHandler(@object.Invoke);
				ModAssemblyHandler.globalResolverIsSet = true;
			}
			string path = Path.Combine(this.mod.RootDir, "Assemblies");
			string path2 = Path.Combine(GenFilePaths.CoreModsFolderPath, path);
			DirectoryInfo directoryInfo = new DirectoryInfo(path2);
			if (!directoryInfo.Exists)
			{
				return;
			}
			FileInfo[] files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo fileInfo = files[i];
				if (!(fileInfo.Extension.ToLower() != ".dll"))
				{
					Assembly assembly = null;
					try
					{
						byte[] rawAssembly = File.ReadAllBytes(fileInfo.FullName);
						string fileName = Path.Combine(fileInfo.DirectoryName, Path.GetFileNameWithoutExtension(fileInfo.FullName)) + ".pdb";
						FileInfo fileInfo2 = new FileInfo(fileName);
						if (fileInfo2.Exists)
						{
							byte[] rawSymbolStore = File.ReadAllBytes(fileInfo2.FullName);
							assembly = AppDomain.CurrentDomain.Load(rawAssembly, rawSymbolStore);
						}
						else
						{
							assembly = AppDomain.CurrentDomain.Load(rawAssembly);
						}
					}
					catch (Exception ex)
					{
						Log.Error("Exception loading " + fileInfo.Name + ": " + ex.ToString());
						break;
					}
					if (assembly != null)
					{
						if (this.AssemblyIsUsable(assembly))
						{
							this.loadedAssemblies.Add(assembly);
						}
					}
				}
			}
		}

		private bool AssemblyIsUsable(Assembly asm)
		{
			try
			{
				asm.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(string.Concat(new object[]
				{
					"ReflectionTypeLoadException getting types in assembly ",
					asm.GetName().Name,
					": ",
					ex
				}));
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("Loader exceptions:");
				if (ex.LoaderExceptions != null)
				{
					Exception[] loaderExceptions = ex.LoaderExceptions;
					for (int i = 0; i < loaderExceptions.Length; i++)
					{
						Exception ex2 = loaderExceptions[i];
						stringBuilder.AppendLine("   => " + ex2.ToString());
					}
				}
				Log.Error(stringBuilder.ToString());
				bool result = false;
				return result;
			}
			catch (Exception ex3)
			{
				Log.Error(string.Concat(new object[]
				{
					"Exception getting types in assembly ",
					asm.GetName().Name,
					": ",
					ex3
				}));
				bool result = false;
				return result;
			}
			return true;
		}
	}
}
