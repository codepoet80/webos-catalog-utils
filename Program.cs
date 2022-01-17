using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Schema;

namespace webOS.AppCatalog
{
    class Program
    {
        #region Environment and UI

        //TODO: These need to come from a config file
        public static string workingDir = Path.Combine(Directory.GetCurrentDirectory(), "../../../");
        public static string appCatalogDir = Path.Combine(workingDir, "../_webOSAppCatalog");  //Change this path to point to where the archive lives
        public static string catalogFile;

        public static string incomingDir = "Incoming";
        public static string appPackageDir = "AppPackages";
        public static string appMetaDir = "AppMetaData";
        public static string appImageDir = "AppImages";
        public static string appReviewDir = "AppReviews";
        public static string appUpdateDir = "AppUpdates";
        public static string appUnknownDir = "AppUnindexed";
        public static string appMatchedDir = "AppMatched";
        public static string appAlternateDir = "AppAlternates";
        public static string reportsDir = "Reports";

        public static string catalogIndexReport = @"CatalogIndexResults.csv";
        public static string MetaReverseReport = @"MetaReverseReport.csv";
        public static string FileReverseReport = @"FileReverseReport.csv";
        public static string ImageSortReport = @"ImageSortReport.csv";
        public static string MissingIconReport = @"MissingIcons.csv";
        public static string MissingPackageInfoReport = @"MissingPackageInfo.csv";
        public static string RequestedPackageReport = @"Requested.csv";

        public static void Main(string[] args)
        {
            //Figure out our base folder
            appCatalogDir = Path.GetFullPath(appCatalogDir);
            Console.WriteLine("Current Destination Folder Base: " + appCatalogDir);
            Console.Write("Enter alternate path, or press enter: ");
            string strInputDir = Console.ReadLine();
            if (strInputDir.Length > 1)
                appCatalogDir = strInputDir;
            Console.WriteLine("Using: " + appCatalogDir);
            if (!Directory.Exists(appCatalogDir))
            {
                Console.WriteLine("Warning: Destination folder not found, will attempt to create!");
                Directory.CreateDirectory(appCatalogDir);
            }
            Console.WriteLine();

            //Read Master App Data (aka the Catalog) into memory
            catalogFile = Path.Combine(appCatalogDir, "masterAppData.json");
            Console.WriteLine("Current Catalog File: " + catalogFile);
            Console.WriteLine("Enter alternate path, or press enter: ");
            string strInputFile = Console.ReadLine();
            if (strInputFile.Length > 1 && File.Exists(strInputFile))
                catalogFile = strInputFile;
            Console.WriteLine("Reading Catalog index from: " + catalogFile);
            List<AppDefinition> appCatalog = ReadCatalogFile(catalogFile);
            Console.WriteLine("Apps in catalog:" + appCatalog.Count.ToString());
            Console.WriteLine();

            //Figure out folders
            incomingDir = Path.Combine(appCatalogDir, incomingDir);
            appPackageDir = Path.Combine(appCatalogDir, appPackageDir);
            if (!Directory.Exists(appPackageDir))
                Console.WriteLine("Warning: AppPackage folder not found. The program will likely crash.");
            appMetaDir = Path.Combine(appCatalogDir, appMetaDir);
            if (!Directory.Exists(appMetaDir))
                Console.WriteLine("Warning: AppMetadata folder not found. The program will likely crash.");
            appImageDir = Path.Combine(appCatalogDir, appImageDir);
            if (!Directory.Exists(appImageDir))
                Console.WriteLine("Warning: AppImages folder not found. The program will likely crash.");
            appReviewDir = Path.Combine(appCatalogDir, appUpdateDir);
            appUpdateDir = Path.Combine(appCatalogDir, appUpdateDir);
            appUnknownDir = Path.Combine(appCatalogDir, appUnknownDir);
            appMatchedDir = Path.Combine(appCatalogDir, appMatchedDir);
            appAlternateDir = Path.Combine(appCatalogDir, appUnknownDir);

            //Figure out report paths
            reportsDir = Path.Combine(workingDir, reportsDir);
            MetaReverseReport = Path.Combine(reportsDir, MetaReverseReport);
            FileReverseReport = Path.Combine(reportsDir, FileReverseReport);
            ImageSortReport = Path.Combine(reportsDir, ImageSortReport);
            MissingIconReport = Path.Combine(reportsDir, MissingIconReport);
            MissingPackageInfoReport = Path.Combine(reportsDir, MissingPackageInfoReport);
            RequestedPackageReport = Path.Combine(reportsDir, RequestedPackageReport);

            //Ask the user what they want to do
            ShowMenu();
            while(GetMenuChoice(appCatalog))
            {
                Console.WriteLine(); 
                ShowMenu();
            }
        }

        public static void ShowMenu()
        {
            //Show Menu
            Console.WriteLine();
            Console.WriteLine("Select function...");
            Console.WriteLine();
            Console.WriteLine("1) Index packages in folder to destination");
            Console.WriteLine("2) Strip leading catalog numbers in folder to subfolder");
            Console.WriteLine("3) Reverse catalog check from metadata");
            Console.WriteLine("4) Update Metadata JSON");
            Console.WriteLine("5) Reverse catalog check from folder");
            Console.WriteLine("6) Generate catalog files from extant apps");
            Console.WriteLine("7) Scrape icons from web to destination");
            Console.WriteLine("8) Search local folder and Internet sources for images in metadata");
            Console.WriteLine("9) Build missing package with possible match report from folder");
            Console.WriteLine("F) Fix Pre 2 Compatibility metadata");
            Console.WriteLine("H) Find and ingest highest update version, including metadata update");
            Console.WriteLine("L) Add New Metadata");
            Console.WriteLine("M) Stage approved matches and update metadata files");
            Console.WriteLine("N) Find next available catalog number, optionally insert new");
            Console.WriteLine("D) Remove duplicate AppUpdates from AppUnknown");
            Console.WriteLine("S) Find and insert star ratings");
            Console.WriteLine("T) Search for catalog matches from Text file");
            Console.WriteLine("X) Search for Exhibition apps");
            Console.WriteLine("Q) Quit");
            Console.WriteLine();
            Console.Write("Selection: ");
        }

