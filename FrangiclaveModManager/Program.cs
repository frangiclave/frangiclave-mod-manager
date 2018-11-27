using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;

namespace FrangiclaveModManager
{
    class Program
    {
        private const string Logo = @"
   __                       _      _
  / _|_ __ __ _ _ __   __ _(_) ___| | __ ___   _____
 | |_| '__/ _` | '_ \ / _` | |/ __| |/ _` \ \ / / _ \
 |  _| | | (_| | | | | (_| | | (__| | (_| |\ V /  __/
 |_| |_|  \__,_|_| |_|\__, |_|\___|_|\__,_| \_/ \___|
                      |___/
";

        private static readonly string[] DefaultGameDirectoryPaths = {
            // Windows
            "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Cultist Simulator",
            "C:\\Program Files\\Steam\\steamapps\\common\\Cultist Simulator",
            "D:\\Program Files (x86)\\Steam\\steamapps\\common\\Cultist Simulator",
            "D:\\Program Files\\Steam\\steamapps\\common\\Cultist Simulator",
            "C:\\GOG Games\\Cultist Simulator",
            "D:\\GOG Games\\Cultist Simulator",

            // macOS
            "$HOME/Library/Application Support/Steam/steamapps/common/Cultist Simulator",

            // Linux
            "$HOME/.local/share/Steam/steamapps/common/Cultist Simulator"
        };

        private static readonly FileIniDataParser ConfigParser = new FileIniDataParser();

        public static void Main(string[] args)
        {

            // Get the version
            string version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

            // Load arguments
            bool waitForKey = true;
            bool showVersion = false;
            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "-n":
                    case "--no-wait":
                        waitForKey = false;
                        break;
                    case "-v":
                    case "--version":
                        showVersion = true;
                        break;
                    default:
                        Console.WriteLine($"Invalid argument: '{arg}'");
                        break;
                }
            }

            // Display just the version if that was requested
            if (showVersion)
            {
                Console.WriteLine(version);
                return;
            }

            // Write the welcome message
            Console.WriteLine(Logo);
            Console.WriteLine($"Version: {version}");
            Console.WriteLine();

            // Load the configuration
            Console.WriteLine("Loading configuration...");
            Config config = LoadConfiguration();
            Console.WriteLine("Configuration loaded.");
            Console.WriteLine();

            // Run the patcher if required
            Patcher patcher = new Patcher(config.GamePath);
            patcher.Patch();
            Console.WriteLine();

            if (!waitForKey)
            {
                return;
            }

            WaitForInput();
        }

        private static Config LoadConfiguration()
        {
            // Load the configuration file if it exists, or create it as part of
            // the first run wizard
            string appDataDirectory = GetAppDataDirectory();
            IniData configData;
            try
            {
                configData = ConfigParser.ReadFile(Path.Combine(appDataDirectory, "config.ini"));
            }
            catch (ParsingException)
            {
                configData = FirstRun();
            }

            // Create the Config file object
            return new Config
            {
                CheckForUpdates = Convert.ToBoolean(configData["Patcher"]["CheckForUpdates"]),
                GamePath = configData["Game"]["Path"]
            };
        }

        private static string GetAppDataDirectory()
        {
            string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string frangiclaveDir = Path.Combine(appDataDir, "Frangiclave");
            if (!Directory.Exists(frangiclaveDir))
            {
                Directory.CreateDirectory(frangiclaveDir);
            }

            return frangiclaveDir;
        }

        private static IniData FirstRun()
        {
            Console.WriteLine("It looks like this is your first time running the Frangiclave Mod Manager.");

            // Try to find the root of the Cultist Simulator game directory by
            // repeatedly checking for the presence of identifying files and
            // folders on the various platforms
            string path = LocateGameDirectory();
            if (path != null)
            {
                Console.Write(
                    "We found a game installation (" + path + "). Is this the installation you wish to mod? [y/n] ");
                if (Console.ReadLine() != "y")
                {
                    path = null;
                }
            }
            while (path == null)
            {
                Console.Write(
                    "Please input the root of your Cultist Simulator directory " +
                    "(on Steam, this is usually <STEAM LOCATION>/steamapps/common/Cultist Simulator): ");
                path = Console.ReadLine();
                if (!Directory.Exists(path))
                {
                    Console.WriteLine("The specified directory does not exist.");
                    path = null;
                }
            }

            // Write out the initial configuration
            IniData configData = new IniData();
            configData["Patcher"]["CheckForUpdate"] = "true";
            configData["Game"]["Path"] = path;
            ConfigParser.WriteFile(Path.Combine(GetAppDataDirectory(), "config.ini"), configData);

            return configData;
        }

        private static string LocateGameDirectory()
        {
            return DefaultGameDirectoryPaths.FirstOrDefault(Directory.Exists);
        }

        private static void WaitForInput()
        {
            Console.Write("Press Enter to close...");
            try
            {
                Console.ReadKey();
            }
            catch (InvalidOperationException)
            {
                Console.Read();
            }
        }
    }
}
