using Advanced_Combat_Tracker;
using FFXIV_ACT_Plugin.Common;
using PlaceKupo.Areas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Zodiark.Namazu;

[assembly: AssemblyTitle("��ࣱ��")]
[assembly: AssemblyDescription("�ڳ����ϱ�����ȫ�����ʾ")]
[assembly: AssemblyVersion("1.1.2.0")]

namespace PlaceKupo
{
    internal interface IPlaceFunc
    {
        void AddDelegates();

        void RemoveDelegates();
    }

    public class PlaceKupo : UserControl, IActPluginV1
    {
        #region ��������

        private Label statusLabel;

        //private static Offsets Offsets;
        private IPlaceFunc area;

        public static ListBox Logger;
        private Panel panel2;
        public ListBox LogList;
        public static Regex Wipe = new Regex(@"^.{14} 21:.{8}:4000001[026]", RegexOptions.Compiled);
        private static Namazu Namazu;
        private Button CopyAll;
        private Button ClearAll;

        #endregion ��������

        /// <summary>
        /// �����¼�
        /// </summary>
        public static IDataSubscription subscription = Namazu.subscription;

        /// <summary>
        /// ��ȡ�����Ϣ��
        /// </summary>
        public static IDataRepository repository = Namazu.repository;

        /// <summary>
        /// �洢����Ͷ�Ӧ�ı�㷽��
        /// </summary>
        private Dictionary<uint, Func<IPlaceFunc>> map;

        public void InitMap()
        {
            map = new Dictionary<uint, Func<IPlaceFunc>>
            {
                //��������PlaceKupo.Areas��ʵ��
                //Ҫ����µķ���,�밴�� {����ID,() => new ����()} �Ĺ淶���
                //����ID���ڴ˵�ַ��ѯ: https://github.com/quisquous/cactbot/blob/main/resources/zone_info.js
                //�����ڰ�װ�˲�����л�����ʱҲ�����������ʾ����ID
                //�Զ�������ʵ��IPlaceFunc�ӿ�
                //��:ʢ��ũׯ
                { 134, () => new ExampleAction() },
#if DEBUG
                { 928, () => new Bunker() },
#endif
                { 917, () => new Bunker() }
            };
        }

        /// <summary>
        /// �����㾫����ָ��
        /// </summary>
        /// <param name="s">һ����Ϸ��ָ��</param>
        public static void SendCommand(string s)
        {
            try
            {
                Namazu.SendCommand(s);
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }

        /// <summary>
        /// ����Ϸ��д��������
        /// </summary>
        /// <param name="preset">һ�ױ��</param>
        public static void WriteWaymark(Preset preset)
        {
            try
            {
                Namazu.WriteWaymark(preset);
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }

        /// <summary>
        /// ��¼һ����־��
        /// </summary>
        /// <param name="s"></param>
        public static void Log(string s)
        {
            Logger.Items.Add(String.Format("[{0:HH:mm:ss}] ", DateTime.Now) + s);
            Logger.SelectedIndex = Logger.Items.Count - 1;
            Logger.SelectedIndex = -1;
        }

        /// <summary>
        /// ʹ��TTS�ʶ�һ�����֡�
        /// </summary>
        /// <param name="s"></param>
        public static void TTS(string s)
        {
            ActGlobals.oFormActMain.TTS(s);
            Log("TTS: " + s);
        }

        /// <summary>
        /// д�뵥�����
        /// </summary>
        /// <param name="waymark">�������</param>
        public static void WriteWaymark(Waymark waymark, int id = -1)
        {
            try
            {
                Namazu.WriteWaymark(waymark, id);
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }

        /// <summary>
        /// ��ȡ��Ϸ�ڵı��
        /// </summary>
        /// <returns>��Ϸ�ڵı��Ԥ��</returns>
        public static Preset ReadWaymark()
        {
            try
            {
                return Namazu.ReadWaymark();
            }
            catch (Exception e)
            {
                Log(e.ToString());
                return Preset.Reset;
            }
        }

        #region ����

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
            statusLabel.Text = "������˳�";
        }

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            pluginScreenSpace.Text = "��ࣱ�㹤��";
            InitMap();
            pluginScreenSpace.Controls.Add(this);
            subscription.ProcessChanged += OnProcessChanged;
            subscription.ZoneChanged += OnZoneChanged;
            statusLabel = pluginStatusText;
            statusLabel.Text = "Working :D";
            Log("��ࣱ��������");
            uint id = repository.GetCurrentTerritoryID();
            Log("��ǰ����ID: " + id.ToString());
            if (map.ContainsKey(id))
            {
                Namazu = Namazu.Instance;
                area = map[id]();
                area.AddDelegates();
            }
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlaceKupo));
            this.panel2 = new System.Windows.Forms.Panel();
            this.LogList = new System.Windows.Forms.ListBox();
            this.CopyAll = new System.Windows.Forms.Button();
            this.ClearAll = new System.Windows.Forms.Button();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
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
            // CopyAll
            //
            resources.ApplyResources(this.CopyAll, "CopyAll");
            this.CopyAll.Name = "CopyAll";
            this.CopyAll.UseVisualStyleBackColor = true;
            this.CopyAll.Click += new System.EventHandler(this.CopyAll_Click);
            //
            // ClearAll
            //
            resources.ApplyResources(this.ClearAll, "ClearAll");
            this.ClearAll.Name = "ClearAll";
            this.ClearAll.UseVisualStyleBackColor = true;
            this.ClearAll.Click += new System.EventHandler(this.ClearAll_Click);
            //
            // PlaceKupo
            //
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.ClearAll);
            this.Controls.Add(this.CopyAll);
            this.Controls.Add(this.panel2);
            this.Name = "PlaceKupo";
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
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

        private void OnProcessChanged(Process _)
        {
            Namazu = Namazu.Instance;
        }

        private void Portbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && e.KeyChar != '\b') e.KeyChar = (char)0;
        }

        private void Portbox_TextChanged(object sender, EventArgs e)
        {
        }

        private void LogList_DoubleClick(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(LogList.SelectedItem);
        }

        private void CopyAll_Click(object sender, EventArgs e)
        {
            var list = new List<object>();
            foreach (var i in LogList.Items)
            {
                list.Add(i);
            }
            Clipboard.SetDataObject(string.Join("\n", list));
        }

        private void ClearAll_Click(object sender, EventArgs e)
        {
            LogList.Items.Clear();
        }

        #endregion ����
    }
}