        public static bool GetMenuChoice(List<AppDefinition> appCatalog)
        {
            ConsoleKeyInfo choice = Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            switch (choice.Key)
            {
                case ConsoleKey.D1: //Index packages against metadata to destination folder
                    {
                        string searchFolder = incomingDir;
                        Console.WriteLine("Current Search Folder: " + searchFolder);
                        Console.WriteLine("Enter alternate path, or press enter: ");
                        string strInputPath = Console.ReadLine();
                        if (strInputPath.Length > 1 && Directory.Exists(strInputPath))
                            searchFolder = strInputPath;

                        Console.WriteLine("Indexing: " + searchFolder + " to: " + appPackageDir);
                        IndexFolder(appCatalog, searchFolder);
                        return true;
                    }
                case ConsoleKey.D2: //Strip filename prefixes into subfolder
                    {
                        string searchFolder = incomingDir;
                        Console.WriteLine("Current Search Folder: " + searchFolder);
                        Console.WriteLine("Enter alternate path, or press enter: ");
                        string strInputPath = Console.ReadLine();
                        if (strInputPath.Length > 1 && Directory.Exists(strInputPath))
                            searchFolder = strInputPath;

                        Console.WriteLine("Stripping file prefixes in : " + searchFolder + " to: " + Path.Combine(searchFolder, "stripped"));
                        StripPrefixFromFilenames(searchFolder);
                        return true;
                    }
                case ConsoleKey.D3: //Reverse check catalog from metadata
                    {
                        ReverseCatalogFromMetaData(appCatalog);
                        return true;
                    }
                case ConsoleKey.D4: //Update catalog from metadata
                    {
                        UpdateMetaFileJSONStructure();
                        return true;
                    }
                case ConsoleKey.D5: //Reverse check catalog from folder
                    {
                        string searchFolder = appPackageDir;
                        Console.WriteLine("Current Search Folder: " + searchFolder);
                        Console.WriteLine("Enter alternate path, or press enter: ");
                        string strInputPath = Console.ReadLine();
                        if (strInputPath.Length > 1 && Directory.Exists(strInputPath))
                            searchFolder = strInputPath;

                        Console.WriteLine("Comparing: " + searchFolder + " contents to: " + appMetaDir);
                        ReverseCatalogFromFolder(searchFolder);
                        return true;
                    }
                case ConsoleKey.D6: //Generate extant Catalog
                    {
                        Console.WriteLine("Catalog extant files to: " + appCatalogDir);
                        Console.WriteLine("Catalog missing files to: " + appCatalogDir);
                        Console.WriteLine();
                        GenerateExtantCatalog(appCatalog);
                        return true;
                    }
                case ConsoleKey.D7: //Scrape Folder for Icons
                    {
                        GetAppIconsFromCatalog(appCatalog);
                        return true;
                    }
                case ConsoleKey.D8: //Search multiple sources for Images
                    {
                        string searchFolder = Path.Combine(appCatalogDir, "_AppImages");
                        Console.WriteLine("Current Search Folder: " + searchFolder);
                        Console.WriteLine("Enter alternate path, or press enter: ");
                        string strInputPath = Console.ReadLine();
                        if (strInputPath.Length > 1 && Directory.Exists(strInputPath))
                            searchFolder = strInputPath;

                        SearchForScreenshotsFromMetaData(searchFolder);
                        return true;
                    }
                case ConsoleKey.D9: //Build missing app info report
                    {
                        string missingMasterDataFile = catalogFile.Replace("master", "Missing");

                        string searchFolder = appUnknownDir;
                        Console.WriteLine("Current Search Folder: " + searchFolder);
                        Console.WriteLine("Enter alternate path, or press enter: ");
                        string strInputPath = Console.ReadLine();
                        if (strInputPath.Length > 1 && Directory.Exists(strInputPath))
                            searchFolder = strInputPath;

                        BuildMissingPackageInfoReport(missingMasterDataFile, searchFolder);
                        return true;
                    }
                case ConsoleKey.L:
                    {
                        Console.WriteLine("Adding New Metadata to Master catalog file...");
                        AddNewMetaData(catalogFile);
                        System.Threading.Thread.Sleep(1000);
                        string extantDataFile = catalogFile.Replace("master", "extant");
                        Console.WriteLine("Adding New Metadata to Extant catalog file...");
                        AddNewMetaData(extantDataFile);
                        System.Threading.Thread.Sleep(1000);
                        return true;
                    }
                case ConsoleKey.M:
                    {
                        StageAndUpdateApprovedMatches(appCatalog, Path.Combine(appCatalogDir, "ApprovedMatches.csv"));
                        return true;
                    }
                case ConsoleKey.N:
                    {
                        appCatalog = FindNextAvailCatalogNum(appCatalog);
                        return true;
                    }
                case ConsoleKey.D:
                    {
                        RemoveAppUpdateDuplicatesFromAppUnknown();
                        return true;
                    }
                case ConsoleKey.F:
                    {
                        string origMasterData = Path.Combine(appCatalogDir, "Pre2AppData.json");
                        Console.WriteLine("Original Master Data: " + origMasterData);
                        Console.WriteLine("Enter alternate path, or press enter: ");
                        string strInputPath = Console.ReadLine();
                        if (strInputPath.Length > 1 && File.Exists(strInputPath))
                            origMasterData = strInputPath;

                        FixPreTwoCompat(origMasterData);
                        return true;
                    }
                case ConsoleKey.H:
                    {
                        FindHighestUpdateVersion();
                        return true;
                    }
                case ConsoleKey.S:
                    {
                        string ratingsDir = appReviewDir;
                        Console.WriteLine("Current Reviews Folder: " + ratingsDir);
                        Console.WriteLine("Enter alternate path, or press enter: ");
                        string strInputPath = Console.ReadLine();
                        if (strInputPath.Length > 1 && Directory.Exists(strInputPath))
                            ratingsDir = strInputPath;

                        FindAndInsertStarRating(ratingsDir);
                        return true;
                    }
                case ConsoleKey.T:
                    {
                        string wantedListFile = Path.Combine(workingDir, reportsDir, "wanted.txt");
                        string dirListFile = Path.Combine(appCatalogDir, "DirListing.txt");
                        
                        Console.WriteLine("Wanted List file: " + wantedListFile);
                        Console.WriteLine("Enter alternate path, or press enter: ");
                        string strInputPath = Console.ReadLine();
                        if (strInputPath.Length > 1 && Directory.Exists(strInputPath))
                            wantedListFile = strInputPath;
                        Console.WriteLine();
                        Console.WriteLine("New Dir List file: " + dirListFile);
                        Console.WriteLine("Enter alternate path, or press enter: ");
                        strInputPath = Console.ReadLine();
                        if (strInputPath.Length > 1 && Directory.Exists(strInputPath))
                            dirListFile = strInputPath;

                        CompareDirListToWantedList(wantedListFile, dirListFile);
                        return true;
                    }
                case ConsoleKey.X:  //Search for Exhibition apps
                    {
                        FindExhibitionApps();
                        return true;
                    }
                case ConsoleKey.Q:  //Quit
                    {
                        Console.WriteLine("Quitting");
                        return false;
                    }
                default:
                    {
                        Console.Beep();
                        Console.Clear();
                        ShowMenu();
                        return true;
                    }
            }
        }

        #endregion

        #region Catalog Utilities
        public static void IndexFolder(List<AppDefinition> appCatalog, string searchFolder)
        {
            Console.WriteLine();
            Console.WriteLine("Indexing app packages against metadata...");
            Console.WriteLine();

            //Setup report
            System.IO.StreamWriter objWriter;
            objWriter = new StreamWriter(Path.Combine(appCatalogDir, catalogIndexReport));
            var headerLine = "AppID,Title,Category,FileName,Status,Filename";
            objWriter.WriteLine(headerLine);
            int i = 0;
            int appsFound = 0;
            int appsMissing = 0;
            int appUpdates = 0;

            //Loop through Catalog
            foreach (var appObj in appCatalog)
            {
                i++;
                int percentDone = (int)Math.Round((double)(100 * i) / appCatalog.Count);
                Console.CursorTop--;
                Console.Write("Searching Index #" + i.ToString() + " - " + percentDone.ToString() + "% Done - ");

                //Figure out meta file name to check
                var stripTitle = appObj.title.Replace(",", "");
                var outputLine = appObj.id + "," + stripTitle + ",";
                var checkFile = Path.Combine(appMetaDir, appObj.id + ".json");
                Console.WriteLine(Path.GetFileName(checkFile) + "     ");

                //Look to see if the app entry exists in the metadata folder
                if (File.Exists(checkFile))
                {
                    //Load some metadata fields for the report
                    outputLine = outputLine + appObj.category + ",";
                    var myJsonString = File.ReadAllText(checkFile);
                    var myJObject = JObject.Parse(myJsonString);
                    var fileName = myJObject.SelectToken("filename").Value<string>();
                    outputLine = outputLine + fileName + ",";

                    //Look to see if the app package exists in the search folder
                    if (File.Exists(Path.Combine(searchFolder, fileName)))
                    {
                        appsFound++;
                        //Copy to new home
                        var newHome = Path.Combine(appCatalogDir, appPackageDir, fileName);
                        if (searchFolder != appPackageDir)
                            File.Move(Path.Combine(searchFolder, fileName), newHome, true);

                        //Do wildcard search for alternate versions
                        var filenameParts = fileName.Split("_");
                        var filenameStart = filenameParts[0];
                        var fileMatches = 0;
                        foreach (var updateFile in Directory.GetFiles(searchFolder, filenameStart + "_*"))
                        {
                            if (Path.GetFileName(updateFile) != fileName)
                            {
                                appUpdates++;
                                fileMatches++;
                                if (searchFolder != appPackageDir)
                                    File.Move(Path.Combine(searchFolder, updateFile), Path.Combine(appUpdateDir, Path.GetFileName(updateFile)), true);
                            }
                        }
                        //Add findings to report
                        if (fileMatches > 0)
                            outputLine = outputLine + "Found +" + fileMatches + " Update,";
                        else
                            outputLine = outputLine + "Found,";
                        outputLine = outputLine + newHome;
                    }
                    else
                    {
                        //Add missing info to report
                        appsMissing++;
                        outputLine = outputLine + "Not Found";
                    }
                }
                else
                {
                    //Add results to report
                    outputLine = outputLine + "Unindexed";
                }

                objWriter.WriteLine(outputLine);
            }
            //Output final report
            objWriter.Close();
            Console.WriteLine();
            Console.WriteLine("Index complete!");
            Console.WriteLine("Apps Found:    " + appsFound);
            Console.WriteLine("Updates Found: " + appUpdates);
            Console.WriteLine("Apps Missing:  " + appsMissing);
            Console.WriteLine();
            Console.WriteLine("Detailed report: " + catalogIndexReport);
        }

