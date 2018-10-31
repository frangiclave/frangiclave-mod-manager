using System;
using System.IO;
using System.Reflection;
using MonoMod;

namespace FrangiclaveModManager
{
    public class Patcher
    {
        private string _assemblyDirectory;

        private static readonly string[] ManagedDirCandidates =
        {
            "cultistsimulator_Data/Managed",
            "Contents/Resources/Data/Managed",
            "CS_Data/Managed"
        };

        private const string AssemblyName = "Assembly-CSharp.dll";
        private const string AssemblyBackupName = "Assembly-CSharp.backup.dll";
        private const string AssemblyPatchName = "FrangiclavePatch.dll";

        private static readonly string[] PatchAssemblies =
        {
            AssemblyPatchName,
            "Mono.Cecil.dll",
            "Mono.Cecil.Mdb.dll",
            "Mono.Cecil.Pdb.dll",
            "MonoMod.exe",
            "MonoMod.Utils.dll"
        };

        public Patcher(string gameDirectory)
        {
            _assemblyDirectory = LocateAssemblyDirectory(gameDirectory);
        }

        public void Patch()
        {
            // Load the assembly to check if it needs to be patched first
            string assemblyPath = Path.Combine(_assemblyDirectory, AssemblyName);
            string assemblyBackupPath = assemblyPath.Replace(AssemblyName, AssemblyBackupName);
            if (!IsPatchingNeeded(assemblyPath))
            {
                Console.WriteLine("Game is already patched, no further action required.");
                return;
            }

            // Copy the required assemblies to the game directory
            foreach (var assemblyName in PatchAssemblies)
            {
                File.Copy(assemblyName, Path.Combine(_assemblyDirectory, assemblyName), true);
            }
            File.Copy(assemblyPath, assemblyBackupPath, true);

            // Run MonoMod
            Console.WriteLine("Patching game...");
            using (MonoModder mm = new MonoModder()
            {
                InputPath = assemblyBackupPath,
                OutputPath = assemblyPath + ".tmp"
            })
            {
                mm.Read();
                mm.ReadMod(Path.Combine(_assemblyDirectory, AssemblyPatchName));
                mm.MapDependencies();
                mm.AutoPatch();
                mm.Write();
            }

            File.Copy(assemblyPath + ".tmp", assemblyPath, true);

        }

        private static string LocateAssemblyDirectory(string gameDirectory)
        {
            foreach (var candidate in ManagedDirCandidates)
            {
                string managedDirectory = Path.Combine(gameDirectory, candidate);
                if (Directory.Exists(managedDirectory))
                {
                    return managedDirectory;
                }
            }
            throw new ApplicationException("Invalid game directory, assemblies not found");
        }

        private static bool IsPatchingNeeded(string assemblyPath)
        {
            return Assembly.Load(File.ReadAllBytes(assemblyPath)).GetType("MonoMod.WasHere") == null;
        }
    }
}
