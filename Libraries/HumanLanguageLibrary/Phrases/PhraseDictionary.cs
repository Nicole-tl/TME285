using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using HumanLanguageLibrary.TextProcessing;

namespace HumanLanguageLibrary.Phrases
{
    [DataContract]
    public class PhraseDictionary
    {
        public const string EQUIVALENCE = "<=>";
        public const string IMPLICATION = "=>";

        private List<PhraseDictionaryItem> compactItemList;
        private List<PhraseDictionaryItem> itemList; // The expanded list of items, generated upon Load().
        private List<ReplacementSpecification> replacementSpecificationList;

        public PhraseDictionary()
        {
            compactItemList = new List<PhraseDictionaryItem>();
            replacementSpecificationList = new List<ReplacementSpecification>();
        }

        public void Load(string filePath)
        {
            List<string> rawDataList = StringListReader.Read(filePath, '%');
            List<string> defaultReplacementSpecificationIdentifierList = ReplacementSpecification.GetDefaultReplacementSpecificationIdentifierList();
            foreach (string rawData in rawDataList)
            {
                if (rawData.StartsWith("$"))
                {
                    ReplacementSpecification replacementSpecification = new ReplacementSpecification();
                    List<string> information = rawData.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    string specification = information[0].Trim();
                    if (defaultReplacementSpecificationIdentifierList.Contains(specification))
                    {
                        replacementSpecification.Editable = false;
                        replacementSpecification.Identifier = information[0].Trim();
                        replacementSpecification.Description = information[1].Trim();
                        if (information.Count == 2)
                        {
                            replacementSpecification.Displayable = false;
                            replacementSpecification.ReplacementList = null;
                        }
                        else
                        {
                            if (information[2].EndsWith(".txt"))  // This option is currently not used. Left here for future expansion.
                            {
                             //   replacementSpecification.RelativeFilePath = information[2].Trim();
                            }
                            else
                            {
                                replacementSpecification.ReplacementList = new List<string>();
                                List<string> replacementList = information[2].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                foreach (string replacement in replacementList)
                                {
                                    replacementSpecification.ReplacementList.Add(replacement.Trim());
                                }
                                replacementSpecificationList.Add(replacementSpecification);
                            }
                        }
                        replacementSpecificationList.Add(replacementSpecification);
                    }
                    else
                    {
                        replacementSpecification.Editable = true;
                        replacementSpecification.Identifier = information[0].Trim();
                        replacementSpecification.Description = information[1].Trim();
                        replacementSpecification.Displayable = true;
                        List<string> replacementList = information[2].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        replacementSpecification.ReplacementList = new List<string>();
                        foreach (string replacement in replacementList)
                        {
                            replacementSpecification.ReplacementList.Add(replacement.Trim());
                        }
                        replacementSpecificationList.Add(replacementSpecification);
                    }
                }
                else
                {
                    PhraseDictionaryItem item = new PhraseDictionaryItem();
                    int operatorCenterIndex = rawData.IndexOf("=");
                    if (rawData[operatorCenterIndex - 1] == '<') { item.OneSidedInterchangeability = false; }
                    else { item.OneSidedInterchangeability = true; }
                    List<string> information = rawData.Split(new char[] { '<', '=', '>' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    //       Phrase firstPhrase = new Phrase();
                    item.Phrase1 = information[0].Trim();
                    item.Phrase2 = information[1].Trim();
                    compactItemList.Add(item);
                }
            }
            Expand();
        }

        private void WriteSymbolExplanation(StreamWriter writer)
        {
            writer.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            writer.WriteLine("% Explanation of symbols");
            writer.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            writer.WriteLine("%");
            writer.WriteLine("% P1 <=> P2 implies bi-directional equality between the phrases.");
            writer.WriteLine("%");
            writer.WriteLine("% P1 => P2 implies that P1 can be replaced by P2, but not (necessarily) the other way around.");
            writer.WriteLine("%");
        }

        private void WriteDefaultReplacements(StreamWriter writer)
        {
            writer.WriteLine("%%%%%%%%%%%%%%%%%%%%%%");
            writer.WriteLine("% DEFAULT REPLACEMENTS");
            writer.WriteLine("%%%%%%%%%%%%%%%%%%%%%%");
            writer.WriteLine("%");
            List<string> defaultReplacementSpecificationIdentifierList = ReplacementSpecification.GetDefaultReplacementSpecificationIdentifierList();
            foreach (string identifier in defaultReplacementSpecificationIdentifierList)
            {
                ReplacementSpecification replacementSpecification = replacementSpecificationList.Find(r => r.Identifier == identifier);
                string information = identifier + " = " + replacementSpecification.Description;
             /*   if (replacementSpecification.RelativeFilePath != null)
                {
                    information += " = " + replacementSpecification.RelativeFilePath;
                }
                else   */
                if (replacementSpecification.ReplacementList != null)
                {
                    information += " = (";
                    foreach (string replacement in replacementSpecification.ReplacementList)
                    {
                        information += replacement + ",";
                    }
                    information = information.TrimEnd(new char[] { ',' }) + ")";
                }
                writer.WriteLine(information);
            }
            writer.WriteLine("%");
        }

        private void WriteCustomReplacements(StreamWriter writer)
        {
            writer.WriteLine("%%%%%%%%%%%%%%%%%%%%%");
            writer.WriteLine("% CUSTOM REPLACEMENTS");
            writer.WriteLine("%%%%%%%%%%%%%%%%%%%%%");
            writer.WriteLine("%");
            List<string> defaultReplacementSpecificationIdentifierList = ReplacementSpecification.GetDefaultReplacementSpecificationIdentifierList();
            foreach (ReplacementSpecification replacementSpecification in replacementSpecificationList)
            {
                if (!defaultReplacementSpecificationIdentifierList.Contains(replacementSpecification.Identifier))
                {
                    string information = replacementSpecification.Identifier + " = " + replacementSpecification.Description + " = ";
                    for (int ii = 0; ii < replacementSpecification.ReplacementList.Count; ii++)
                    {
                        information += replacementSpecification.ReplacementList[ii] + ",";
                    }
                    information = information.TrimEnd(new char[] { ',' });
                    writer.WriteLine(information);
                }
            }
            writer.WriteLine("%");
        }

        private void WriteCompactPhrases(StreamWriter writer)
        {
            writer.WriteLine("%%%%%%%%%");
            writer.WriteLine("% PHRASES");
            writer.WriteLine("%%%%%%%%%");
            writer.WriteLine("%");
            foreach (PhraseDictionaryItem compactItem in compactItemList)
            {
                string phrase1Information = compactItem.Phrase1;
                string operatorString = "";
                if (compactItem.OneSidedInterchangeability) { operatorString = "  => "; }
                else { operatorString = " <=> "; }
                string phrase2Information = compactItem.Phrase2;
                writer.WriteLine(phrase1Information + operatorString + phrase2Information);
            }
        }

        private void Expand()
        {
            itemList = new List<PhraseDictionaryItem>();
            foreach (PhraseDictionaryItem compactItem in compactItemList)
            {
                if (compactItem.Phrase1.Contains("$"))
                {
                    List<string> phrase1Parts = compactItem.Phrase1.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    int index1 = phrase1Parts.FindIndex(p => p.Contains("$"));
                    List<string> phrase2Parts = compactItem.Phrase2.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    int index2 = phrase2Parts.FindIndex(p => p.Contains("$"));
                    string replacementSpecificationString = phrase1Parts[index1];
                    ReplacementSpecification replacementSpecification = replacementSpecificationList.Find(r => r.Identifier == replacementSpecificationString);
                    if (replacementSpecification != null)
                    {
                    /*    if (replacementSpecification.RelativeFilePath != null)
                        {
                            if (replacementSpecification.ReplacementList == null)
                            {
                                // Expand the specification here:
                            }
                        }  */
                        if (replacementSpecification.ReplacementList != null)
                        {
                            for (int ii = 0; ii < replacementSpecification.ReplacementList.Count; ii++)
                            {
                                phrase1Parts[index1] = replacementSpecification.ReplacementList[ii];
                                string phrase1Modified = "";
                                foreach (string phrase1Part in phrase1Parts) { phrase1Modified += phrase1Part + " "; }
                                phrase1Modified = phrase1Modified.Trim();
                                phrase2Parts[index2] = replacementSpecification.ReplacementList[ii];
                                string phrase2Modified = "";
                                foreach (string phrase2Part in phrase2Parts) { phrase2Modified += phrase2Part + " "; }
                                phrase2Modified = phrase2Modified.Trim();
                                // Add all versions of the item to the expanded list of phrase dictionary items
                                PhraseDictionaryItem item = new PhraseDictionaryItem();
                                item.OneSidedInterchangeability = compactItem.OneSidedInterchangeability;
                                item.Phrase1 = phrase1Modified;
                                item.Phrase2 = phrase2Modified;
                                itemList.Add(item);
                            }
                        }
                        else // Default replacement: Cannot be expanded
                        {
                            string phrase1Information = compactItem.Phrase1;
                            string phrase2Information = compactItem.Phrase2;
                            // Add the item to the expanded list of phrase dictionary items
                            PhraseDictionaryItem item = new PhraseDictionaryItem();
                            item.OneSidedInterchangeability = compactItem.OneSidedInterchangeability;
                            item.Phrase1 = compactItem.Phrase1;
                            item.Phrase2 = compactItem.Phrase2;
                            itemList.Add(item);
                        }
                    }
                }
                else  // No $ => No expansion needed
                {
                    string phrase1Information = compactItem.Phrase1;
                    string phrase2Information = compactItem.Phrase2;
                    // Add the item to the expanded list of phrase dictionary items
                    PhraseDictionaryItem item = new PhraseDictionaryItem();
                    item.OneSidedInterchangeability = compactItem.OneSidedInterchangeability;
                    item.Phrase1 = compactItem.Phrase1;
                    item.Phrase2 = compactItem.Phrase2;
                    itemList.Add(item);
                }
            }
        }

        // 20191112 - replaces the ExportCompact and Export method: Phrase dictionaries
        // are always saved in compact format. Expansion is carried out upon Load().
        public void Save(string fileName)
        {
            StreamWriter writer = new StreamWriter(fileName);
            WriteSymbolExplanation(writer);
            WriteDefaultReplacements(writer);
            WriteCustomReplacements(writer);
            WriteCompactPhrases(writer);
            writer.Close();
        }

        public List<PhraseDictionaryItem> ItemList
        {
            get { return itemList; }
        }

        [DataMember]
        public List<PhraseDictionaryItem> CompactItemList
        {
            get { return compactItemList; }
            set { compactItemList = value; }
        }

        [DataMember]
        public List<ReplacementSpecification> ReplacementSpecificationList
        {
            get { return replacementSpecificationList; }
            set { replacementSpecificationList = value; }
        }
    }
}
