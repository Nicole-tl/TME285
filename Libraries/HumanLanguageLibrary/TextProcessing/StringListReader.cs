using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanLanguageLibrary.TextProcessing
{
    public class StringListReader
    {
        public static List<string> Read(string filePath, char ignoreLineCharacter)
        {
            string ignoreLineString = ignoreLineCharacter.ToString();
            List<string> textList = new List<string>();
            StreamReader textReader = new StreamReader(filePath);
            while (!textReader.EndOfStream)
            {
                string text = textReader.ReadLine();
                if (text != "")
                {
                    if (!text.StartsWith(ignoreLineString))
                    {
                        textList.Add(text);
                    }
                }
            }
            textReader.Close();
            return textList;
        }
    }
}
