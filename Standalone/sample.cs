using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Advanced_Combat_Tracker;

[assembly: AssemblyTitle("ACT��־��ʵ�ù���")]
[assembly: AssemblyDescription("��ӡ����ACT��־������")]
[assembly: AssemblyVersion("1.0.1.1")]

namespace Logline_Utils
{
    public class Logline_Utils : IActPluginV1
    {
        Label statusLabel;
        StreamWriter sw;

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            pluginScreenSpace.Text = "ACT��־��ʵ�ù���";
            Label lbl = new Label();
            lbl.Location = new System.Drawing.Point(8, 8);
            lbl.AutoSize = true;
            lbl.Text = "ACT��־�����Զ������ ACT��Ŀ¼�� ��act_logline.log��";
            pluginScreenSpace.Controls.Add(lbl);
            sw = new StreamWriter("act_logline.log", true);
            ActGlobals.oFormActMain.OnLogLineRead += new LogLineEventDelegate(oFormActMain_OnLogLineRead);
            statusLabel = pluginStatusText;
            statusLabel.Text = "Working :D";
        }

        void oFormActMain_OnLogLineRead(bool isImport, LogLineEventArgs logInfo)
        {
            sw.Flush();
            sw.WriteLine(logInfo.originalLogLine);
        }

        public void DeInitPlugin()
        {
            ActGlobals.oFormActMain.OnLogLineRead -= oFormActMain_OnLogLineRead;
            sw.Close();
            statusLabel.Text = "������˳�";
        }
    }
}
