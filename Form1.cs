using Ionic.Zip;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using Bunifu.Framework.UI;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using Ultrakill_Mod_Installer.Properties;
using System.Linq;

namespace Ultrakill_Mod_Installer
{
    public partial class Form1 : Form
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
            IntPtr pdv, [System.Runtime.InteropServices.In] ref uint pcFonts);

        private PrivateFontCollection fonts = new PrivateFontCollection();

        Font vcrFont;

        string settingsFile = Assembly.GetEntryAssembly().Location.Replace(AppDomain.CurrentDomain.FriendlyName, "config.json");
        string modsImgFolder = Assembly.GetEntryAssembly().Location.Replace(AppDomain.CurrentDomain.FriendlyName, "ModsThumbnails");

        private List<Control> GetAllControls(Control container, List<Control> list)
        {
            foreach (Control c in container.Controls)
            {

                if (c.Controls.Count > 0)
                    list = GetAllControls(c, list);
                else
                    list.Add(c);
            }

            return list;
        }
        private List<Control> GetAllControls(Control container)
        {
            return GetAllControls(container, new List<Control>());
        }

        void installUMM(object sender, EventArgs e)
        {
            if (!bunifuTextBox1.Text.Contains(@"\common\ULTRAKILL"))
            {
                MessageBox.Show("Provide a valid game folder.");
                return;
            }

            if (File.Exists($"{bunifuTextBox1.Text}/BepInEx/plugins/UMM/UMM.dll") && Directory.Exists($"{bunifuTextBox1.Text}/BepInEx/UMM Mods"))
            {
                MessageBox.Show("You already have UMM installed");
                return;
            }

            if (!File.Exists(Path.Combine(bunifuTextBox1.Text, "winhttp.dll")) && !Directory.Exists(Path.Combine(bunifuTextBox1.Text, "BepInEx/core")))
            {
                MessageBox.Show("You don't have BepInEx installed");
            }

            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(UMMDownloadComplete);
            webClient.DownloadFileAsync(new Uri("https://github.com/Temperz87/ultra-mod-manager/releases/download/0.4.2/UMM.v0.4.2.zip"), Path.Combine(bunifuTextBox1.Text, "UMM.v0.4.2.zip"));
        }

        void UMMDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            using (ZipFile zip1 = ZipFile.Read(Path.Combine(bunifuTextBox1.Text, "UMM.v0.4.2.zip")))
            {
                foreach (ZipEntry z in zip1)
                {
                    if(z.FileName == "UMM Mods/")
                    {
                        z.Extract(Path.Combine(bunifuTextBox1.Text, "BepInEx"), ExtractExistingFileAction.OverwriteSilently);
                    }
                    else
                    {
                        z.Extract(Path.Combine(bunifuTextBox1.Text, "BepInEx/plugins"), ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }
            File.Delete(Path.Combine(bunifuTextBox1.Text, "UMM.v0.4.2.zip"));

            MessageBox.Show("UMM installed");
        }

        void installBepInEx(object sender_, EventArgs e_)
        {
            if (!bunifuTextBox1.Text.Contains(@"\common\ULTRAKILL"))
            {
                MessageBox.Show("Provide a valid game folder.");
                return;
            }

            if (File.Exists($"{bunifuTextBox1.Text}/winhttp.dll") && Directory.Exists($"{bunifuTextBox1.Text}/BepInEx/core"))
            {
                MessageBox.Show("You already have BepInEx installed");
                return;
            }

            bool is64bitOS = Environment.Is64BitOperatingSystem;
            string downloadUrl = is64bitOS ? "https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x64_5.4.21.0.zip" : "https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x86_5.4.21.0.zip";
            string downloadFile = Path.Combine(bunifuTextBox1.Text, $"{downloadUrl.Split('/')[8]}");

            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += (sender, e) => BepinexDownloadComplete(sender, e, downloadFile);
            webClient.DownloadFileAsync(new Uri(downloadUrl), downloadFile);
        }

        private void BepinexDownloadComplete(object sender, AsyncCompletedEventArgs e, string downloadFile)
        {
            using (ZipFile zip1 = ZipFile.Read(downloadFile))
            {
                foreach (ZipEntry z in zip1)
                {
                    z.Extract(bunifuTextBox1.Text, ExtractExistingFileAction.OverwriteSilently);
                }
            }

            Directory.CreateDirectory(Path.Combine(bunifuTextBox1.Text, "BepInEx/plugins"));
            File.Delete(downloadFile);

            MessageBox.Show("BepInEx installed");
        }

        void createCard(JToken mod, bool isRecommended)
        {
            Image thumbnail = Resources.none;
            int xMultiplier = 1;
            int yMultiplier = 1;
            string imgPath = Path.Combine(modsImgFolder, $"{(string)mod["name"]}.png");
            int cardCount = 0;

            foreach (var e in Controls.OfType<BunifuCards>())
            {
                cardCount++;
                if(cardCount == 4)
                {
                    yMultiplier += 5;
                    xMultiplier = -15;
                    cardCount = 0;
                }
                xMultiplier += 16;
            }
            

            if (File.Exists(imgPath) && new FileInfo(imgPath).Length > 0)
            {
                thumbnail = Image.FromFile(imgPath);
            }
            else
            {
                if ((string)mod["thumbnail"] != "none" && (string)mod["thumbnail"] != "" && (string)mod["thumbnail"] != null)
                {
                    if (!Directory.Exists(modsImgFolder)) Directory.CreateDirectory(modsImgFolder);

                    WebClient webClient = new WebClient();
                    webClient.DownloadFileAsync(new Uri((string)mod["thumbnail"]), imgPath);
                }
            }

            int XcardMath = 29 * xMultiplier;
            int YcardMath = 106 * yMultiplier;
            int XlabelMath = XcardMath + 11;
            //int YlabelMath = YcardMath + 144;

            BunifuCards newCard = new BunifuCards();
            Label label3 = new Label();
            Label label2 = new Label();
            Label label1 = new Label();

            newCard.BackColor = Color.Transparent;
            newCard.BackgroundImageLayout = ImageLayout.Zoom;
            newCard.BorderRadius = 3;
            newCard.BorderStyle = BorderStyle.FixedSingle;
            newCard.BottomSahddow = true;
            newCard.color = isRecommended ? Color.Gray : Color.Gold;
            newCard.Controls.Add(label3);
            newCard.Controls.Add(label2);
            newCard.Controls.Add(label1);
            newCard.Cursor = Cursors.Hand;
            newCard.ForeColor = Color.Transparent;
            newCard.LeftSahddow = true;
            //newCard.Location = new Point(XcardMath, YcardMath);
            newCard.Margin = new Padding(0);
            newCard.Name = "card" + xMultiplier;
            newCard.RightSahddow = true;
            newCard.ShadowDepth = 0;
            newCard.Size = new Size(395, 495);
            newCard.TabIndex = 0;
            if (mod["useUMM"] == null) 
            {
                newCard.Click += new EventHandler((sender, e) => clickCard(sender, e, (string)mod["url"], false));
            }
            else
            {
                newCard.Click += new EventHandler((sender, e) => clickCard(sender, e, (string)mod["url"], (bool)mod["useUMM"]));
            }

            newCard.Paint += new PaintEventHandler((sender, e) => drawCard(sender, e, thumbnail));

            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(10, newCard.Location.Y + 230);
            label3.Name = "label3" + xMultiplier;
            label3.Size = new Size(287, 29);
            label3.TabIndex = 3;
            label3.Tag = "h1";
            label3.Text = (string)mod["name"];
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(10, newCard.Location.Y + 270);
            label2.Name = "label2" + xMultiplier;
            label2.Size = new Size(94, 29);
            label2.TabIndex = 2;
            label2.Tag = "h1";
            label2.Text = (string)mod["contributors"];
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, newCard.Location.Y + 330);
            label1.Name = "label1" + xMultiplier;
            label1.Size = new Size(350, 29);
            label1.TabIndex = 1;
            label1.Text = (string)mod["description"];
            label1.TextAlign = ContentAlignment.MiddleLeft;

            if (label3.Text.Contains("\n"))
            {
                label2.Location = new Point(XlabelMath, label2.Location.Y + 30);
                label1.Location = new Point(XlabelMath, label1.Location.Y + 30);
            }

            if (label2.Text.Contains("\n"))
            {
                label1.Location = new Point(XlabelMath, label1.Location.Y + 30);
            }

            Controls.OfType<FlowLayoutPanel>().First().Controls.Add(newCard);
        }

        public Form1()
        {
            InitializeComponent();

            byte[] fontData = Resources.vcr;
            IntPtr fontPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(fontData.Length);
            System.Runtime.InteropServices.Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
            uint dummy = 0;
            fonts.AddMemoryFont(fontPtr, Resources.vcr.Length);
            AddFontMemResourceEx(fontPtr, (uint)Resources.vcr.Length, IntPtr.Zero, ref dummy);
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(fontPtr);

            vcrFont = new Font(fonts.Families[0], 13.0F);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"https://raw.githubusercontent.com/radsi/modsDB/main/modsDB.json");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            JObject mods = JObject.Parse(new StreamReader(response.GetResponseStream()).ReadToEnd());

            List<JToken> lateCards = new List<JToken>();

            foreach (var mod in mods["cards"])
            {
                string imgPath = Path.Combine(modsImgFolder, $"{(string)mod["name"]}.png");

                if ((string)mod["thumbnail"] != "none" && (string)mod["thumbnail"] != "" && (string)mod["thumbnail"] != null && !File.Exists(imgPath))
                {
                    if (!Directory.Exists(modsImgFolder)) Directory.CreateDirectory(modsImgFolder);

                    WebClient webClient = new WebClient();
                    webClient.DownloadFileAsync(new Uri((string)mod["thumbnail"]), imgPath);
                }
            }

            foreach (var mod in mods["cards"])
            {
                if (!(bool)mod["recommended"])
                {
                    lateCards.Add(mod);
                    continue;
                }

                createCard(mod, false);
            }

            foreach (var mod in lateCards)
            {
                createCard(mod, true);
            }

            List<Control> allControls = GetAllControls(this);
            allControls.ForEach(k =>
            {
                k.Font = vcrFont;

                if(k.Tag != null)
                {
                    switch (k.Tag.ToString())
                    {
                        case "h1":
                            k.Font = new Font(fonts.Families[0], 15.0F);
                            break;
                    }
                }
            });
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(settingsFile))
            {
                bunifuTextBox1.Text = (string)JObject.Parse(File.ReadAllText(settingsFile))["DP"];
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle, Color.FromArgb(44, 47, 51), ButtonBorderStyle.Solid);
        }

        private void bunifuTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (!bunifuTextBox1.Text.Contains(@"\common\ULTRAKILL"))
            {
                MessageBox.Show("Invalid path");
            }
            else
            {
                JObject data = new JObject(
                    new JProperty("DP", bunifuTextBox1.Text)
                 );

                File.WriteAllText(settingsFile, data.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (!dlg.SelectedPath.Contains(@"\common\ULTRAKILL"))
                {
                    MessageBox.Show("Invalid path");
                }
                else
                {
                    bunifuTextBox1.Text = dlg.SelectedPath;

                    JObject data = new JObject(
                        new JProperty("DP", bunifuTextBox1.Text)
                    );

                    File.WriteAllText(settingsFile, data.ToString());
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Clear();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"https://raw.githubusercontent.com/radsi/modsDB/main/modsDB.json");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            JObject mods = JObject.Parse(new StreamReader(response.GetResponseStream()).ReadToEnd());

            List<JToken> lateCards = new List<JToken>();

            foreach (var mod in mods["cards"])
            {
                if (!(bool)mod["recommended"])
                {
                    lateCards.Add(mod);
                    continue;
                }

                createCard(mod, false);
            }

            foreach (var mod in lateCards)
            {
                createCard(mod, true);
            }

            List<Control> allControls = GetAllControls(this);
            allControls.ForEach(k =>
            {
                k.Font = vcrFont;

                if (k.Tag != null)
                {
                    switch (k.Tag.ToString())
                    {
                        case "h1":
                            k.Font = new Font(fonts.Families[0], 15.0F);
                            break;
                    }
                }
            });
        }

        private void drawCard(object sender, PaintEventArgs e, Image thumbnail)
        {
            e.Graphics.DrawImage(thumbnail, new Rectangle(0, 0, 400, 210));
        }

        void askUMM()
        {
            Form form2 = new Form();
            form2.Size = new Size(1000, 700);
            form2.BackColor = Color.FromArgb(44, 47, 51);
            form2.Font = vcrFont;
            form2.ForeColor = Color.White;
            form2.StartPosition = FormStartPosition.CenterScreen;
            Label qlabel = new Label();
            qlabel.Size = new Size(1000, 100);
            qlabel.Location = new Point(145, 100);
            qlabel.Text = "This mod use UMM, do you want to install UMM?";
            Button yesButton = new Button();
            yesButton.BackColor = Color.DodgerBlue;
            yesButton.FlatAppearance.BorderColor = Color.White;
            yesButton.FlatAppearance.BorderSize = 0;
            yesButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(133)))));
            yesButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(107)))), ((int)(((byte)(214)))));
            yesButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            yesButton.ForeColor = Color.White;
            yesButton.Location = new Point(280, 200);
            yesButton.Name = "buttonYes";
            yesButton.Size = new Size(174, 45);
            yesButton.TabIndex = 7;
            yesButton.Text = "Yes";
            yesButton.UseVisualStyleBackColor = false;
            yesButton.Click += new EventHandler(installUMM);
            Button noButton = new Button();
            noButton.BackColor = Color.DodgerBlue;
            noButton.FlatAppearance.BorderColor = Color.White;
            noButton.FlatAppearance.BorderSize = 0;
            noButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(66)))), ((int)(((byte)(133)))));
            noButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(107)))), ((int)(((byte)(214)))));
            noButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            noButton.ForeColor = Color.White;
            noButton.Location = new Point(480, 200);
            noButton.Name = "buttonNo";
            noButton.Size = new Size(174, 45);
            noButton.TabIndex = 7;
            noButton.Text = "No";
            noButton.UseVisualStyleBackColor = false;
            noButton.Click += new EventHandler((sender, e) => closeQuestionForm(sender, e, form2));
            form2.Controls.Add(qlabel);
            form2.Controls.Add(yesButton);
            form2.Controls.Add(noButton);
            form2.ShowDialog();
        }

        void closeQuestionForm(object sender, EventArgs e, Form form)
        {
            form.Close();
        }

        private void clickCard(object sender_, EventArgs e_, string url, bool useUMM)
        {
            if (!bunifuTextBox1.Text.Contains(@"\common\ULTRAKILL"))
            {
                MessageBox.Show("Provide a valid game folder.");
                return;
            }

            if (useUMM && !Directory.Exists($@"{bunifuTextBox1.Text}\BepInEx\UMM Mods\"))
            {
               askUMM();
               return;
            }

            WebClient webClient = new WebClient();

            string filePath = useUMM ? $@"{bunifuTextBox1.Text}\BepInEx\UMM Mods\{url.Split('/')[url.Split('/').Length - 1]}" : $@"{bunifuTextBox1.Text}\BepInEx\plugins\{url.Split('/')[url.Split('/').Length - 1]}";
            string folderPath = !useUMM ? $@"{bunifuTextBox1.Text}\BepInEx\UMM Mods\" : $@"{bunifuTextBox1.Text}\BepInEx\plugins\";

            if (url.EndsWith(".rar") || url.EndsWith(".zip"))
            {
               
               webClient.DownloadFileCompleted += new AsyncCompletedEventHandler((sender, e) => installCompressedMod(sender, e, filePath, folderPath));
               webClient.DownloadFileAsync(new Uri(url), filePath);
               MessageBox.Show($"{Path.GetFileNameWithoutExtension(url.Split('/')[url.Split('/').Length - 1])} installed");
            }
            else if(url.EndsWith(".dll"))
            {
               webClient.DownloadFileAsync(new Uri(url), filePath);
               MessageBox.Show($"{Path.GetFileNameWithoutExtension(url.Split('/')[url.Split('/').Length - 1])} installed");
            }
            else
            {
                System.Diagnostics.Process.Start(url);
            }
        }

        void installCompressedMod(object sender, AsyncCompletedEventArgs e, string filePath, string folderPath)
        {
            using (ZipFile zip1 = ZipFile.Read(filePath))
            {  
                foreach (ZipEntry z in zip1)
                {
                    if (zip1.Entries.First().IsDirectory)
                    {
                        z.Extract(bunifuTextBox1.Text, ExtractExistingFileAction.OverwriteSilently);
                    }
                    else
                    {
                        z.Extract(folderPath, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }
            File.Delete(filePath);
        }
    }
}
