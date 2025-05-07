using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DRSAT
{
    internal class Program
    {
        private static readonly Dictionary<string, string> _consoles = new Dictionary<string, string>()
        {
            { "adsi", @"""C:\Windows\System32\adsiedit.msc""" },
            { "aduc", @"""C:\Windows\System32\dsa.msc""" },
            { "cert", @"""C:\Windows\System32\certsrv.msc""" },
            { "dns", @"""C:\Windows\System32\dnsmgmt.msc""" },
            { "gpo", @"""C:\Windows\System32\gpmc.msc""" },
            { "template", @"""C:\Windows\System32\certtmpl.msc""" },
        };

        private static readonly string _mmcPath = @"C:\Windows\System32\mmc.exe";
        private static readonly string _gpmePath = @"""C:\Windows\System32\gpme.msc""";

        private static readonly string _programHelp = @"[!] Usage: DRSAT.exe adsi|aduc|cert|dns|gpo|template|custom target.domain [C:\Windows\System32\custom.msc]";

        private static string AddQuotesIfNeeded(string arg)
        {
            if (arg.StartsWith("/gpobject"))
            {
                return $@"/gpobject:""{arg.Substring(10)}""";
            }

            return arg;
        }

        private static void PrintHelpAndExit()
        {
            Console.WriteLine(_programHelp);
            System.Environment.Exit(-1);
        }

        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintHelpAndExit();
            }

            string commandLine = null;
            
            string subcommand = args[0];

            if (_consoles.ContainsKey(subcommand))
            {
                commandLine = _consoles[subcommand];
            }
            else if (subcommand == "custom" && args.Length == 3)
            {
                commandLine = args[2];
            }

            string domainController = "";

            string targetDomain = args[1];

            if (targetDomain.ToLower().StartsWith("/gpobject:"))
            {
                commandLine = args.Aggregate(_gpmePath, (current, next) => $@"{current} {AddQuotesIfNeeded(next)}");

                Uri uri = new Uri(targetDomain.Substring(10));
                domainController = uri.Host;
                targetDomain = domainController.Substring(domainController.IndexOf('.') + 1);

                Console.WriteLine($"[=] Detected GPO edit action - DC={domainController}, TargetDomain={targetDomain}");
            }

            if (commandLine == null)
            {
                PrintHelpAndExit();
            }

            string channelName = null;

            EasyHook.RemoteHooking.IpcCreateServer<DRSATHook.ServerRpc>(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton);

            string injectionLibrary = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "DRSATHook.dll");

            EasyHook.RemoteHooking.CreateAndInject(_mmcPath, commandLine, 0, EasyHook.InjectionOptions.DoNotRequireStrongName,
                injectionLibrary, injectionLibrary, out var targetPID, new object[] { targetDomain, domainController, channelName });

            Console.WriteLine($"[+] Launched MMC with PID {targetPID}, waiting for process to exit...");

            Process.GetProcessById(targetPID).WaitForExit();
        }
    }
}
