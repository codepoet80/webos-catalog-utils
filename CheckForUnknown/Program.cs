//Check for Unknown Apps
//  Given an app list from the App Museum, generated from a directory listing
//  AND given an inventory from a device, generated from the palm SDK
//  Generate a list of apps on the device that don't exist in the App Museum

//environment stuff
var verbose = true;
var inventoryPath = @"c:\users\" + Environment.UserName + @"\Desktop\Check Apps\";

string[] skipPrefix = { "com.palm.app-museum2", "com.palm.app.calculator", "com.palm.calculator", "com.palm.app.contacts", "com.palm.quickoffice", "com.palm.app.camera", "com.palm.app.notes", "com.palm.app.email", "com.palm.app.searchpreferences", "com.palm.app.usbpassthrough", "com.palm.app.youtube", "com.palm.payment", "com.palm.app.photos", "com.palm.app.musicplayer", "com.palm.app.messaging", "com.palm.app.calendar", "com.palm.app.flashplugin", "com.palm.app.ondevlog", "com.yellowpages.ypmobile.preload", "com.palm.sysapp.voicedial", "com.palm.app.clock", "com.palm.app.phone", "com.palm.app.crotest" };

var skipFiles = new Dictionary<string, string>();
skipFiles.Add("extantList", "ExtantApps.txt");
skipFiles.Add("unmatchedList", "UnmatchedApps.txt");
skipFiles.Add("prewareList", "PrewareApps.txt");
skipFiles.Add("wantedList", "wanted.txt");

string[] prewareUrls = { "http://ipkg.preware.net/feeds/webos-internals/all/Packages", "http://ipkg.preware.net/feeds/precentral/Packages", "http://ipkg.preware.net/feeds/precentral-themes/Packages" };
var wantedUrl = "https://appcatalog.webosarchive.org/wanted.txt";

List<string> knownApps = new List<string>();
List<string> knownAppsExact = new List<string>();

List<string> wantedApps = new List<string>();
List<string> wantedAppsExact = new List<string>();

List<string> matchWanted = new List<string>();

List<string> cleanupCmds = new List<string>();

List<string> manualReview = new List<string>();

List<string> skippedFiles = new List<string>();

//UI stuff
Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("webOS Catalog Compare");
Console.WriteLine("=====================");
Console.WriteLine();

//read extant catalog
//  generate from server like: ls > ExtantApps.txt
if (File.Exists(Path.Combine(inventoryPath, skipFiles["extantList"])))
{
    knownApps = readFileIntoInventory(inventoryPath, skipFiles["extantList"], knownApps, false);
    knownAppsExact = readFileIntoInventory(inventoryPath, skipFiles["extantList"], knownAppsExact, true);
    Console.WriteLine("Count of known apps now: " + knownApps.Count);
}
else
{
    Console.WriteLine("Existing apps list does not exist!");
    Console.WriteLine("Generate on server like: ls webshare/AppPackages > ../ExtantApps.txt");
    Console.WriteLine("Then place in the working directory: " + inventoryPath);
    Console.WriteLine();
    Console.WriteLine("Press a key to proceed...");
    Console.WriteLine();
    Console.ReadKey();
    Environment.Exit(1);
    
}

//read Unmatched list
//  generate from server like: ls > UnmatchedApps.txt
if (File.Exists(Path.Combine(inventoryPath, skipFiles["unmatchedList"])))
{
    knownApps = readFileIntoInventory(inventoryPath, skipFiles["unmatchedList"], knownApps, false);
    knownAppsExact = readFileIntoInventory(inventoryPath, skipFiles["unmatchedList"], knownAppsExact, true);
    Console.WriteLine("Count of known apps now: " + knownApps.Count);
    Console.WriteLine();
}
else
{
    Console.WriteLine("Unmatched apps list does not exist!");
    Console.WriteLine("Generate on server like: ls webshare/Unmatched > ../UnmatchedApps.txt");
    Console.WriteLine("Then place in the working directory: " + inventoryPath);
    Console.WriteLine();
    Console.WriteLine("Press a key to proceed...");
    Console.WriteLine();
    Console.ReadKey();
    Environment.Exit(1);
}

//read Preware catalog
//  seperate tool generated this list from feeds, keep with this repo
File.Delete(Path.Combine(inventoryPath, skipFiles["prewareList"]));
foreach (string prewareFeed in prewareUrls)
{
    var useFileName = prewareFeed.Split('/')[4];
    await getRemoteFile(prewareFeed, Path.Combine(Environment.GetEnvironmentVariable("TEMP"), useFileName));
    await ParsePrewareFeed(Path.Combine(Environment.GetEnvironmentVariable("TEMP"), useFileName), Path.Combine(inventoryPath, skipFiles["prewareList"]));
}
knownApps = readFileIntoInventory(inventoryPath, skipFiles["prewareList"], knownApps, false);
knownAppsExact = readFileIntoInventory(inventoryPath, skipFiles["prewareList"], knownAppsExact, true);
Console.WriteLine("Count of known apps now: " + knownApps.Count);

//read wanted list
await getRemoteFile(wantedUrl, Path.Combine(inventoryPath, skipFiles["wantedList"]));
Console.WriteLine();
wantedApps = readFileIntoInventory(inventoryPath, skipFiles["wantedList"], wantedApps, false);
wantedAppsExact = readFileIntoInventory(inventoryPath, skipFiles["wantedList"], wantedAppsExact, true);
Console.WriteLine("Count of wanted apps now: " + wantedApps.Count);
Console.WriteLine();

