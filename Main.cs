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

[assembly: AssemblyTitle("库啵标点")]
[assembly: AssemblyDescription("在场地上标明安全点等提示")]
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
        #region 其它变量

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

        #endregion 其它变量

        /// <summary>
        /// 网络事件
        /// </summary>
        public static IDataSubscription subscription = Namazu.subscription;

        /// <summary>
        /// 获取玩家信息等
        /// </summary>
        public static IDataRepository repository = Namazu.repository;

        /// <summary>
        /// 存储区域和对应的标点方法
        /// </summary>
        private Dictionary<uint, Func<IPlaceFunc>> map;

        public void InitMap()
        {
            map = new Dictionary<uint, Func<IPlaceFunc>>
            {
                //方法请在PlaceKupo.Areas中实现
                //要添加新的方法,请按照 {区域ID,() => new 类名()} 的规范添加
                //区域ID可在此地址查询: https://github.com/quisquous/cactbot/blob/main/resources/zone_info.js
                //另外在安装此插件后切换区域时也会在聊天框显示区域ID
                //自定义类请实现IPlaceFunc接口
                //例:盛夏农庄
                { 134, () => new ExampleAction() },
#if DEBUG
                { 928, () => new Bunker() },
#endif
                { 917, () => new Bunker() }
            };
        }

        /// <summary>
        /// 向鲶鱼精发送指令
        /// </summary>
        /// <param name="s">一条游戏内指令</param>
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
        /// 向游戏内写入标点数据
        /// </summary>
        /// <param name="preset">一套标点</param>
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
            ActGlobals.oFormActMain.TTS(s);
            Log("TTS: " + s);
        }

        /// <summary>
        /// 写入单个标点
        /// </summary>
        /// <param name="waymark">标点数据</param>
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
        /// 读取游戏内的标点
        /// </summary>
        /// <returns>游戏内的标点预设</returns>
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

        #region 其它

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
            statusLabel.Text = "插件已退出";
        }

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            pluginScreenSpace.Text = "库啵标点工具";
            InitMap();
            pluginScreenSpace.Controls.Add(this);
            subscription.ProcessChanged += OnProcessChanged;
            subscription.ZoneChanged += OnZoneChanged;
            statusLabel = pluginStatusText;
            statusLabel.Text = "Working :D";
            Log("库啵标点已启动");
            uint id = repository.GetCurrentTerritoryID();
            Log("当前区域ID: " + id.ToString());
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

        #endregion 其它
    }
}