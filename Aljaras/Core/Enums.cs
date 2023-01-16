using LiteDB;
using System.Reflection;
using System;
using System.IO;

namespace Aljaras.Core
{
    public class GlobalVariables
    {
        public static readonly string? AppName = Assembly.GetExecutingAssembly().GetName().Name;//"Aljaras"
        public static readonly string AppLocation = AppDomain.CurrentDomain.BaseDirectory;//"C:\\Users\\Rellax\\source\\repos\\Aljaras\\Aljaras\\bin\\Debug\\net7.0-windows\\"
        public static readonly string dbName = AppName + ".jrsdb";//"Aljaras.jrsdb"
        public static readonly string dbBackupName = AppName + ".jrsbck";//"Aljaras.jrsbck"
        public static readonly string PCCurrentUserName = Environment.UserName;//"Rellax"
        //public static readonly string fullDBPath = Path.GetFullPath(PCCurrentUserName + dbName); //AppLocation + PCCurrentUserName + dbName;
        public static readonly string dbConnectionString = string.Concat("Filename=", PCCurrentUserName + dbName /*fullDBPath*/, ";Connection=shared");//"Filename=RellaxAljaras.jrsdb;Connection=shared"
        public static readonly LiteDatabase db = new(dbConnectionString);
    }

    public enum DbTables
    {
        Schedules,
        Alarms,
        UserSettings,
        Holidays
    }

    public enum GetDayTime
    {
        AM,
        PM
    }

    public enum GetVisibility
    {
        Collapsed,
        Hidden,
        Visible
    }

    public enum MessageBackground
    {
        DarkSlateBlue,
        Goldenrod,
        IndianRed,
        LightCoral,
        LightSeaGreen,
        MediumOrchid,
        MediumPurple,
        MediumSeaGreen,
        MediumSlateBlue,
        OliveDrab,
        RoyalBlue,
        SeaGreen,
        SlateBlue,
        SlateGray,
        SteelBlue,
        Teal
    }
}
