namespace CQ
{
	public class AssemblyDefinitionStructure
	{
		public string name { get; set; }
		public string[] references { get; set; }
		public string[] includePlatforms { get; set; }
		public string[] excludePlatforms { get; set; }
		public bool allowUnsafeCode { get; set; }
		public bool overrideReferences { get; set; }
		public string[] precompiledReferences { get; set; }
		public bool autoReferenced { get; set; }
		public string[] defineConstraints { get; set; }
		public string[] versionDefines { get; set; }
		public bool noEngineReferences { get; set; }
		public string[] optionalUnityReferences { get; set; }

		public AssemblyDefinitionStructure(string packageName)
		{
			name = packageName;
		}

		internal static AssemblyDefinitionStructure AsRuntime(string name)
		{
			 return new AssemblyDefinitionStructure($"{name}.runtime");
		}
		
		internal static AssemblyDefinitionStructure AsEditor(string name, string guid)
		{
			var asmdef =  new AssemblyDefinitionStructure($"{name}.editor");
			asmdef.includePlatforms = new[] {"Editor"};
			asmdef.references = new[] {$"GUID:{guid}"};
			
			return asmdef;
		} 
		
		internal static AssemblyDefinitionStructure AsEditor(string name)
		{
			var asmdef =  new AssemblyDefinitionStructure($"{name}.editor");
			asmdef.includePlatforms = new[] {"Editor"};
			
			return asmdef;
		} 
		
		internal static AssemblyDefinitionStructure AsRuntimeTest(string name)
		{
			var asmdef =  new AssemblyDefinitionStructure($"{name}.test.runtime");
			asmdef.optionalUnityReferences = new[] {"TestAssemblies"};

			return asmdef;
		}
		
		internal static AssemblyDefinitionStructure AsEditorTest(string name)
		{
			return new AssemblyDefinitionStructure(name);
		} 
	}
}