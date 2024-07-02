/* Made by Fabian Müller
 * GNU-License ♥
 * 
 * 
 * 
*/


using System;
using System.IO;
using System.Text.RegularExpressions;

namespace dirplus
{
    class Program
    {
        public const string Version = "DiR++ Version 0.0.2. © 2024 Fabian Müller. Made in Germany.";
        public static void Main(string[] args)
        {
            
            ConsoleColor OFG = System.Console.ForegroundColor;
            ConsoleColor OBG = System.Console.BackgroundColor;
            
            string path = Environment.CurrentDirectory;
            bool recursive = false;
            
            
            // Argument-Handling.
            for(int i =0; i<args.Length; i++) {
                if (args[i] == "-version" || args[i] == "-ver" ||args[i] == "-v"){
                    System.Console.WriteLine(Version);
                    Environment.Exit(0);
                } else if (args[i] == "-?" ||args[i] == "-help" || args[i] == "-wtf") {
                    helpDialog();
                } else if (args[i] == "~") {
                    recursive = true;
                } else if (System.IO.Directory.Exists(args[i])){
                    path = args[i];
                }
            }
            
            
            // If window is too small-> Exit to prevent cursor overflow
            if (System.Console.WindowWidth < 65) {
                System.Console.WriteLine("Window is too small!");
                Environment.Exit(0);
            }
            
            // Check if Path exists. Otherwise-> Exit.
            // It is (quite) impossible to trigger this, but if the directory is deleted while looping
            // This will "catch" the exception.
            if (!System.IO.Directory.Exists(path)) {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.BackgroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Path'" + path + "'is invalid or does not exist!");
                System.Console.ForegroundColor = OFG;
                System.Console.BackgroundColor = OBG;
                Environment.Exit(0);
                
            }
            
            
            
            
            string [] subdirectoryEntries = Directory.GetDirectories(path); // Directory Content Array
            
            subdirectoryEntries = intelligentSort(subdirectoryEntries, args); // Search and sort Array


            if(recursive) { // If argument "~" is set, work 1 depth recursive
                string[] localsubdirectoryEntries = Directory.GetDirectories(path);
                int substringindex = localsubdirectoryEntries.Length > 0 ? localsubdirectoryEntries[0].LastIndexOf("\\") + 1 : 0;

                // For each subdirectory in the main directory
                foreach (string subdirectory in intelligentSort(localsubdirectoryEntries, args))
                {
                    DirectoryInfo dir = new DirectoryInfo(subdirectory);
                    string InfoString = "";
                    try
                    {
                        drawFile(subdirectory.Substring(subdirectory.LastIndexOf("\\") + 1) + InfoString, true, dir.GetFiles().Length.ToString() + " files");

                        int counter = 0;
                        string hierarchyChar = "├─■ "; // Visual Symbol to understand the depth as a subfolder-File.
                        string[] subSubdirectoryEntries = Directory.GetDirectories(subdirectory);
                        subSubdirectoryEntries = intelligentSort(subSubdirectoryEntries, args); // Sort and filter subdirectories

                        // For each subdirectory in the subdirectory
                        foreach (string subsubdirectory in subSubdirectoryEntries)
                        {
                            if (counter == subSubdirectoryEntries.Length - 1)
                            {
                                hierarchyChar = "└─■ ";
                            }
                            counter++;
                            DirectoryInfo subdir = new DirectoryInfo(subsubdirectory);
                            string subInfoString = "";
                            try
                            { // Write file info
                                drawFile(hierarchyChar + subsubdirectory.Substring(subsubdirectory.LastIndexOf("\\") + 1) + InfoString, true, subdir.GetFiles().Length.ToString() + " files");
                                listFiles(subsubdirectory, args, true); // List files in subdirectory
                            }
                            catch (Exception e)
                            { // Handle access error
                                drawFilePermissionError(hierarchyChar + subsubdirectory.Substring(subsubdirectory.LastIndexOf("\\") + 1) + InfoString, true);
                                listFiles(subsubdirectory, args, true); // List files in subdirectory even if access error
                            }
                        }
                    }
                    catch (Exception e)
                    { // Handle access error for main subdirectory
                        drawFilePermissionError(subdirectory.Substring(subdirectory.LastIndexOf("\\") + 1) + InfoString, true);
                    }
                }
            }


            // For each File in Directory
            foreach (string subdirectory in subdirectoryEntries) {
                DirectoryInfo dir = new DirectoryInfo(subdirectory);
                string InfoString = "";
                try { // Access okay-> Nice
                    drawFile(subdirectory.Substring(subdirectory.LastIndexOf("\\")+1) + InfoString,true, dir.GetFiles().Length.ToString() + " files");
                } catch (Exception e) { // Access not okay-> fuck it  ¯\_(ツ)_/¯
                    drawFilePermissionError(subdirectory.Substring(subdirectory.LastIndexOf("\\")+1)  + InfoString,true);
                }
            }
            listFiles(path,args, false);
            // Re-Set consoles colors back to the original colors
            System.Console.ForegroundColor = OFG;
            System.Console.BackgroundColor = OBG;
            
            
        }
        
