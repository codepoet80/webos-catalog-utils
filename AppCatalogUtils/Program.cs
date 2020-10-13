using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

        public static string catalogFile;
        public static string destBaseDir = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\");
        public static string fileDir = @"D:\webOS";

        const string AppPackageSub = "AppPackages";
        public static string appPackageDir;
        const string AppMetaSub = "AppMetaData";
        public static string appMetaDir;
        const string AppImageSub = "AppImages";
        public static string appImageDir;
        const string AppUpdateSub = "AppUpdates";
        public static string appUpdateDir;

        public static string catalogIndexReport = @"CatalogIndexResults.csv";
        public static string MetaReverseReport = @"MetaReverseReport.csv";
        public static string FileReverseReport = @"FileReverseReport.csv";
        public static string ImageSortReport = @"ImageSortReport.csv";

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
            List<AppDefinition> appCatalog = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AppDefinition>>(File.ReadAllText(catalogFile));
            Console.WriteLine("Apps in catalog:" + appCatalog.Count.ToString());
            Console.WriteLine();
            
            //Figure out other folders
            appPackageDir = Path.Combine(destBaseDir, AppPackageSub);
            appMetaDir = Path.Combine(destBaseDir, AppMetaSub);
            appImageDir = Path.Combine(destBaseDir, AppImageSub);
            appUpdateDir = Path.Combine(destBaseDir, AppUpdateSub);
            MetaReverseReport = Path.Combine(destBaseDir, MetaReverseReport);
            FileReverseReport = Path.Combine(destBaseDir, FileReverseReport);
            ImageSortReport = Path.Combine(destBaseDir, ImageSortReport);

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
            Console.WriteLine("6) Scrape Wayback Machine for images to destination");
            Console.WriteLine("7) Scrape icons from web to destination");
            Console.WriteLine("8) Search folder for images in metadata");
            Console.WriteLine("9) Generate catalog files from extant apps");
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
                case ConsoleKey.D6: //Scrape Wayback Machine for Images
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("Not implemented");
                        return true;
                    }
                case ConsoleKey.D7: //Scrape Folder for Icons
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                        GetAppIconsFromCatalog(appCatalog);
                        return true;
                    }
                case ConsoleKey.D8: //Search folder for Images
                    {
                        Console.WriteLine();
                        Console.WriteLine();

                        string searchFolder = Path.Combine(destBaseDir, "_AppImages");
                        Console.WriteLine("Current Search Folder: " + searchFolder);
                        Console.WriteLine("Enter alternate path, or press enter: ");
                        string strInputPath = Console.ReadLine();
                        if (strInputPath.Length > 1 && Directory.Exists(strInputPath))
                            searchFolder = strInputPath;

                        GetScreenShotsInFolderFromMetaData(searchFolder);
                        return true;
                    }
                case ConsoleKey.D9: //Generate extant Catalog
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("Catalog extant files to: " + destBaseDir);
                        Console.WriteLine("Catalog missing files to: " + destBaseDir);
                        Console.WriteLine();
                        GenerateExtantCatalog(appCatalog);
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
                        Console.CursorTop--;
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

        public static void GetScreenShotsInFolderFromMetaData(string searchFolder)
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
                            Console.WriteLine("Not found");
                            totalMissing++;
                            screensFound++;
                        }
                        else
                        {
                            Console.WriteLine("Found");
                            screensMissing++;
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
                            Console.WriteLine("Not found");
                            totalMissing++;
                            thumbsMissing++;
                        }
                        else
                        {
                            Console.WriteLine("Found");
                            totalFound++;
                            thumbsFound++;
                        }
                    }
                }
                
                //Update report with findings
                objWriter.WriteLine(metaFileIndexStr + "," + thumbsFound + "," + screensFound + "," + thumbsMissing + "," + screensMissing);
                i++;
            }
            objWriter.Close();
            Console.WriteLine();
            Console.WriteLine("Total Meta Files:             " + fileEntries.Length.ToString());
            Console.WriteLine("Total Images Found: " + totalFound.ToString());
            Console.WriteLine("Total Images Missing: " + totalMissing.ToString());
            Console.WriteLine();
            Console.WriteLine("Detailed report: " + ImageSortReport);
        }

        public static bool TryGetImageFromPalmCDN(string getPath, string savePath)
        {
            string palmCDNPath = "http://cdn.downloads.palm.com/public/" + getPath;
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(palmCDNPath), savePath);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public static bool TrySortImageFromFolder(string appId, string imageSize, string imageFile, string imagePath, string searchPath)
        {
            string searchImage = appId + "_" + imageSize + "_uri_" + imageFile;
            imagePath = imagePath.Replace("/", "\\");
            Console.Write("     Sorting " + searchImage + "...");
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
