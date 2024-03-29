﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Web.Script.Serialization;
using System.Text;

namespace RallyUpServer
{
    class LilRally
    {
        string clientData;
        DateTime rallyStartTime;

        public LilRally(string clientData, DateTime rallyStartTime)
        {
            this.clientData = clientData;
            this.rallyStartTime = rallyStartTime;
        }

        public void runLilRally()
        {
            string[] prefix = { clientData.Split(':')[0], clientData.Split(':')[1] };
            string[] lengthsArray = prefix[1].Split(',');
            string infoString = clientData.Substring(prefix[0].Length + prefix[1].Length + 2);
            string senderName = infoString.Substring(0, Convert.ToInt32(lengthsArray[0]));
            string tagline = infoString.Substring(Convert.ToInt32(lengthsArray[0]), Convert.ToInt32(lengthsArray[1]));
            List<string> rallyFriendsList = new List<string>();
            int firstPoint = Convert.ToInt32(lengthsArray[0]) + Convert.ToInt32(lengthsArray[1]);
            int secondPoint;
            for (int i = 2; i < lengthsArray.Length; i++)
            {
                secondPoint = firstPoint + Convert.ToInt32(lengthsArray[i]);
                rallyFriendsList.Add(infoString.Substring(firstPoint, Convert.ToInt32(lengthsArray[i])));
                firstPoint = secondPoint;
            }

            foreach (string friendName in rallyFriendsList)
            {
                SendRallyNotification(senderName, tagline, friendName);
            }
        }

        public static void SendRallyNotification(string notifyTopic, string notifyBody, string notifySender)
        {
            string str;
            try
            {
                string applicationID = "1:1022085567661:android:794e934c87823234";
                string senderId = "1022085567661";
                //string deviceId = "d16EVnuaNF8:APA91bHj_ANySih5rBxITWW91i-qTyFSOrJ8CxOi1Vd-_zx8e7Uhrz8tgVLglCl0KihwRN2igr6ihjJJDL4XYkzLu62w-zVVmZ9liERsiTIMnOc6rwPDF5AxNFYoFAOadUlhV2zEDrpG";
                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";
                var data = new
                {
                    to = "/topics/" + notifyTopic,
                    notification = new
                    {
                        body = notifyBody,
                        title = "New Rally from " + notifySender,
                        sound = "Enabled"
                    }
                };

                var serializer = new JavaScriptSerializer();
                var json = serializer.Serialize(data);
                Byte[] byteArray = Encoding.UTF8.GetBytes(json);
                tRequest.Headers.Add(string.Format("Authorization: key=AAAA7fkMFK0:APA91bHtsHvR58_yg1wrKu2nGP6rmOhHkOM_3Zapq5fo4ZBKmvngPJiCjirBmIRHFXjAR82xFccSqxVt-IAYobnMns_1hZmxQqqcpaitip-ae9Y4dcwlLv5T8xHpLeACqbiLRLQgEHCB", applicationID));
                tRequest.Headers.Add(string.Format("Sender: id={0}", senderId));
                tRequest.ContentLength = byteArray.Length;

                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                String sResponseFromServer = tReader.ReadToEnd();
                                str = sResponseFromServer;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                str = ex.Message;
            }
            Console.WriteLine(str);
        }
    }
}