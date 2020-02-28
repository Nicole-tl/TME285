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
    public class yelpDataDownloader : DataDownloader
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
                    string html = webClient.DownloadString("https://www.bookatable.co.uk/gothenburg-restaurants?c=auto_area&q=Gothenburg");
                    List<string> htmlSplitList = html.Split(new string[] { "street" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    List<string> rawDataInfoList = htmlSplitList.FindAll(h => h.StartsWith("\":\""));

                    //remove the last one
                    //int l = rawDataInfoList.Count;
                    //rawDataInfoList.RemoveAt(l-1);
                    List<string> dataInfoList = new List<string>();
                    List<string> dataInfoList2 = new List<string>();
                    List<string> dataInfoList3 = new List<string>();
                    List<string> dataInfoList4 = new List<string>();
                    List<string> dataInfoList5 = new List<string>();
                    List<string> dataInfoList6 = new List<string>();


                    foreach (string rawDataInfo in rawDataInfoList)
                    {
                        // Name
                        string dataNameInfo = rawDataInfo.Split(new string[] { "urlName\":\"", "\",\"cuisines" }, StringSplitOptions.None).ToList()[1];
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


                    }

                    int lenOfArray = dataInfoList.Count;

                    for (int i=0; i<=lenOfArray-1; i=i+1)
                    {
                        DataItem dataItem = new DataItem();
                        dataItem.ContentList.Add(new TagValueUnit("item", dataInfoList[i]));

                        // One tag with two values
                        List<string> cuisineList = dataInfoList2[i].Split(new string[] { "\",\"" }, StringSplitOptions.None).ToList();
                        foreach (string cuisine in cuisineList)
                        {
                            dataItem.ContentList.Add(new TagValueUnit("cuisine", cuisine));
                        };
                        dataItem.ContentList.Add(new TagValueUnit("rating", dataInfoList3[i]));
                        dataItem.ContentList.Add(new TagValueUnit("Geolocation", dataInfoList4[i]));
                        dataItem.ContentList.Add(new TagValueUnit("street", dataInfoList5[i]));
                        dataItem.TimeStamp = DateTime.Now;
                        dataItemList.Add(dataItem);
                    };
                    
                }
                catch (WebException e)
                { 
                    // ToDo: add some error handler here
                }
            }                                         
            return dataItemList;
        }
    }
}
