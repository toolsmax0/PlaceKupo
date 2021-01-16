using Advanced_Combat_Tracker;
using FFXIV_ACT_Plugin.Common;
using Newtonsoft.Json;
using PlaceKupo.Areas;
using PlaceKupo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

[assembly: AssemblyTitle("库啵标点")]
[assembly: AssemblyDescription("在场地上标明安全点等提示")]
[assembly: AssemblyVersion("1.0.0.0")]

namespace PlaceKupo
{
    internal interface IPlaceFunc
    {
        void AddDelegates();

        void RemoveDelegates();
    }

    public class PlaceKupo : UserControl, IActPluginV1
    {
        #region 其它变量

        private static HttpClient client;
        private Label label1;
        private static string namazu;
        private TextBox portbox;
        private readonly string settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\PluginSample.config.xml");
        private Label statusLabel;
        private SettingsSerializer xmlSettings;
        private IPlaceFunc area;
        public static IDataSubscription subscription;
        public static IDataRepository repository;
        public static ListBox Logger;
        private Panel panel1;
        private Panel panel2;
        public ListBox LogList;

        #endregion 其它变量

        /// <summary>
        /// 存储区域和对应的标点方法
        /// </summary>
        private Dictionary<uint, Func<IPlaceFunc>> map;

        public void InitMap()
        {
            map = new Dictionary<uint, Func<IPlaceFunc>>
            {
                //要添加新的方法,请按照 {区域ID,() => new 类名()} 的规范添加,例:
                //区域ID可在此地址查询: https://github.com/quisquous/cactbot/blob/main/resources/zone_info.js
                //另外在安装此插件后切换区域时也会在聊天框显示区域ID
                //例:盛夏农庄
                //自定义类请实现IPlaceFunc接口
                { 134, () => new ExampleAction() },
                { 917, () => new Bunker() }
            };
        }

