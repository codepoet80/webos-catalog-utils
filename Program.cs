﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace AppCatalogUtils
{
    class Program
    {
        #region Environment and UI
        [JsonObject]
        public class AppDefinition
        {
            public int id;
            public string title;
            public string author;
            public string summary;
            public string appIcon;
            public string appIconBig;
            public string category;
            public string vendorId;
            public bool Pixi;
            public bool Pre;
            public bool Pre2;
            public bool Pre3;
            public bool Veer;
            public bool TouchPad;
            public bool touchpad_exclusive;
        }

        [JsonObject]
        public class ScreenshotDefinition
        {
            public string screenshot;
            public string thumbnail;
            public string orientation;
            public string device;
        }

        public class AppVersionDefinition
        {
            public string packageName;
            public string publisherPrefix;
            public string appNameSuffix;
            public string appNameModifier;
            public int majorVersion;
            public int minorVersion;
            public int buildVersion;
            public string platform;
        }

        public static string catalogFile;
        public static string destBaseDir = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\");
        public static string fileDir = @"D:\webOS";

        const string AppPackageSub = "AppPackages";
        public static string appPackageDir;
        const string AppMetaSub = "AppMetaData";
        public static string appMetaDir;
        const string AppImageSub = "AppImages";
        public static string appImageDir;
        const string AppUpdateSub = "AppUpdates";
        public static string appUpdateDir;
        const string AppUnknownSub = "AppUnknown";
        public static string appUnknownDir;

        public static string catalogIndexReport = @"CatalogIndexResults.csv";
        public static string MetaReverseReport = @"MetaReverseReport.csv";
        public static string FileReverseReport = @"FileReverseReport.csv";
        public static string ImageSortReport = @"ImageSortReport.csv";
        public static string MissingPackageInfoReport = @"MissingPackageInfo.csv";

        public static void Main(string[] args)
        {
            //Figure out our base folder
            destBaseDir = Path.GetFullPath(destBaseDir);
            Console.WriteLine("Current Destination Folder Base: " + destBaseDir);
            Console.Write("Enter alternate path, or press enter: ");
            string strInputDir = Console.ReadLine();
            if (strInputDir.Length > 1)
                destBaseDir = strInputDir;
            Console.WriteLine("Using: " + destBaseDir);
            if (!Directory.Exists(destBaseDir))
            {
                Console.WriteLine("Warning: Destination folder not found, will attempt to create!");
                Directory.CreateDirectory(destBaseDir);
            }
            Console.WriteLine();

            //Read Master App Data (aka the Catalog) into memory
            catalogFile = Path.Combine(destBaseDir, "masterAppData.json");
            Console.WriteLine("Current Catalog File: " + catalogFile);
            Console.WriteLine("Enter alternate path, or press enter: ");
            string strInputFile = Console.ReadLine();
            if (strInputFile.Length > 1 && File.Exists(strInputFile))
                catalogFile = strInputFile;
            Console.WriteLine("Reading Catalog index from: " + catalogFile);
            List<AppDefinition> appCatalog = ReadCatalogFile(catalogFile);
            Console.WriteLine("Apps in catalog:" + appCatalog.Count.ToString());
            Console.WriteLine();
            
            //Figure out other folders
            appPackageDir = Path.Combine(destBaseDir, AppPackageSub);
            appMetaDir = Path.Combine(destBaseDir, AppMetaSub);
            appImageDir = Path.Combine(destBaseDir, AppImageSub);
            appUpdateDir = Path.Combine(destBaseDir, AppUpdateSub);
            appUnknownDir = Path.Combine(destBaseDir, AppUnknownSub);
            MetaReverseReport = Path.Combine(destBaseDir, MetaReverseReport);
            FileReverseReport = Path.Combine(destBaseDir, FileReverseReport);
            ImageSortReport = Path.Combine(destBaseDir, ImageSortReport);
            MissingPackageInfoReport = Path.Combine(destBaseDir, MissingPackageInfoReport);

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
            Console.WriteLine("4) * Update catalog from metadata");
            Console.WriteLine("5) Reverse catalog check from folder");
            Console.WriteLine("6) Generate catalog files from extant apps");
            Console.WriteLine("7) Scrape icons from web to destination");
            Console.WriteLine("8) Search local folder and Internet sources for images in metadata");
            Console.WriteLine("9) Build missing package with possible match report from folder");
            Console.WriteLine("X) Exit");
            Console.WriteLine();
            Console.Write("Selection: ");
        }

        public static bool GetMenuChoice(List<AppDefinition> appCatalog)
        {
            ConsoleKeyInfo choice = Console.ReadKey();
            switch (choice.Key)
            {
                case ConsoleKey.D1: //Index packages against metadata to destination folder
                    {
                        Console.WriteLine();
                        Console.WriteLine();

                        string searchFolder = fileDir;
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
                        Console.WriteLine();
                        Console.WriteLine();

                        string searchFolder = fileDir;
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
                        Console.WriteLine();
                        Console.WriteLine();
                        ReverseCatalogFromMetaData(appCatalog);
                        return true;
                    }
                case ConsoleKey.D4: //Update catalog from metadata
                    {
                        Console.WriteLine();
                        Console.WriteLine("Not Implemented");
                        return true;
                    }
                case ConsoleKey.D5: //Reverse check catalog from folder
                    {
                        Console.WriteLine();
                        Console.WriteLine();

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
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("Catalog extant files to: " + destBaseDir);
                        Console.WriteLine("Catalog missing files to: " + destBaseDir);
                        Console.WriteLine();
                        GenerateExtantCatalog(appCatalog);
                        return true;
                    }
                case ConsoleKey.D7: //Scrape Folder for Icons
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                        GetAppIconsFromCatalog(appCatalog);
                        return true;
                    }
                case ConsoleKey.D8: //Search multiple sources for Images
                    {
                        Console.WriteLine();
                        Console.WriteLine();

                        string searchFolder = Path.Combine(destBaseDir, "_AppImages");
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
                        Console.WriteLine();
                        Console.WriteLine();
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
                case ConsoleKey.X:  //Quit
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("Quitting");
                        return false;
                    }
                default:
                    {
                        Console.CursorTop = Console.CursorTop - 16;
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
            objWriter = new StreamWriter(Path.Combine(destBaseDir, catalogIndexReport));
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
                        var newHome = Path.Combine(destBaseDir, appPackageDir, fileName);
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

        public static void GetAppIconsFromCatalog(List<AppDefinition> appCatalog)
        {
            Console.WriteLine("Searching for app icons on the web...");
            int i = 0;
            StreamWriter objWriter;
            objWriter = new StreamWriter(Path.Combine(destBaseDir, "missingIcons.csv"));
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
                        Console.WriteLine("Added   ");
                        extantAppCatalog.Add(appObj);
                    }
                    else
                    {
                        Console.WriteLine("Skipped ");
                        missingAppCatalog.Add(appObj);
                    }
                }
            }
            WriteCatalogFile("Extant", extantAppCatalog);
            WriteCatalogFile("Missing", missingAppCatalog);
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

        public static AppVersionDefinition ExtractAppVersionDefinition(string fileName)
        {
            fileName = fileName.ToLower();
            AppVersionDefinition thisApp = new AppVersionDefinition();
            string[] fileNameParts = fileName.Split("_");
            thisApp.packageName = fileNameParts[0];
            string versionNumber;
            if (fileNameParts.Length > 2)
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
                thisApp.platform = fileNameParts[^1].Replace(".ipk", "");
            }
            string[] packageNameParts = thisApp.packageName.Split(".");
            
            //Rule out common app name modifiers
            int actualAppNamePos = packageNameParts.Length-1;
            List<string> commonAppModifiers = GetCommonAppNameModifiers();
            for (var i=packageNameParts.Length-1; i > 0; i--)
            {
                if (commonAppModifiers.Contains(packageNameParts[i]))
                {
                    thisApp.appNameModifier = packageNameParts[i];
                    if (i > 0)
                    {
                        actualAppNamePos = i - 1;
                    }
                }
            }
            thisApp.appNameSuffix = packageNameParts[actualAppNamePos];
            for (var i=0; i < actualAppNamePos; i++)
            {
                thisApp.publisherPrefix += packageNameParts[i] + ".";
            }
            if (thisApp.publisherPrefix != null)
                thisApp.publisherPrefix = thisApp.publisherPrefix.Substring(0, thisApp.publisherPrefix.Length - 1);
            if (thisApp.appNameModifier != null && thisApp.appNameModifier != "")
            {
                Debug.Print("found one");
            }
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
            commonModifiers.Add("app");
            commonModifiers.Add("touchpad");
            return commonModifiers;
        }

        public static List<AppDefinition> ReadCatalogFile(string catalogFileName)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<AppDefinition>>(File.ReadAllText(catalogFileName));
        }

        public static void WriteCatalogFile(string catalogName, List<AppDefinition> newAppCatalog)
        {
            Console.WriteLine(catalogName + " app catalog count: " + newAppCatalog.Count);
            string newAppCatJson = Newtonsoft.Json.JsonConvert.SerializeObject(newAppCatalog);
            StreamWriter objWriter;
            objWriter = new StreamWriter(Path.Combine(destBaseDir, catalogName + "AppData.json"));
            objWriter.WriteLine(newAppCatJson);
            objWriter.Close();
        }

        #endregion
    }


}
