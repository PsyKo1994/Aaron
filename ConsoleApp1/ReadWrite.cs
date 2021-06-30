using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class ReadWrite
    {
        public static void Write(string reaction)
        {
            //Create file if it doesn't exist
            string path = @"C:\AFR\Discord\CurrentForms\attendance.txt";
            if ((!File.Exists(path))) //Checking if scores.txt exists or not
            {
                FileStream fs = File.Create(path); //Creates Attendance file
                fs.Close(); //Closes file stream
            }

            //Get name from string
            int startindex = reaction.IndexOf('<');
            int Endindex = reaction.IndexOf('>');
            string name = reaction.Substring(startindex + 1, Endindex - startindex - 1);

            string[] file = File.ReadAllLines(path);

            //if file is empty write to file, else check file
            if(file.Length == 0)
            {
                string appendText = reaction + Environment.NewLine;
                File.AppendAllText(path, appendText, Encoding.UTF8);
            }
            else
            {
                //Check if file contains entry, if so replace. Else write new entry
                bool entryupdated = false;
                for (int i = 0; i < file.Length; i++)
                {
                    if (file[i].Contains(name))
                    {
                        file[i] = reaction;
                        entryupdated = true;
                    }
                    i++;
                }

                if(entryupdated = true)
                {
                    string appendText = reaction + Environment.NewLine;
                    File.AppendAllText(path, appendText, Encoding.UTF8);
                    //File.WriteAllLines(path, file);
                }
            }

            /*
            //Write to file
            string text = File.ReadAllText(path);
            if (text.Contains(name))
            {
                text = text.Replace(text, reaction);
                File.WriteAllText(path, text);
                Console.WriteLine("match");
            }
            else
            {
                string appendText = reaction + Environment.NewLine;
                File.AppendAllText(path, appendText, Encoding.UTF8);
                Console.WriteLine("no match, writing new line");
            }
            */


            //string appendText = reaction + Environment.NewLine;
            //File.AppendAllText(path, appendText, Encoding.UTF8);

            // Open the file to read from.
            string[] readText = File.ReadAllLines(path, Encoding.UTF8);
            foreach (string s in readText)
            {
                Console.WriteLine(s);
            }

        }

    }
}
