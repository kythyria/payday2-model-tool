//Original code by PoueT

using PD2ModelParser.Misc;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using Mono.Options;
using PD2ModelParser.Modelscript;

namespace PD2ModelParser
{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            Log.Default = new ConsoleLogger();

            bool gui = HandleCommandLine(args);
            if (!gui)
                return;

            Updates.Startup();

            Application.EnableVisualStyles();
            Form1 form = new Form1();
            Application.Run(form);
        }

        private static bool HandleCommandLine(string[] args)
        {
            // If there are no args, assume we're in GUI mode
            if (args.Length == 0)
            {
                // Set a sane default logging level to avoid console spam
                ConsoleLogger.minimumLevel = LoggerLevel.Info;

                return true;
            }

            bool show_help = false;
            bool gui = false;
            int verbosity = (int) LoggerLevel.Info;

            var script = new List<IScriptItem>();

            var assembly = Assembly.GetExecutingAssembly();
            var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            OptionSet p = new OptionSet
            {
                $"Version: {informationalVersion?.InformationalVersion}",
                "",
                $"Usage: {AppDomain.CurrentDomain.FriendlyName} [OPTIONS]+",
                "",
                "Import and export DieselX .model files",
                "",
                "If the program is called without any arguments," +
                " GUI mode is enabled by default.",
                "If the program is running in command-line mode, then" +
                " the file commands will be run in sequence, eg.,",
                "--load=a.model --import=b.obj --root-point=Hips ",
                "    --import-pattern-uv=b_pattern_uv.obj --save=c.model",
                "Will import a model called a.model, add an object, and save it as c.model",
                "",
                "Note you can process many models in one run of the program, which is faster" +
                " than running the program once for each model",
                "",
                "General options:",
                {
                    "g|gui", "Enable the GUI",
                    v => gui = v != null
                },
                {
                    "v|verbose", "increase debug message verbosity",
                    v =>
                    {
                        if (v != null) --verbosity;
                    }
                },
                {
                    "q|quiet", "Decrease debug message verbosity",
                    v =>
                    {
                        if (v != null) ++verbosity;
                    }
                },
                {
                    "h|help", "show this message and exit",
                    v => show_help = v != null
                },
                "",
                "Actions:",
                {
                    "n|new-objects", "Sets whether or not new objects will be "
                                     + "created (append + or - to set this) default false",
                    v => script.Add(new CreateNewObjects() { Create = v != null })
                },
                {
                    // Colon means an optional value
                    "r|root-point:", "Sets the default bone for new objects to be attached to",
                    v => script.Add(new SetRootPoint() { Name = v })
                },
                {
                    "new", "Creates an entirely new model",
                    v => script.Add(new NewModel())
                },
                {
                    "load=", "Load a .model file",
                    v => script.Add(new LoadModel() { File = v })
                },
                {
                    "save=", "Save to a .model file",
                    v => script.Add(new SaveModel() { File = v })
                },
                {
                    "import=", "Imports from a 3D model file",
                    v => script.Add(new Import() { File = v })
                },
                {
                    "import-pattern-uv=", "Imports a pattern UV .OBJ file",
                    v => script.Add(new PatternUV() { File = v })
                },
                {
                    "export=", "Exports to a 3D model file",
                    v => script.Add(new Export() { File = v })
                },
                {
                    "export-type=", "Sets the type for mass exports",
                    v => {
                        if(FileTypeInfo.TryParseName(v, out var type))
                        {
                            script.Add(new SetDefaultExportType() {FileType = type});
                        }
                        else
                        {
                            Log.Default.Error("Unknown export filetype {0}", v);
                            Environment.Exit(1);
                        }
                    }
                },
                {
                    "script=", "Executes a model script",
                    v => script.Add(new RunScript() { File = v })
                },
                {
                    "batch-export=", "Recursively scans a directory for .model files and exports them all",
                    v => script.Add(new BatchExport { Directory = v })
                },
                {
                    "dump-anims=", "Write out a modelscript with the current model's animations in",
                    v => script.Add(new DumpAnims { File = v })
                },
                {
                    "dump-skins=", "Write out a modelscript with the current model's skin data in",
                    v => script.Add(new DumpSkins() { File = v })
                }
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                ShowError(e.Message);
                return false;
            }

            if (show_help)
            {
                p.WriteOptionDescriptions(Console.Out);
                return false;
            }

            if (extra.Count != 0)
            {
                ShowError($"Unknown argument \"{extra[0]}\"");
                return false;
            }

            if (script.Count != 0 && gui)
            {
                ShowError("Cannot process files in GUI mode!");
                return false;
            }

            if (verbosity > (int) LoggerLevel.Error || verbosity < 0)
            {
                Console.WriteLine("Cannot be that verbose or quiet (verbosity \n"
                                  + "{0} of 0-{1})!", verbosity, (int) LoggerLevel.Error);
                return false;
            }

            ConsoleLogger.minimumLevel = (LoggerLevel) verbosity;

            var state = new Modelscript.ScriptState()
            {
                WorkDir = System.IO.Directory.GetCurrentDirectory(),
                CreateNewObjects = false,
                Data = new FullModelData()
            };
            state.ExecuteItems(script);

            return gui;
        }

        private static void ShowError(string err)
        {
            string exe_name = AppDomain.CurrentDomain.FriendlyName;
            Console.Write("{0}:", exe_name);
            Console.WriteLine(err);
            Console.WriteLine("Try `{0} --help' for more information.", exe_name);
        }
    }

    internal class ConsoleLogger : BaseLogger
    {
        internal static LoggerLevel minimumLevel;

        public override void Log(LoggerLevel level, string message, params object[] value)
        {
            if (level < minimumLevel)
                return;

            Console.WriteLine(level + @": " + GetCallerName(3) + @": " + string.Format(message, value));
        }
    }
}
