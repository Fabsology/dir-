using System;
using System.IO;
using System.Text.RegularExpressions;

namespace dirplus
{
    class Program
    {
        public const string Version = "DYR++ Version 0.0.1. © 2022 Fabian Müller. Made in Germany.";
        public static void Main(string[] args)
        {
            
            string[] sizes = {"B ","KB","MB","GB","TB","PB", "YB"};
            string path = Environment.CurrentDirectory;
            bool recursive = false;

            for(int i =0; i<args.Length; i++) {
                if (args[i] == "-version" || args[i] == "-ver" ||args[i] == "-v"){
                    System.Console.WriteLine(Version);
                    Environment.Exit(0);
                } else if (args[i] == "-?" ||args[i] == "-help" || args[i] == "-wtf") {
                    helpDialog();
                }
            }
            
            if (System.Console.WindowWidth < 65) {
                System.Console.WriteLine("Window is too small!");
                Environment.Exit(0);
            }
            
            if (args.Length>0 && (args[0]).Substring(0,1) != "-"){
                path = args[0];
                System.Console.WriteLine(path);
            }
            if (!System.IO.Directory.Exists(path)) {
                
                ConsoleColor OFG = System.Console.ForegroundColor;
                ConsoleColor OBG = System.Console.BackgroundColor;
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.BackgroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Path'" + path + "'is invalid or does not exist!");
                System.Console.ForegroundColor = OFG;
                System.Console.BackgroundColor = OBG;
                Environment.Exit(0);
                
            }
            
            string [] subdirectoryEntries = Directory.GetDirectories(path);
            
            subdirectoryEntries = intelligentSort(subdirectoryEntries, args);
            
            
            foreach(string subdirectory in subdirectoryEntries) {
                DirectoryInfo dir = new DirectoryInfo(subdirectory);
                string InfoString = "";
                try {
                    drawFile(subdirectory.Substring(subdirectory.LastIndexOf("\\")+1) + InfoString,true, dir.GetFiles().Length.ToString() + " files");
                } catch (Exception e) {
                    drawFilePermissionError(subdirectory.Substring(subdirectory.LastIndexOf("\\")+1)  + InfoString,true);
                }
            }
            
            
            string [] fileEntries = Directory.GetFiles(path);
            fileEntries = intelligentSort(fileEntries, args);
            foreach(string fileName in fileEntries) {
                FileInfo file = new FileInfo(fileName);
                long fl = file.Length;
                int depth = 0;
                while (fl >= 1024) {
                    fl /= 1024;
                    depth++;
                }
                
                
                
                try {
                    drawFile(fileName.Substring(fileName.LastIndexOf("\\")+1),false,Convert.ToString(fl) + " " + sizes[depth]);
                } catch (Exception e) {
                    drawFilePermissionError(fileName.Substring(fileName.LastIndexOf("\\")+1),false);
                }
            }
            
            //System.Console.ReadLine();
        }
        
