using Advanced_Combat_Tracker;
using System.Windows.Forms;

namespace MyActPlugin
{
    public class PluginMain : IActPluginV1
    {
        /// <summary>
        /// 插件状态文本，显示在ACT插件列表中。
        /// </summary>
        private Label statusText;

        /// <summary>
        /// 插件TabPage控件，将插入ACT插件列表中。
        /// </summary>
        private TabPage pluginTab;

        /// <summary>
        /// 插件初始化代码，入口点。
        /// </summary>
        /// <param name="pluginScreenSpace">插件TabPage控件</param>
        /// <param name="pluginStatusText">插件状态文本</param>
        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            /// 初始化与传递控件给ACT
            statusText = new Label();
            pluginStatusText = statusText;
            pluginTab = new TabPage();
            pluginScreenSpace = pluginTab;
            /// Do Something

            statusText.Text = "插件工作中";

        }

        /// <summary>
        /// 插件反初始化代码
        /// </summary>
        public void DeInitPlugin()
        {
            
            statusText.Text = "插件已退出";
        }
    }
}
