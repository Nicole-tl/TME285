using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace InternetDataAcquisitionLibrary
{
    public class HTMLParser
    {
        protected string rawHtml;
        protected List<string> processedHtmlList = null;

        public HTMLParser(string rawHtml)
        {
            this.rawHtml = WebUtility.HtmlDecode(rawHtml);  // Fixes special characters etc.
        }

        public static string RemoveHTMLTags(string inputString)
        {
            string outputString = "";
            Boolean inTag = false;
            for (int kk = 0; kk < inputString.Length; kk++)
            {
                if (!inTag)
                {
                    if (inputString[kk] == '<')
                    {
                        inTag = true;
                    }
                    else { outputString += inputString[kk]; }
                }
                else
                {
                    if (inputString[kk] == '>')
                    {
                        inTag = false;
                    }
                }
            }
            return outputString;
        }

        public void Split(List<string> splitStringList)
        {
            if (processedHtmlList == null)
            {
                processedHtmlList = rawHtml.Split(splitStringList.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            else
            {
                List<string> newProcessedHtmlList = new List<string>();
                foreach (string processedHtmlString in processedHtmlList)
                {
                    List<string> newProcessedHtmlPartialList = processedHtmlString.Split(splitStringList.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                    newProcessedHtmlList.AddRange(newProcessedHtmlPartialList);
                }
                processedHtmlList = new List<string>();
                foreach (string newProcessedHtmlString in newProcessedHtmlList)
                {
                    processedHtmlList.Add(newProcessedHtmlString);
                }
            }
        }

        public List<string> ExtractAllSubstrings(string firstIdentifier, string lastIdentifier)
        {
            List<string> matchingStringList = new List<string>();
            foreach (string processedHTMLString in processedHtmlList)
            {
                if (processedHTMLString.Contains(firstIdentifier))
                {
                    int startIndex = processedHTMLString.IndexOf(firstIdentifier);
                    string subString = processedHTMLString.Substring(startIndex, processedHTMLString.Length - startIndex);
                    if (subString.Contains(lastIdentifier))
                    {
                        int length = subString.Length;
                        int endIndex = subString.IndexOf(lastIdentifier) +lastIdentifier.Length;
                        string subSubString = subString.Substring(0, endIndex);
                        matchingStringList.Add(subSubString);
                    }
                }
            }
            return matchingStringList;
        }

        public List<string> GetAllStrings(List<string> identifierList)
        {
            List<string> matchingStringList = new List<string>();
            foreach (string processedHTMLString in processedHtmlList)
            {
                Boolean ok = true;
                foreach (string identifier in identifierList)
                {
                    if (!processedHTMLString.Contains(identifier))
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok) { matchingStringList.Add(processedHTMLString); }
            }
            return matchingStringList;
        }

        public List<string> ProcessedHtmlList
        {
            get { return processedHtmlList; }
        }
    }
}