        public static string[] intelligentSort(string[] entries, string[] args){
            for (int i=0; i< args.Length; i++){
                if (args[i].StartsWith("-")){
                    //Argument Found
                    string arg = args[i].Substring(1);
                    if (arg.Contains("r")){
                        Array.Reverse(entries);
                    }
                    
                    #region Searching
                    if (arg == "s"){ // Searching with patterns
                        if (args.Length > i+1){
                            string searchPattern = args[i +1];
                            bool isRegex = false;
                            if (searchPattern.StartsWith("?")){
                                searchPattern = searchPattern.Substring(1);
                                isRegex = true;
                            }
                            
                            
                            
                            if (!isValidRegex(searchPattern)){
                                ConsoleColor OFG = System.Console.ForegroundColor;
                                ConsoleColor OBG = System.Console.BackgroundColor;
                                System.Console.ForegroundColor = ConsoleColor.Yellow;
                                System.Console.BackgroundColor = ConsoleColor.Red;
                                System.Console.Write("The given regular expression is invalid! Given:");
                                System.Console.ForegroundColor = ConsoleColor.Magenta;
                                System.Console.BackgroundColor = ConsoleColor.Yellow;
                                System.Console.WriteLine(searchPattern);
                                
                                System.Console.ForegroundColor = OFG;
                                System.Console.BackgroundColor = OBG;
                                
                                Environment.Exit(1);
                                
                            }
                            
                            string regularPattern = searchPattern;
                            Regex rg = new Regex(regularPattern);
                            for (int ent = entries.Length-1; ent>=0; ent--){
                                if (!entries[ent].Substring(entries[ent].LastIndexOf("\\")+1).Contains(searchPattern) && !rg.IsMatch(entries[ent].Substring(entries[ent].LastIndexOf("\\")+1))) {
                                    RemoveAt(ref entries,ent);
                                }
                            }
                        }
                    }
                    #endregion
                    
                    
                    
                    #region Searching in Files
                    if (arg == "sf"){ // Searching with patterns
                        if (args.Length > i+1){
                            string searchPattern = args[i +1];
                            bool isRegex = false;
                            if (searchPattern.StartsWith("?")){
                                searchPattern = searchPattern.Substring(1);
                                isRegex = true;
                            }
                            
                            
                            
                            if (!isValidRegex(searchPattern)){
                                ConsoleColor OFG = System.Console.ForegroundColor;
                                ConsoleColor OBG = System.Console.BackgroundColor;
                                System.Console.ForegroundColor = ConsoleColor.Yellow;
                                System.Console.BackgroundColor = ConsoleColor.Red;
                                System.Console.Write("The given regular expression is invalid! Given:");
                                System.Console.ForegroundColor = ConsoleColor.Magenta;
                                System.Console.BackgroundColor = ConsoleColor.Yellow;
                                System.Console.WriteLine(searchPattern);
                                
                                System.Console.ForegroundColor = OFG;
                                System.Console.BackgroundColor = OBG;
                                
                                Environment.Exit(1);
                                
                            }
                            
                            string regularPattern = searchPattern;
                            Regex rg = new Regex(regularPattern);
                            for (int ent = entries.Length-1; ent>=0; ent--){
                                bool fileContains = false;
                                try {
                                    
                                    foreach (string line in File.ReadAllLines(entries[ent]))
                                    {
                                        
                                        if (line.Contains(searchPattern) || rg.IsMatch(line)) {
                                            fileContains = true;
                                        }
                                    }
                                } catch (Exception e) {
                                    
                                    RemoveAt(ref entries,ent);
                                }
                                
                                if (!fileContains) {
                                    RemoveAt(ref entries,ent);
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
            return entries;
        }
        
        
        public static void drawFile(string text, bool type, string filesize){
            ConsoleColor OFG = System.Console.ForegroundColor;
            ConsoleColor OBG = System.Console.BackgroundColor;
            
            System.Console.CursorLeft = 0;
            
            if (type == true) {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.Write("./");
                System.Console.ForegroundColor = ConsoleColor.Green;
            } else {
                System.Console.ForegroundColor = ConsoleColor.Blue;
            }
            
            
            
            if (text.Length + 20 > System.Console.WindowWidth) {
                text = text.Substring(0,System.Console.WindowWidth-15) + "...";
            }
            
            System.Console.WriteLine(text.PadRight(System.Console.WindowWidth-12,' '));
            
            System.Console.CursorTop--;
            System.Console.CursorLeft = System.Console.WindowWidth-12;
            if (type == true){
                System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            } else {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
            }
            System.Console.Write(filesize.PadLeft(8,' '));
            
            System.Console.CursorTop++;
            System.Console.ForegroundColor = OFG;
            System.Console.BackgroundColor = OBG;
            
        }
        
        
        public static void drawFilePermissionError(string text, bool type){
            ConsoleColor OFG = System.Console.ForegroundColor;
            ConsoleColor OBG = System.Console.BackgroundColor;
            
            System.Console.CursorLeft = 0;
            
            if (type == true) {
                System.Console.ForegroundColor = ConsoleColor.DarkYellow;
                System.Console.Write("./");
            }
            System.Console.ForegroundColor = ConsoleColor.Red;
            
            
            
            if (text.Length + 20 > System.Console.WindowWidth) {
                text = text.Substring(0,System.Console.WindowWidth-15) + "...";
            }
            
            System.Console.WriteLine(text.PadRight(System.Console.WindowWidth-12,' '));
            
            System.Console.CursorTop--;
            System.Console.CursorLeft = System.Console.WindowWidth-12;
            if (type == true){
                System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            } else {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
            }
            System.Console.Write("A/D".PadLeft(8,' '));
            
            System.Console.CursorTop++;
            System.Console.ForegroundColor = OFG;
            System.Console.BackgroundColor = OBG;
        }
        
        
        
        public static void helpDialog(){
            System.Console.WriteLine(Version);
            System.Console.WriteLine("-s\t\t Search for Files");
            System.Console.WriteLine("-sf\t\t Search in Files");
            System.Console.WriteLine("-r\t\t Reverse the output");
            System.Console.WriteLine("-help, -wtf, -?\t This Help Dialog");
            System.Console.WriteLine("".PadRight(System.Console.WindowWidth,'='));
            
            
            
            
            
            Environment.Exit(0);
        }
        
        //Operational Additional Methods:
        public static void RemoveAt<T>(ref T[] arr, int index)
        {
            for (int a = index; a < arr.Length - 1; a++)
            {
                // moving elements downwards, to fill the gap at [index]
                arr[a] = arr[a + 1];
            }
            // finally, let's decrement Array's size by one
            if (arr.Length > 0) {
                Array.Resize(ref arr, arr.Length - 1);
            }
        }
        
        private static bool isValidRegex(string pattern)
        {
            try
            {
                Regex.Match("", pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }
            
            return true;
        }
        
    }
}