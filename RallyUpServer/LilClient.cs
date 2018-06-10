using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Web.Script.Serialization;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;

using RallyUpLibrary;

namespace RallyUpServer
{
    class LilClient
    {
        private TcpClient clientSocket;
        private static string connection = @"Server=(localdb)\MSSQLLocalDB;Database=RallyUpDB;Trusted_Connection=True;ConnectRetryCount=0";
        SqlConnection sqlConnection1 = new SqlConnection(connection);

        public LilClient(TcpClient clientSocket)
        {
            this.clientSocket = clientSocket;
        }

        public void runClientThread()
        {
            try
            {
                string clientData = clientSocket.ReadString();
                Console.WriteLine(clientData);

                // Ping Function
                if (clientData == "Ping")
                {
                    System.Threading.Thread.Sleep(5000);
                    SendRallyNotification("Testing", "Ponging", "Pingster");
                }

                // Registration
                else if (clientData.Split(':')[0] == "Register")
                {
                    string[] prefix = { clientData.Split(':')[0], clientData.Split(':')[1] };
                    int usernameLength = Convert.ToInt32(prefix[1].Split(',')[0]);
                    int passwordLength = Convert.ToInt32(prefix[1].Split(',')[1]);
                    int screenNameLength = Convert.ToInt32(prefix[1].Split(',')[2]);
                    int prefixLength = 10 + prefix[1].Length;
                    string suffix = clientData.Substring(prefixLength);
                    string potentialUsername = suffix.Substring(0, usernameLength);
                    string password = suffix.Substring(usernameLength, passwordLength);
                    string screenName = suffix.Substring(usernameLength + passwordLength, screenNameLength);

                    if (checkUsernameUnique(potentialUsername))
                    {
                        registerUser(potentialUsername, password, screenName);
                        clientSocket.WriteString("RegistrationSuccessful");
                        Console.WriteLine("RegistrationSuccessful");
                    }
                    else
                    {
                        clientSocket.WriteString("UsernameAlreadyRegistered");
                        Console.WriteLine("UsernameAlreadyRegistered: " + potentialUsername);
                    }
                }

                // Login
                else if (clientData.Split(':')[0] == "Login")
                {
                    string[] prefix = { clientData.Split(':')[0], clientData.Split(':')[1] };
                    int usernameLength = Convert.ToInt32(prefix[1].Split(',')[0]);
                    int passwordLength = Convert.ToInt32(prefix[1].Split(',')[1]);
                    int prefixLength = 7 + prefix[1].Length;
                    string suffix = clientData.Substring(prefixLength);
                    string username = suffix.Substring(0, usernameLength);
                    string password = suffix.Substring(usernameLength, passwordLength);

                    string query = "SELECT pwSalt, pwHash FROM RallyUpUser WHERE RallyUpUser.username = @username";
                    SqlCommand cmd = new SqlCommand(query, sqlConnection1);
                    cmd.Parameters.AddWithValue("@username", username);
                    SqlDataReader reader;
                    sqlConnection1.Open();
                    reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var salt = reader.GetValue(0);
                            var realHash = reader.GetValue(1);

                            if (((byte[])realHash).SequenceEqual(GenerateHash(Encoding.UTF32.GetBytes(password), (byte[])salt, 500, 50)))
                            {
                                clientSocket.WriteString("ValidCredentials");
                                Console.WriteLine("ValidCredentials");
                            }
                            else
                            {
                                clientSocket.WriteString("BadPassword");
                                Console.WriteLine("BadPassword");
                            }
                        }
                    }
                    else
                    {
                        clientSocket.WriteString("noUser");
                        Console.WriteLine(username + " not found");
                    }
                    sqlConnection1.Close();
                }

