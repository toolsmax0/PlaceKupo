using Advanced_Combat_Tracker;
using FFXIV_ACT_Plugin.Common.Models;
using System.Linq;
using System.Text.RegularExpressions;

namespace PlaceKupo.Areas
{
    public class ExampleAction : IPlaceFunc
    {
        private Combatant cmb;

        void IPlaceFunc.AddDelegates()
        {
            ActGlobals.oFormActMain.BeforeLogLineRead += HelloWorld;
        }

        /// <summary>
        /// 在盛夏农庄对木人做出动作后会在聊天栏默语Hello World!
        /// </summary>
        /// <param name="isImport"></param>
        /// <param name="logInfo"></param>
        private void HelloWorld(bool isImport, LogLineEventArgs logInfo)
        {
            string log = logInfo.logLine;//ACT日志行
            if (Regex.IsMatch(log, "^.{14} 00:001d:.*?木人"))
            {
                if (cmb == null)
                {
                    var list = PlaceKupo.repository.GetCombatantList();
                    cmb = list.FirstOrDefault(cmb => cmb.ID == PlaceKupo.repository.GetCurrentPlayerID());
                }
                PlaceKupo.SendCommand("/e Hello World!");
            }
#if DEBUG
            if (Regex.IsMatch(log, "testbar"))
            {
                PlaceKupo.Log(log);
            }
#endif
        }

        void IPlaceFunc.RemoveDelegates()
        {
            ActGlobals.oFormActMain.BeforeLogLineRead -= HelloWorld;
        }
    }
}