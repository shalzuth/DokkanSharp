using System;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DokkanSharp
{
    class Program
    {
        static String BasicAuth;
        static String MacSecret;
        static String MacId;
        static String AdId;
        static String UniqueId;
        static String GetData(String url)
        {
            return PostData(url, null);
        }
        static String PostData(String action, String data)
        {
            return SendData(action, data, false);
        }
        static String PutData(String action, String data)
        {
            return SendData(action, data, true);
        }
        static String SendData(String action, String data, Boolean put)
        {
            var url = "https://ishin-global.aktsk.com/" + action;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Clear();
            webRequest.Method = WebRequestMethods.Http.Get;
            webRequest.Timeout = 150000;
            webRequest.Accept = "*/*";
            webRequest.ContentType = "application/json";
            webRequest.Headers["X-Platform"] = "android";
            webRequest.Headers["X-ClientVersion"] = "3.3.0";
            webRequest.Headers["X-Language"] = "en";
            if (!String.IsNullOrEmpty(MacId))
                webRequest.Headers["Authorization"] = "MAC " + GetMAC(webRequest.Method, new Uri(url));
            else if (!String.IsNullOrEmpty(BasicAuth))
                webRequest.Headers["Authorization"] = "Basic " + BasicAuth;
            webRequest.Headers["X-AssetVersion"] = "1501678366";
            webRequest.Headers["X-DatabaseVersion"] = "1501735310";
            webRequest.Headers["X-RequestVersion"] = "24";
            if (data != null)
            {
                webRequest.Method = WebRequestMethods.Http.Post;
                if (put)
                    webRequest.Method = WebRequestMethods.Http.Put;
                if (!String.IsNullOrEmpty(MacId))
                    webRequest.Headers["Authorization"] = "MAC " + GetMAC(webRequest.Method, new Uri(url));
                byte[] byteArray = Encoding.ASCII.GetBytes(data);
                webRequest.ContentLength = byteArray.Length;
                using (Stream requestStream = webRequest.GetRequestStream())
                    requestStream.Write(byteArray, 0, byteArray.Length);
            }
            String html = "";
            using (StreamReader streamReader = new StreamReader((webRequest.GetResponse() as HttpWebResponse).GetResponseStream()))
                html = streamReader.ReadToEnd();
            return html;
        }
        static String GetMAC(String method, Uri uri)
        {
            var ts = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
            var nonce = ts + ":" + Guid.NewGuid();
            var value = ts + "\n" + nonce + "\n" + method + "\n" + uri.PathAndQuery + "\n" + uri.Host + "\n" + 3001 + "\n\n";
            var hashGenerator = new HMACSHA256(Encoding.ASCII.GetBytes(MacSecret));
            var mac = Convert.ToBase64String(hashGenerator.ComputeHash(Encoding.ASCII.GetBytes(value)));
            return "id=\"" + MacId + "\" nonce=\"" + nonce + "\" ts=\"" + ts + "\" mac=\"" + mac + "\"";
        }
        static void Main(string[] args)
        {
            //Console.WriteLine(GetData("ping"));
            //Console.WriteLine(GetData("tutorial/assets"));
            AdId = Guid.NewGuid().ToString().ToLower();
            UniqueId = Guid.NewGuid().ToString().ToLower() + ":" + Guid.NewGuid().ToString().ToLower().Substring(0, 8);
            var user_account = new JObject();
            user_account.Add(new JProperty("ad_id", AdId));
            user_account.Add(new JProperty("country", "US"));
            user_account.Add(new JProperty("currency", "USD"));
            user_account.Add(new JProperty("device", "samsung"));
            user_account.Add(new JProperty("device_model", "SM-E7000"));
            user_account.Add(new JProperty("os_version", "4.4.2"));
            user_account.Add(new JProperty("platform", "android"));
            user_account.Add(new JProperty("unique_id", UniqueId));
            var sign_up_obj = new JObject();
            sign_up_obj.Add(new JProperty("user_account", user_account));
            var sign_up = PostData("auth/sign_up", JsonConvert.SerializeObject(sign_up_obj, Formatting.None));
            var sign_up_response = (JObject)JsonConvert.DeserializeObject(sign_up);
            var pw_acc = Encoding.UTF8.GetString(Convert.FromBase64String(sign_up_response["identifier"].ToString()));
            var acc_pw = pw_acc.Substring(pw_acc.IndexOf(":") + 1) + ":" + pw_acc.Substring(0, pw_acc.IndexOf(":"));
            BasicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(acc_pw));

            var sign_in_obj = new JObject();
            sign_in_obj.Add(new JProperty("ad_id", AdId));
            sign_in_obj.Add(new JProperty("unique_id", UniqueId));
            var sign_in = PostData("auth/sign_in", JsonConvert.SerializeObject(sign_in_obj, Formatting.None));
            var sign_in_response = (JObject)JsonConvert.DeserializeObject(sign_in);
            MacId = sign_in_response["access_token"].ToString();
            MacSecret = sign_in_response["secret"].ToString();

            var user = GetData("user");
            var teams = GetData("teams");
            var cards = GetData("cards");
            //var client_assets = GetData("client_assets?is_tutorial=true");
            //var tutorial_assets = GetData("tutorial_assets");

            var tutorial_obj = new JObject();
            tutorial_obj.Add(new JProperty("progress", 30));
            var tutorial = PutData("tutorial", JsonConvert.SerializeObject(tutorial_obj, Formatting.None));

            var tutorial_gasha = PostData("tutorial/gasha", "null");

            tutorial_obj["progress"] = 40;
            tutorial = PutData("tutorial", JsonConvert.SerializeObject(tutorial_obj, Formatting.None));

            var name = new JObject();
            name.Add(new JProperty("name", "Abra"));
            var username = new JObject();
            username.Add(new JProperty("user", name));
            tutorial = PutData("user", JsonConvert.SerializeObject(username, Formatting.None));

            tutorial_obj["progress"] = 50;
            tutorial = PutData("tutorial", JsonConvert.SerializeObject(tutorial_obj, Formatting.None));
            tutorial_obj["progress"] = 60;
            tutorial = PutData("tutorial", JsonConvert.SerializeObject(tutorial_obj, Formatting.None));
            teams = GetData("teams");
            cards = GetData("cards");
            tutorial_obj["progress"] = 70;
            tutorial = PutData("tutorial", JsonConvert.SerializeObject(tutorial_obj, Formatting.None));
            tutorial_obj["progress"] = 77;
            tutorial = PutData("tutorial", JsonConvert.SerializeObject(tutorial_obj, Formatting.None));
            tutorial_obj["progress"] = 80;
            tutorial = PutData("tutorial", JsonConvert.SerializeObject(tutorial_obj, Formatting.None));
            var tutorial_finish = PutData("tutorial/finish", "null");
            user = GetData("user");
            var user_areas = GetData("user_areas");
            var put_forward = PostData("missions/put_forward", "null");
            tutorial_obj["progress"] = 90;
            tutorial = PutData("tutorial", JsonConvert.SerializeObject(tutorial_obj, Formatting.None));
            var apologies_accept = PutData("apologies/accept", "null");
            var gifts = GetData("gifts");
            var dragonball_sets = GetData("dragonball_sets");
            var banners = GetData("banners");
            var missions = GetData("missions");
            var budokai = GetData("budokai");
            var sns_campaign = GetData("sns_campaign");
            var reward_campaigns = GetData("shops/reward_campaigns");
            var googleplay_special_shop_products = GetData("iap_rails/googleplay_special_shop_products?timestamp=");
            var friendships = GetData("friendships");
            var rtbattles = GetData("rtbattles");
            tutorial_obj["progress"] = 999;
            tutorial = PutData("tutorial", JsonConvert.SerializeObject(tutorial_obj, Formatting.None));
            var login_bonuses_accept = PostData("login_bonuses/accept", "null");
            gifts = GetData("gifts");
            var announcements = GetData("announcements?important=true");
            var start_dash_gasha_status = GetData("start_dash_gasha_status");
            var popups = GetData("popups?popup_master_ids=1");
            var beginners_guide_start = PostData("beginners_guide/start", "null");
            missions = GetData("missions");
            var missions_accept = PostData("missions/438523003/accept", "null");
            var special_items = GetData("special_items");
            var gashas = GetData("gashas");
            var draw = PostData("gashas/76/courses/1/draw", "null");
            gashas = GetData("gashas");
            var missions_accept2 = PostData("missions/438521068/accept", "null");
            start_dash_gasha_status = GetData("start_dash_gasha_status");

            var champName = new WebClient().DownloadString("https://dbz.space/cards/" + ((JObject)JsonConvert.DeserializeObject(draw))["gasha_items"][0]["item_id"]);
            champName = champName.Substring(champName.IndexOf("\"og:title\" content=\"") + 20);
            champName = champName.Substring(0, champName.IndexOf("| Game Cards | DBZ Space") - 1);
            Console.WriteLine("Rerolled: " + champName);
            Console.WriteLine(BasicAuth);
            Console.Read();
        }
    }
}