//check each inventory file
//  Use palm sdk to generate like: palm-log -l > thisdevice.txt
//  (manually remove any lines from text file that are not app listings)
var foundInventoryFile = false;
foreach (string file in Directory.EnumerateFiles(inventoryPath, "*.txt"))
{
    //Console.WriteLine("Checking inventory: " + Path.GetFileName(file) + " contained? " + skipFiles.ContainsValue(Path.GetFileName(file)));

    if (!skipFiles.ContainsValue(Path.GetFileName(file)))
    {
        var contents = File.ReadAllLines(file);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Checking inventory: " + file + " with " + contents.Length + " apps...");

        foreach (var line in contents)
        {
            if (line != "" && line.ToLower() != "_do not delete.txt" && line.ToLower() != "readme.md")
            {
                //clean up extra info in app listing
                var skip = false;
                var cleanline = line.Replace("*", "");
                cleanline = cleanline.Trim();
                var filename = cleanline;
                var lineParts = cleanline.Split(' ');
                cleanline = lineParts[0];
                lineParts = cleanline.Split('_');
                cleanline = lineParts[0];
                var match = false;
                var closeMatch = "";
                if (knownApps.Contains(cleanline))
                {
                    if (knownAppsExact.Contains(filename))
                    {
                        if (verbose)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Exact match " + filename + " found in Catalog");
                            cleanupCmds.Add("rm " + filename);
                        }
                        match = true;
                    }
                    else
                    {
                        int index = knownAppsExact.FindIndex(str => str.Contains(cleanline));
                        closeMatch = knownAppsExact[index];
                    }
                }
                if (!match)
                {
                    foreach (var prefix in skipPrefix)  //skip well-known apps by prefix (defined above)
                    {
                        if (cleanline.IndexOf(prefix) == 0)
                        {
                            skip = true;
                            if (verbose)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("Skipping " + cleanline + " because prefix: " + prefix);
                                skippedFiles.Add(filename);
                                cleanupCmds.Add("rm " + filename);
                            }
                        }
                    }
                    if (!skip)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        var lineDone = false;
                        Console.Write(line + " NOT previously known...");
                        if (closeMatch != "")
                        {
                            Console.WriteLine(" Close match: " + closeMatch);
                            lineDone = true;
                            cleanupCmds.Add("mv " + filename + " ../OtherVersions");
                        }
                        if (wantedApps.IndexOf(cleanline) != -1)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(" and WANTED: " + line);
                            matchWanted.Add(line);
                            Console.WriteLine();
                            lineDone = true;
                        } 
                        else
                        {
                            if (closeMatch == "") {
                                manualReview.Add(filename);
                                Console.ForegroundColor = ConsoleColor.Magenta;
                                Console.Write( " MANUAL review");
                                Console.WriteLine();
                                lineDone = true;
                            }
                        }
                        if (!lineDone)
                            Console.WriteLine();
                    }
                }
            }
        }
        foundInventoryFile = true;
        Console.WriteLine("Read inventory file:" + Path.GetFileName(file));
        Console.ForegroundColor = ConsoleColor.White;
    }
}
if (!foundInventoryFile)
{
    Console.WriteLine("No inventory file found to check. Generate locally like: ls > inventory.txt");
}

Console.WriteLine();
Console.WriteLine("New Matches from Wanted List: " + matchWanted.Count);
Console.WriteLine("===============================");
foreach (string app in matchWanted)
{
    Console.WriteLine("* " + app);
}

Console.WriteLine();
Console.WriteLine("Skipped files: ");
Console.WriteLine("=======================");
foreach (string file in skippedFiles)
{
    Console.WriteLine(file);
}

Console.WriteLine();
Console.WriteLine("Manual reviews: " + manualReview.Count);
Console.WriteLine("=======================");
foreach (string file in manualReview)
{
    Console.WriteLine(file);
}

Console.WriteLine();
Console.WriteLine("Suggested clean-ups: " + cleanupCmds.Count);
Console.WriteLine("=======================");
foreach (string cmd in cleanupCmds)
{
    Console.WriteLine(cmd);
}

List<string> readFileIntoInventory(string directory, string fileName, List<string> inv, bool exact)
{
    var contents = File.ReadAllLines(Path.Combine(directory, fileName));
    if (!exact)
        Console.WriteLine("Scanning inventory " + Path.Combine(directory, fileName) + " with " + contents.Length + " apps...");
    for (var i = 0; i < contents.Length; i++)
    {
        var newVal = "";
        if (exact)
            newVal = contents[i];
        else
            newVal = contents[i].Split("_")[0];  //remove extra info from appid in filename
        inv.Add(newVal.Trim());
    }
    return inv;
}

async Task getRemoteFile(string url, string filePath)
{
    Console.Write("Loading remote feed: " + Path.GetFileName(filePath));
    using StreamWriter file = new(filePath);
    var result = await new HttpClient().GetStringAsync(url);
    await file.WriteAsync(result);
}

async Task ParsePrewareFeed(string inputPath, string outputPath)
{
    StreamWriter sw = new StreamWriter(outputPath, true);
    var contents = File.ReadAllLines(inputPath);
    var count = 0;
    foreach (var line in contents)
    {
        if (line.IndexOf("Filename:") != -1)
        {
            //parse this line
            var cleanLine = line.Substring(10, line.Length - 10);
            //Console.WriteLine(cleanLine);
            await sw.WriteLineAsync(cleanLine);
            count++;
        }
    }
    sw.Close();
    Console.Write(" parsed " + count + " apps...");
    Console.WriteLine();
}
