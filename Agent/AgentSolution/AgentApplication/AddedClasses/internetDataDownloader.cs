using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AgentLibrary.Internet;
using AgentLibrary.Memories;
using InternetDataAcquisitionLibrary;

namespace AgentApplication.AddedClasses
{
    public class internetDataDownload : DataDownloader
    { 
        //
        // Note to students: This is a simple demo downloader that downloads the most
        //                   recent headline from a web page.
        //                   The code is very basic - an obvious improvement would be
        //                   to store the most recent downloaded headline, and only
        //                   return a new headline if it is different from the last
        //                   downloaded one.
        // 
        //                   Note also that, by defauly, the downloader only runs
        //                   once. In order to make it run multiple times, set
        //                   runRepeatedly = true.
        //
        protected override List<DataItem> ProcessDownload()
        {
            List<DataItem> dataItemList = null;
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    dataItemList = new List<DataItem>();
                    List<string> allHtml = new List<string>();
                    string html1 = webClient.DownloadString("https://www.bookatable.co.uk/gothenburg-restaurants?c=auto_area&q=Gothenburg");
                    string html2 = webClient.DownloadString("https://whichmuseum.com/sweden/gothenburg/museums");
                    allHtml.Add(html1);
                    allHtml.Add(html2);

                    foreach (string html in allHtml)
                    {
                        if (html == html1)
                        {
                            List<string> htmlSplitList = html.Split(new string[] { "street" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            List<string> rawDataInfoList = htmlSplitList.FindAll(h => h.StartsWith("\":\""));

                            List<string> dataInfoList = new List<string>();
                            List<string> dataInfoList2 = new List<string>();
                            List<string> dataInfoList3 = new List<string>();
                            List<string> dataInfoList4 = new List<string>();
                            List<string> dataInfoList5 = new List<string>();
                            List<string> dataInfoList6 = new List<string>();


                            foreach (string rawDataInfo in rawDataInfoList)
                            {
                                // Name
                                string dataNameInfo = rawDataInfo.Split(new string[] { "urlName\":\"", "\",\"cuisines" }, StringSplitOptions.RemoveEmptyEntries).ToList()[1];
                                dataInfoList.Add(dataNameInfo);

                                // Cuisine
                                string dataCuisineInfo = rawDataInfo.Split(new string[] { "cuisines\":[\"", "\"],\"styles\"" }, StringSplitOptions.RemoveEmptyEntries).ToList()[1];
                                dataInfoList2.Add(dataCuisineInfo);

                                // Rating
                                string dataRatingInfo = rawDataInfo.Split(new string[] { "\"ratings\":{\"overall\":", ",\"formattedOverall" }, StringSplitOptions.RemoveEmptyEntries).ToList()[1];
                                dataInfoList3.Add(dataRatingInfo);

                                // Geographic location
                                string dataGeoInfo = rawDataInfo.Split(new string[] { "\"geolocation\":{\"", "},\"reviewsSummary\"" }, StringSplitOptions.RemoveEmptyEntries).ToList()[1];
                                dataInfoList4.Add(dataGeoInfo);

                                // Address - street
                                string dataStreetInfo = rawDataInfo.Split(new string[] { "\":\"", "\",\"city\"" }, StringSplitOptions.RemoveEmptyEntries).ToList()[0];
                                dataInfoList5.Add(dataStreetInfo);

                                //URL
                                List<string> dataURLInfoList = rawDataInfo.Split(new string[] { "generatedURL\":\"", "\", \"" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                string dataURLInfo = dataURLInfoList.Find(h => h.StartsWith("https://www.bookatable.co.uk/"));
                                string dataURL = dataURLInfo.Split(new string[] { "\"" }, StringSplitOptions.RemoveEmptyEntries).ToList()[0];
                                dataURL = dataURL + "#location-tab";
                                string html3 = webClient.DownloadString(dataURL);
                                List<string> dataPhoneList = html3.Split(new string[] { "telephone\": \""}, StringSplitOptions.RemoveEmptyEntries).ToList();
                                string phoneNumb = dataPhoneList[1].Split(new string[] { "\"," }, StringSplitOptions.RemoveEmptyEntries).ToList()[0];
                                dataInfoList6.Add(phoneNumb);



                            }

                            int lenOfArray = dataInfoList.Count;
                            string allCuisineList = "";

                            List<string> allCuisines = new List<string>();
                            for (int i = 0; i <= lenOfArray - 1; i = i + 1)
                            {
                                DataItem dataItem = new DataItem();
                                dataItem.ContentList.Add(new TagValueUnit("item", dataInfoList[i]));

                                // One tag with two values
                                List<string> cuisineList = dataInfoList2[i].Split(new string[] { "\",\"" }, StringSplitOptions.None).ToList();
                                foreach (string cuisine in cuisineList)
                                {
                                    dataItem.ContentList.Add(new TagValueUnit("cuisine", cuisine));
                                    if (!allCuisineList.Contains(cuisine))
                                    {
                                        allCuisineList = allCuisineList + cuisine + "\r\n";
                                    }
                                };
                                dataItem.ContentList.Add(new TagValueUnit("rating", dataInfoList3[i]));
                                dataItem.ContentList.Add(new TagValueUnit("geolocation", dataInfoList4[i]));
                                dataItem.ContentList.Add(new TagValueUnit("street", dataInfoList5[i]));
                                dataItem.ContentList.Add(new TagValueUnit("phoneNumber", dataInfoList6[i]));
                                dataItem.TimeStamp = DateTime.Now;
                                dataItemList.Add(dataItem);
                            };

                            DataItem dataItem1 = new DataItem();
                            dataItem1.ContentList.Add(new TagValueUnit("allCuisines", allCuisineList));
                            dataItemList.Add(dataItem1);
                        }
                        else
                        {
                            List<string> htmlSplitList = html.Split(new string[] { "<a href=\"/sweden/gothenburg/" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            List<string> rawDataInfoList = htmlSplitList.FindAll(h => !h.Contains("<!doctype html>"));
                            string allCategory = "";


                            foreach (string rawDataInfo in rawDataInfoList)
                            {
                                DataItem dataItem = new DataItem();

                                // Name
                                string dataNameInfo = rawDataInfo.Split(new string[] { "\">" }, StringSplitOptions.RemoveEmptyEntries).ToList()[0];
                                TagValueUnit nameTagValueUnit = new TagValueUnit("item", dataNameInfo);
                                dataItem.ContentList.Add(nameTagValueUnit);

                                // Category
                                if (rawDataInfo.Contains("<span class=\""))
                                {
                                    string dataCategoryInfo = rawDataInfo.Split(new string[] { "<span class=\"", "</span>" }, StringSplitOptions.RemoveEmptyEntries).ToList()[1];
                                    string categoryListString = dataCategoryInfo.Split(new string[] { "\">" }, StringSplitOptions.RemoveEmptyEntries).ToList()[1];
                                    List<string> categoryList = categoryListString.Split(new string[] { "&amp;" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                    string categoryString ="";
                                    foreach (string category in categoryList)
                                    {
                                        categoryString = categoryString + "and" + category ;
                                    }
                                    string categoryString1 = categoryString.Remove(0, 3);
                                    TagValueUnit categoryTagValueUnit = new TagValueUnit("museumCategory", categoryString1);
                                    dataItem.ContentList.Add(categoryTagValueUnit);

                                    if (!allCategory.Contains(categoryString1))
                                    {
                                        allCategory = allCategory + categoryString1 + "\r\n";
                                    }

                                }
                                else
                                {
                                    TagValueUnit categoryTagValueUnit = new TagValueUnit("museumCategory", "not mentioned");
                                    dataItem.ContentList.Add(categoryTagValueUnit);

                                }

                                // webpage
                                string homepageURL = "https://whichmuseum.com/sweden/gothenburg/" + dataNameInfo;
                                string htmlHomepage = webClient.DownloadString(homepageURL);
                                if (htmlHomepage.Contains("<strong>#"))
                                {
                                    string ranking = htmlHomepage.Split(new string[] { "<strong>#", "</strong> in the top 50 of Sweden</a>" }, StringSplitOptions.RemoveEmptyEntries).ToList()[1];
                                    TagValueUnit rankingTagValueUnit = new TagValueUnit("ranking", ranking);
                                    dataItem.ContentList.Add(rankingTagValueUnit);

                                }
                                else
                                {
                                    TagValueUnit rankingTagValueUnit = new TagValueUnit("ranking", null);
                                    dataItem.ContentList.Add(rankingTagValueUnit);
                                }
                                // Address - street
                                //string dataAddressString = rawDataInfo.Split(new string[] { "<td class='show-for-medium-up'>"}, StringSplitOptions.RemoveEmptyEntries).ToList()[1];
                                //string dataAddressInfo = dataAddressString.Split(new string[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries).ToList()[0];
                                //TagValueUnit addressTagValueUnit = new TagValueUnit("address", dataAddressInfo);
                                //dataItem.ContentList.Add(addressTagValueUnit);

                                dataItemList.Add(dataItem);

                            }

                            DataItem dataItem1 = new DataItem();
                            dataItem1.ContentList.Add(new TagValueUnit("allMuseum", allCategory));
                            dataItemList.Add(dataItem1);


                        }

                    }

                    

                }
                catch (WebException e)
                {

                }
            }                                         
            return dataItemList;
        }
    }
}