        public static void listFiles(string path, string [] args, bool isSubFile){
            
            int counter = 0;
            string hierarchyChar = "     ├─■ ";//
                    
            string[] sizes = {"B ","KB","MB","GB","TB","PB", "YB"};
            string [] fileEntries = Directory.GetFiles(path);
            fileEntries = intelligentSort(fileEntries, args);
            foreach(string fileName in fileEntries) {
                if (isSubFile) {
                    if (counter == fileEntries.Length-1){
                        hierarchyChar = "     └─■ ";
                    }
                } else {
                    hierarchyChar = " ";
                }
                counter++;
                FileInfo file = new FileInfo(fileName);
                
                long fl = 0; // FileSize
                
                try { // Get FileSize, if System allows it ♥
                    fl = file.Length;
                } catch (Exception e) { // otherwise just set it to 0. I mean, who cares?
                    fl = 0;
                }
                
                // Calculate File-Size Allocation from Byte to PetaByte♥
                int depth = 0;
                while (fl >= 1024) {
                    fl /= 1024;
                    depth++;
                }
                
                
                
                try { // Write File to Screen
                    drawFile(hierarchyChar + fileName.Substring(fileName.LastIndexOf("\\")+1),false,Convert.ToString(fl) + " " + sizes[depth]);
                } catch (Exception e) { // Otherwise, throw error and say something like "bla, not good" and so on
                    drawFilePermissionError(hierarchyChar + fileName.Substring(fileName.LastIndexOf("\\")+1),false);
                }
            }
        }
        
        
        
        // Search and Sort array. Regex or not; Depends on the arguments.
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
                            
                            
                            
                            if (!isValidRegex(searchPattern)){ // Is Regular Expression valid? No? This!
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
                            Regex rg = new Regex(regularPattern); // Declare and initialize RegEx
                            for (int ent = entries.Length-1; ent>=0; ent--){ // Search and remove stuff, that does not match.
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
        public static string validateSeperator(string path){
            return path.Replace("\\","/");
        }
        
        public static void drawFile(string text, bool type, string filesize){
            text = validateSeperator(text);
            ConsoleColor OFG = System.Console.ForegroundColor;
            ConsoleColor OBG = System.Console.BackgroundColor;
            
            System.Console.CursorLeft = 0;
            
            if (type == true && !(text.StartsWith("├")) && !(text.StartsWith("└"))) {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.Write("./");
                System.Console.ForegroundColor = ConsoleColor.Green;
            } else {
                System.Console.ForegroundColor = ConsoleColor.Blue;
            }
            if ((text.StartsWith("├")) || (text.StartsWith("└"))) {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.Write(" ");
                System.Console.ForegroundColor = ConsoleColor.Yellow;
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
            text = validateSeperator(text);
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
            ConsoleColor OFG = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.DarkMagenta;
            System.Console.WriteLine(" _______   __                               ");
            System.Console.ForegroundColor = ConsoleColor.Magenta;
            System.Console.WriteLine("|       \\ |  \\              __        __    ");
            System.Console.ForegroundColor = ConsoleColor.DarkRed;
            System.Console.WriteLine("| $$$$$$$\\ \\$$  ______     |  \\      |  \\   ");
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("| $$  | $$|  \\ /      \\  __| $$__  __| $$__ ");
            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            System.Console.WriteLine("| $$  | $$| $$|  $$$$$$\\|    $$  \\|    $$  \\");
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("| $$  | $$| $$| $$   \\$$ \\$$$$$$$$ \\$$$$$$$$");
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("| $$__/ $$| $$| $$         | $$      | $$   ");
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine("| $$    $$| $$| $$          \\$$       \\$$   ");
            System.Console.ForegroundColor = ConsoleColor.Blue;
            System.Console.WriteLine(" \\$$$$$$$  \\$$ \\$$                          ");
            System.Console.WriteLine("");
            System.Console.ForegroundColor = ConsoleColor.Magenta;
            System.Console.WriteLine(Version);
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("-s\t\t Search for Files");
            System.Console.WriteLine("-sf\t\t Search in Files");
            System.Console.WriteLine("-r\t\t Reverse the output");
            System.Console.WriteLine("-help, -wtf, -?\t This Help Dialog");
            System.Console.WriteLine("".PadRight(System.Console.WindowWidth,'='));
            System.Console.WriteLine("~\t\tSubdirectories");
            System.Console.WriteLine("?YOUR_REGEX\tRegEx");
            System.Console.ForegroundColor = OFG;
            
            
            
            
            
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
