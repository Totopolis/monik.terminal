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

        public static string TerminalModeToString(TerminalMode aMode)
        {
            switch (aMode)
            {
                case TerminalMode.Single:
                    return "single";
                case TerminalMode.Stream:
                    return "stream";
                default:
                    throw new NotImplementedException();
            }
        }

        public static TerminalMode StringToTerminalMode(string aStr)
        {
            return aStr == "single" ? TerminalMode.Single : TerminalMode.Stream;
        }

        public static string Truncate(string value, int maxLen, string dots = "..")
        {
            return value.Length <= maxLen ? value : value.Substring(0, maxLen - dots.Length) + dots;
        }
    } //end of class
}