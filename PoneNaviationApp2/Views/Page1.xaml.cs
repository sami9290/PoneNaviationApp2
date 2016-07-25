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
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Page1 : Page
    {
        public Page1()
        {
            this.InitializeComponent();
          
        }
        public string realid;
        public string sClientID ;
        public string realData ;
        public string AccessToken;
        public DateTime TokenExpiry;
        public string syncCurrentData="";
        public class UserData { public string userEmail; public string userPassword; }
        public void extractDATA(string clientData)
        {
            var posPLUS = clientData.IndexOf('+');
            var posPLUS1 = clientData.IndexOf('#');
            if (!(posPLUS < 0))
            {
                sClientID = clientData.Substring(0, posPLUS);
                syncCurrentData = "";
                realData = clientData.Substring(posPLUS1 + 1);
            }

        }
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


#if WINDOWS_PHONE_APP
    WebAuthenticationBroker.AuthenticateAndContinue(startUri, endUri, null, WebAuthenticationOptions.None);
#endif

        WebAuthenticationResult result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startUri, endUri);
        await ParseAuthenticationResult(result);

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
        public static  Quobject.SocketIoClientDotNet.Client.Socket soketname;
        public static Quobject.SocketIoClientDotNet.Client.Socket getsocket() { return soketname; }
        public static string userid;
        public static string getid() { return userid; }
        private void button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var socket = Quobject.SocketIoClientDotNet.Client.IO.Socket("http://localhost:3000");
            soketname = socket;
            socket.On(Quobject.SocketIoClientDotNet.Client.Socket.EVENT_CONNECT, (data) =>
            {
                //socket.Emit("hi");
                var userData = new UserData() { userEmail = "LTXuser@mail.com", userPassword = "hs-fulda" };

                UserData[] arr = new UserData[] { userData };
                string json_data = JsonConvert.SerializeObject(arr);
                //var obstring = @"{ ""userEmail"": ""LTXuser@mail.com"", ""userPassword"": ""hs-fulda"" }";
                //string json = "{\"userEmail\": \"LTXuser@mail.com\", \"userPassword\": \"hs-fulda\"}";

                socket.Emit("client_login", json_data);
                
            });
            socket.On("login_granted", (data) =>
            {
                extractDATA(data.ToString());
             
            });
          
            //await Task.Delay(2000);
           
                gotonextpage();
            Debug.WriteLine("fff");
        }
        //async Task PutTaskDelay()
        //{
        //    //await Task.Delay(50000);
        //}
        public void gotonextpage()
        {
            
            if (sClientID != null)
            {
                this.Frame.Navigate(typeof(Page2));
        }
    }
    }
}
