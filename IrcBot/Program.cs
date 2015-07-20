using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace IrcBot {
    class Program {
        static IrcClient Irc;

        static void Main(string[] args) {
            //                          Bot settings
            string server = "irc.twitch.tv";                            // Server ip / domain. Will require modifications to user other ircs.
            int port = 6667;                                            // Port used to connect. Default = 6667
            string botnick = "botusername";                             // Bot username. The bots username
            string botpass = "oauth:";    				// Bot oauth token. Needs to be generated on twitch.

            string owner = "YOURUSERNAME";                              // Bot owner irc name. THIS WOULD BE YOUR USERNAME ON TWITCH.

            bool UseWhitelist = true;                                   // Enable whitelist. Defaut = true.
            List<string> whitelist = new List<string>();                // Users who can do commands.
            whitelist.Add(owner);

            List<string> channels = new List<string>();                 // Bot default channels.

            channels.Add(owner); //Always join owners channel, so it is easy command the bot.
            //channels.Add("username"); // twitch.tv/username
            
            //                          /Bot settings



            Console.Title = "Twitch IRC Bot - " + botnick;

            cw("[c] Connecting...");
            Irc = new IrcClient(server, port);

            if(!Irc.Sock.Connected) {
                cw("[c] Connection failed.");
                return;
            }

            Irc.Write(
                "PASS " + botpass + "\r\n" +
                "USER " + botnick + " 0 * :" + owner + "\r\n" +
                "NICK " + botnick + "\r\n");

            string data;

            for(data = Irc.Read(); ; data = Irc.Read()) {
                cw("[i] " + data);

                if(data.StartsWith("PING ")) {
                    string m = data.Replace("PING", "PONG") + "\r\n";
                    Irc.Write(m);
                    cw("[o] " + m.Replace("\r\n", ""));
                }
                if(data[0] != ':') continue;

                string user, type, chan, msg;
                ParseMessage(data, out user, out type, out chan, out msg);
                
                //cw("[d] " + msg); // debug message output, this will spam!
                if(owner == user || whitelist.Contains(user) || !UseWhitelist) {
                    if(type == "PRIVMSG") {
                        if(msg.StartsWith("!" + "ping")) {
                            Irc.Write("pong!", chan);
                        } else if(msg.StartsWith("!")) {
                            if(user == owner) {
                                if(msg.StartsWith("!" + "whitelist")) {
                                    if(msg.StartsWith("!" + "whitelist-enable")) {
                                        UseWhitelist = true;
                                    } else if(msg.StartsWith("!" + "whitelist-disable")) {
                                        UseWhitelist = false;
                                    } else if(msg.StartsWith("!" + "whitelist-add")) {
                                        string[] tmp = msg.Split(' ');
                                        if(tmp.Length == 3) {
                                            if(!whitelist.Contains(tmp[1])) {
                                                whitelist.Add(tmp[1]);
                                            }
                                        }
                                    } else if(msg.StartsWith("!" + "whitelist-rem")) {
                                        string[] tmp = msg.Split(' ');
                                        if(tmp.Length == 3) {
                                            if(owner == tmp[1]) {
                                                Irc.Write("Action not possible");
                                            } else {
                                                whitelist.Remove(tmp[1]);
                                            }
                                        }
                                    } else {
                                        string wlist = "";
                                        foreach(string tmpUser in whitelist) {
                                            if(wlist == "") {
                                                wlist += tmpUser;
                                            } else {
                                                wlist += ", " + tmpUser;
                                            }
                                        }
                                        Irc.Write("Whitelisted users: " + wlist, chan);
                                    }
                                }
                            }

                            if(msg.StartsWith("!" + "join")) {
                                Irc.Join(msg);
                            } else if(msg.StartsWith("!" + "part")) {
                                Irc.Part(msg);
                            }
                        }
                    }
                }

                if(data.Split(' ')[1] == "001") {
                    string n = "MODE " + botnick + " +B\r\n";
                    foreach(var item in channels) {
                        n += "JOIN #" + item + "\r\n";
                    }
                    Irc.Write(n);
                    cw("[c] Connection succesful.");
                }
            }
        }

        public static void ParseMessage(string lastMessage, out string user, out string type, out string chan, out string message) {
            string[] msg = lastMessage.Split(' ');
            user = msg[0].Split('!').First().Split(':').Last();
            type = msg[1];
            chan = msg[2];
            message = "";

            for(int i = 3; i < msg.Length; i++) {
                if(i == 3) {
                    message += Regex.Replace(msg[3], ":", "") + " ";
                } else {
                    message += msg[i] + " ";
                }
            }
            message.Trim().ToLower();
        }

        public static void cw(string m) {
            Console.WriteLine(m);
        }
    }
}
