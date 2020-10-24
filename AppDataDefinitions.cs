using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace webOS.AppCatalog
{
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

    [JsonObject]
    public class UserRatingsDefinition
    {
        public RatingsDefinition[] UserRatingList;
    }

    [JsonObject]
    public class RatingsDefinition
    {
        public string accountId;
        public string comment;
        public string creationtime;
        public string creator;
        public int id;
        public bool isAnonymous;
        public bool isInappropriate;
        public string locale;
        public int score;
    }
}
