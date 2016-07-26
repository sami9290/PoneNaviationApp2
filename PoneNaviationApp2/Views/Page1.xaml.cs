using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using System;
using Facebook;
using System.Text.RegularExpressions;

namespace PoneNaviationApp2.Views
{
    /// <summary>
    /// This is the main login page were the login id is generated and passed to the 
    /// </summary>
    public sealed partial class Page1 : Page
    {
        public Page1()
        {
            this.InitializeComponent();
        }

        ///Some public variables
        public string realid;
        public string sClientID ;
        public string realData ;
        public string AccessToken;
        public DateTime TokenExpiry;
        public string syncCurrentData="";
        public static Quobject.SocketIoClientDotNet.Client.Socket soketname;
        public static Quobject.SocketIoClientDotNet.Client.Socket getsocket() { return soketname; }
        public static string userid;
        public static string getid() { return userid; }
        public class UserData { public string userEmail; public string userPassword; }

        /// Getting the clientID from the server
        public void extractDATA(string clientData)
        {
            Debug.WriteLine(clientData);
           var posPLUS = clientData.IndexOf('+');
                sClientID = clientData.Substring(0, posPLUS);
        }

        /// Facebook Login #Under Construction!!#
        private async Task Login()
        {
            //Client ID of the Facebook App (retrieved from the Facebook Developers portal)
            var clientId = "1779097902326012";
            //Required permissions
            var scope = "public_profile, email";

            var redirectUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString();
            var fb = new FacebookClient();
            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = clientId,
                redirect_uri = redirectUri,
                response_type = "token",
                scope = scope
            });
            Uri startUri = loginUrl;
            Uri endUri = new Uri(redirectUri, UriKind.Absolute);
        WebAuthenticationResult result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startUri, endUri);
        await ParseAuthenticationResult(result);

            /// Facebook Login #Under Construction!!#
        }
        public async Task ParseAuthenticationResult(WebAuthenticationResult result)
        {
            switch (result.ResponseStatus)
            {
                case WebAuthenticationStatus.ErrorHttp:
                    Debug.WriteLine("Error");
                    break;
                case WebAuthenticationStatus.Success:
                    var pattern = string.Format("{0}#access_token={1}&expires_in={2}", WebAuthenticationBroker.GetCurrentApplicationCallbackUri(), "(?<access_token>.+)", "(?<expires_in>.+)");
                    var match = Regex.Match(result.ResponseData, pattern);

                    var access_token = match.Groups["access_token"];
                    var expires_in = match.Groups["expires_in"];

                    AccessToken = access_token.Value;
                    TokenExpiry = DateTime.Now.AddSeconds(double.Parse(expires_in.Value));

                    break;
                case WebAuthenticationStatus.UserCancel:
                    Debug.WriteLine("Operation aborted");
                    break;
                default:
                    break;
            }
        }
        /// Buttonclick for the login 
        private void button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            soketname = Quobject.SocketIoClientDotNet.Client.IO.Socket("http://localhost:3000");
            soketname.On(Quobject.SocketIoClientDotNet.Client.Socket.EVENT_CONNECT, (data) =>
            {
                var userData = new UserData() { userEmail = "LTXuser@mail.com", userPassword = "hs-fulda" };
                UserData[] arr = new UserData[] { userData };
                string json_data = JsonConvert.SerializeObject(arr);
                soketname.Emit("client_login", json_data);
            });
            gotonextpage();
            Debug.WriteLine("fff");
        }
        private async void gotonextpage()
        {
            //await Task.Delay(2000);
            soketname.On("login_granted", (data2) =>
            {
                extractDATA(data2.ToString());
            });
            await Task.Delay(3000);
            if (sClientID != null)
            {
            Debug.WriteLine(sClientID);
            this.Frame.Navigate(typeof(Page2),sClientID);
            }
        }
    }
}
