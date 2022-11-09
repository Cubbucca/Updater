


using Microsoft.Extensions.Configuration;
using System.Diagnostics;
Console.WriteLine("Update Starting Soon...");
if (Update())
{
    Console.WriteLine("Enter to Continue:");
    Console.ReadKey();
    Process.Start(AppSet("ExeName"));
    Environment.Exit(0);
}

bool Update()
{
    var currentDir = Environment.CurrentDirectory;
    if (!File.Exists("appsettings.json")) throw new Exception("appsettings not found");
    Console.WriteLine("Settings Loading...");
    var target = AppSet("ExeName");
    Console.WriteLine(target);
    var source = AppSet("UpdateFrom");
    Console.WriteLine(source);
    Console.WriteLine("Checking File Versions ...");
    if (VersionMatch($"{source}{target}", target)) return true;
    CopyFolderContents(source, currentDir,"*.*", true, true);
    return true;
}

bool VersionMatch(string source, string target)
{
    var filepath = source;
    if (!File.Exists(target))
    {
        Console.WriteLine($"No Local Exe, Updated Will Now Install It");
        return false;
    }
    if (File.Exists(filepath))
    {
        var updateinfo = FileVersionInfo.GetVersionInfo(filepath).FileVersion;
        var currentinfo = FileVersionInfo.GetVersionInfo(target).FileVersion;
        if (updateinfo != currentinfo)
        {
            Console.WriteLine($"Version miss match current:{currentinfo} @source:{updateinfo}");
            return false;
        }
        Console.WriteLine($"{currentinfo} is already latest Version");
        return true;
    }
    throw new Exception("source can not be read");
}

static string AppSet(string key)
{
    IConfiguration configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();
    return configuration[key];
}

void CopyFolderContents(string sourceFolder, string destinationFolder, string mask, Boolean createFolders, Boolean recurseFolders)
{
    try
    {
        Console.WriteLine($"CopyFolder Started...");
        if (!sourceFolder.EndsWith(@"\")) { sourceFolder += @"\"; }
        if (!destinationFolder.EndsWith(@"\")) { destinationFolder += @"\"; }

        var exDir = sourceFolder;
        var dir = new DirectoryInfo(exDir);
        SearchOption so = (recurseFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        Console.WriteLine($"CopyFolder Loop...");
        foreach (string sourceFile in Directory.GetFiles(dir.ToString(), mask, so))
        {
            
            FileInfo srcFile = new FileInfo(sourceFile);
            string srcFileName = srcFile.Name;
            Console.WriteLine($"CopyFolder srcFileName...");
            if (srcFileName == "appsettings.json" || srcFileName == "Microsoft.Extensions.Configuration.dll") continue;
            Console.WriteLine($"Checking - {srcFileName}");
            // Create a destination that matches the source structure
            FileInfo destFile = new FileInfo(destinationFolder + srcFile.FullName.Replace(sourceFolder, ""));

            if (!Directory.Exists(destFile.DirectoryName) && createFolders)
            {
                Console.WriteLine($"Creating Dir - {destFile.DirectoryName}");
                Directory.CreateDirectory(destFile.DirectoryName!);
            }

            if (!destFile.Exists || srcFile.LastWriteTime > destFile.LastWriteTime)
            {
                Console.WriteLine($"Copying File - {destFile.FullName}");
                File.Copy(srcFile.FullName, destFile.FullName, true);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        System.Diagnostics.Debug.WriteLine(ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace);
    }
}