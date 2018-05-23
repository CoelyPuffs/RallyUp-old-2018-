using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using RallyUpLibrary;
using System.Web.Script.Serialization;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace RallyUpServer
{
    // This is the client handler on the server side. 
    class LilClient
    {
        private TcpClient clientSocket;
        private string clientName;
        private static string connection = @"Server=(localdb)\MSSQLLocalDB;Database=RallyUpDB;Trusted_Connection=True;ConnectRetryCount=0";
        SqlConnection sqlConnection1 = new SqlConnection(connection);

        public void initializeClient(TcpClient clientSocket, string clientName)
        {
            this.clientName = clientName;
            this.clientSocket = clientSocket;
            Thread thread = new Thread(runClientThread);
            thread.Start();
        }

        private void runClientThread()
        {
            while (true)
            {
                try
                {
                    string clientData = clientSocket.ReadString();
                    Console.WriteLine(clientData);
                    if (clientData == "Ping")
                    {
                        System.Threading.Thread.Sleep(5000);
                        SendPushNotification();
                    }
                    else if (clientData.Split(':')[0] == "Register")
                    {
                        string potentialUsername = clientData.Split(':')[1];
                        if (checkUsernameUnique(potentialUsername))
                        {
                            registerUser(potentialUsername, clientData.Split(':')[2]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        // Function from https://stackoverflow.com/questions/37412963/send-push-to-android-by-c-sharp-using-fcm-firebase-cloud-messaging
        public static void SendPushNotification()
        {
            string str;
            try
            {
                string applicationID = "1:1022085567661:android:794e934c87823234";
                string senderId = "1022085567661";
                string deviceId = "d16EVnuaNF8:APA91bHj_ANySih5rBxITWW91i-qTyFSOrJ8CxOi1Vd-_zx8e7Uhrz8tgVLglCl0KihwRN2igr6ihjJJDL4XYkzLu62w-zVVmZ9liERsiTIMnOc6rwPDF5AxNFYoFAOadUlhV2zEDrpG";
                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";
                var data = new
                {
                    to = "/topics/Testing",
                    notification = new
                    {
                        body = "Ponging",
                        title = "Pong",
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

        bool checkUsernameUnique(string username)
        {
            bool isUnique = false;
            string query = "SELECT username FROM RallyUpUser WHERE RallyUpUser.username = @potentialName";
            
            SqlCommand cmd = new SqlCommand(query, sqlConnection1);
            cmd.Parameters.AddWithValue("@potentialName", username);
            SqlDataReader reader;

            sqlConnection1.Open();

            reader = cmd.ExecuteReader();
            isUnique = !reader.HasRows;

            sqlConnection1.Close();

            return isUnique;
        }

        void registerUser(string username, string password)
        {
            string query = "INSERT INTO RallyUpUser(username, pwSalt, pwHash) VALUES (@username, @pwSalt, @pwHash)";
            byte[] pwSalt = GenerateSalt();
            byte[] pwHash = GenerateHash(Encoding.ASCII.GetBytes(password), pwSalt, 500, 50);
            SqlCommand cmd = new SqlCommand(query, sqlConnection1);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@pwSalt", pwSalt);
            cmd.Parameters.AddWithValue("@pwHash", pwHash);
            SqlDataReader reader;
            sqlConnection1.Open();
            reader = cmd.ExecuteReader();
        }

        byte[] GenerateSalt()
        {
            byte[] bytes = new byte[15];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return bytes;
        }

        byte[] GenerateHash(byte[] password, byte[] salt, int iterations, int length)
        {
            var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations);
            return deriveBytes.GetBytes(length);
        }
    }
}
