using System.Collections.Generic;
using System.Linq;
using MonikTerminal.Interfaces;
using MonikTerminal.ModelsApp;
using System.Threading.Tasks;
using System;
using MonikTerminal.ModelsApi;
using System.Text;
using MonikTerminal.Enums;

namespace MonikTerminal
{
    public class Converter
    {
        public static string LevelTypeToString(LevelType aType)
        {
            switch (aType)
            {
                case LevelType.None:
                    return "non";
                case LevelType.System:
                    return "sys";
                case LevelType.Application:
                    return "app";
                case LevelType.Logic:
                    return "lgc";
                case LevelType.Security:
                    return "sec";
                default:
                    throw new NotImplementedException();
            }
        }

        public static LevelType StringToLevelType(string aStr)
        {
            return aStr == "sys" || aStr == "system"      ? LevelType.System :
                aStr    == "app" || aStr == "application" ? LevelType.Application :
                aStr    == "lgc" || aStr == "logic"       ? LevelType.Logic :
                aStr    == "sec" || aStr == "security"    ? LevelType.Security : LevelType.None;
        }

        public static string SeverityToString(SeverityCutoffType aSeverity)
        {
            switch (aSeverity)
            {
                case SeverityCutoffType.None:
                    return "non";
                case SeverityCutoffType.Verbose:
                    return "ver";
                case SeverityCutoffType.Info:
                    return "inf";
                case SeverityCutoffType.Warning:
                    return "wrn";
                case SeverityCutoffType.Error:
                    return "err";
                case SeverityCutoffType.Fatal:
                    return "fat";
                default:
                    throw new NotImplementedException();
            }
        }

        public static SeverityCutoffType StringToSeverity(string aStr)
        {
            return aStr == "ver" || aStr == "verbose" ? SeverityCutoffType.Verbose :
                   aStr == "inf" || aStr == "info"    ? SeverityCutoffType.Info :
                   aStr == "wrn" || aStr == "warning" ? SeverityCutoffType.Warning :
                   aStr == "err" || aStr == "error"   ? SeverityCutoffType.Error :
                   aStr == "fat" || aStr == "fatal"   ? SeverityCutoffType.Fatal : SeverityCutoffType.None;
        }
    } //end of class
}