        public static void StripPrefixFromFilenames(string searchFolder)
        {
            //Remove catalog id prefix from filenames, since some archives include this
            Console.WriteLine();
            Console.WriteLine("Stripping prefix from packages in: " + searchFolder);
            Console.WriteLine();
            string[] fileEntries = Directory.GetFiles(searchFolder);
            foreach (string fileName in fileEntries)
            {
                var justTheFile = Path.GetFileName(fileName);
                var justTheFileParts = justTheFile.Split("--");
                justTheFile = justTheFileParts[justTheFileParts.Length - 1];
                var newHome = Path.Combine(searchFolder, "Stripped");
                if (!Directory.Exists(newHome))
                    Directory.CreateDirectory(newHome);
                newHome = Path.Combine(newHome, justTheFile);
                Console.WriteLine("New Path: " + newHome);
                System.IO.File.Copy(fileName, newHome, true);
            }
        }

        public static void ReverseCatalogFromMetaData(List<AppDefinition> appCatalog)
        {
            //Check each metadata file to make sure its in the catalog file
            Console.WriteLine();
            Console.WriteLine("Indexing metadata against catalog...");
            Console.WriteLine();

            //Setup report
            System.IO.StreamWriter objWriter;
            objWriter = new StreamWriter(MetaReverseReport);

            //Load in app IDs from catalog for faster search
            List<int> appIds = new List<int>();
            foreach (var appObj in appCatalog)
            {
                if (appIds.Contains(appObj.id))
                {
                    Console.WriteLine("Duplicate App ID Found: " + appObj.id + " - " + appObj.title);
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadLine();
                }
                appIds.Add(appObj.id);
            }

            //Loop through all metadata files
            string[] fileEntries = Directory.GetFiles(appMetaDir, "*.json");
            int totalFound = 0;
            int totalMissing = 0;
            int i = 0;
            foreach (string metaFile in fileEntries)
            {
                int metaFileIndex = 0;
                var metaFileIndexStr = Path.GetFileNameWithoutExtension(metaFile);
                int.TryParse(metaFileIndexStr, out metaFileIndex);
                int percentDone = (int)Math.Round((double)(100 * i) / fileEntries.Length);
                Console.CursorTop--;
                Console.Write("Meta File: " + metaFileIndex.ToString() + " - ");
                
                bool found = true;
                if (!appIds.Contains(metaFileIndex))
                {
                    //Meta data file not referenced in catalog
                    found = false;
                    totalMissing++;
                    objWriter.WriteLine(metaFileIndex);
                }
                else
                    totalFound++;
                //Update output with findings
                Console.Write(found.ToString());
                Console.WriteLine(" - " + percentDone.ToString() + "%          ");
                i++;
            }
            //Finalize report
            objWriter.Close();
            Console.WriteLine();
            Console.WriteLine("Total Records in Catalog: " + appCatalog.Count.ToString());
            Console.WriteLine("Total Files Scanned:      " + fileEntries.Length.ToString());
            Console.WriteLine("Total Found in Catalog:   " + totalFound);
            Console.WriteLine("Total Missing in Catalog: " + totalMissing);
            Console.WriteLine();
            Console.WriteLine("Missing Item report: " + MetaReverseReport);
        }

        public static void ReverseCatalogFromFolder(string searchFolder)
        {
            //Check each package file in a folder to see if it exists in the catalog
            Console.WriteLine();
            Console.WriteLine("Indexing folder contents against catalog metadata...");

            //Find all the meta files the catalog metadata folder knows about
            Console.Write("Building index...");
            string[] metaEntries = Directory.GetFiles(appMetaDir, "*.json");
            List<string> appFileNames = new List<string>();
            foreach (string metaFile in metaEntries)
            {
                //Index the package file names from each meta data file for faster searching
                var myJsonString = File.ReadAllText(metaFile);
                var myJObject = JObject.Parse(myJsonString);
                var fileName = myJObject.SelectToken("filename").Value<string>();
                appFileNames.Add(fileName);
            }
            Console.WriteLine("Done with " + metaEntries.Length + " entries" );

            //Setup the report
            System.IO.StreamWriter objWriter;
            objWriter = new StreamWriter(FileReverseReport);

            //Scan the search folder for files and check if they're found in meta data index
            string[] fileEntries = Directory.GetFiles(searchFolder, "*.ipk");
            int totalFound = 0;
            int totalMissing = 0;
            int i = 0;
            Console.WriteLine("Scanning " + fileEntries.Length + " files...");
            Console.WriteLine();
            foreach (string pkgFile in fileEntries)
            {
                Console.CursorTop--;

                var pkgFileName = Path.GetFileName(pkgFile);
                i++;
                int percentDone = (int)Math.Round((double)(100 * i) / fileEntries.Length);
                Console.Write(percentDone.ToString() + "% - ");

                if (!appFileNames.Contains(pkgFileName))
                {
                    //If the current package file is not found in the metadata file index
                    Console.Write("Missing: ");
                    objWriter.WriteLine(pkgFileName);
                    totalMissing++;
                }
                else
                {
                    Console.Write("Found: ");
                    totalFound++;
                }
                Console.Write("Package File: " + pkgFileName + " - ");
                for (int l = Console.CursorLeft; l < Console.WindowWidth-2; l++)
                    Console.Write(" ");
                Console.WriteLine();
            }
            //Finalize report
            objWriter.Close();
            Console.WriteLine();
            Console.WriteLine("Total Files Scanned:      " + fileEntries.Length);
            Console.WriteLine("Total Found in Catalog:   " + totalFound);
            Console.WriteLine("Total Missing in Catalog: " + totalMissing);
            Console.WriteLine();
            Console.WriteLine("Missing Item report: " + FileReverseReport);
        }

        public static void RemoveAppUpdateDuplicatesFromAppUnknown()
        {

            //Scan the search folder for files and check if they're found in meta data index
            string[] fileEntries = Directory.GetFiles(appUpdateDir, "*.ipk");
            int totalFound = 0;
            int i = 0;
            Console.WriteLine("Scanning " + fileEntries.Length + " files...");
            Console.WriteLine();
            foreach (string pkgFile in fileEntries)
            {
                string checkDupeName = Path.GetFileName(pkgFile);
                string checkDupePath = Path.Combine(appUnknownDir, checkDupeName);
                if (File.Exists(checkDupePath))
                {
                    Console.WriteLine("Delete: AppUnknown\\" + checkDupeName);
                    //File.Delete(checkDupePath);
                    totalFound++;
                }
            }
            Console.WriteLine("Total deleted: " + totalFound);
        }

