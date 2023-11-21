using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExoticManager
{

    internal class Program
    {
        const string VERSION = "v5.0";
        static string AccountManger = "";
        private static bool Authorization = false;
        public string PCNAME = Dns.GetHostName();

        public static bool CloseIfMemoryLow = false;

        public static void Saymessage(string infomation, string textinfomation)
        {
            Console.Write("[");
            Console.Write(infomation, (object)(Console.ForegroundColor = ConsoleColor.Cyan));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("] " + textinfomation);
            Console.ResetColor();
        }

        public static void Saymessage2(string infomation, string textinfomation)
        {
            Console.Write("[");
            Console.Write(infomation, (object)(Console.ForegroundColor = ConsoleColor.Red));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("] " + textinfomation);
        }

        public static void Saymessage3(string infomation, string textinfomation)
        {
            Console.Write("[");
            Console.Write(infomation, (object)(Console.ForegroundColor = ConsoleColor.Green));
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("] " + textinfomation);
            Console.ResetColor();
        }

        public static void Check_Whitelist(string Hwid_Check)
        {
            if (new WebClient() { Proxy = ((IWebProxy)null) }.DownloadString("https://pastebin.com/tHKGxHJA").Contains(Hwid_Check))
            {
                Program.Saymessage("INFO", "Whitelist.");
                Thread.Sleep(2000);
                Console.Clear();
                Program.Authorization = true;
            }
            else
            {
                Program.Saymessage2("INFO", "Not Found Whitelist.");
                Thread.Sleep(2000);
                Console.Clear();
                Program.Authorization = false;
            }
        }
        static void WaitForProcess()
        {
            ManagementEventWatcher startWatch = new ManagementEventWatcher(
              new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            startWatch.EventArrived
                                += new EventArrivedEventHandler(OnProcessStarted);
            startWatch.Start();
        }
        static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(chars.Length);
                stringBuilder.Append(chars[index]);
            }

            return stringBuilder.ToString();
        }

        private static object processLock = new object(); 
        static string ExecuteCommand(string command)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "cmd.exe";
            processStartInfo.Arguments = command;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;
            Process process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
            string text = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return text;
        }
        static bool IsRobloxTabInGame(Process process) 
        {
            string Command = "/C netstat -ano | find \"" + process.Id.ToString()+"\"";
            string output = ExecuteCommand(Command);
            return output.Contains("UDP");
        }
        static void OnProcessStarted(object sender, EventArrivedEventArgs e)
        {
            if (!Commands["AutoInject"])
            {
                return;
            }
            lock (processLock)
            {
                try
                {
                    if (e.NewEvent.Properties["ProcessName"].Value.ToString() == "Windows10Universal.exe")
                    {
                        Thread.Sleep(120000);

                        string id = e.NewEvent.Properties["ProcessId"].Value.ToString();

                        Process process = Process.GetProcessById(int.Parse(id));

                        string FluxusPath = GetFluxusPath(process);
                        Thread thread = new Thread(() => {
                            Thread.Sleep(1000);

                            if (!string.IsNullOrEmpty(FluxusPath))
                            {
                                DllInject.Inject(process, FluxusPath);
                            }
                            Thread.Sleep(2000);
                        });
                        thread.IsBackground = true;
                        thread.Start();
                    }
                }
                catch (Exception)
                {

                }
            }
        }
        static async void HttpServer()
        {
            // Mảng chứa địa chỉ Http lắng nghe
            // http =  giao thức http, * = ip bất kỳ, 8080 = cổng lắng nghe
            string[] prefixes = new string[] { "http://localhost:4953/" };

            HttpListener listener = new HttpListener();

            if (!HttpListener.IsSupported) throw new Exception("Support HttpListener.");

            if (prefixes == null || prefixes.Length == 0) throw new ArgumentException("prefixes");

            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }

            Console.WriteLine("Server start ...");


            listener.Start();

           
            do
            {
          
                HttpListenerContext context = await listener.GetContextAsync();
                await Console.Out.WriteLineAsync(context.Request.Url.LocalPath);
       
                if (context.Request.Url.LocalPath == "/JoinServer")
                {
                   // JoinGame(context.Request.Url.Query);
                }
                var response = context.Response;                                       
                var outputstream = response.OutputStream;                              

                context.Response.Headers.Add("content-type", "text/html");              
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Hello world!");    
                response.ContentLength64 = buffer.Length;
                await outputstream.WriteAsync(buffer, 0, buffer.Length);                  
                outputstream.Close();

            }
            while (listener.IsListening);
        }
        static long GetTime()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        const string RAMPORT = "7963";
        const string RAMPASS = "exotic";

        public static HttpClient HttpClient = new HttpClient();
        static Dictionary<string, int> FailJoin = new Dictionary<string, int>();
        static string HttpGet(string url) {
            return (HttpClient.GetAsync(url).Result.Content.ReadAsStringAsync().Result);
        }
        static string JoinGame(string accname, string placeid, string jobid)
        {
            string url = "http://localhost:" + RAMPORT + "/LaunchAccount?Password=" + RAMPASS + "&Account=" + accname + "&PlaceId=" + placeid;
            if (!string.IsNullOrEmpty(jobid))
            {
                url += "&JobId=" + jobid;
            }
            return (HttpClient.GetAsync(url).Result.Content.ReadAsStringAsync().Result);
        }
        static Dictionary<string,string> CachedHash = new Dictionary<string,string>();
        static string GetFluxusPath(Process process)
        {
            string FluxusDlls = "C:\\Program Files (x86)";

            string file;
            if (CachedHash.ContainsKey(process.MainModule.FileName))
            {
                file = CachedHash[process.MainModule.FileName];
            }
            else
            {
                file = CalculateSHA384(process.MainModule.FileName);
                CachedHash.Add(process.MainModule.FileName, file);
            }
            if (File.Exists(Path.Combine(FluxusDlls,file+".dll")))
            {
                return Path.Combine(FluxusDlls, file + ".dll");
            }
            string Url = "https://flux.li/windows/external/get_dll_hash.php?hash="+ file;
            string DownloadUrl = HttpGet(Url);
            if (!string.IsNullOrEmpty(DownloadUrl))
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(DownloadUrl, file+".dll");
                    Thread.Sleep(100);
                    var fs = File.GetAccessControl(file + ".dll");
                    fs.SetAccessRuleProtection(false, false);
                    File.Move(file + ".dll", Path.Combine(FluxusDlls, file + ".dll"));
                    File.SetAccessControl(Path.Combine(FluxusDlls, file + ".dll"), fs);
                    Thread.Sleep(100);
                    return Path.Combine(FluxusDlls, file + ".dll");
                }
            }
            return "";
        }
        static string CalculateSHA384(string filePath)
        {
            using (var sha384 = SHA384.Create())
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] hashBytes = sha384.ComputeHash(fileStream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
        }
        static string[] GetRobloxAccsAppData() 
        {
            List<string> folder = new List<string>();
            string[] folders = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Packages", "*", SearchOption.TopDirectoryOnly);
            foreach (var item in folders)
            {
                if (item.Contains("ROBLOXCORPORATION.ROBLOX"))
                {
                    folder.Add(item);
                    folder.Add(item);
                }
            }
            return folder.ToArray();
        }
        static Dictionary<string, bool> LoadConfig(Dictionary<string, bool> Config, string filename)
        {
            Dictionary<string, bool> ReturnedConfig;
            try
            {
                ReturnedConfig = JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText(filename));
            }
            catch (Exception)
            {
                return Config;
            }
            if (ReturnedConfig == null)
            {
                return Config;
            }
            foreach (var item in Config)
            {
                if (!ReturnedConfig.ContainsKey(item.Key))
                {
                    ReturnedConfig.Add(item.Key, item.Value);
                }
            }
            return ReturnedConfig;
        }
        static void SaveConfig(Dictionary<string, bool> Config, string filename)
        {
            File.WriteAllText(filename, JsonConvert.SerializeObject(Config, Formatting.Indented));
        }
        static void DisplayHelp(Dictionary<string, bool> Commands)
        {
            //Console.WriteLine("  - Use custom Join function: AccountManagerSupporter.JoinServer(UserName, PlaceId, JobId)");
        }
        static Dictionary<string, bool> Commands = new Dictionary<string, bool>()
        {
            ["deletecache"] = false,
            ["fluxuskey"] = false,
            ["check"] = false,
        };
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        private const int WM_CLOSE = 0x0010;

        [DllImport("user32", CharSet = CharSet.Unicode)]
        private static extern
         IntPtr SendMessage(
                 IntPtr handle,
                 int Msg,
                 IntPtr wParam,
                 IntPtr lParam
          );
        static void OpenAccountTab(string AccountFolder)
        {
            Process.Start("explorer.exe", "shell:appsFolder\\"+ AccountFolder+"!App");
        }
        static void Main(string[] args)
        {
           Console.SetWindowSize(80, 30);
           string str = WindowsIdentity.GetCurrent().User.Value;
           string PCNAME = Dns.GetHostName();
           Program.Check_Whitelist(str);
            if (Program.Authorization)
            {
                string Varriable = "";
                AccountManger = Environment.CurrentDirectory;
                Console.Title = "Exotic Tool - " + VERSION;
                Console.WriteLine("Exotic Tool - made by exotic9160");
                Console.WriteLine("");
                Console.BackgroundColor = ConsoleColor.Magenta;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" All Function Commands ");
                Console.ResetColor();
                Console.WriteLine("");
                Program.Saymessage3("1", "func" + "            |   show the current status of all function.");
                Program.Saymessage3("2", "profile" + "         |   user info.");
                Program.Saymessage3("3", "kill" + "            |   kill all roblox process");
                Program.Saymessage3("4", "check" + "           |   check all roblox process running (true/false)");
                Program.Saymessage3("5", "fluxuskey" + "       |   auto get key fluxus (true/false)");
                Program.Saymessage3("6", "deletecache" + "     |   auto delete roblox cache every 24 hours (true/false)");
                Program.Saymessage3("7", "updateroblox" + "    |   update all roblox instance to the last version.\n");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.ResetColor();
                ;
                Thread thread = new Thread(() =>
                {
                    //HttpServer();
                });
                thread.IsBackground = true;
                thread.Start();
                WaitForProcess();
                string[] folders = GetRobloxAccsAppData();
                long LastDelete = 0;
                long LastKey = 0;
                long LastCheckMainTab = 0;
                string ConfigFileName = "ExoticToolConfig.json";
                dynamic accs = JsonConvert.DeserializeObject(File.ReadAllText(AccountManger + "\\AccountControlData.json"));
                Commands = LoadConfig(Commands, ConfigFileName);
                Thread GetCommand = new Thread(() =>
                {
                    while (true)
                    {
                        string InputCommand = Console.ReadLine();
                        string[] Splited = InputCommand.Split(' ');
                        string Command = Splited[0];
                        switch (Command)
                        {
                            case "func":
                                foreach (var item in Commands)
                                {
                                    Program.Saymessage("INFO", "" + item.Key + " - " + item.Value);
                                }
                                break;
                            case "profile":
                                Program.Saymessage("INFO", "Username - " + PCNAME);
                                Program.Saymessage("INFO", "Hwid - " + str);
                                Program.Saymessage("INFO", "Expiration - " + "Lifetime key");
                                break;
                            case "updateroblox":
                                string OriginalPath = "";
                                foreach (var item in Directory.GetDirectories("C:\\Program Files\\WindowsApps"))
                                {
                                    if (item.Contains("ROBLOXCORPORATION.ROBLOX") && File.Exists(Path.Combine(item, "Windows10Universal.exe")))
                                    {
                                        OriginalPath = item;
                                    }
                                }
                                if (!string.IsNullOrEmpty(OriginalPath))
                                {
                                    Console.WriteLine(Path.Combine(AccountManger, "UWP_Instances", "Windows10Universal.exe"));
                                    foreach (var item in Directory.GetDirectories(Path.Combine(AccountManger, "UWP_Instances")))
                                    {
                                        if (File.Exists(Path.Combine(item, "Windows10Universal.exe")))
                                        {
                                            File.Copy(Path.Combine(item, "Windows10Universal.exe"), Path.Combine(AccountManger, "UWP_Instances", "Windows10Universal.exe"), true);
                                            Console.WriteLine("Updated - " + (new FileInfo(item)).Name);
                                        }
                                    }
                                    Console.WriteLine("Updated all instance");
                                }
                                else
                                {
                                    Console.WriteLine("Could not find last version");
                                }
                                break;
                            case "kill":
                                foreach (var item in Process.GetProcessesByName("Windows10Universal"))
                                        {
                                            item.Kill();
                                        }
                                break;
                            default:
                                if (Commands.ContainsKey(Command))
                                {
                                    if (Splited.Length < 2 || !(Splited[1] == "true" || Splited[1] == "false"))
                                    {
                                        Console.WriteLine("Invalid Value");
                                    }
                                    else
                                    {
                                        bool Val = bool.Parse(Splited[1]);
                                        Commands[Splited[0]] = Val;
                                        Console.WriteLine("Set " + Splited[0] + " - " + Splited[1]);
                                        SaveConfig(Commands, ConfigFileName);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Invalid Command");
                                }
                                break;
                        }
                    }
                });
                GetCommand.IsBackground = true;
                GetCommand.Start();
                while (true)
                {
                    try
                    {
                        if (GetTime() - LastCheckMainTab > 30)
                        {
                            foreach (var item in Process.GetProcessesByName("Windows10Universal"))
                            {
                               
                                if (DateTime.Now - item.StartTime > TimeSpan.FromSeconds(60))
                                {
                                    if (!IsRobloxTabInGame(item))
                                    {
                                        item.Kill();
                                    }
                                }

                                if ((DateTime.Now - item.StartTime).TotalSeconds > 30) // Roblox shouldn't take that long to startup, right? Surely nobody will be using a potato with these settings.
                                {
                                    if (CloseIfMemoryLow && item.WorkingSet64 / 1024 / 1024 < 600)
                                        item.Kill();
                                }
                            }
                            LastCheckMainTab = GetTime();
                        }
                    }
                    catch (Exception)
                    {
                    }
             
                    try
                        {
                        var handle = FindWindow(null, "Fluxus");
                        if (handle != IntPtr.Zero)
                        {
                            SendMessage(handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                        }
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    try
                    {
                        if (Process.GetProcessesByName("Roblox Account Manager").Length == 0)
                        {
                                Process.Start(Path.Combine(AccountManger, "Roblox Account Manager.exe"));
                                Thread.Sleep(1000);
                        }

                        if (GetTime() - LastDelete > 3600 * 24)
                        {
                            if (Commands["deletecache"])
                            {
                                Console.WriteLine("Deleting Cache");
                                Thread.Sleep(5000);
                                LastDelete = GetTime();
                               // foreach (var item in Process.GetProcessesByName("Windows10Universal"))
                               // {
                              //     item.Kill();
                             //   }
                                Thread.Sleep(1000);

                                foreach (var item in folders)
                                {
                                    if (item.Contains("ROBLOXCORPORATION.ROBLOX"))
                                    {
                                        try
                                        {
                                            Directory.Delete(item + "\\LocalState", true);
                                        }
                                        catch (Exception e)
                                        {
                                        }
                                    }
                                }
                                Console.WriteLine("Deleted");
                            }
                        }
                        if (GetTime() - LastKey > 3600 * 24)
                        {
                            if (Commands["fluxuskey"])
                            {
                                Console.WriteLine("Starting Bypassing");
                                Thread.Sleep(5000);
                                LastKey = GetTime();
                                Thread.Sleep(1000);
                                Process.Start(Path.Combine(AccountManger, "Exotic", "bypasser", "devn", "__pycache__", "__init__.cpython-311.py"));
                                Thread.Sleep(60000);
                                Console.WriteLine("Bypassed");
                            }
                        }
                        if (Commands["check"])
                        {
                            if (Process.GetProcessesByName("ExoticChecker").Length == 0)
                            { 
                             Process.Start(Path.Combine(AccountManger, "Exotic", "ExoticChecker.exe"));
                             Thread.Sleep(1000);
                            }
                        }

                        Thread.Sleep(1000);
                        foreach (Process process in Process.GetProcessesByName("cmd"))
                        {
                            Thread.Sleep(4000);
                            process.Kill();
                        }
                    }

                    catch (Exception e)
                    {
                    }
                }
            }
            else
            {
                Program.Saymessage2("LOG", "Not Found Whitelist!!");
                Thread.Sleep(500);
                Program.Saymessage3("HWID", str);
                Console.WriteLine("Press any key to continue . . .");
                Console.ReadKey();
            }
        }
    }
}
