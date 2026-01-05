


using Microsoft.Extensions.Configuration;
using System.Diagnostics;
Console.WriteLine("Update Starting Soon...");
try
{
    if (await Update())
    {
        Console.WriteLine("App will start in 3 Seconds...");
        await Task.Delay(3000);
        Process.Start(AppSet("ExeName"));
        Environment.Exit(0);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Update Failed: {ex.Message}");
}
finally
{
    Console.WriteLine("Press Any Key To Exit");
    Console.ReadKey();
    Environment.Exit(0);
}


async Task<bool> Update()
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
    await CopyFolderContents(source, currentDir, "*.*", true, true);
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
    return configuration[key] ?? throw new Exception($"appsetting key: {key} not found");
}

async Task CopyFolderContents(string sourceFolder, string destinationFolder, string mask, Boolean createFolders, Boolean recurseFolders)
{
    Console.WriteLine($"CopyFolder Started...");
    if (!sourceFolder.EndsWith('\\')) { sourceFolder += '\\'; }
    if (!destinationFolder.EndsWith('\\')) { destinationFolder += '\\'; }

    var exDir = sourceFolder;
    var dir = new DirectoryInfo(exDir);
    SearchOption so = (recurseFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
    Console.WriteLine($"CopyFolder Loop...");
    foreach (string sourceFile in Directory.GetFiles(dir.ToString(), mask, so))
    {

        FileInfo srcFile = new(sourceFile);
        string srcFileName = srcFile.Name;
        //if (srcFileName == "appsettings.json") continue;
        Console.WriteLine($"Checking - {srcFileName}");
        // Create a destination that matches the source structure
        FileInfo destFile = new(destinationFolder + srcFile.FullName.Replace(sourceFolder, ""));

        if (!Directory.Exists(destFile.DirectoryName) && createFolders)
        {
            Console.WriteLine($"Creating Dir - {destFile.DirectoryName}");
            Directory.CreateDirectory(destFile.DirectoryName!);
        }
        var destfileexists = destFile.Exists;
        if (!destfileexists || srcFile.LastWriteTime > destFile.LastWriteTime)
        {
            if (destfileexists)
            {
                FileAttributes attrs = File.GetAttributes(destFile.FullName);
                if ((attrs & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    Console.WriteLine($"Readonly File Detected - {destFile.FullName}");
                    File.SetAttributes(destFile.FullName, attrs & ~FileAttributes.ReadOnly);
                }
            }
            Console.WriteLine($"Copying File - {destFile.FullName}");
            try
            {
                File.Copy(srcFile.FullName, destFile.FullName, true);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access Denied Copying - {destFile.FullName}");
                await Task.Delay(2000);
            }
        }
    }
}