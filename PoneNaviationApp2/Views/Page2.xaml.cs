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

namespace PoneNaviationApp2.Views
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class Page2 : Page
    {
        public string clientID;
        public Page2()
        {
            this.InitializeComponent();
            clientID = Page1.getid();
        }
        public static Quobject.SocketIoClientDotNet.Client.Socket soketname=Page1.getsocket();
        public string realData;
        public string curDoc = "";
        public string syncCurrentData = "";
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
        private void editor_TextChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
        }
        class Res
        {
            public String Name { get; set; }
            public String ID { get; set; }
        }
        public string[] terms = new string[9];
        public Windows.UI.Xaml.Input.Pointer ptr;
        private async void button_Copy_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            int j = 0;
            List<string> docs = new List<string>();
            
            if (clientID != null)
            {
                var sSTR = clientID + '+'+ curDoc+ '#' + "get files from the folder";
                soketname.Emit("getLXdocs", sSTR);  // get existing LaTeX docs on the server 
                Debug.WriteLine("fffffffffffffffffffff");
                soketname.On("avail_docs", (data) =>
                {
                    extractDATA(data.ToString());
                    //int start = data.ToString().IndexOf("+");
                    //int stop = data.ToString().IndexOf("#");
                    //string output = data.ToString().Substring(start + 1, stop - start - 1);
                    //realData = output;
                    ////Debug.WriteLine(realData);
                    //terms[j] = realData;
                    //j++;
                    Debug.WriteLine(realData);
                    int offset = realData.IndexOf("#");
                    string docnames = realData.Substring(offset+1, realData.Length);
                    int firstchar = 0;
                    int lastchar = 0;
                    for (int i = 1; i < 9; i++)
                    {

                        if (offset == -1) continue;
                        offset = docnames.IndexOf("#", offset + 1);
                        if (firstchar == 0 && lastchar == 0)
                        {
                            firstchar = offset;
                        }
                        else if (firstchar != 0 && lastchar == 0)
                        {
                            lastchar = offset;
                            string output = docnames.Substring(firstchar + 1, lastchar - firstchar - 1);
                            realData = output;
                            firstchar = lastchar;
                            lastchar = 0;
                            //Debug.WriteLine(realData);
                            terms[j] = realData;
                            j++;
                        }
                    }
                });
                Debug.WriteLine(realData);
            }
            await Task.Delay(1000);
            gotonextpage(j);
        }

        public async void gotonextpage(int j)
        {
            await Task.Delay(1000);
            int k = 0;
            List<Res> myList = new List<Res>();
            for (int u = 0; u <= j; u++)
            {
                Res binder1 = new Res();
                binder1.Name = terms[u];
                binder1.ID = terms[u];
                myList.Add(binder1);
                Debug.WriteLine(terms[u]);
            }
            lstData.ItemsSource = myList;
        }

        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            Debug.WriteLine(clickedButton.DataContext.ToString());
            curDoc = clickedButton.DataContext.ToString();
            var sSTR = clientID + '+' + clickedButton.DataContext.ToString();
            Debug.WriteLine(clientID + '+' + clickedButton.DataContext.ToString());
            soketname.Emit("get_file", clientID + '+'+ curDoc+ '#' + clickedButton.DataContext.ToString());
            soketname.On("deliver_doc", (data) =>
            {
                extractDATA(data.ToString());
            });
            await Task.Delay(1000);
            editor.Visibility = Windows.UI.Xaml.Visibility.Visible;

            button_Copy.Margin = new Thickness(24, 490, 0, 0);

            // Load the file into the Document property of the RichEditBox.

            soketname.On("editing_granted", (data) =>
           {
               //    Debug.WriteLine("zzzzzzzzzzzzzzzzzzzzzz");
                  extractDATA(data.ToString());
               //    editor.Document.SetText(Windows.UI.Text.TextSetOptions.None, realData);
               //    //await Task.Delay(1000);
               //    //Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
               //    //  () =>
               //    //  {

               //    //      update();
               //    //  }
               //    //  );
           });
            soketname.On("server_character", (data) =>
            {
                extractDATA(data.ToString());
                Debug.WriteLine(realData);
                //await Task.Delay(1000);
                Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {

                        update();
                    }
                    );
            });
            editor.Document.SetText(Windows.UI.Text.TextSetOptions.None, realData);
            Debug.WriteLine(realData);
            var sTxt = "User: " + realData + " wants to edit the file: " + clickedButton.DataContext.ToString() + "!\n\n";
            //soketname.Emit("edit_allowed", sTxt);
        }

        private  void button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
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
            //soketname.On("editing_granted", (data) =>
            //{
            //    Debug.WriteLine("ggggggggggggggggggggggg");
            //    extractDATA(data.ToString());

            //    //await Task.Delay(1000);
            //    Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            //      () =>
            //      {

            //          update();
            //      }
            //      );
            //});
            soketname.On("server_character",  (data) =>
           {
               extractDATA(data.ToString());
               Debug.WriteLine(realData);
               //await Task.Delay(1000);
               Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                   () =>
                   {

                       update();
                   }
                   );
           }); 
        }
        public async void update()
        {
            await Task.Delay(1000);
            
            if (syncCurrentData.Substring(0, 3) == curDoc.Substring(0, 3)|| syncCurrentData==curDoc)
            {
                //editor.Document.GetText(Windows.UI.Text.TextGetOptions.None, out text);
                //int start = 68;
                //int end = text.Length - 15;
                //editor.Document.Selection.SetRange(start, end);
                //editor.Document.SetText(TextSetOptions.None, string.Empty);
                editor.Document.SetText(TextSetOptions.ApplyRtfDocumentDefaults, realData);
            }
        }
        public  void extractDATA(string clientData)
        {
            int start = clientData.IndexOf("+") + 1;
            int end = clientData.IndexOf("#", start);
            var posPLUS = clientData.IndexOf('+');
            var posPLUS1 = clientData.IndexOf('#');
            if (!(posPLUS < 0))
            {
                clientID = clientData.Substring(0, posPLUS);
                syncCurrentData = clientData.Substring(start, posPLUS1 - posPLUS);
                realData = clientData.Substring(posPLUS1 - posPLUS + 2);
                //syncCurrentData= Regex.Match(clientData, @"\(+\w+)\+").Groups[1].Value;
                //realData= clientData.Substring(posPLUS1+1);
            }
        }
        //public  void extractDATAall(string clientData)
        //{
        //    //await Task.Delay(1000);
        //    var posPLUS = clientData.IndexOf('+');
        //    if (!(posPLUS < 0))
        //    {
        //        realData = clientData.Substring(posPLUS + 1);
        //    }
        //}

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string content;
            var socketpage = Page1.getsocket();
            var sCURRENT_LTXdoc = clientID + "@document";
            editor.Document.GetText(Windows.UI.Text.TextGetOptions.None, out content);
            var sSTR2 = clientID + '+' + sCURRENT_LTXdoc + '+' + content;
            socketpage.Emit("save_doc", sSTR2);
        }

        private void editor_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            string text;
            editor.Document.GetText(Windows.UI.Text.TextGetOptions.None, out text);          
            string sSTR2 = clientID +'+' + curDoc + '#' +  text;
            soketname.Emit("client_character", sSTR2);
            Debug.WriteLine(sSTR2);
        }


    }
}
