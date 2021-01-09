using Advanced_Combat_Tracker;
using System.IO;
using System.Text.RegularExpressions;

namespace PlaceKupo.Areas
{
    public class ExampleAction : IPlaceFunc
    {
        void IPlaceFunc.AddDelegates()
        {
            ActGlobals.oFormActMain.BeforeLogLineRead += HelloWorld;
        }

        /// <summary>
        /// 在盛夏农庄对木人做出动作后会在聊天栏默语Hello World!
        /// </summary>
        /// <param name="isImport"></param>
        /// <param name="logInfo"></param>
        void HelloWorld(bool isImport, LogLineEventArgs logInfo)
        {
            string log = logInfo.logLine;//ACT日志行
            if (Regex.IsMatch(log, "^.{14} 00:001d:.*?木人"))
            {
                PlaceKupo.SendCommand("/e Hello World!");
                PlaceKupo.Log("Hello World!");
            }
        }

        void IPlaceFunc.RemoveDelegates()
        {
            ActGlobals.oFormActMain.BeforeLogLineRead -= HelloWorld;
        }
    }
}
