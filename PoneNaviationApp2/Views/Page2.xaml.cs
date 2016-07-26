using Microsoft.VisualBasic;
using System;
using System.IO;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.UI.Core;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Windows.UI.Popups;

namespace PoneNaviationApp2.Views
{
    /// Auther @Sami
    /// In this page the main sending and receiving to socket server operation exists
    /// Three main option, open new LateX file, open an already existing LateX file from the server or save your file as new file
    /// 
    public sealed partial class Page2 : Page
    {  
        public Page2()
        {
            this.InitializeComponent();
        }

        /// Some variables used in this view 
        public static Quobject.SocketIoClientDotNet.Client.Socket soketname=Page1.getsocket();
        public string realData;
        public static string clientID ;
        public string curDoc = "";
        public string syncCurrentData = "";
        public string[] terms = new string[9];

        /// The Navegation from the prevoius page with the clientid as a parameter
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                clientID = e.Parameter.ToString();
            }
            catch (Exception)
            {
                var dialog = new MessageDialog("Please Login First!!!");
                await dialog.ShowAsync();
            }
        }

        ///  a calass used in viwing and opening the documents
        class Res
        {
            public String Name { get; set; }
            public String ID { get; set; }
        }

        /// Get the available documents on the server into a matrix
        private async void button_Copy_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            int j = 0;
            List<string> docs = new List<string>();
            if (clientID != null)
            {
                var sSTR = clientID + '+' + "get files from the folder";
                soketname.Emit("getLXdocs", sSTR);  // get existing LaTeX docs on the server 
                soketname.On("avail_docs", (data) =>
                {
                    extractDATAAll(data.ToString());    
                    int offset = realData.IndexOf("#");
                    realData = "#" + realData;
                    int firstchar = 0;
                    int lastchar = 0;
                    for (int i = 1; i < 8; i++)
                    {
                            if (offset == -1) continue;
                            offset = realData.IndexOf("#", offset + 1);         
                            lastchar = offset;
                            string output = realData.Substring(firstchar + 1, lastchar - firstchar - 1);
                            firstchar = lastchar;
                            lastchar = 0;
                            terms[j] = output;
                            j++;
                    }
                });
            }
            await Task.Delay(3000);
            gotonextpage(j);
        }

        /// View the previous matrix of the documents
        public async void gotonextpage(int j)
        {
            j = 6;
            await Task.Delay(1000);
            int k = 0;
            List<Res> myList = new List<Res>();
            for (int u = 0; u <= j; u++)
            {
                Res binder1 = new Res();
                binder1.Name = terms[u];
                binder1.ID = terms[u];
                myList.Add(binder1);
            }
            lstData.ItemsSource = myList;
        }

        /// Open a selected Latex File
        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            curDoc = clickedButton.DataContext.ToString();
            var sSTR = clientID + '+' + clickedButton.DataContext.ToString();
            soketname.Emit("get_file", clientID + '+'+ curDoc+ '#' + curDoc);
            soketname.On("deliver_doc", (data) =>
            {
                extractDATA(data.ToString());
            });
            await Task.Delay(1000);
            editor.Visibility = Windows.UI.Xaml.Visibility.Visible;
            button_Copy.Margin = new Thickness(24, 490, 0, 0);
            soketname.On("editing_granted", (data) =>
           {
               extractDATA(data.ToString());
               Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                 () =>
                 {

                     update();
                 }
                 );
           });
            soketname.On("server_character", (data) =>
            {
                extractDATA(data.ToString());
                Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {

                        update();
                    }
                    );
            });
            editor.Document.SetText(Windows.UI.Text.TextSetOptions.None, realData);
            var sTxt = "User: " + realData + " wants to edit the file: " + clickedButton.DataContext.ToString() + "!\n\n";
        }

        ///Open a new Latex file
        private void button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            editor.Visibility = Windows.UI.Xaml.Visibility.Visible;         
            string text = File.ReadAllText("emty.txt");
            // Load the file into the Document property of the RichEditBox.
            editor.Document.SetText(Windows.UI.Text.TextSetOptions.None, text);

            button_Copy.Margin = new Thickness(24, 490, 0, 0);
            soketname.On("deliver_doc", (data) =>
            {
                extractDATA(data.ToString());
            });
            soketname.On("editing_granted",  (data) =>
            {
                extractDATA(data.ToString());
            });
            soketname.On("server_character",  (data) =>
          {
              extractDATAAll(data.ToString());
              Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                  () =>
                  {

                      update();
                  }
                  );
          }); 
        }

        /// Update the textblock with the new values
        public async void update()
        {
            await Task.Delay(1000);
            Debug.WriteLine(syncCurrentData + " >>>  " + curDoc);
            if (syncCurrentData == curDoc || syncCurrentData.Substring(0, 3) == curDoc.Substring(0, 3))
            {
                editor.Document.SetText(TextSetOptions.ApplyRtfDocumentDefaults, realData);
            }
        }

        /// Get the content of the latex file and the document name
        public void extractDATA(string clientData)
        {
            int start = clientData.IndexOf("+") + 1;
            int end = clientData.IndexOf("#", start);
            var posPLUS = clientData.IndexOf('+');
            var posPLUS1 = clientData.IndexOf('#');
            if (clientData.Contains("+#"))
            {
                realData = clientData.Substring(posPLUS1  + 1);
            }
            else
                syncCurrentData = clientData.Substring(clientData.IndexOf("+") + 1, clientData.IndexOf("#"));
            Debug.WriteLine(syncCurrentData);
            realData = clientData.Substring(posPLUS1 + 1);
        }

        /// Get only the content of the documnet
        public void extractDATAAll(string clientData)
        {
            var posPLUS = clientData.IndexOf('+');
            realData = clientData.Substring(posPLUS+1);
        }

        /// Save the nocument on the server as a new LateX file
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string content;
            var socketpage = Page1.getsocket();
            var sCURRENT_LTXdoc = clientID + "@document";
            editor.Document.GetText(Windows.UI.Text.TextGetOptions.None, out content);
            var sSTR2 = clientID + '+' + sCURRENT_LTXdoc + '+' + content;
            socketpage.Emit("save_doc", sSTR2);
        }

        /// Key event to sent the updated vcontent of the textbloc to the server
        private void editor_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            string text;
            editor.Document.GetText(Windows.UI.Text.TextGetOptions.None, out text);          
            string sSTR2 = clientID +'+' + syncCurrentData + '#' +  text;
            soketname.Emit("client_character", sSTR2);
        }
    }
}
