﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json.Linq;
using Parameters;
using GksKatowiceBot.Helpers;
using System.Json;
using WzorzecBot.Helpers;

namespace GksKatowiceBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            try
            {
                if (activity.Type == ActivityTypes.Message)
                {

                    if (BaseDB.czyAdministrator(activity.From.Id) != null && (((activity.Text != null && activity.Text.IndexOf("!!!") == 0) || (activity.Attachments != null && activity.Attachments.Count > 0))))
                    {
                        WebClient client = new WebClient();


                        if (activity.Text != null && activity.Text.ToUpper().IndexOf("!!!ANKIETA") > -1)
                        {
                            try
                            {
                                //     int index = activity.Text.ToUpper().IndexOf("!!!ANKIETA");
                                DataTable dt = BaseDB.DajAnkiete(Convert.ToInt32(activity.Text.ToUpper().Substring(10)));
                                if (dt.Rows.Count > 0)
                                {
                                    CreateAnkieta(dt, activity.From.Id.ToString());
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }

                        else if (activity.Text != null && activity.Text.ToUpper().IndexOf("!!!ALERT") > -1)
                        {
                            try
                            {
                                //     int index = activity.Text.ToUpper().IndexOf("!!!ANKIETA");
                                DataTable dt = BaseDB.DajAlert(Convert.ToInt32(activity.Text.ToUpper().Substring(8)));
                                if (dt.Rows.Count > 0)
                                {
                                    CreateAlert(dt, activity.From.Id.ToString());
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }



                        else if (activity.Attachments != null)
                        {
                            //Uri uri = new Uri(activity.Attachments[0].ContentUrl);
                            string filename = activity.Attachments[0].ContentUrl.Substring(activity.Attachments[0].ContentUrl.Length - 4, 3).Replace(".", "");


                            //  WebClient client = new WebClient();
                            client.Credentials = new NetworkCredential("serwer1606926", "Tomason1910");
                            client.BaseAddress = "ftp://serwer1606926.home.pl/public_html/pub/";


                            byte[] data;
                            using (WebClient client2 = new WebClient())
                            {
                                data = client2.DownloadData(activity.Attachments[0].ContentUrl);
                            }
                            if (activity.Attachments[0].ContentType.Contains("image")) client.UploadData(filename + ".png", data); //since the baseaddress
                            else if (activity.Attachments[0].ContentType.Contains("video")) client.UploadData(filename + ".mp4", data);
                        }

                        if (activity.Text.ToUpper().Contains("ANKIETA") != true && activity.Text.ToUpper().Contains("ALERT") != true)
                        {

                            CreateMessage(activity.Attachments, activity.Text == null ? "" : activity.Text.Replace("!!!", ""), activity.From.Id);
                        }
                    }
                    else
                    {
                        string komenda = "";
                        if (activity.ChannelData != null)
                        {
                            BaseDB.AddToLog(activity.ChannelData.ToString());
                            try
                            {
                                if (activity.ChannelData.ToString().Contains("GET_STARTED_PAYLOAD"))
                                {
                                    var stuff = JsonConvert.DeserializeObject<ClassHelper.RootObject>(activity.ChannelData.ToString());
                                    komenda = stuff.postback.payload;
                                }
                                else
                                {
                                    var stuff = JsonConvert.DeserializeObject<ClassHelper2.RootObject>(activity.ChannelData.ToString());
                                    komenda = stuff.message.quick_reply.payload;
                                }
                            }
                            catch (Exception ex)
                            {
                                BaseDB.AddToLog("Bład rozkładania Jsona " + ex.ToString());
                            }
                        }
                        var toReply = activity.CreateReply(String.Empty);
                        var connectorNew = new ConnectorClient(new Uri(activity.ServiceUrl));
                        toReply.Type = ActivityTypes.Typing;
                        await connectorNew.Conversations.SendToConversationAsync(toReply);



                        MicrosoftAppCredentials.TrustServiceUrl(@"https://facebook.botframework.com", DateTime.MaxValue);
                        if (komenda == "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowAktualnosci" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowAktualnosci")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //       BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //        BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.Text = "Zobacz co nowego w gminie :information_source:";
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",


                                buttons = new dynamic[]
                            {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                            },

                                quick_replies = new dynamic[]
                                   {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowAktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Sport",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowSport",
                               //       image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowGaleria",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Kalendarium",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowKalendarium",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                   }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }


                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowZabytki" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowZabytki")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //       BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //        BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.Text = "Zabytkowe miejsca w naszej gminie które musisz zobaczyc :european_castle:";
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",


                                buttons = new dynamic[]
                            {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                            },

                                quick_replies = new dynamic[]
                                   {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Zabytki",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowZabytki",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Pieszo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowPieszo",
                               //       image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Na rowerze",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowNaRowerze",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Konno",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowKonno",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                   }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            message.Attachments = BaseGETMethod.GetCardsAttachmentsZabytki(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_DlaTurystow" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_DlaTurystow")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //       BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //        BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.Text = "Zabytkowe miejsca w naszej gminie które musisz zobaczyc :european_castle:";
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",


                                buttons = new dynamic[]
                            {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                            },

                                quick_replies = new dynamic[]
                                   {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Zabytki",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowZabytki",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Pieszo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowPieszo",
                               //       image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Na rowerze",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowNaRowerze",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Konno",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowKonno",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                   }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            message.Attachments = BaseGETMethod.GetCardsAttachmentsZabytki(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowPieszo" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowPieszo")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //       BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //        BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.Text = "Lubisz długie wędrówki? :ok_hand:";
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",


                                buttons = new dynamic[]
                            {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                            },

                                quick_replies = new dynamic[]
                                   {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Zabytki",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowZabytki",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Pieszo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowPieszo",
                               //       image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Na rowerze",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowNaRowerze",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Konno",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowKonno",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                   }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            message.Attachments = BaseGETMethod.GetCardsAttachmentsPieszo(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowNaRowerze" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowNaRowerze")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //       BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //        BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.Text = "A może rowerem? :bike:";
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",


                                buttons = new dynamic[]
                            {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                            },

                                quick_replies = new dynamic[]
                                   {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Zabytki",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowZabytki",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Pieszo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowPieszo",
                               //       image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Na rowerze",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowNaRowerze",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Konno",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowKonno",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                   }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            message.Attachments = BaseGETMethod.GetCardsAttachmentsNaRowerze(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowKonno" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowKonno")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //       BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //        BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.Text = "Konno? :racehorse:";
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",


                                buttons = new dynamic[]
                            {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                            },

                                quick_replies = new dynamic[]
                                   {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Zabytki",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowZabytki",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Pieszo",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowPieszo",
                               //       image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Na rowerze",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowNaRowerze",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Konno",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystowKonno",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                   }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            message.Attachments = BaseGETMethod.GetCardsAttachmentsKonno(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_DlaInwestorow" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_DlaInwestorow")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //       BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //        BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.Text = "Konno? :racehorse:";
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
   {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Dla mieszkańców",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancow",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Dla turystów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystow",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Dla inwestorów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaInwestorow",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                  }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            message.Attachments = BaseGETMethod.GetCardsAttachmentsDlaInwestorow(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else if (komenda.Contains("DEVELOPER_DEFINED_PAYLOAD_Odpowiedz") || activity.Text.Contains("DEVELOPER_DEFINED_PAYLOAD_Odpowiedz"))
                        {

                            if (komenda.Substring(35, 1) == "1")
                            {

                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                BaseDB.zapiszOdpowiedzi(komenda.Substring(komenda.LastIndexOf('_') + 1), 1, 0, 0, 0, 0, 0);

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",


                                    buttons = new dynamic[]
                                {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                                },

                                    quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Dla mieszkańców",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancow",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Dla turystów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystow",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Dla inwestorów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaInwestorow",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                                });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.Text = "Super dziękuję za oddanie głosu :)";
                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (komenda.Substring(35, 1) == "2")
                            {

                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                BaseDB.zapiszOdpowiedzi(komenda.Substring(komenda.LastIndexOf('_') + 1), 0, 1, 0, 0, 0, 0);

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",


                                    buttons = new dynamic[]
                                {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                                },

                                    quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Dla mieszkańców",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancow",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Dla turystów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystow",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Dla inwestorów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaInwestorow",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                                });


                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.Text = "Super dziękuję za oddanie głosu :)";
                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (komenda.Substring(35, 1) == "3")
                            {

                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;
                                BaseDB.zapiszOdpowiedzi(komenda.Substring(komenda.LastIndexOf('_') + 1), 0, 0, 1, 0, 0, 0);

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",


                                    buttons = new dynamic[]
                                {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                                },

                                    quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Dla mieszkańców",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancow",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Dla turystów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystow",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Dla inwestorów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaInwestorow",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                                });



                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.Text = "Super dziękuję za oddanie głosu :)";
                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (komenda.Substring(35, 1) == "4")
                            {

                                BaseDB.zapiszOdpowiedzi(komenda.Substring(komenda.LastIndexOf('_') + 1), 0, 0, 0, 1, 0, 0);

                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",


                                    buttons = new dynamic[]
                                {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                                },

                                    quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Dla mieszkańców",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancow",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Dla turystów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystow",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Dla inwestorów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaInwestorow",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                                });



                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.Text = "Super dziękuję za oddanie głosu :)";
                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (komenda.Substring(35, 1) == "5")
                            {

                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                BaseDB.zapiszOdpowiedzi(komenda.Substring(komenda.LastIndexOf('_') + 1), 0, 0, 0, 0, 1, 0);

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",


                                    buttons = new dynamic[]
                                {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                                },

                                    quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Dla mieszkańców",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancow",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Dla turystów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystow",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Dla inwestorów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaInwestorow",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                                });




                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.Text = "Super dziękuję za oddanie głosu :)";
                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                            else if (komenda.Substring(35, 1) == "6")
                            {

                                Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                                userStruct.userName = activity.From.Name;
                                userStruct.userId = activity.From.Id;
                                userStruct.botName = activity.Recipient.Name;
                                userStruct.botId = activity.Recipient.Id;
                                userStruct.ServiceUrl = activity.ServiceUrl;

                                BaseDB.zapiszOdpowiedzi(komenda.Substring(komenda.LastIndexOf('_') + 1), 0, 0, 0, 0, 0, 1);

                                Parameters.Parameters.listaAdresow.Add(userStruct);
                                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                                var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                                connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                IMessageActivity message = Activity.CreateMessageActivity();
                                message.ChannelData = JObject.FromObject(new
                                {
                                    notification_type = "REGULAR",


                                    buttons = new dynamic[]
                                {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                                },

                                    quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Dla mieszkańców",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancow",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Dla turystów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystow",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Dla inwestorów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaInwestorow",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                                });



                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id);
                                message.Text = "Super dziękuję za oddanie głosu :)";
                                await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                        }

                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowSport" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowSport")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //       BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //        BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();


                            message.Text = "Co nowego w spocie? :sunglasses:";
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",


                                buttons = new dynamic[]
                            {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                            },

                                quick_replies = new dynamic[]
                                   {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowAktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Sport",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowSport",
                               //       image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowGaleria",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Kalendarium",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowKalendarium",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                   }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            message.Attachments = BaseGETMethod.GetCardsAttachmentsSport(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowGaleria" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowGaleria")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //       BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //        BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.Text = "Sprawdź nasze najnowsze galerie :camera:";
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",


                                buttons = new dynamic[]
                            {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                            },

                                quick_replies = new dynamic[]
                                   {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowAktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Sport",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowSport",
                               //       image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowGaleria",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Kalendarium",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowKalendarium",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                   }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            message.Attachments = BaseGETMethod.GetCardsAttachmentsGaleria(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowKalendarium" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowKalendarium")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //       BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //        BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();


                            message.Text = "Wydarzenia w Naszej gminie :date:";
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",


                                buttons = new dynamic[]
       {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
       },

                                quick_replies = new dynamic[]
              {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowAktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Sport",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowSport",
                               //       image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowGaleria",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Kalendarium",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowKalendarium",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                              }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            message.Attachments = BaseGETMethod.GetCardsAttachmentsKalendarium(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else if (komenda == "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancow" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancow")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //       BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //        BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.Text = "Poniżej aktualności z życia gminy, możesz wybrać również jedną z podpowiedzi. ";
                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",


                                buttons = new dynamic[]
                            {
                            new
                        {
                                type = "web_url",
                                url = "https://petersfancyapparel.com/classic_white_tshirt",
                                title = "Wyniki",
                                webview_height_ratio = "compact"
                            }
                            },

                                quick_replies = new dynamic[]
                                   {
                                //new
                                //{oh
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Aktualności",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowAktualnosci",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                 //   image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Sport",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowSport",
                               //       image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Galeria",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowGaleria",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                new
                                {
                                    content_type = "text",
                                    title = "Kalendarium",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancowKalendarium",
                                //       image_url = "https://www.samo-lepky.sk/data/11/hokej5.png"
                                },
                                                                   }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();

                            message.Attachments = BaseGETMethod.GetCardsAttachments(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }

                        else
                                if (komenda == "GET_STARTED_PAYLOAD" || activity.Text == "USER_DEFINED_PAYLOAD")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //           BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.Text = "Hej, wybierz obszar działania gminy :)";

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Dla mieszkańców",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancow",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Dla turystów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystow",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Dla inwestorów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaInwestorow",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });



                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Text = @"Cześć
Jestem BOTem, Twoim asystentem do kontaktu ze stronami internetowymi gminy Wiueruszów. Raz dziennie powiadomię Cię o aktualnościach z życia gminy. Ponadto spodziewaj się powiadomień w formie komunikatów, bądź innych informacji przekazywanych przez moderatora.   
";
                            // message.Attachments = GetCardsAttachments(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                            message.Text = @"Współpraca między nami jest bardzo prosta.Wydajesz mi polecenia, a ja za Ciebie wykonuje robotę.
Zaznacz tylko w rozwijanym menu lub skorzystaj z podpowiedzi, która sekcja cię interesuje, a ja automatycznie połączę Cię z aktualnościami z wybranej sekcji.
";

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                        else
                                if (komenda == "DEVELOPER_DEFINED_PAYLOAD_HELP" || activity.Text == "DEVELOPER_DEFINED_PAYLOAD_HELP")
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //               BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //                 BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Dla mieszkańców",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancow",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Dla turystów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystow",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Dla inwestorów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaInwestorow",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Text = @"Cześć
Jestem BOTem, Twoim asystentem do kontaktu ze stronami internetowymi gminy Wiueruszów. Raz dziennie powiadomię Cię o aktualnościach z życia gminy. Ponadto spodziewaj się powiadomień w formie komunikatów, bądź innych informacji przekazywanych przez moderatora.   
";
                            // message.Attachments = GetCardsAttachments(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);

                            message.Text = @"Współpraca między nami jest bardzo prosta.Wydajesz mi polecenia, a ja za Ciebie wykonuje robotę.
Zaznacz tylko w rozwijanym menu lub skorzystaj z podpowiedzi, która sekcja cię interesuje, a ja automatycznie połączę Cię z aktualnościami z wybranej sekcji.
";

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }



                        else
                        {
                            Parameters.Parameters.userDataStruct userStruct = new Parameters.Parameters.userDataStruct();
                            userStruct.userName = activity.From.Name;
                            userStruct.userId = activity.From.Id;
                            userStruct.botName = activity.Recipient.Name;
                            userStruct.botId = activity.Recipient.Id;
                            userStruct.ServiceUrl = activity.ServiceUrl;

                            //    BaseDB.AddToLog("UserName: " + userStruct.userName + " User Id: " + userStruct.userId + " BOtId: " + userStruct.botId + " BotName: " + userStruct.botName + " url: " + userStruct.ServiceUrl);
                            //             BaseDB.AddUser(userStruct.userName, userStruct.userId, userStruct.botName, userStruct.botId, userStruct.ServiceUrl, 1);

                            Parameters.Parameters.listaAdresow.Add(userStruct);
                            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var userAccount = new ChannelAccount(name: activity.From.Name, id: activity.From.Id);
                            var botAccount = new ChannelAccount(name: activity.Recipient.Name, id: activity.Recipient.Id);
                            connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                            var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                            IMessageActivity message = Activity.CreateMessageActivity();

                            message.ChannelData = JObject.FromObject(new
                            {
                                notification_type = "REGULAR",
                                //buttons = new dynamic[]
                                // {
                                //     new
                                //     {
                                //    type ="postback",
                                //    title="Tytul",
                                //    vslue = "tytul",
                                //    payload="DEVELOPER_DEFINED_PAYLOAD"
                                //     }
                                // },
                                quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Dla mieszkańców",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancow",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Dla turystów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystow",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Dla inwestorów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaInwestorow",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                            });


                            message.From = botAccount;
                            message.Recipient = userAccount;
                            message.Conversation = new ConversationAccount(id: conversationId.Id);
                            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                            List<IGrouping<string, string>> hrefList = new List<IGrouping<string, string>>();
                            message.Text = "Wybierz jedną z opcji";
                            // message.Attachments = BaseGETMethod.GetCardsAttachmentsGallery(ref hrefList, true);

                            await connector.Conversations.SendToConversationAsync((Activity)message);
                        }
                    }
                }

                else
                {
                    HandleSystemMessage(activity);
                }
            }
            catch (Exception ex)
            {
                BaseDB.AddToLog("Wysylanie wiadomosci: " + ex.ToString());
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        public async static void CreateMessage(IList<Attachment> foto, string wiadomosc, string fromId)
        {
            try
            {
                BaseDB.AddToLog("Wywołanie metody CreateMessage");

                string uzytkownik = "";
                DataTable dt = BaseGETMethod.GetUser();

                try
                {
                    MicrosoftAppCredentials.TrustServiceUrl(@"https://facebook.botframework.com", DateTime.MaxValue);

                    IMessageActivity message = Activity.CreateMessageActivity();
                    message.ChannelData = JObject.FromObject(new
                    {
                        notification_type = "REGULAR",
                        quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Dla mieszkańców",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancow",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Dla turystów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystow",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Dla inwestorów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaInwestorow",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                    });


                    message.AttachmentLayout = null;

                    if (foto != null && foto.Count > 0)
                    {
                        string filename = foto[0].ContentUrl.Substring(foto[0].ContentUrl.Length - 4, 3).Replace(".", "");

                        if (foto[0].ContentType.Contains("image")) foto[0].ContentUrl = "http://serwer1606926.home.pl/pub/" + filename + ".png";//since the baseaddress
                        else if (foto[0].ContentType.Contains("video")) foto[0].ContentUrl = "http://serwer1606926.home.pl/pub/" + filename + ".mp4";

                        //foto[0].ContentUrl = "http://serwer1606926.home.pl/pub/" + filename + ".png";

                        message.Attachments = foto;
                    }


                    //var list = new List<Attachment>();
                    //if (foto != null)
                    //{
                    //    for (int i = 0; i < foto.Count; i++)
                    //    {
                    //        list.Add(GetHeroCard(
                    //       foto[i].ContentUrl, "", "",
                    //       new CardImage(url: foto[i].ContentUrl),
                    //       new CardAction(ActionTypes.OpenUrl, "", value: ""),
                    //       new CardAction(ActionTypes.OpenUrl, "", value: "https://www.facebook.com/sharer/sharer.php?u=" + "")));
                    //    }
                    //}

                    message.Text = wiadomosc;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        try
                        {
                            if (fromId != dt.Rows[i]["UserId"].ToString())
                            {

                                var userAccount = new ChannelAccount(name: dt.Rows[i]["UserName"].ToString(), id: dt.Rows[i]["UserId"].ToString());
                                uzytkownik = userAccount.Name;
                                var botAccount = new ChannelAccount(name: dt.Rows[i]["BotName"].ToString(), id: dt.Rows[i]["BotId"].ToString());
                                var connector = new ConnectorClient(new Uri(dt.Rows[i]["Url"].ToString()), "e805b456-0519-498a-9b18-b53d44dad43d", "3dVp6ZUhEcch9FiaZ0U1hS5");
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id, isGroup: false);
                                //await connector.Conversations.SendToConversationAsync((Activity)message).ConfigureAwait(false);

                                var returne = await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                        }
                        catch (Exception ex)
                        {
                            BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                BaseDB.AddToLog("Błąd wysłania wiadomosci: " + ex.ToString());
            }
        }


        public async static void CreateAlert(DataTable dtAnkieta, string fromId)
        {
            try
            {
                BaseDB.AddToLog("Wywołanie metody CreateAlert");


                string uzytkownik = "";
                DataTable dt = BaseGETMethod.GetUser();

                try
                {
                    MicrosoftAppCredentials.TrustServiceUrl(@"https://facebook.botframework.com", DateTime.MaxValue);

                    IMessageActivity message = Activity.CreateMessageActivity();
                    message.ChannelData = JObject.FromObject(new
                    {
                        notification_type = "REGULAR",
                        quick_replies = new dynamic[]
                            {
                                //new
                                //{
                                //    content_type = "text",
                                //    title = "Aktualności",
                                //    payload = "DEFINED_PAYLOAD_FOR_PICKING_BLUE",
                                //    image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Blue%20Ball.png"
                                //},
                                new
                                {
                                    content_type = "text",
                                    title = "Dla mieszkańców",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaMieszkancow",
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                            //        image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = "Dla turystów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaTurystow",
                       //             image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = "Dla inwestorów",
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_DlaInwestorow",
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                           }
                    });



                    message.AttachmentLayout = null;


                    //var list = new List<Attachment>();
                    //if (foto != null)
                    //{
                    //    for (int i = 0; i < foto.Count; i++)
                    //    {
                    //        list.Add(GetHeroCard(
                    //       foto[i].ContentUrl, "", "",
                    //       new CardImage(url: foto[i].ContentUrl),
                    //       new CardAction(ActionTypes.OpenUrl, "", value: ""),
                    //       new CardAction(ActionTypes.OpenUrl, "", value: "https://www.facebook.com/sharer/sharer.php?u=" + "")));
                    //    }
                    //}

                    message.Text = dtAnkieta.Rows[0]["Tresc"].ToString();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        try
                        {
                            if (fromId != dt.Rows[i]["UserId"].ToString())
                            {

                                var userAccount = new ChannelAccount(name: dt.Rows[i]["UserName"].ToString(), id: dt.Rows[i]["UserId"].ToString());
                                uzytkownik = userAccount.Name;
                                var botAccount = new ChannelAccount(name: dt.Rows[i]["BotName"].ToString(), id: dt.Rows[i]["BotId"].ToString());
                                var connector = new ConnectorClient(new Uri(dt.Rows[i]["Url"].ToString()), "e805b456-0519-498a-9b18-b53d44dad43d", "3dVp6ZUhEcch9FiaZ0U1hS5");
                                var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                message.From = botAccount;
                                message.Recipient = userAccount;
                                message.Conversation = new ConversationAccount(id: conversationId.Id, isGroup: false);
                                //await connector.Conversations.SendToConversationAsync((Activity)message).ConfigureAwait(false);

                                var returne = await connector.Conversations.SendToConversationAsync((Activity)message);
                            }
                        }
                        catch (Exception ex)
                        {
                            BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                BaseDB.AddToLog("Błąd wysłania wiadomosci: " + ex.ToString());
            }
        }


        public async static void CreateAnkieta(DataTable dtAnkieta, string fromId)
        {
            try
            {
                BaseDB.AddToLog("Wywołanie metody CreateAnkieta");

                string uzytkownik = "";
                DataTable dt = BaseGETMethod.GetUser();

                MicrosoftAppCredentials.TrustServiceUrl(@"https://facebook.botframework.com", DateTime.MaxValue);

                IMessageActivity message = Activity.CreateMessageActivity();

                try
                {
                    if (dtAnkieta.Rows.Count >= 3)
                    {

                        message.ChannelData = JObject.FromObject(new
                        {
                            notification_type = "REGULAR",

                            quick_replies = new dynamic[]
      {

                                new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz1"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz1_"+dtAnkieta.Rows[0]["ID"],
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                   // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz2"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz2_"+dtAnkieta.Rows[0]["ID"],
                           //         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz3"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz3_"+dtAnkieta.Rows[0]["ID"],
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },



                                     }
                        });

                        message.AttachmentLayout = null;



                        message.Text = dtAnkieta.Rows[0]["Tresc"].ToString();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            try
                            {
                                if (fromId != dt.Rows[i]["UserId"].ToString())
                                {

                                    var userAccount = new ChannelAccount(name: dt.Rows[i]["UserName"].ToString(), id: dt.Rows[i]["UserId"].ToString());
                                    uzytkownik = userAccount.Name;
                                    var botAccount = new ChannelAccount(name: dt.Rows[i]["BotName"].ToString(), id: dt.Rows[i]["BotId"].ToString());
                                    var connector = new ConnectorClient(new Uri(dt.Rows[i]["Url"].ToString()), "e805b456-0519-498a-9b18-b53d44dad43d", "3dVp6ZUhEcch9FiaZ0U1hS5");
                                    var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                    message.From = botAccount;
                                    message.Recipient = userAccount;
                                    message.Conversation = new ConversationAccount(id: conversationId.Id, isGroup: false);
                                    //await connector.Conversations.SendToConversationAsync((Activity)message).ConfigureAwait(false);

                                    var returne = await connector.Conversations.SendToConversationAsync((Activity)message);
                                }
                            }
                            catch (Exception ex)
                            {
                                BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                            }
                        }
                    }
                    else if (dtAnkieta.Rows.Count == 4)
                    {

                        message.ChannelData = JObject.FromObject(new
                        {
                            notification_type = "REGULAR",

                            quick_replies = new dynamic[]
      {

                                new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz1"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz1_"+dtAnkieta.Rows[0]["ID"],
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                   // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz2"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz2_"+dtAnkieta.Rows[0]["ID"],
                           //         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz3"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz3_"+dtAnkieta.Rows[0]["ID"],
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },

                                                               new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz4"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz4_"+dtAnkieta.Rows[0]["ID"],
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },

                                     }
                        });

                        message.AttachmentLayout = null;




                        //var list = new List<Attachment>();
                        //if (foto != null)
                        //{
                        //    for (int i = 0; i < foto.Count; i++)
                        //    {
                        //        list.Add(GetHeroCard(
                        //       foto[i].ContentUrl, "", "",
                        //       new CardImage(url: foto[i].ContentUrl),
                        //       new CardAction(ActionTypes.OpenUrl, "", value: ""),
                        //       new CardAction(ActionTypes.OpenUrl, "", value: "https://www.facebook.com/sharer/sharer.php?u=" + "")));
                        //    }
                        //}

                        message.Text = dtAnkieta.Rows[0]["Tresc"].ToString();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            try
                            {
                                if (fromId != dt.Rows[i]["UserId"].ToString())
                                {

                                    var userAccount = new ChannelAccount(name: dt.Rows[i]["UserName"].ToString(), id: dt.Rows[i]["UserId"].ToString());
                                    uzytkownik = userAccount.Name;
                                    var botAccount = new ChannelAccount(name: dt.Rows[i]["BotName"].ToString(), id: dt.Rows[i]["BotId"].ToString());
                                    var connector = new ConnectorClient(new Uri(dt.Rows[i]["Url"].ToString()), "e805b456-0519-498a-9b18-b53d44dad43d", "3dVp6ZUhEcch9FiaZ0U1hS5");
                                    var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                    message.From = botAccount;
                                    message.Recipient = userAccount;
                                    message.Conversation = new ConversationAccount(id: conversationId.Id, isGroup: false);
                                    //await connector.Conversations.SendToConversationAsync((Activity)message).ConfigureAwait(false);

                                    var returne = await connector.Conversations.SendToConversationAsync((Activity)message);
                                }
                            }
                            catch (Exception ex)
                            {
                                BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                            }
                        }
                    }
                    else if (dtAnkieta.Rows.Count == 5)
                    {
                        message.ChannelData = JObject.FromObject(new
                        {
                            notification_type = "REGULAR",

                            quick_replies = new dynamic[]
      {

                                new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz1"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz1_"+dtAnkieta.Rows[0]["ID"],
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                   // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz2"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz2_"+dtAnkieta.Rows[0]["ID"],
                           //         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz3"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz3_"+dtAnkieta.Rows[0]["ID"],
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },

                                                               new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz4"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz4_"+dtAnkieta.Rows[0]["ID"],
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                                                            new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz5"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz5_"+dtAnkieta.Rows[0]["ID"],
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },

                                     }
                        });

                        message.AttachmentLayout = null;




                        //var list = new List<Attachment>();
                        //if (foto != null)
                        //{
                        //    for (int i = 0; i < foto.Count; i++)
                        //    {
                        //        list.Add(GetHeroCard(
                        //       foto[i].ContentUrl, "", "",
                        //       new CardImage(url: foto[i].ContentUrl),
                        //       new CardAction(ActionTypes.OpenUrl, "", value: ""),
                        //       new CardAction(ActionTypes.OpenUrl, "", value: "https://www.facebook.com/sharer/sharer.php?u=" + "")));
                        //    }
                        //}

                        message.Text = dtAnkieta.Rows[0]["Tresc"].ToString();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            try
                            {
                                if (fromId != dt.Rows[i]["UserId"].ToString())
                                {

                                    var userAccount = new ChannelAccount(name: dt.Rows[i]["UserName"].ToString(), id: dt.Rows[i]["UserId"].ToString());
                                    uzytkownik = userAccount.Name;
                                    var botAccount = new ChannelAccount(name: dt.Rows[i]["BotName"].ToString(), id: dt.Rows[i]["BotId"].ToString());
                                    var connector = new ConnectorClient(new Uri(dt.Rows[i]["Url"].ToString()), "e805b456-0519-498a-9b18-b53d44dad43d", "3dVp6ZUhEcch9FiaZ0U1hS5");
                                    var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                    message.From = botAccount;
                                    message.Recipient = userAccount;
                                    message.Conversation = new ConversationAccount(id: conversationId.Id, isGroup: false);
                                    //await connector.Conversations.SendToConversationAsync((Activity)message).ConfigureAwait(false);

                                    var returne = await connector.Conversations.SendToConversationAsync((Activity)message);
                                }
                            }
                            catch (Exception ex)
                            {
                                BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                            }
                        }
                    }
                    else
                    {
                        message.ChannelData = JObject.FromObject(new
                        {
                            notification_type = "REGULAR",

                            quick_replies = new dynamic[]
      {

                                new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz1"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz1_"+dtAnkieta.Rows[0]["ID"],
                                    //     image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                   // image_url = "http://archiwum.koluszki.pl/zdjecia/naglowki_nowe/listopad%202013/pi%C5%82ka[1].png"
                                },
                                new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz2"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz2_"+dtAnkieta.Rows[0]["ID"],
                           //         image_url = "https://gim7bytom.edupage.org/global/pics/iconspro/sport/volleyball.png"
                                },                                new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz3"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz3_"+dtAnkieta.Rows[0]["ID"],
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },

                                                               new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz4"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz4_"+dtAnkieta.Rows[0]["ID"],
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                                                            new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz5"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz5_"+dtAnkieta.Rows[0]["ID"],
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },
                                                                                                                         new
                                {
                                    content_type = "text",
                                    title = dtAnkieta.Rows[0]["Odpowiedz6"].ToString(),
                                    payload = "DEVELOPER_DEFINED_PAYLOAD_Odpowiedz6_"+dtAnkieta.Rows[0]["ID"],
                                //       image_url = "https://cdn3.iconfinder.com/data/icons/developperss/PNG/Green%20Ball.png"
                                },

                                     }
                        });

                        message.AttachmentLayout = null;




                        //var list = new List<Attachment>();
                        //if (foto != null)
                        //{
                        //    for (int i = 0; i < foto.Count; i++)
                        //    {
                        //        list.Add(GetHeroCard(
                        //       foto[i].ContentUrl, "", "",
                        //       new CardImage(url: foto[i].ContentUrl),
                        //       new CardAction(ActionTypes.OpenUrl, "", value: ""),
                        //       new CardAction(ActionTypes.OpenUrl, "", value: "https://www.facebook.com/sharer/sharer.php?u=" + "")));
                        //    }
                        //}

                        message.Text = dtAnkieta.Rows[0]["Tresc"].ToString();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            try
                            {
                                if (fromId != dt.Rows[i]["UserId"].ToString())
                                {

                                    var userAccount = new ChannelAccount(name: dt.Rows[i]["UserName"].ToString(), id: dt.Rows[i]["UserId"].ToString());
                                    uzytkownik = userAccount.Name;
                                    var botAccount = new ChannelAccount(name: dt.Rows[i]["BotName"].ToString(), id: dt.Rows[i]["BotId"].ToString());
                                    var connector = new ConnectorClient(new Uri(dt.Rows[i]["Url"].ToString()), "e805b456-0519-498a-9b18-b53d44dad43d", "3dVp6ZUhEcch9FiaZ0U1hS5");
                                    var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);
                                    message.From = botAccount;
                                    message.Recipient = userAccount;
                                    message.Conversation = new ConversationAccount(id: conversationId.Id, isGroup: false);
                                    //await connector.Conversations.SendToConversationAsync((Activity)message).ConfigureAwait(false);

                                    var returne = await connector.Conversations.SendToConversationAsync((Activity)message);
                                }
                            }
                            catch (Exception ex)
                            {
                                BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                            }
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    BaseDB.AddToLog("Błąd wysyłania wiadomości do: " + uzytkownik + " " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                BaseDB.AddToLog("Błąd wysłania wiadomosci: " + ex.ToString());
            }
        }




        public static void CallToChildThread()
        {
            try
            {
                Thread.Sleep(5000);
            }

            catch (ThreadAbortException e)
            {
                Console.WriteLine("Thread Abort Exception");
            }
            finally
            {
                Console.WriteLine("Couldn't catch the Thread Exception");
            }
        }






        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                BaseDB.DeleteUser(message.From.Id);
            }
            else
                if (message.Type == ActivityTypes.ConversationUpdate)
            {
            }
            else
                    if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
            }
            else
                        if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else
                            if (message.Type == ActivityTypes.Ping)
            {
            }
            else
                                if (message.Type == ActivityTypes.Typing)
            {
            }
            return null;
        }







    }
}
