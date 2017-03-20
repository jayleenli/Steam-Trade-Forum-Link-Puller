using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SteamTradeForumLinkPuller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public String BadgeUsernameID;
        List<String> allIDsToPostTo = new List<String>(); //by steamAppID, with the ones not needed removed.
        List<String> BlacklistList = new List<String>();
        public bool hasBlacklist = false;
        public MainWindow()
        {
            InitializeComponent();
        }
        public void startProcess(Object sender, EventArgs e) //get contents of the badge webpage
        {
            pullUpButton.Content = "Loading...May take a few minutes";
            var contents = "";
            
            contents = webpageHTML.Text;

            var findUserStart = contents.IndexOf("var g_strProfileURL = \'");
            var findUserEnd = contents.IndexOf("<div class=\"pagecontent\">");
            BadgeUsernameID = contents.Substring(findUserStart + 52, (findUserEnd - findUserStart) - 65);
            Username.Text = BadgeUsernameID;
            if (blacklist.Text != "Blacklist(optional-use commas to separate steam IDs)") //no blacklist set
            {
                hasBlacklist = true;
                makeBlacklist();
            }
            putintoArray(contents);
            pullUpButton.Content = "Completed.";
        }

        public void putintoArray(String x) //get all locations of badge row overlay and store into array
        {
            //List<String> allGamesToMakeBadge = new List<String>(); //by Steam ID
            string webpage = x;
            var appID = "";
            int loc = webpage.IndexOf("class=\"badge_row_overlay\""); //for public
            
            while (loc != -1)
            {
                bool blacklisted = false;
                var toAdd = 61 + BadgeUsernameID.Length + 11; //href="http://steamcommunity.com/id/BADGEUSERNAME/gamecards/APPID/ 
                var endofSteamAppID = (webpage.Substring(loc + toAdd, 8)).IndexOf("/");//Assuming steam appIDs wont go over 6 nums anytime soon
                if (endofSteamAppID <= 0)
                {
                    //Don't add, badge isn't a game badge (ex steam ones)
                }
                else
                {
                    appID = webpage.Substring(loc + toAdd, endofSteamAppID);
                    if (appID.Trim() == "><" || appID.Trim() == "<" || appID == "566020")
                    {
                        //Don't add, get rid of all other non steam game cases
                        //566020 = steam awards
                    }
                    else
                    {
                        var loc2 = webpage.IndexOf("class=\"badge_craft_button\"",loc,2500); //looking for the ready button, means you got a full set
                        var loc3 = webpage.IndexOf("class=\"badge_progress_info\"", loc, 2500); //Looking to see if you have cards of the badge, so it doesnt pull up pages for badges you have no cards of.
                        if (loc2 >= 0 || loc3 <= 0) //found a match of full set or no cards of badge
                        {
                            //don't add
                        }
                        else
                        {
                            //Another check to see if you have any of the cards, from the first one in progress bar if has owned
                            var loc4 = webpage.IndexOf("0 of", loc3, 150);

                            if (loc4 >= 0)
                            {
                                //don't add
                            }
                            else
                            {
                                for (int q = 0; q < BlacklistList.Count; q++) //check to see if ID is on the blacklist
                                {
                                    if (appID == BlacklistList[q])
                                    {
                                        blacklisted = true;
                                    }
                                }
                                if (blacklisted == true)
                                {
                                    //Don't add to list
                                }
                                else
                                {
                                    if (checkIfPosted(appID) == false) //if not posted on first page of trading forum
                                    {
                                        allIDsToPostTo.Add(appID);
                                    }
                                    else { } //Don't add to list
                                }
                            }
                        }

                    }
                }

                loc = webpage.IndexOf("class=\"badge_row_overlay\"", loc + 1);
            }

            string printAllID = "";
            for (int i = 0; i < allIDsToPostTo.Count; i++)
            {
                printAllID += allIDsToPostTo[i] + " , ";
            }

            listOfAppIDs.Text = printAllID;
        }

        public void makeBlacklist()
        {
            var blacklistString = blacklist.Text;
            string[] blacklistStringArray = blacklistString.Split(',');
            for (int x = 0; x < blacklistStringArray.Length; x++)
            {
                BlacklistList.Add(blacklistStringArray[x]);
            }
        }

        public bool checkIfPosted(String appID) //Check to see if you already have a trading post in forum (for first page only)
        {
            var tradingForumToCheck = "";
            String tradeForumLink = "http://steamcommunity.com/app/" + appID + "/tradingforum/"; //format is http://steamcommunity.com/app/appID/tradingforum/

            using (var client = new System.Net.WebClient())
            {
                tradingForumToCheck = client.DownloadString(tradeForumLink).Trim(); 
            }
            var ifPosted = tradingForumToCheck.IndexOf(BadgeUsernameID); //search to see for stuff you posted, not just replied to or whatever
            //webpageHTML.Text = tradingForumToCheck; //debugline
            if (ifPosted <0) //not there
            {
                return false;
            }
            else
            {
                return true;
                //already posted, no need to post
            }
        }

        public void pullPages(Object sender, EventArgs e)
        {
            for (int i = 0; i < allIDsToPostTo.Count; i++)
            {
                String appID = allIDsToPostTo[i];
                String tradeForumLink = "http://steamcommunity.com/app/" + appID + "/tradingforum/"; //format is http://steamcommunity.com/app/appID/tradingforum/
                Process.Start(tradeForumLink);
            }
        }
    }
}
