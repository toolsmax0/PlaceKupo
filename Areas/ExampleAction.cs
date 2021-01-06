using Advanced_Combat_Tracker;
using System.Text.RegularExpressions;

namespace PlaceKupo.Areas
{
    public class ExampleAction : IPlaceFunc
    {

        /// <summary>
        /// 在战斗中对木人做出动作后会在聊天栏默语Hello World!
        /// </summary>
        /// <param name="isImport"></param>
        /// <param name="logInfo"></param>
        void IPlaceFunc.PlaceFunction(bool isImport, LogLineEventArgs logInfo)
        {
            string log = logInfo.logLine;//ACT日志行
            if (Regex.IsMatch(log, "^.{14} 00:001d:.*?木人"))
                PlaceKupo.SendCommand("/e Hello World!");
        }
    }
}
