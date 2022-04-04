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
using System;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Line = Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models.Line;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Diagnostics;
using Org.BouncyCastle.Asn1.Ocsp;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        static string subscriptionKey = "8fe3267095eb4af7915bdd9659494aa2";
        static string endpoint = "https://cvtest220.cognitiveservices.azure.com/";

        private ComputerVisionClient client { get; set; }
        private static ListBox lisB { get; set; }
        private static TextBox texB { get; set; }
        private static bool tr { get; set; }
        private static Thread th { get; set; }
        private static Thread save { get; set; }
        private static string img1 { get; set; }
        private static string SpeechResult { get; set; }
        private static int iterTh { get; set; }
        private static WebBrowser webBrowser1 { get; set; }
        private static string txtLocalA { get; set; }
        private static string NomerA { get; set; }
        private static string[] strArr { get; set; }
        public delegate void NextPrimeDelegate();

        public MainWindow()
        {
            InitializeComponent();
            client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey)) { Endpoint = endpoint };
            lisB = listBox1;
            tr = false;
            img1 = "";
            NomerA = "";
            SpeechResult = "";
            texB = textBox1;
            iterTh = 0;
            webBrowser1 = new WebBrowser();
            strArr = new string[8] { "1", "1", "1", "1", "1", "1", "1", "1" };
            string path = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            StreamWriter sw;
            //listBox1.Items.Add(path);
            save = new Thread(async () => {
                while (true)
                {
                    Thread.Sleep(100);
                    try
                    {
                        sw = new StreamWriter(path + "\\info.txt");
                        for (int i = 0; i < listBox1.Items.Count; i++)
                        {
                            sw.WriteLine(listBox1.Items[i]);
                        }
                        sw.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception: " + ex.Message);
                    }
                }
            });
            
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
            path = path + "\\bin\\Help.txt";
            System.Diagnostics.Process txt = new System.Diagnostics.Process();
            txt.StartInfo.FileName = "notepad.exe";
            txt.StartInfo.Arguments = path;
            txt.Start();

        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show((textBox1.Text.Length).ToString());
            listBox1.Items.Clear();
            textBox1.Text = "";
            pictureBox1.Source = null;
            //MessageBox.Show(CountNum(textBox1.Text).ToString());
            //MessageBox.Show(CountS(textBox1.Text).ToString());
            //String sssString = new String("www");
            //Stream sssStream = null;
            //Dispatcher.Invoke(() => textBox1.AppendText((result.Text).Trim(new char[] { ' ', '*', '.' })));
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            th = new Thread(async () => {
                var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
                var recognizer = new SpeechRecognizer(SpeechConfig.FromSubscription("d80f3427245240b2874919aeb9234cd2", "westeurope"), audioConfig);

                //listBox1.Items.Add("Speak into your microphone.");
                var result = await recognizer.RecognizeOnceAsync();
                //listBox1.Items.Add($"RECOGNIZED: Text={result.Text}");
                //listBox1.Items.Add($"{(result.Text).Trim(new Char[] { ' ', '*', '.' })}");
                //textBox1.Text = (result.Text).Trim(new Char[] { ' ', '*', '.' });
                Dispatcher.Invoke(() => textBox1.AppendText((result.Text).Trim(new char[] { ' ', '*', '.' })));
            });
            listBox1.Items.Clear();
            textBox1.Text = "";
            th.Start();
        }

        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                listBox1.Items.Clear();
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = "Vision img";
                dialog.Filter = "img files (*.jpeg, *.jpg, *.png, *.bmp, *.gif)|*.jpeg;*.jpg;*.png;*.bmp;*.gif|All files (*.*)|*.*";
                var txtLocalA = "";

                if (dialog.ShowDialog() == true) //dialog.ShowDialog() == DialogResult.OK
                {
                    //textBox1.Text = Path.Combine(dialog.InitialDirectory, dialog.FileName);
                    txtLocalA = System.IO.Path.Combine(dialog.InitialDirectory, dialog.FileName);
                }

                //txtLocalA = System.IO.Path.Combine(dialog.InitialDirectory, dialog.FileName);
                await AnalyzeImageLocal(client, txtLocalA, textBox1);
                await ReadFileLocal(client, txtLocalA, textBox1);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не вдалося розпізнати текст!");
                Console.WriteLine("Exception: " + ex.Message);
                //listBox1.Items.Clear();
                //listBox1.Items.Add("Exception Caught!");
            }
        }

        private async void button3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((textBox1.Text.Length).ToString() == "8" && (textBox1.Text).IndexOf("-") == -1)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        try
                        {
                            client.DefaultRequestHeaders.Add("Accept", "application/json");
                            client.DefaultRequestHeaders.Add("X-Api-Key", "b4d6c74c983b8ff524950af5d8a27956");
                            //MessageBox.Show((textBox1.Text.Length).ToString());
                            HttpResponseMessage response = await client.GetAsync($"https://baza-gai.com.ua/nomer/{textBox1.Text}");
                            //AO6071HC
                            //ВС3225МТ
                            response.EnsureSuccessStatusCode();
                            string responseBody = await response.Content.ReadAsStringAsync();
                            // Above three lines can be replaced with new helper method below

                            //string responseBody = await client.GetStringAsync(uri);
                            //listBox1.Items.Add(Get(responseBody));

                            // номер, марка, дата реестрации, прикмети, адрес, викрадений, операція, фото
                            var s = responseBody;
                            var s1 = s;
                            var s2 = "";
                            var s3 = "";
                            //listBox1.Items.Add(s);
                            listBox1.Items.Clear();
                            APIstr(s1, s2, s3);
                            NomerA = textBox1.Text;
                            //System.IO.Path
                            //MessageBox.Show(System.IO.Path);
                        }
                        catch (HttpRequestException ex)
                        {
                            listBox1.Items.Clear();
                            listBox1.Items.Add("Такого номеру в базі даних не існує!");
                            pictureBox1.Source = null;
                        }
                    }

                    using (HttpClient client = new HttpClient())
                    {
                        try
                        {
                            HttpResponseMessage response = await client.GetAsync($"{img1}");
                            //AO6071HC
                            //ВС3225МТ
                            response.EnsureSuccessStatusCode();
                            Stream responseBody = await response.Content.ReadAsStreamAsync();
                            //string path = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
                            string path = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
                            path = path + "\\Image\\imAuto.jpg";
                            //path + "\\info.txt"
                            //MessageBox.Show(path);
                            //Stream responseBody = await response.Content.ReadAsByteArrayAsync();
                            //var fileStream = File.Create(path);responseBody.Seek(0, SeekOrigin.Begin);responseBody.CopyTo(fileStream);fileStream.Close();Thread.Sleep(500);
                            BitmapImage bimg = new BitmapImage();
                            bimg.BeginInit();
                            bimg.UriSource = new Uri($"{img1}");
                            bimg.EndInit();
                            pictureBox1.Stretch = Stretch.Fill;
                            pictureBox1.Source = bimg;
                            pictureBox1.Stretch = Stretch.Uniform;
                            //pictureBox1.Source = new BitmapImage(new Uri($"{img1}"));

                        }
                        catch (HttpRequestException ex)
                        {
                            listBox1.Items.Clear();
                            listBox1.Items.Add("Такого номеру в базі даних не існує!");
                            pictureBox1.Source = null;
                        }
                    }

                }
                else
                {
                    listBox1.Items.Clear();
                    MessageBox.Show("Такого номеру в базі даних не існує!");
                    pictureBox1.Source = null;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка #404");
            }
        }
        
        public int CountNum(string s)
        {
            char[] ch = s.ToCharArray();
            int count = ch.Where((n) => n >= '0' && n <= '9').Count();
            return count;
        }

        public int CountS(string s)
        {
            char[] ch = (s.ToUpper()).ToCharArray();
            int count = ch.Where((n) => n >= 'A' && n <= 'Z').Count();
            return count;
        }

        public static async Task AnalyzeImageLocal(ComputerVisionClient client, string localImage, TextBox lb)
        {

            // Creating a list that defines the features to be extracted from the image. 
            List<VisualFeatureTypes> features = new List<VisualFeatureTypes>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects
            };

            using (Stream analyzeImageStream = File.OpenRead(localImage))
            {
                // Analyze the local image.
                ImageAnalysis results = await client.AnalyzeImageInStreamAsync(analyzeImageStream);
            }
        }

        public void APIstr(string s1, string s2, string s3)
        {
            //AA4747KK
            //AO6071HC
            //ВС3225МТ
            //AA1111AA
            //MessageBox.Show(s2 + " 0000 " + s3);
            //MessageBox.Show("1");
            //MessageBox.Show(s2 + " 0000 " + s3);
            //MessageBox.Show(s2 + " 0000 " + s3);
            s2 = s1; s3 = s1;
            s2 = s2.Remove(0, s1.LastIndexOf("\"digits\": \"") + ("\"digits\": \"").Length + 1);
            s2 = s2.Substring(0, s2.IndexOf("\""));
            s2 = s2 + ". База ГАИ 2022";
            listBox1.Items.Add("Номер авто: " + s2); strArr[0] = "Номер авто: " + s2;
            //
            s2 = s1; s3 = s1;
            s2 = s2.Remove(0, s1.IndexOf("\"vendor\"") + ("\"vendor\"").Length + 1);
            s2 = s2.Substring(1, s2.IndexOf("\"photo_url\"") - 2); s3 = s2; s2 = s2.Substring(0, s2.IndexOf("\"model\"") - 2); //MessageBox.Show(s2);
            if ((((s3.Substring(s3.IndexOf("\"model\":\"") + "\"model\":\"".Length, s3.IndexOf("\",\"model_year\":") - "\",\"model_year\":".Length)).IndexOf("\"")).ToString())=="-1")
            {
                s2 = s2 + " " + s3.Substring(s3.IndexOf("\"model\":\"") + "\"model\":\"".Length, s3.IndexOf("\",\"model_year\":") - "\",\"model_year\":".Length);
            }
            else
            {
                s2 = s2 + " " + (s3.Substring(s3.IndexOf("\"model\":\"") + "\"model\":\"".Length, s3.IndexOf("\",\"model_year\":") - "\",\"model_year\":".Length)).Remove((s3.Substring(s3.IndexOf("\"model\":\"") + "\"model\":\"".Length, s3.IndexOf("\",\"model_year\":") - "\",\"model_year\":".Length)).IndexOf("\"")).Replace(" ", "");
            }
            //s2 = s2 + " " + s3.Substring(s3.IndexOf("\"model\":\"") + "\"model\":\"".Length, s3.IndexOf("\",\"model_year\":") - "\",\"model_year\":".Length);
            s2 = s2 + " " + s3.Remove(0, s3.IndexOf("\"model_year\"") + "\"model_year\"".Length + 1);
            listBox1.Items.Add("Марка авто: " + s2); strArr[1] = "Марка авто: " + s2;
            
            //
            s2 = s1; s3 = s1;
            s2 = s2.Remove(0, s1.LastIndexOf("\"registered_at\"") + ("\"registered_at\"").Length + 2);
            s2 = s2.Substring(0, s2.IndexOf("\""));
            listBox1.Items.Add("Дата реєстрації: " + s2); strArr[2] = "Дата реєстрації: " + s2;
            //
            s2 = s1; s3 = s1;
            s2 = s2.Remove(0, s1.LastIndexOf("\"color\"") + ("\"color\"").Length);
            s2 = s2.Remove(0, s2.IndexOf("\"ua\"") + ("\"ua\"").Length + 2);
            s2 = s2.Substring(0, s2.IndexOf("\"")); s3 = s2; s2 = s1;
            s2 = s2.Remove(0, s1.LastIndexOf("\"kind\""));
            s2 = s2.Remove(0, s2.IndexOf("\"ua\"") + ("\"ua\"").Length + 2);
            s2 = s2.Substring(0, s2.IndexOf("\""));
            s3 = s3 + ", " + s2; s2 = s3;
            listBox1.Items.Add("Прикмети авто: " + s2); strArr[3] = "Прикмети авто: " + s2;
            //
            s2 = s1; s3 = s1;
            s2 = s2.Remove(0, s1.LastIndexOf("\"address\"") + ("\"address\"").Length + 2);
            s2 = s2.Substring(0, s2.IndexOf("\""));
            listBox1.Items.Add("Адрес: " + s2); strArr[4] = "Адрес: " + s2;
            //
            s2 = s1; s3 = s1;
            s2 = s2.Remove(0, s1.LastIndexOf("\"is_stolen\"") + ("\"is_stolen\"").Length + 1);
            s2 = s2.Substring(0, s2.IndexOf(","));
            if (s2 == "false") { s2 = "Не викрадений"; } else { s2 = "Був викрадений"; }
            listBox1.Items.Add("За даними МВС: " + s2); strArr[5] = "За даними МВС: " + s2;
            //
            s2 = s1; s3 = s1;
            s2 = s2.Remove(0, s1.LastIndexOf("\"operation_group\"") + ("\"operation_group\"").Length);
            s2 = s2.Remove(0, s2.LastIndexOf("\"ua\"") + ("\"ua\"").Length + 2);
            s2 = s2.Substring(0, s2.IndexOf("\"")); s3 = s2; s2 = s1;
            s2 = s2.Remove(0, s1.LastIndexOf("\"operation\"") + ("\"operation\"").Length);
            s2 = s2.Remove(0, s2.IndexOf("\"ua\"") + ("\"ua\"").Length + 2);
            s2 = s2.Substring(0, s2.IndexOf("\"")); s3 = s3 + " (" + s2; s2 = s1;
            s2 = s2.Remove(0, s1.LastIndexOf("\"department\"") + ("\"department\"").Length + 2);
            s2 = s2.Substring(0, s2.IndexOf("\"")); s3 = s3 + ", " + s2 + ")"; s2 = s3;
            listBox1.Items.Add("Оперіція: " + s2); strArr[6] = "Оперіція: " + s2;
            //
            s2 = s1; s3 = s1;
            s2 = s2.Remove(0, s1.LastIndexOf("\"photo_url\"") + ("\"photo_url\"").Length + 2);
            s2 = s2.Substring(0, s2.IndexOf("\""));
            strArr[7] = "Img: " + s2; img1 = s2; //listBox1.Items.Add("Img: " + s2);
        }

        public static async Task ReadFileLocal(ComputerVisionClient client, string localFile, TextBox lb)
        {

            //lb.Items.Add("----------------------------------------------------------");
            //lb.Items.Add("READ FILE FROM LOCAL");
            //lb.Items.Add("");

            // Read text from URL
            var textHeaders = await client.ReadInStreamAsync(File.OpenRead(localFile), language: "en");
            // After the request, get the operation location (operation ID)
            string operationLocation = textHeaders.OperationLocation;
            Thread.Sleep(2000);

            // <snippet_extract_response>
            // Retrieve the URI where the recognized text will be stored from the Operation-Location header.
            // We only need the ID and not the full URL
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            // Extract the text
            ReadOperationResult results;
            //lb.Items.Add($"Reading text from local file {Path.GetFileName(localFile)}...");
            //lb.Items.Add("");
            do
            {
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while ((results.Status == OperationStatusCodes.Running ||
                results.Status == OperationStatusCodes.NotStarted));
            // </snippet_extract_response>

            // <snippet_extract_display>
            // Display the found text.
            //lb.Items.Add("");
            var textUrlFileResults = results.AnalyzeResult.ReadResults;
            var str1 = "";
            foreach (ReadResult page in textUrlFileResults)
            {
                foreach (Line line in page.Lines)
                {
                    //lb.Items.Add(line.Text);
                    str1 += line.Text;
                }
            }
            //lb.Items.Add(str1);
            //AO6071HC
            //ВС3225МТ
            string s220 = (str1.Replace(" ", "")).ToUpper();
            s220 = s220.Remove(s220.IndexOf("UA"), 2);
            lb.Text = s220;
            //lb.Items.Add("");
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            string strIter = "";
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                for (int i = 0; i < strArr.Length; i++)
                {
                    strIter += strArr[i] + "\n";
                }
                File.WriteAllText(saveFileDialog.FileName, strIter); //MessageBox.Show(strArr[0]);
            }
            if (iterTh == 0)
            {
                save.Start();
            }
            iterTh++;
            
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            save.Abort();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            save.Abort();
        }
    }
}
