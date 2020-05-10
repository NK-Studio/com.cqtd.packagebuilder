#pragma warning disable CS0649
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CQ.PackageManager
{
	class PackageWizard : ScriptableWizard
	{
		public string path;
		public string packageName;
		public string displayName;
		public string version = "1.0.0";
		public string description;
		public PackageDependency[] dependencies;
		public string type;

		[Header("Assembly Definition")] 
		public bool runtime = true;
		public bool editor = true;
		public bool testRuntime;
		// public bool testEditor;

		[Serializable]
		public class PackageDependency
		{
			public string packageName;
			public string version;
		}

		void Awake()
		{
			path = Application.dataPath.Replace("Assets", "Packages") + "/";
		}

		void OnWizardCreate()
		{
			string assetPath = path;
			Dictionary<string, object> dictionary1 = new Dictionary<string, object>();

			dictionary1["name"] = packageName;

			if (!string.IsNullOrEmpty(packageName))
				dictionary1["displayName"] = displayName.Trim();
			else
				dictionary1.Remove("displayName");

			if (!string.IsNullOrEmpty(version))
				dictionary1["version"] = version;
			else
				dictionary1["version"] = "1.0.0";

			if (!string.IsNullOrEmpty(description))
				dictionary1["description"] = (object) description.Trim();
			else
				dictionary1.Remove("description");

			if (!string.IsNullOrEmpty(type))
				dictionary1["type"] = (object) type.Trim();
			else
				dictionary1.Remove("type");

			if (dependencies.Length > 0)
			{
				Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
				foreach (PackageDependency dependency in dependencies)
					if (!string.IsNullOrEmpty(dependency.packageName))
						dictionary2.Add(dependency.packageName.Trim(), dependency.version);

				dictionary1["dependencies"] = (object) dictionary2;
			}
			else
			{
				dictionary1.Remove("dependencies");
			}

			try
			{
				Directory.CreateDirectory($"{path}/{packageName}");
				File.WriteAllText($"{path}/{packageName}/package.json",
					Json.Serialize((object) dictionary1, true, "  "));
			}
			catch
			{
				Debug.Log((object) ("Couldn't write package manifest file " + assetPath + "."));
			}

			if (runtime)
			{
				Directory.CreateDirectory($"{path}/{packageName}/Runtime");
				File.WriteAllText($"{path}/{packageName}/Runtime/{packageName}.runtime.asmdef", Json.Serialize(AssemblyDefinitionStructure.AsRuntime(packageName)));
			}

			if (editor)
			{
				Directory.CreateDirectory($"{path}/{packageName}/Editor");
				
				// AssetDatabase.ImportAsset($"{path}/{packageName}/Runtime/{packageName}.runtime.asmdef");
				// var guid = AssetDatabase.AssetPathToGUID($"{path}/{packageName}/Runtime/{packageName}.runtime.asmdef");
				
				File.WriteAllText($"{path}/{packageName}/Editor/{packageName}.editor.asmdef", Json.Serialize(AssemblyDefinitionStructure.AsEditor(packageName)));
			}

			if (testRuntime
			    // || testEditor
			    )
			{
				Directory.CreateDirectory($"{path}/{packageName}/Test");

				if (testRuntime)
				{
					Directory.CreateDirectory($"{path}/{packageName}/Test/Runtime");
					File.WriteAllText($"{path}/{packageName}/Test/Runtime/{packageName}.test.runtime.asmdef", Json.Serialize(AssemblyDefinitionStructure.AsRuntimeTest(packageName)));
				}

				// if (testEditor)
				// {
				// 	Directory.CreateDirectory($"{path}/{packageName}/Test/Editor");
				// 	File.WriteAllText($"{path}/{packageName}/Test/{packageName}.test.editor.asmdef", Json.Serialize(AssemblyDefinitionStructure.AsEditorTest(packageName)));
				// }
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}

	public class PackageGenerator
	{
		[MenuItem("Tools/Package/Create")]
		static void Show()
		{
			ScriptableWizard.DisplayWizard<PackageWizard>("Wizard");
		}
	}
}

#pragma warning restore CS0649