                // Return list of friends
                else if(clientData.Split(':')[0] == "GetFriends")
                {
                    string username = clientData.Substring(11);
                    List<List<string>> friendList = new List<List<string>>();
                    string query = "SELECT friendName FROM RallyUpFriend WHERE RallyUpFriend.username = @username";
                    string friendInfoQuery = "SELECT screenName FROM RallyUpUser WHERE RallyUpUser.username = @friendName";
                    SqlCommand cmd = new SqlCommand(query, sqlConnection1);
                    cmd.Parameters.AddWithValue("@username", username);
                    try
                    {
                        SqlDataReader reader;
                        sqlConnection1.Open();
                        reader = cmd.ExecuteReader();
                        int i = 0;
                        while (reader.Read())
                        {
                            string friendUsername = (reader.GetValue(0)).ToString();
                            friendList.Add(new List<string>());
                            friendList[i].Add(friendUsername.Length.ToString());
                            friendList[i].Add(friendUsername);
                            i++;
                        }
                        foreach (List<string> friend in friendList)
                        {
                            reader.Close();
                            cmd.CommandText = friendInfoQuery;
                            cmd.Parameters.AddWithValue("@friendName", friend[1]);
                            reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                string friendScreenName = reader.GetValue(0).ToString();
                                friend.Add(friendScreenName.Length.ToString());
                                friend.Add(friendScreenName);
                            }
                            cmd.Parameters.Clear();
                        }
                        string friendListString = "";
                        foreach (List<string> friend in friendList)
                        {
                            friendListString += friend[0] + ',' + friend[2] + ',';
                        }
                        friendListString = friendListString.TrimEnd(',');
                        friendListString += ':';
                        foreach (List<string> friend in friendList)
                        {
                            friendListString += friend[1] + friend[3];
                        }
                        clientSocket.WriteString(friendListString);
                        Console.WriteLine(friendListString);
                    }
                    catch(Exception ex)
                    {
                        clientSocket.WriteString("FriendsNotFound");
                        Console.WriteLine("FriendsNotFound");
                        Console.WriteLine(ex.Message);
                    }
                }
                else if(clientData.Split(':')[0] == "AddFriend")
                {
                    string[] prefix = { clientData.Split(':')[0], clientData.Split(':')[1] };
                    int usernameLength = Convert.ToInt32(prefix[1].Split(',')[0]);
                    int friendNameLength = Convert.ToInt32(prefix[1].Split(',')[1]);
                    int prefixLength = 11 + prefix[1].Length;
                    string suffix = clientData.Substring(prefixLength);
                    string username = suffix.Substring(0, usernameLength);
                    string friendName = suffix.Substring(usernameLength, friendNameLength);

                    try
                    {
                        string checkExistsQuery = "SELECT username FROM RallyUpUser WHERE username = @friendName";
                        string insertQuery = "INSERT INTO RallyUpFriend VALUES (@username, @friendName)";
                        SqlCommand cmd = new SqlCommand(checkExistsQuery, sqlConnection1);
                        cmd.Parameters.AddWithValue("@friendName", friendName);
                        sqlConnection1.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        bool friendExists = reader.HasRows;
                        sqlConnection1.Close();
                        if (friendExists)
                        {
                            cmd = new SqlCommand(insertQuery, sqlConnection1);
                            cmd.Parameters.AddWithValue("@username", username);
                            cmd.Parameters.AddWithValue("@friendName", friendName);
                            int readResult;
                            sqlConnection1.Open();
                            readResult = cmd.ExecuteNonQuery();
                            if (readResult == 1)
                            {
                                clientSocket.WriteString("FriendAdded");
                            }
                            else
                            {
                                clientSocket.WriteString("ErrorAddingFriend");
                            }
                            sqlConnection1.Close();
                        }
                        else
                        {
                            clientSocket.WriteString("FriendNotExist");
                        }
                    }
                    catch
                    {
                        clientSocket.WriteString("ErrorAddingFriend");
                    }
                }
                else if(clientData.Split(':')[0] == "Rally")
                {
                    // Add parsing stuff
                    string[] prefix = { clientData.Split(':')[0], clientData.Split(':')[1] };
                    string[] lengthsArray = prefix[1].Split(',');
                    string infoString = clientData.Substring(prefix.Length + 1);
                    string senderName = infoString.Substring(0, Convert.ToInt32(lengthsArray[0]));
                    string tagline = infoString.Substring(Convert.ToInt32(lengthsArray[0], Convert.ToInt32(lengthsArray[1])));


                    /*string senderName = clientData.Split(':')[1];
                    string tagline = clientData.Split('≡')[2];
                    List<string> recipients = new List<string>();
                    for (int i = 3; i < clientData.Split('≡').Length; i++)
                    {
                        SendRallyNotification(clientData.Split('≡')[i], tagline, senderName);
                    }*/
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        // Function from https://stackoverflow.com/questions/37412963/send-push-to-android-by-c-sharp-using-fcm-firebase-cloud-messaging
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

        void registerUser(string username, string password, string screenName)
        {
            string query = "INSERT INTO RallyUpUser(username, pwSalt, pwHash, screenName) VALUES (@username, @pwSalt, @pwHash, @screenName)";
            byte[] pwSalt = GenerateSalt();
            byte[] pwHash = GenerateHash(Encoding.UTF32.GetBytes(password), pwSalt, 500, 50);
            SqlCommand cmd = new SqlCommand(query, sqlConnection1);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@pwSalt", pwSalt);
            cmd.Parameters.AddWithValue("@pwHash", pwHash);
            cmd.Parameters.AddWithValue("@screenName", screenName);
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
