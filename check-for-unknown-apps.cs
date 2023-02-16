//Check for Unknown Apps
//  Given an app list from the App Museum, generated from a directory listing
//  AND given an inventory from a device, generated from the palm SDK
//  Generate a list of apps on the device that don't exist in the App Museum

//environment stuff
var verbose = false;
var inventoryPath = @"c:\users\" + Environment.UserName + @"\Desktop\Check Apps\";
var extantFile = "extant.txt";
string[] skipPrefix = { "org.webosinternals", "ca.canucksoftware", "com.palm.app-museum2", "com.palm.app.calculator", "com.palm.calculator", "com.palm.app.contacts", "com.palm.quickoffice", "com.palm.app.camera", "com.palm.app.notes", "com.palm.app.email", "com.palm.app.searchpreferences", "com.palm.app.usbpassthrough", "com.palm.app.youtube", "com.palm.payment", "com.palm.app.photos", "com.palm.app.musicplayer", "com.palm.app.messaging", "com.palm.app.calendar", "com.palm.app.flashplugin", "com.palm.app.ondevlog", "com.yellowpages.ypmobile.preload", "com.palm.sysapp.voicedial", "com.palm.app.clock", "com.palm.app.phone", "com.palm.app.crotest" };

//UI stuff
Console.WriteLine("webOS Catalog Compare");
Console.WriteLine("=====================");
Console.WriteLine();

//read extant catalog
//  generate from server like: ls > extant.txt
var apps = File.ReadAllLines(inventoryPath + extantFile);
for (var i = 0; i < apps.Length; i++)
{
    var line = apps[i].Split("_");  //remove extra info from appid in filename
    apps[i] = line[0].Trim();
}
Console.WriteLine("Comparing to catalog of " + apps.Length + " apps...");
Console.WriteLine();

//check each inventory file
//  Use palm sdk to generate like: palm-log -l > thisdevice.txt
//  (manually remove any lines from text file that are not app listings)
foreach (string file in Directory.EnumerateFiles(inventoryPath, "*.txt"))
{
    if (!file.Contains(extantFile))
    {
        var contents = File.ReadAllLines(file);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Checking inventory: " + file + " with " + contents.Length + " apps...");

        foreach (var line in contents)
        {
            //clean up extra info in app listing
            var skip = false;
            var cleanline = line.Replace("*", "");
            cleanline = cleanline.Trim();
            var lineParts = cleanline.Split(' ');
            cleanline = lineParts[0];
            if (apps.Contains(cleanline))
            {
                if (verbose)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(cleanline + " found in Catalog");
                }
            }
            else
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
                        }
                    }
                }
                if (!skip)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(cleanline + " NOT found in Catalog");
                }
            }
        }
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
    }
}