        public static void FindHighestUpdateVersion()
        {
            //Scan the updates folder and find the highest version
            string[] fileEntries = Directory.GetFiles(appUpdateDir, "*.ipk");
            int i = 0;
            Console.WriteLine("Scanning " + fileEntries.Length + " Updates...");
            Console.WriteLine();
            Dictionary<string, AppVersionDefinition> highestUpdateVersions = new Dictionary<string, AppVersionDefinition>();
            foreach (string pkgFile in fileEntries)
            {
                //Find the base file name
                AppVersionDefinition thisApp = ExtractAppVersionDefinition(Path.GetFileNameWithoutExtension(pkgFile));
                string[] otherFiles = Directory.GetFiles(appUpdateDir, thisApp.packageName + "*.ipk");
                if (!highestUpdateVersions.ContainsKey(thisApp.packageName))
                    highestUpdateVersions.Add(thisApp.packageName, thisApp);
                else
                {
                    AppVersionDefinition currHighest = highestUpdateVersions[thisApp.packageName];
                    if (thisApp.majorVersion > currHighest.majorVersion
                        || thisApp.majorVersion == currHighest.majorVersion && thisApp.minorVersion > currHighest.minorVersion
                        || thisApp.majorVersion == currHighest.majorVersion && thisApp.minorVersion == currHighest.minorVersion && thisApp.buildVersion > currHighest.buildVersion)
                    {
                        highestUpdateVersions[thisApp.packageName] = thisApp;
                    }
                }
                Console.CursorTop--;
                int percentDone = (int)Math.Round((double)(100 * i) / fileEntries.Length);
                Console.WriteLine(percentDone.ToString() + "% Done");
                i++;
            }
            Console.WriteLine();

            //Scan the metadata and find the cataloged version
            Console.WriteLine("Reading Metadata...");
            Console.WriteLine();
            Dictionary<string, AppVersionDefinition> catalogVersions = new Dictionary<string, AppVersionDefinition>();
            string[] metaFiles = Directory.GetFiles(appMetaDir, "*.json");
            int j = 0;
            foreach (string metaFile in metaFiles)
            {
                //Load enough metadata to get an AppDefinition to compare
                string myJsonString = File.ReadAllText(metaFile);
                JObject myJObject = JObject.Parse(myJsonString);

                string metaAppFilename = myJObject.SelectToken("filename").Value<string>();
                AppVersionDefinition thisApp = ExtractAppVersionDefinition(Path.GetFileNameWithoutExtension(metaAppFilename), Path.GetFileNameWithoutExtension(metaFile));
                if (!catalogVersions.ContainsKey(thisApp.packageName))
                    catalogVersions.Add(thisApp.packageName, thisApp);

                //Update status on screen
                Console.CursorTop--;
                int percentDone = (int)Math.Round((double)(100 * j) / metaFiles.Length);
                Console.WriteLine(percentDone.ToString() + "% Done");
                j++;
            }
            Console.WriteLine();

            Console.WriteLine("Listing results...");
            //Now decide which is a better version to use
            int updateCount = 0;
            Dictionary<string, AppVersionDefinition> failedUpdates = new Dictionary<string, AppVersionDefinition>();
            foreach (KeyValuePair<string, AppVersionDefinition> highestUpdate in highestUpdateVersions)
            {
                if (catalogVersions.ContainsKey(highestUpdate.Key))
                {
                    AppVersionDefinition highestUpdateDefinition = highestUpdate.Value;
                    AppVersionDefinition catalogAppDefinition = catalogVersions[highestUpdate.Key];
                    if (catalogAppDefinition.majorVersion > highestUpdateDefinition.majorVersion
                        || catalogAppDefinition.majorVersion == highestUpdateDefinition.majorVersion && catalogAppDefinition.minorVersion > highestUpdateDefinition.minorVersion
                        || catalogAppDefinition.majorVersion == highestUpdateDefinition.majorVersion && catalogAppDefinition.minorVersion == highestUpdateDefinition.minorVersion && catalogAppDefinition.buildVersion >= highestUpdateDefinition.buildVersion)
                    {
                        //do nothing... but its easier to evaluate everything as true and do an else
                    }
                    else
                    {
                        string updateMetaFilename = Path.Combine(appMetaDir, (catalogAppDefinition.appId + ".json"));
                        Console.Write("Update " + catalogAppDefinition.fileName + " (" + catalogAppDefinition.appId + ") to " + highestUpdateDefinition.fileName + "...");
                        string myJsonString = File.ReadAllText(updateMetaFilename);
                        JObject myJObject = JObject.Parse(myJsonString);
                        try
                        {
                            //Update metadata
                            long fileLength = new System.IO.FileInfo(Path.Combine(appUpdateDir, highestUpdateDefinition.fileName + ".ipk")).Length;
                            myJObject.Remove("version");
                            string newVersion = highestUpdateDefinition.majorVersion + "." + highestUpdateDefinition.minorVersion + "." + highestUpdateDefinition.buildVersion;
                            myJObject.Add("version", newVersion);
                            myJObject.Remove("filename");
                            myJObject.Add("filename", highestUpdateDefinition.fileName + ".ipk");
                            myJObject.Remove("appSize");
                            myJObject.Add("appSize", fileLength.ToString());
                            //Save metadata file
                            StreamWriter newJsonWriter = new StreamWriter(Path.Combine(appMetaDir, updateMetaFilename));
                            newJsonWriter.WriteLine(myJObject.ToString());
                            newJsonWriter.Close();
                            //Copy app package
                            File.Move(Path.Combine(appUpdateDir, highestUpdateDefinition.fileName + ".ipk"), Path.Combine(appPackageDir, highestUpdateDefinition.fileName + ".ipk"), true);
                            Console.WriteLine("OK");
                            updateCount++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Fail");
                            failedUpdates.Add(catalogAppDefinition.packageName, highestUpdateDefinition);
                        }
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine("Total updates performed: " + updateCount);
            Console.WriteLine("Total updates failed:    " + failedUpdates.Count);
            Console.WriteLine("Total unneeded updates:  " + (fileEntries.Length - failedUpdates.Count));
            if (failedUpdates.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Failures: ");
                foreach(KeyValuePair<string, AppVersionDefinition> failedUpdate in failedUpdates)
                {
                    Console.WriteLine(failedUpdate.Value.fileName);
                }
            }
        }

        public static void BuildMissingPackageInfoReport(string missingAppDataFile, string folderToSeachForPossibleMatches)
        {
            List<AppDefinition> missingAppCatalog = ReadCatalogFile(missingAppDataFile);
            //loop through catalog
            Console.WriteLine("Collecting details for missing apps and searching possible matches...");
            Console.WriteLine();
            int i = 0;
            int appsWithPossibleMatch = 0;
            StreamWriter objWriter;
            objWriter = new StreamWriter(MissingPackageInfoReport);
            objWriter.WriteLine("App Name, App Package, Possible Matches");
            foreach (var appObj in missingAppCatalog)
            {
                i++;
                int percentDone = (int)Math.Round((double)(100 * i) / missingAppCatalog.Count);
                Console.CursorTop--;
                Console.WriteLine("Reading Index #" + i.ToString() + " - " + percentDone.ToString() + "% Done");

                string newLine = appObj.title + ",";
                //Figure out meta file name to check
                var checkFile = Path.Combine(appMetaDir, appObj.id + ".json");
                objWriter.Write(appObj.title.Replace(",","") + ",");

                //Look to see if the app entry exists in the metadata folder
                if (File.Exists(checkFile))
                {
                    //open json
                    var myJsonString = File.ReadAllText(checkFile);
                    var myJObject = JObject.Parse(myJsonString);
                    var fileName = myJObject.SelectToken("filename").Value<string>();
                    objWriter.Write(fileName + ",");
                    List<string> possibleMatches = FindPossibleMatchesInFolder(fileName, folderToSeachForPossibleMatches);
                    if (possibleMatches.Count > 0)
                        appsWithPossibleMatch++;
                    foreach (string possibleMatch in possibleMatches)
                    {
                        objWriter.Write(possibleMatch + ",");
                    }
                }
                objWriter.WriteLine();
            }
            objWriter.Close();
            Console.WriteLine();
            Console.WriteLine("Missing Packages: " + missingAppCatalog.Count);
            Console.WriteLine("Possible Matches: " + appsWithPossibleMatch);
            Console.WriteLine();
            Console.WriteLine("Missing package list: " + MissingPackageInfoReport);
        }

        public static void UpdateMetaFileJSONStructure()
        {
            //Check each metadata file to make sure its in the catalog file
            Console.WriteLine();
            Console.WriteLine("Updating metafile structure...");
            Console.WriteLine();

            //Loop through all metadata files
            string[] fileEntries = Directory.GetFiles(appMetaDir, "*.json");
            int i = 0;
            foreach (string metaFile in fileEntries)
            {
                string myJsonString = File.ReadAllText(metaFile);
                JObject myJObject = JObject.Parse(myJsonString);
                try
                {
                    string ipkFilename = myJObject.SelectToken("filename").Value<String>();
                    bool dirty = false;
                    //Add field for original file name so we can preserve history but also move forward
                    if (myJObject.SelectToken("originalFileName") == null)
                    {
                        dirty = true;
                        myJObject.Add("originalFileName", ipkFilename);
                    }
                    //Add field to host star rating, to be calculated from scraped data
                    if (myJObject.SelectToken("starRating") == null)
                    {
                        dirty = true;
                        myJObject.Add("starRating", null);
                    }
                    if (dirty)
                    {
                        StreamWriter newJsonWriter = new StreamWriter(Path.Combine(appMetaDir, Path.GetFileName(metaFile)));
                        newJsonWriter.WriteLine(myJObject.ToString());
                        newJsonWriter.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    Console.ReadLine();
                }

                var metaFileIndexStr = Path.GetFileNameWithoutExtension(metaFile);
                int.TryParse(metaFileIndexStr, out int metaFileIndex);
                
                Console.CursorTop--;
                Console.Write("Meta File: " + metaFileIndex.ToString() + " - ");

                //Update output with findings
                int percentDone = (int)Math.Round((double)(100 * i) / fileEntries.Length);
                Console.WriteLine(percentDone.ToString() + "%          ");
                i++;
            }
            //Finalize report
            Console.WriteLine();
            Console.WriteLine("Total JSON files converted: " + i.ToString());
            Console.WriteLine();
        }

        public static void FindAndInsertStarRating(string ratingsDir)
        {
            //Check each metadata file to make sure its in the catalog file
            Console.WriteLine();
            Console.WriteLine("Looking for star ratings...");
            Console.WriteLine();

            //Loop through all metadata files
            string[] fileEntries = Directory.GetFiles(appMetaDir, "*.json");
            int i = 0;
            foreach (string metaFile in fileEntries)
            {
                string myJsonString = File.ReadAllText(metaFile);
                JObject myJObject = JObject.Parse(myJsonString);
                int ratingValue = 0;
                try
                {
                    if (TryParseStarRating(Path.Combine(ratingsDir, Path.GetFileName(metaFile)), out ratingValue) && ratingValue > 0)
                    {
                        //Add field to host star rating, to be calculated from scraped data
                        if (myJObject.SelectToken("starRating") != null)
                            myJObject.Remove("starRating");
                        myJObject.Add("starRating", ratingValue);
                        StreamWriter newJsonWriter = new StreamWriter(Path.Combine(appMetaDir, Path.GetFileName(metaFile)));
                        newJsonWriter.WriteLine(myJObject.ToString());
                        newJsonWriter.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    Console.ReadLine();
                }

                var metaFileIndexStr = Path.GetFileNameWithoutExtension(metaFile);
                int.TryParse(metaFileIndexStr, out int metaFileIndex);

                Console.CursorTop--;
                Console.Write("Meta File: " + metaFileIndex.ToString() + " - ");

                //Update output with findings
                int percentDone = (int)Math.Round((double)(100 * i) / fileEntries.Length);
                Console.WriteLine(percentDone.ToString() + "% - " + ratingValue + " stars   ");
                i++;
            }
            //Finalize report
            Console.WriteLine();
            Console.WriteLine("Total Ratings Added: " + i.ToString());
            Console.WriteLine();
        }

        public static bool TryParseStarRating(string ratingsFile, out int rating)
        {
            rating = 0;
            if (File.Exists(ratingsFile))
            {
                string myJsonString = File.ReadAllText(ratingsFile);
                JObject myJObject = JObject.Parse(myJsonString);
                try
                {
                    foreach (JProperty prop in myJObject.Properties())
                    {
                        //HACK: This is a horribly inefficient search, because I couldn't figure out how to parse HP's weird JSON
                        if (prop.Name == "UserRatingList")
                        {
                            foreach (JObject innerObj in prop)
                            {
                                try
                                {
                                    JArray ratingsList = innerObj.SelectToken("ratings").Value<JArray>();
                                    if (ratingsList != null)
                                    {
                                        double average;
                                        List<int> scores = new List<int>();
                                        foreach (var r in ratingsList.Children<JObject>())
                                        {
                                            RatingsDefinition thisChild = r.ToObject<RatingsDefinition>();
                                            if (thisChild != null)
                                            {
                                                scores.Add(thisChild.score);
                                            }
                                        }
                                        average = scores.Take(scores.Count).Average();
                                        rating = (int)Math.Round(average, 1);
                                        return true;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    try
                                    {
                                        RatingsDefinition thisChild = innerObj.ToObject<RatingsDefinition>();
                                        rating = thisChild.score;
                                        return true;
                                    }
                                    catch (Exception exIn)
                                    {
                                        //Oh well, we tried
                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    //Oh well, we tried
                }
            }
            return false;
        }

        public static void StageAndUpdateApprovedMatches(List<AppDefinition> appCatalog, string pathToMatchFile)
        {
            //loop through catalog
            if (!Directory.Exists(appMatchedDir))
            {
                Directory.CreateDirectory(appMatchedDir);
            }
            Console.WriteLine("Reading approved matches...");
            Console.WriteLine();

            var matchLines = File.ReadAllLines(pathToMatchFile);
            Dictionary<string, string> matchApps = new Dictionary<string, string>();
            foreach (var match in matchLines)
            {
                string[] matchParts = match.Split(",");
                matchApps.Add(matchParts[1], matchParts[2]);
            }

            int i = 0;
            int appsWithMatch = 0;
            int failedMatch = 0;
            foreach (var appObj in appCatalog)
            {
                i++;
                int percentDone = (int)Math.Round((double)(100 * i) / appCatalog.Count);
                Console.CursorTop--;
                Console.WriteLine("Reading Index #" + i.ToString() + " - " + percentDone.ToString() + "% Done");

                //Figure out meta file name to check
                var checkFile = Path.Combine(appMetaDir, appObj.id + ".json");

                //Look to see if the app entry exists in the metadata folder
                if (File.Exists(checkFile))
                {
                    //open json
                    var myJsonString = File.ReadAllText(checkFile);
                    var myJObject = JObject.Parse(myJsonString);
                    var fileNameToken = myJObject.SelectToken("filename");
                    var originalFileName = fileNameToken.Value<string>();
                    if (matchApps.ContainsKey(fileNameToken.Value<string>()))
                    {
                        try
                        {
                            File.Move(Path.Combine(appUnknownDir, matchApps[originalFileName]), Path.Combine(appMatchedDir, matchApps[originalFileName]), true);
                            fileNameToken.Replace(matchApps[originalFileName]);
                            StreamWriter newJsonWriter = new StreamWriter(Path.Combine(appMetaDir, checkFile));
                            newJsonWriter.WriteLine(myJObject.ToString());
                            newJsonWriter.Close();
                            appsWithMatch++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Error: Failed to stage close match: " + checkFile + " - " + appObj.title);
                            Console.WriteLine();
                            failedMatch++;
                        }
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine("Matches Approved: " + matchApps.Count);
            Console.WriteLine("Matches Found:    " + appsWithMatch);
            Console.WriteLine("Matches Failed:    " + failedMatch);
        }

        public static List<AppDefinition> FindNextAvailCatalogNum(List<AppDefinition> appCatalog)
        {
            int highestIndexNum = 0;
            foreach (var appObj in appCatalog)
            {
                if (appObj.id > highestIndexNum)
                    highestIndexNum = appObj.id;
            }
            highestIndexNum++;
            Console.WriteLine("Highest index number found:  " + (highestIndexNum-1));
            Console.WriteLine("Suggested next index number: " + highestIndexNum);
            Console.WriteLine();
            Console.Write("Stub in new app to extant, master and metadata files? ");
            ConsoleKeyInfo choice = Console.ReadKey();
            switch (choice.Key)
            {
                case ConsoleKey.N:
                    {
                        break;
                    }
                case ConsoleKey.Y:
                    {
                        Console.WriteLine();
                        string newName = "";
                        Console.WriteLine("Name of the new app: ");
                        string strInputPath = Console.ReadLine();
                        if (strInputPath.Length > 1)
                            newName = strInputPath;
                        Console.WriteLine();

                        //Read in extant catalog
                        catalogFile = Path.Combine(appCatalogDir, "extantAppData.json");
                        Console.Write("Reading Extant Catalog: " + catalogFile + "...");
                        List<AppDefinition> extantCatalog = ReadCatalogFile(catalogFile);
                        Console.WriteLine(extantCatalog.Count + " apps found.");

                        //Define new app
                        AppDefinition newApp = new AppDefinition
                        {
                            id = highestIndexNum,
                            title = newName,
                            author = "none",
                            summary = "none",
                            appIcon = "",
                            appIconBig = "",
                            category = "Revisionist History",
                            vendorId = "",
                            TouchPad = true
                        };
                        extantCatalog.Add(newApp);
                        appCatalog.Add(newApp);

                        //Update file
                        Console.WriteLine();
                        WriteCatalogFile("extant", extantCatalog);
                        WriteCatalogFile("master", appCatalog);
                        File.Copy(Path.Combine(workingDir, "app-template.json"), Path.Combine(appCatalogDir, "AppMetadata", highestIndexNum + ".json"));
                        break;
                    }
            }

            return appCatalog;
        }

        public static void GetAppIconsFromCatalog(List<AppDefinition> appCatalog)
        {
            Console.WriteLine("Searching for app icons on the web...");
            int i = 0;
            StreamWriter objWriter;
            objWriter = new StreamWriter(MissingIconReport);
            foreach (var appObj in appCatalog)
            {
                i++;
                bool iconMissing = false;
                string bigIconPath = appObj.appIconBig;
                string smallIconPath = appObj.appIcon;
                if (bigIconPath != string.Empty)
                {
                    //Find icon file names
                    string[] bigIconPathParts = bigIconPath.Split("/");
                    string bigIcon = bigIconPathParts[bigIconPathParts.Length - 1];
                    
                    //Find and/or make icon directories
                    string bigIconDir = Path.Combine(appImageDir, appObj.id.ToString(), "icon");
                    if (!Directory.Exists(bigIconDir))
                        Directory.CreateDirectory(bigIconDir);

                    //Try to get the image
                    string iconSavePath = Path.Combine(bigIconDir, bigIcon);
                    if (!File.Exists(iconSavePath))
                    {
                        if (!TryGetImageFromPalmCDN(bigIconPath, iconSavePath))
                        {
                            Console.WriteLine("Could not get big icon for " + appObj.id + " from any source");
                            iconMissing = true;
                        }
                        else
                            Console.WriteLine("Got big icon for:   " + appObj.id + "");
                    }
                }
                if (smallIconPath != string.Empty)
                {
                    //Find icon file names
                    string[] smallIconPathParts = smallIconPath.Split("/");
                    string smallIcon = smallIconPathParts[smallIconPathParts.Length - 1];

                    //Find and/or make icon directories
                    string smallIconDir = Path.Combine(appImageDir, appObj.id.ToString(), "icon", "s");
                    if (!Directory.Exists(smallIconDir))
                        Directory.CreateDirectory(smallIconDir);

                    //Try to get the image
                    string iconSavePath = Path.Combine(smallIconDir, smallIcon);
                    if (!File.Exists(iconSavePath))
                    {
                        if (!TryGetImageFromPalmCDN(bigIconPath, iconSavePath))
                        {
                            Console.WriteLine("Could not get small icon for " + appObj.id + " from any source");
                            iconMissing = true;
                        }
                        else
                            Console.WriteLine("Got small icon for: " + appObj.id + "");
                    }
                }
                if (iconMissing)
                {
                    objWriter.WriteLine(appObj.id.ToString());
                }
                Console.WriteLine((appCatalog.Count - i).ToString() + " apps remaining...");
            }
            objWriter.Close();
        }

        public static void SearchForScreenshotsFromMetaData(string searchFolder)
        {

            Console.WriteLine("Searching for app icons in " + searchFolder);
            int i = 0;
            //Setup report
            StreamWriter objWriter;
            objWriter = new StreamWriter(ImageSortReport);
            objWriter.WriteLine("Metafile,Thumbnails Found,Screenshots Found,Thumbnails Missing,Screenshots Missing");

            //Loop through all metadata files
            string[] fileEntries = Directory.GetFiles(appMetaDir, "*.json");
            int totalFound = 0;
            int totalMissing = 0;

            foreach (string metaFile in fileEntries)
            {
                int thumbsFound = 0;
                int thumbsMissing = 0;
                int screensFound = 0;
                int screensMissing = 0;
                string missingImageList = "";
                int metaFileIndex = 0;
                var metaFileIndexStr = Path.GetFileNameWithoutExtension(metaFile);
                int.TryParse(metaFileIndexStr, out metaFileIndex);
                int percentDone = (int)Math.Round((double)(100 * i) / fileEntries.Length);
                Console.Write("Meta File: " + metaFileIndex.ToString() + " - "  + percentDone.ToString() + "% ");
                List<ScreenshotDefinition> screenshotList = new List<ScreenshotDefinition>();

                //Try multiple ways to get the image list -- since the structure varies between documents
                string myJsonString = File.ReadAllText(metaFile);
                JObject myJObject = JObject.Parse(myJsonString);
                try
                {
                    JArray imagesSection = myJObject.SelectToken("images").Value<JArray>();
                    if (imagesSection != null)
                    {
                        foreach (var child in imagesSection.Children<JObject>())
                        {
                            ScreenshotDefinition thisChild = child.ToObject<ScreenshotDefinition>();
                            screenshotList.Add(thisChild);
                        }
                    }
                }
                catch(Exception ex)
                {
                    JObject imagesSection = myJObject.SelectToken("images").Value<JObject>();
                    if (imagesSection != null)
                    {
                        foreach (var c in imagesSection)
                        {
                            JObject myChildObject = JObject.Parse(c.Value.ToString());
                            ScreenshotDefinition thisChild = myChildObject.ToObject<ScreenshotDefinition>();
                            screenshotList.Add(thisChild);
                        }
                    }
                }

                Console.WriteLine("- " + (screenshotList.Count * 2).ToString() + " images");

                //For each image in the list, try to extract each type
                foreach (var image in screenshotList)
                {
                    string screenshotPath = image.screenshot;
                    if (screenshotPath != string.Empty)
                    {
                        //Find image file names
                        string[] screenshotPathParts = screenshotPath.Split("/");
                        string screenshotFile = screenshotPathParts[screenshotPathParts.Length - 1];

                        if (!TrySortImageFromFolder(metaFileIndexStr, "L", screenshotFile, screenshotPath, searchFolder))
                        {
                            //Fall back to Palm CDN... TODO: then Wayback?
                            string savePath = Path.Combine(appImageDir, screenshotPath.Replace("/", "\\"));
                            if (!TryGetImageFromPalmCDN(screenshotPath, savePath))
                            {
                                if (!TryGetImageFromWaybackMachine(screenshotPath, savePath))
                                {
                                    Console.WriteLine("Not found");
                                    missingImageList = missingImageList + "," + screenshotPath;
                                    totalMissing++;
                                    screensMissing++;
                                }
                                else
                                {
                                    Console.WriteLine("Found in Wayback");
                                    totalFound++;
                                    thumbsFound++;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Found online");
                                totalFound++;
                                thumbsFound++;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Found in folder");
                            screensFound++;
                            totalFound++;
                        }
                    }

                    string thumbnailPath = image.thumbnail;
                    if (thumbnailPath != string.Empty)
                    {
                        //Find image file names
                        string[] thumbnailPathParts = thumbnailPath.Split("/");
                        string thumbnailFile = thumbnailPathParts[thumbnailPathParts.Length - 1];

                        if (!TrySortImageFromFolder(metaFileIndexStr, "S", thumbnailFile, thumbnailPath, searchFolder))
                        {
                            //Fall back to Palm CDN...
                            string savePath = Path.Combine(appImageDir, thumbnailPath.Replace("/", "\\"));
                            if (!TryGetImageFromPalmCDN(thumbnailPath, savePath))
                            {
                                if (!TryGetImageFromWaybackMachine(thumbnailPath, savePath))
                                {
                                    Console.WriteLine("Not found");
                                    missingImageList = missingImageList + "," + thumbnailPath;
                                    totalMissing++;
                                    screensMissing++;
                                }
                                else
                                {
                                    Console.WriteLine("Found in Wayback");
                                    totalFound++;
                                    thumbsFound++;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Found online");
                                totalFound++;
                                thumbsFound++;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Found in folder");
                            totalFound++;
                            thumbsFound++;
                        }
                    }
                }
                
                //Update report with findings
                objWriter.WriteLine(metaFileIndexStr + "," + thumbsFound + "," + screensFound + "," + thumbsMissing + "," + screensMissing + "," + missingImageList);
                i++;
            }
            objWriter.Close();
            Console.WriteLine();
            Console.WriteLine("Total Meta Files:     " + fileEntries.Length.ToString());
            Console.WriteLine("Total Images Found:   " + totalFound.ToString());
            Console.WriteLine("Total Images Missing: " + totalMissing.ToString());
            Console.WriteLine();
            Console.WriteLine("Detailed report: " + ImageSortReport);
        }

        public static bool TryGetImageFromPalmCDN(string getPath, string savePath)
        {
            string palmCDNPath = "http://cdn.downloads.palm.com/public/" + getPath;
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(savePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(palmCDNPath), savePath);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("404"))
                    return false;
                else
                {
                    Console.Write("ERROR: " + ex.Message);
                    return false;
                }
            }
            return true;
        }

        public static bool TryGetImageFromWaybackMachine(string getPath, string savePath)
        {
            string archiveResultPath = "http://web.archive.org/web/http://cdn.downloads.palm.com/public/" + getPath;
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(savePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                using (WebClient client = new WebClient())
                {
                    //Get the WayBack machine wrapper page for the content we want
                    string WaybackResponse = client.DownloadString(new Uri(archiveResultPath));
                    //Find the iframe that has the actual content we're interested in
                    string[] ResponseParts = WaybackResponse.ToLower().Split("<iframe id=\"playback\"");
                    string searchPart = ResponseParts[^1];
                    ResponseParts = searchPart.Split("</iframe>");
                    searchPart = ResponseParts[0];
                    //Rip out the src of the iframe
                    ResponseParts = searchPart.Split("src=\"");
                    searchPart = ResponseParts[^1];
                    ResponseParts = searchPart.Split("\"");
                    searchPart = ResponseParts[0];
                    client.DownloadFile(new Uri(searchPart), savePath);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("404"))
                    return false;
                else
                {
                    Console.Write("ERROR: " + ex.Message);
                    return false;
                }
            }
            return true;
        }

        public static bool TrySortImageFromFolder(string appId, string imageSize, string imageFile, string imagePath, string searchPath)
        {
            string searchImage = appId + "_" + imageSize + "_uri_" + imageFile;
            imagePath = imagePath.Replace("/", "\\");
            Console.Write("   Sorting " + searchImage + "...");
            searchImage = Path.Combine(searchPath, searchImage);
            if (File.Exists(searchImage))
            {
                imagePath = Path.Combine(appImageDir, imagePath);
                string imageDir = imagePath.Replace(Path.GetFileName(imagePath), "");
                if (!Directory.Exists(imageDir))
                    Directory.CreateDirectory(imageDir);
                File.Copy(searchImage, imagePath, true);
                return true;
            }
            return false;
        }

        public static void GenerateExtantCatalog(List<AppDefinition> appCatalog)
        {
            int i = 0;
            List<AppDefinition> extantAppCatalog = new List<AppDefinition>();
            List<AppDefinition> missingAppCatalog = new List<AppDefinition>();
            foreach (var appObj in appCatalog)
            {
                i++;
                int percentDone = (int)Math.Round((double)(100 * i) / appCatalog.Count);
                Console.CursorTop--;
                Console.Write("Checking Index #" + i.ToString() + " - " + percentDone.ToString() + "% Done - ");

                //Figure out meta file name to check
                var checkFile = Path.Combine(appMetaDir, appObj.id + ".json");
                Console.Write(Path.GetFileName(checkFile) + " ");

                //Look to see if the app entry exists in the metadata folder
                if (File.Exists(checkFile))
                {
                    //Load some metadata fields for the report
                    var myJsonString = File.ReadAllText(checkFile);
                    var myJObject = JObject.Parse(myJsonString);
                    var fileName = myJObject.SelectToken("filename").Value<string>();

                    //Look to see if the app package exists in the search folder
                    if (File.Exists(Path.Combine(appPackageDir, fileName)))
                    {
                        Console.WriteLine("Added       ");
                        extantAppCatalog.Add(appObj);
                    }
                    else
                    {
                        Console.WriteLine("Skipped     ");
                        missingAppCatalog.Add(appObj);
                    }
                }
            }
            WriteCatalogFile("Extant", extantAppCatalog);
            WriteCatalogFile("Missing", missingAppCatalog);
        }

        public static void FixPreTwoCompat(string origMasterDataFile)
        {
            //Read origMasterDataFile into memory
            Console.WriteLine("Reading original catalog...");
            List<AppDefinitionOld> OriginalAppData = ReadOldCatalogFile(origMasterDataFile);
            //For each .json file
            foreach (string checkFileName in Directory.GetFiles(appCatalogDir, "*.json"))
            {
                //If its not the origMasterDataFile
                if (Path.GetFileName(checkFileName) != Path.GetFileName(origMasterDataFile))
                {
                    Console.WriteLine("Correcting " + Path.GetFileName(checkFileName) + "...");
                    //Read it into memory
                    List<AppDefinition> thisAppData = ReadCatalogFile(checkFileName);
                    //For each app in the file
                    foreach (AppDefinition thisApp in thisAppData)
                    {
                        foreach (AppDefinitionOld sourceApp in OriginalAppData)
                        {
                            //check origMasterDataFile in memory for matching appid
                            //If they're the same
                            if (thisApp.id == sourceApp.id)
                            {
                                //Copy "Pre 2" value in orig to Pre2 value in this file
                                thisApp.Pre2 = sourceApp.Pre2;
                            }
                        }
                    }
                    string writeFileName = Path.GetFileNameWithoutExtension(checkFileName);
                    writeFileName = writeFileName.Replace("AppData", "");
                    WriteCatalogFile(writeFileName, thisAppData);
                }
            }                
        }

        public static void AddNewMetaData(string useMasterDataFile)
        {
            Console.WriteLine("Reading original file: " + useMasterDataFile + "...");
            Console.WriteLine();
            List<AppDefinition> thisAppData = ReadCatalogFile(useMasterDataFile);

            int i = 0;
            foreach (AppDefinition thisApp in thisAppData)
            {
                i++;
                int percentDone = (int)Math.Round((double)(100 * i) / thisAppData.Count);
                Console.CursorTop--;
                Console.WriteLine("Modifying " + i.ToString() + " - " + percentDone.ToString() + "% Done");

                if (!thisApp.LuneOS)
                {
                    thisApp.LuneOS = false;
                }
                if (!thisApp.Adult)
                {
                    thisApp.Adult = false;
                }
            }
            string writeFileName = Path.GetFileNameWithoutExtension(useMasterDataFile);
            writeFileName = writeFileName.Replace("AppData", "");            
            WriteCatalogFile(writeFileName, thisAppData);
            Console.WriteLine();
        }

        public static void FindExhibitionApps()
        {
            //Check each metadata file to make sure its in the catalog file
            Console.WriteLine();
            Console.WriteLine("Looking for star ratings...");
            Console.WriteLine();

            catalogFile = Path.Combine(appCatalogDir, "extantAppData.json");
            Console.WriteLine("Reading Extant Catalog: " + catalogFile + "...");
            Console.WriteLine();
            List<AppDefinition> extantCatalog = ReadCatalogFile(catalogFile);
            int x = 0;
            foreach (var appObj in extantCatalog)
            {
                var checkFile = Path.Combine(appMetaDir, appObj.id + ".json");
                var metaFile = Path.GetFileName(checkFile);
                //Console.WriteLine(checkFile + " ");

                string myJsonString = File.ReadAllText(checkFile);
                JObject myJObject = JObject.Parse(myJsonString);
                try
                {
                    try
                    {
                        if (myJObject["attributes"]["provides"]["dockMode"].ToString() == "True")
                        {
                            Console.Write(appObj.title);
                            try
                            {
                                Console.Write(",http://appcatalog.webosarchive.com/AppPackages/" + myJObject["filename"].ToString());
                            }
                            catch (Exception whoCares) { }
                            
                            x++;
                            Console.WriteLine();
                        }
                    }
                    catch (Exception iEx)
                    {
                        //Oh well
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    Console.ReadLine();
                }
            }

            //Finalize report
            Console.WriteLine();
            Console.WriteLine("Total Exhibition Apps Found: " + x.ToString());
            Console.WriteLine();
        }

        public static void CompareDirListToWantedList(string wantedListFile, string dirListFile)
        {
            //Setup the report
            StreamWriter objWriter;
            objWriter = new StreamWriter(RequestedPackageReport);

            //Check each package file in a folder to see if it exists in the catalog
            Console.WriteLine();
            Console.WriteLine("Loading Wanted List...");

            //Find all the file names in the wanted list
            int wantedCount = 0;
            int requestCount = 0;
            string line;
            List<string> wantedFileNames = new List<string>();
            List<string> requestFileNames = new List<string>();
            StreamReader wantedFile = new StreamReader(wantedListFile);
            while ((line = wantedFile.ReadLine()) != null)
            {
                wantedFileNames.Add(line);
                wantedCount++;
            }

            Console.WriteLine("Scanning for " + wantedCount + " wanted files...");
            Console.WriteLine();
            int i = 0;
            //Compare each line of the directory list to the wanted list
            var lineCount = File.ReadLines(dirListFile).Count();
            StreamReader dirFile = new StreamReader(dirListFile);
            while ((line = dirFile.ReadLine()) != null)
            {
                Console.CursorTop--;
                bool added = false;
                i++;
                int percentDone = (int)Math.Round((double)(100 * i) / lineCount);
                Console.Write(percentDone.ToString() + "% - ");
                if (line.ToLower().IndexOf(" copy") == -1)
                {
                    if (wantedFileNames.IndexOf(line) != -1)
                    {
                        requestFileNames.Add(line);
                        objWriter.WriteLine(line + "," + line);
                        added = true;
                        requestCount++;
                    }
                    else
                    {
                        AppVersionDefinition checkApp = ExtractAppVersionDefinition(line);
                        if (line.IndexOf("britannica") != -1)
                        { 
                            //Console.WriteLine("got one!"); 
                        }
                        foreach (string wantFile in wantedFileNames)
                        {
                            if (wantFile.IndexOf("britannica") != -1)
                            { 
                                //Console.WriteLine("got another one!"); 
                            }
                            AppVersionDefinition thisApp = ExtractAppVersionDefinition(wantFile);
                            if (thisApp.publisherPrefix != null && thisApp.publisherPrefix != "" && thisApp.publisherPrefix == checkApp.publisherPrefix)
                            {
                                if (thisApp.appNameSuffix != null && thisApp.appNameSuffix != "" && (thisApp.appNameSuffix == checkApp.appNameSuffix || thisApp.appNameSuffix.Contains(checkApp.appNameSuffix) || checkApp.appNameSuffix.Contains(thisApp.appNameSuffix)))
                                {
                                    if (!added)
                                    {
                                        requestFileNames.Add(line);
                                        objWriter.WriteLine(wantFile + "," + line);
                                        added = true;
                                        requestCount++;
                                    }
                                }
                            }
                        }
                    }
                }
                if (added)
                    Console.Write("MATCH!");
                else
                    Console.Write("      ");
                Console.WriteLine();
            }

            //Finalize report
            objWriter.Close();
            Console.WriteLine();
            Console.WriteLine("Total Files Scanned:      " + i);
            Console.WriteLine("Total New Requests: " + requestFileNames.Count);
            Console.WriteLine();
            Console.WriteLine("Missing Item report: " + RequestedPackageReport);
        }

        public static List<string> FindPossibleMatchesInFolder(string fileName, string searchFolder)
        {
            List<string> possibleMatches = new List<string>();

            AppVersionDefinition thisApp = ExtractAppVersionDefinition(fileName);

            foreach (var checkFileName in Directory.GetFiles(searchFolder))
            {
                bool added = false;
                AppVersionDefinition checkApp = ExtractAppVersionDefinition(checkFileName);
                if (thisApp.publisherPrefix != null && thisApp.publisherPrefix != "" && thisApp.publisherPrefix == checkApp.publisherPrefix)
                {
                    if (!added)
                    {
                        possibleMatches.Add(Path.GetFileName(checkFileName));
                        added = true;
                    }
                    
                }
                if (thisApp.appNameSuffix != null && thisApp.appNameSuffix != "" && thisApp.appNameSuffix == checkApp.appNameSuffix)
                {
                    if (!added)
                    {
                        possibleMatches.Add(Path.GetFileName(checkFileName));
                        added = true;
                    }
                }
                /* Faulty search
                if (thisApp.appNameSuffix.Contains(checkApp.appNameSuffix) || checkApp.appNameSuffix.Contains(thisApp.appNameSuffix))
                {
                    if (!added)
                    {
                        possibleMatches.Add(Path.GetFileName(checkFileName));
                        added = true;
                    }
                }
                */
            }
            return possibleMatches;
        }

        public static AppVersionDefinition ExtractAppVersionDefinition(string fileName, string appId)
        {
            AppVersionDefinition thisApp = ExtractAppVersionDefinition(fileName);
            thisApp.appId = appId;
            return thisApp;
        }

        public static AppVersionDefinition ExtractAppVersionDefinition(string fileName)
        {
            fileName = fileName.ToLower();
            AppVersionDefinition thisApp = new AppVersionDefinition();
            thisApp.fileName = fileName;
            string[] fileNameParts = fileName.Split("_");
            thisApp.packageName = fileNameParts[0];
            string versionNumber;
            if (fileNameParts.Length >= 2)
            {
                versionNumber = fileNameParts[1];
                if (versionNumber != String.Empty)
                {
                    string[] versionNumberParts = versionNumber.Split(".");
                    int.TryParse(versionNumberParts[0], out thisApp.majorVersion);
                    if (versionNumberParts.Length > 1)
                        int.TryParse(versionNumberParts[1], out thisApp.minorVersion);
                    if (versionNumberParts.Length > 2)
                        int.TryParse(versionNumberParts[2], out thisApp.buildVersion);
                }
                if (fileNameParts[^1].Replace(".ipk", "") != versionNumber)
                    thisApp.platform = fileNameParts[^1].Replace(".ipk", "");
            }
            string[] packageNameParts = thisApp.packageName.Split(".");
            
            //Rule out common app name modifiers
            int actualAppNamePos = packageNameParts.Length-1;

            List<string> commonAppModifiers = GetCommonAppNameModifiers();
            for (var i=packageNameParts.Length-1; i > 0; i--)
            {
                var thisPartMod = "";
                var thisPartSep = packageNameParts[i].Split("-");
                if (thisPartSep.Length > 0)
                    thisPartMod = thisPartSep[thisPartSep.Length - 1];
                if (commonAppModifiers.Contains(packageNameParts[i]) || commonAppModifiers.Contains(thisPartMod))
                {
                    /*if (thisPartMod != "")
                    {
                        packageNameParts[i] = packageNameParts[i].Replace(thisPartMod, "");
                        packageNameParts[i] = packageNameParts[i].Replace("-", "");
                        thisApp.appNameModifier = thisPartMod;
                    }
                    else
                    {*/
                        thisApp.appNameModifier = packageNameParts[i];
                        if (i > 0)
                        {
                            actualAppNamePos = i - 1;
                        }
                   // }
                }
            }
            thisApp.appNameSuffix = packageNameParts[actualAppNamePos];
            for (var i=0; i < actualAppNamePos; i++)
            {
                thisApp.publisherPrefix += packageNameParts[i] + ".";
            }
            if (thisApp.publisherPrefix != null)
                thisApp.publisherPrefix = thisApp.publisherPrefix.Substring(0, thisApp.publisherPrefix.Length - 1);
            return thisApp;
        }

        public static List<string> GetCommonAppNameModifiers()
        {
            List<string> commonModifiers = new List<string>();
            commonModifiers.Add("hd");
            commonModifiers.Add("pro");
            commonModifiers.Add("lite");
            commonModifiers.Add("free");
            commonModifiers.Add("webos");
            commonModifiers.Add("touchpad");
            commonModifiers.Add("uk");
            commonModifiers.Add("us");
            commonModifiers.Add("touchpad");
            commonModifiers.Add("na");
            commonModifiers.Add("eu");
            return commonModifiers;
        }

        public static List<AppDefinition> ReadCatalogFile(string catalogFileName)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<AppDefinition>>(File.ReadAllText(catalogFileName));
        }

        public static List<AppDefinitionOld> ReadOldCatalogFile(string catalogFileName)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<AppDefinitionOld>>(File.ReadAllText(catalogFileName));
        }

        public static void WriteCatalogFile(string catalogName, List<AppDefinition> newAppCatalog)
        {
            Console.WriteLine(catalogName + " app catalog count now: " + newAppCatalog.Count);
            string newAppCatJson = Newtonsoft.Json.JsonConvert.SerializeObject(newAppCatalog);
            StreamWriter objWriter;
            objWriter = new StreamWriter(Path.Combine(appCatalogDir, catalogName + "AppData.json"));
            objWriter.WriteLine(newAppCatJson);
            objWriter.Close();
        }

        #endregion
    }


}
