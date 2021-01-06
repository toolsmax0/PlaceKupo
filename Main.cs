using Advanced_Combat_Tracker;
using FFXIV_ACT_Plugin.Common;
using PlaceKupo.Areas;
using PlaceKupo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Windows.Forms;
using System.Xml;

[assembly: AssemblyTitle("��ࣱ��")]
[assembly: AssemblyDescription("�ڳ����ϱ�����ȫ�����ʾ")]
[assembly: AssemblyVersion("1.0.0.0")]

namespace PlaceKupo
{
    internal interface IPlaceFunc
    {
        void PlaceFunction(bool isImport, LogLineEventArgs logLine);
    }

    public class PlaceKupo : UserControl, IActPluginV1
    {
        #region ��������

        private static HttpClient client;
        private Label label1;
        private static string namazu;
        private TextBox portbox;
        private string settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\PluginSample.config.xml");
        private Label statusLabel;
        private SettingsSerializer xmlSettings;
        private IPlaceFunc area;
        private IDataSubscription subscription;

        #endregion ��������

        /// <summary>
        /// �洢����Ͷ�Ӧ�ı�㷽��
        /// </summary>
        private Dictionary<uint, Func<IPlaceFunc>> map;

        public void InitMap()
        {
            map = new Dictionary<uint, Func<IPlaceFunc>>();
            //Ҫ����µķ���,�밴�� <����ID,() => new ����()> �Ĺ淶���,��:
            //����ID���ڴ˵�ַ��ѯ: https://github.com/quisquous/cactbot/blob/main/resources/zone_info.js
            //�����ڰ�װ�˲�����л�����ʱҲ�����������ʾ����ID
            //��:ʢ��ũׯ
            //�Զ�������ʵ��IPlaceFunc�ӿ�
            map.Add(134, () => new ExampleAction());
            //map.Add(917, BunkerLaser);
        }

        /// <summary>
        /// �����㾫����ָ��
        /// </summary>
        /// <param name="s">һ����Ϸ��ָ��</param>
        public static void SendCommand(string s)
        {
            var data = new StringContent(s, System.Text.Encoding.UTF8, "application/json");
            var res = client.PostAsync(namazu + "command", data);
            res.Result.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// ����Ϸ��д��������
        /// </summary>
        /// <param name="preset">һ�ױ��</param>
        public static void WriteWaymark(Preset preset)
        {
            var s = JsonSerializer.Serialize(preset);
            var data = new StringContent(s, System.Text.Encoding.ASCII, "application/json");
            var res = client.PostAsync(namazu + "place", data);
            res.Result.Content.ReadAsStringAsync();
        }

        #region ����

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
        }

        public void DeInitPlugin()
        {
            if (area != null)
                ActGlobals.oFormActMain.OnLogLineRead -= area.PlaceFunction;
            subscription.ZoneChanged -= OnZoneChanged;
            SaveSettings();
            statusLabel.Text = "������˳�";
        }

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            pluginScreenSpace.Text = "��ࣱ�㹤��";
            client = new HttpClient();
            GetSubscription();
            InitMap();
            xmlSettings = new SettingsSerializer(this);
            LoadSettings();
            pluginScreenSpace.Controls.Add(this);
            subscription.ZoneChanged += OnZoneChanged;
            statusLabel = pluginStatusText;
            statusLabel.Text = "Working :D";
        }

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.portbox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "���㾫�˿�:";
            //
            // portbox
            //
            this.portbox.Location = new System.Drawing.Point(81, 1);
            this.portbox.Name = "portbox";
            this.portbox.Size = new System.Drawing.Size(100, 21);
            this.portbox.TabIndex = 1;
            this.portbox.TextChanged += new System.EventHandler(this.portbox_TextChanged);
            this.portbox.KeyPress += new KeyPressEventHandler(this.portbox_KeyPress);
            //
            // PlaceKupo
            //
            this.Controls.Add(this.portbox);
            this.Controls.Add(this.label1);
            this.Name = "PlaceKupo";
            this.Size = new System.Drawing.Size(470, 295);
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
                    ActGlobals.oFormActMain.BeforeLogLineRead -= area.PlaceFunction;
                    area = null;
                }
                if (map.ContainsKey(id))
                {
                    area = map[id]();
                    ActGlobals.oFormActMain.BeforeLogLineRead += area.PlaceFunction;
                }
            }
            catch (Exception e)
            {
                SendCommand("/e " + e);
            }
        }

        private void portbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && e.KeyChar != '\b') e.KeyChar = (char)0;
        }

        private void portbox_TextChanged(object sender, EventArgs e)
        {
            namazu = String.Format("http://localhost:{0}/", portbox.Text);
        }

        private void SaveSettings()
        {
            FileStream fs = new FileStream(settingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            XmlTextWriter xWriter = new XmlTextWriter(fs, System.Text.Encoding.UTF8);
            xWriter.Formatting = Formatting.Indented;
            xWriter.Indentation = 1;
            xWriter.IndentChar = '\t';
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

        #endregion ����
    }
}