        /// <summary>
        /// 向鲶鱼精发送指令
        /// </summary>
        /// <param name="s">一条游戏内指令</param>
        public static void SendCommand(string s)
        {
            var data = new StringContent(s, System.Text.Encoding.UTF8, "application/json");
            var res = client.PostAsync(namazu + "command", data);
            res.Result.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 向游戏内写入标点数据
        /// </summary>
        /// <param name="preset">一套标点</param>
        public static void WriteWaymark(Preset preset)
        {
            try
            {
                var s = JsonConvert.SerializeObject(preset);
                var data = new StringContent(s, System.Text.Encoding.ASCII, "application/json");
                var res = client.PostAsync(namazu + "place", data);
                res.Result.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }

        /// <summary>
        /// 向游戏内写入标点数据
        /// </summary>
        /// <param name="marks">一组标点</param>
        public static void WriteWaymark(Waymark[] marks)
        {
            Array.Resize(ref marks, 8);
            Preset preset = new Preset
            {
                A = marks[0],
                B = marks[1],
                C = marks[2],
                D = marks[3],
                One = marks[4],
                Two = marks[5],
                Three = marks[6],
                Four = marks[7]
            };
            WriteWaymark(preset);
        }

        /// <summary>
        /// 记录一条日志。
        /// </summary>
        /// <param name="s"></param>
        public static void Log(string s)
        {
            Logger.Items.Add(String.Format("[{0:HH:mm:ss}] ", DateTime.Now) + s);
            Logger.SelectedIndex = Logger.Items.Count - 1;
            Logger.SelectedIndex = -1;
        }

        /// <summary>
        /// 使用TTS朗读一句文字。
        /// </summary>
        /// <param name="s"></param>
        public static void TTS(string s)
        {
            try
            {
                ActGlobals.oFormActMain.TTS(s);
                Log("TTS: " + s);
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }

        #region 其它

        private IDataSubscription GetSubscription()
        {
            if (subscription != null)
                return subscription;

            var FFXIV = ActGlobals.oFormActMain.ActPlugins.FirstOrDefault(x => x.lblPluginTitle.Text == "FFXIV_ACT_Plugin.dll");
            if (FFXIV != null && FFXIV.pluginObj != null)
            {
                subscription = (IDataSubscription)FFXIV.pluginObj.GetType().GetProperty("DataSubscription").GetValue(FFXIV.pluginObj);
            }

            return subscription;
        }

        public PlaceKupo()
        {
            InitializeComponent();
            Logger = LogList;
        }

        public void DeInitPlugin()
        {
            if (area != null)
                area.RemoveDelegates();
            subscription.ZoneChanged -= OnZoneChanged;
            SaveSettings();
            statusLabel.Text = "插件已退出";
        }

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            pluginScreenSpace.Text = "库啵标点工具";
            client = new HttpClient();
            GetSubscription();
            GetRepository();
            InitMap();
            xmlSettings = new SettingsSerializer(this);
            LoadSettings();
            pluginScreenSpace.Controls.Add(this);
            subscription.ZoneChanged += OnZoneChanged;
            statusLabel = pluginStatusText;
            statusLabel.Text = "Working :D";
            Log("库啵标点已启动");
            uint id = repository.GetCurrentTerritoryID();
            Log("当前区域ID: " + id.ToString());
            if (map.ContainsKey(id))
            {
                area = map[id]();
                area.AddDelegates();
            }
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlaceKupo));
            this.label1 = new System.Windows.Forms.Label();
            this.portbox = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.LogList = new System.Windows.Forms.ListBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            //
            // label1
            //
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            //
            // portbox
            //
            resources.ApplyResources(this.portbox, "portbox");
            this.portbox.Name = "portbox";
            this.portbox.TextChanged += new System.EventHandler(this.Portbox_TextChanged);
            this.portbox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Portbox_KeyPress);
            //
            // panel1
            //
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.portbox);
            this.panel1.Name = "panel1";
            //
            // panel2
            //
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.LogList);
            this.panel2.Name = "panel2";
            //
            // LogList
            //
            resources.ApplyResources(this.LogList, "LogList");
            this.LogList.FormattingEnabled = true;
            this.LogList.Name = "LogList";
            this.LogList.DoubleClick += new System.EventHandler(this.LogList_DoubleClick);
            //
            // PlaceKupo
            //
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "PlaceKupo";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSettings()
        {
            // Add any controls you want to save the state of
            xmlSettings.AddControlSetting(portbox.Name, portbox);

            if (File.Exists(settingsFile))
            {
                FileStream fs = new FileStream(settingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                XmlTextReader xReader = new XmlTextReader(fs);

                try
                {
                    while (xReader.Read())
                    {
                        if (xReader.NodeType == XmlNodeType.Element)
                        {
                            if (xReader.LocalName == "SettingsSerializer")
                            {
                                xmlSettings.ImportFromXml(xReader);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    statusLabel.Text = "Error loading settings: " + ex.Message;
                }
                xReader.Close();
            }
        }

        private void OnZoneChanged(uint id, string name)
        {
            try
            {
                SendCommand(string.Format("/e {0}:{1}", id, name));
                if (area != null)
                {
                    area.RemoveDelegates();
                    area = null;
                }
                if (map.ContainsKey(id))
                {
                    area = map[id]();
                    area.AddDelegates();
                }
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }

        private void Portbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && e.KeyChar != '\b') e.KeyChar = (char)0;
        }

        private void Portbox_TextChanged(object sender, EventArgs e)
        {
            namazu = String.Format("http://localhost:{0}/", portbox.Text);
        }

        private void SaveSettings()
        {
            FileStream fs = new FileStream(settingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            XmlTextWriter xWriter = new XmlTextWriter(fs, System.Text.Encoding.UTF8)
            {
                Formatting = System.Xml.Formatting.Indented,
                Indentation = 1,
                IndentChar = '\t'
            };
            xWriter.WriteStartDocument(true);
            xWriter.WriteStartElement("Config");    // <Config>
            xWriter.WriteStartElement("SettingsSerializer");    // <Config><SettingsSerializer>
            xmlSettings.ExportToXml(xWriter);   // Fill the SettingsSerializer XML
            xWriter.WriteEndElement();  // </SettingsSerializer>
            xWriter.WriteEndElement();  // </Config>
            xWriter.WriteEndDocument(); // Tie up loose ends (shouldn't be any)
            xWriter.Flush();    // Flush the file buffer to disk
            xWriter.Close();
        }

        private void LogList_DoubleClick(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(LogList.SelectedItem);
        }

        private IDataRepository GetRepository()
        {
            if (repository != null)
                return repository;

            var FFXIV = ActGlobals.oFormActMain.ActPlugins.FirstOrDefault(x => x.lblPluginTitle.Text == "FFXIV_ACT_Plugin.dll");
            if (FFXIV != null && FFXIV.pluginObj != null)
            {
                try
                {
                    repository = (IDataRepository)FFXIV.pluginObj.GetType().GetProperty("DataRepository").GetValue(FFXIV.pluginObj);
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                }
            }

            return repository;
        }

        #endregion 其它
    }
}