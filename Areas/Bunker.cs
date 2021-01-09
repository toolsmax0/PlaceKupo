using Advanced_Combat_Tracker;
using PlaceKupo.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PlaceKupo.Areas
{
    internal class Bunker : IPlaceFunc
    {
        internal class Pod
        {
            public Point Loc;
            public uint Id;
            public bool isLaser;
            public bool isActive;
        }

        private readonly Dictionary<uint, int> map;
        private readonly Pod[] pods;
        private readonly Point center = new Point(200, -500, -100);
        private int count;

        public Bunker()
        {
            map = new Dictionary<uint, int>();
            pods = new Pod[10];
        }

        private int GetLoc(Pod x)
        {
            float angle = (float)(Point.GetAngle(center, x.Loc) * 4 / Math.PI);
            int a = (int)Math.Round(angle, 0, 0);
            if (a < 0) a += 8;
            return a;
        }

        //void Countdown(bool _, LogLineEventArgs logInfo)
        //{
        //    string log = logInfo.logLine;
        //    if (Regex.IsMatch(log, ".{14} 00:0044:905P：装备重型陆战装置[:：]安装通信官21O自我数据！ 强行连接自卫系统！"))
        //    {
        //        System.Threading.Thread.Sleep(2000);
        //        PlaceKupo.SendCommand("/e countdown 30");
        //    }
        //}

        private void OnCombatantAdded(bool _, LogLineEventArgs logInfo)
        {
            try
            {
                string log = logInfo.logLine;
                Match m = Regex.Match(log, @"^.{14} 03:(.*?):Added new combatant 辅助机.*Pos: \((.*?),(.*?),(.*?)\)");
                if (m.Success)
                {
                    Pod pod = new Pod
                    {
                        Id = Convert.ToUInt32(m.Groups[1].Value, 16)
                    };
                    float x = float.Parse(m.Groups[2].Value);
                    float y = float.Parse(m.Groups[3].Value);
                    float z = float.Parse(m.Groups[4].Value);
                    pod.Loc = new Point(x, z, y);
                    pod.isActive = false;
                    int index = GetLoc(pod);
                    pods[index] = pod;
                    map.Add(pod.Id, index);
                    PlaceKupo.Log("已添加辅助机: " + pod.Id.ToString("X") + ", 位置: " + pod.Loc.ToString() + ", 编号: " + index);
                }
            }
            catch (Exception e)
            {
                PlaceKupo.Log(e.ToString());
            }
        }

        private void OnCombatantRemoved(bool _, LogLineEventArgs logInfo)
        {
            try
            {
                string log = logInfo.logLine;
                Match m = Regex.Match(log, @"^.{14} 04:(.*?):Removing combatant 辅助机");
                if (m.Success)
                {
                    uint id = Convert.ToUInt32(m.Groups[1].Value, 16);
                    if (map.ContainsKey(id))
                    {
                        int index = map[id];
                        pods[index] = null;
                        map.Remove(id);
                    }
                    PlaceKupo.Log("已移除辅助机: " + id.ToString("X") + '。');
                }
            }
            catch (Exception e)
            {
                PlaceKupo.Log(e.ToString());
            }
        }

        private void OnCastStarted(uint _, int type, string log)
        {
            try
            {
                if (type != 20) return;//20
                Match m = Regex.Match(log, @"^([^\|]*)\|辅助机\|([^\|]*)\|");
                if (m.Success)
                {
                    bool isLaser = m.Groups[2].Value == "4FF0";
                    uint id = Convert.ToUInt32(m.Groups[1].Value, 16);
                    if (map.ContainsKey(id))
                    {
                        int index = map[id];
                        Pod pod = pods[index];
                        pod.isActive = true;
                        pod.isLaser = isLaser;
                        count++;
                        PlaceKupo.Log("辅助机" + id.ToString("X") + "开始咏唱" + (isLaser ? "激光。" : "重锤。"));
                    }
                    if (count == 8)
                    {
                        Waymark[] set = new Waymark[8];
                        int cnt = 0;
                        for (int i = 0; i < 8; i++)
                        {
                            var x = pods[i];
                            var y = pods[(i + 1) % 8];
                            if (x.isActive && y.isActive && x.isLaser && y.isLaser)
                            {
                                Point mid = Point.Midpoint(x.Loc, y.Loc);
                                set[cnt++] = (new Waymark(mid, 0, true));
                                PlaceKupo.Log("已标记安全点: " + mid.ToString());
                            }
                        }
                        PlaceKupo.WriteWaymark(set);
                    }
                }
            }
            catch (Exception e)
            {
                PlaceKupo.Log(e.ToString());
            }
        }

        private void OnCastEnded(bool _, LogLineEventArgs logInfo)
        {
            try
            {
                string log = logInfo.logLine;
                Match m = Regex.Match(log, @"^.{14} 1[56]:(.*?):辅助机");
                if (m.Success)
                {
                    uint id = Convert.ToUInt32(m.Groups[1].Value, 16);
                    if (map.ContainsKey(id))
                    {
                        int index = map[id];
                        Pod pod = pods[index];
                        if (pod.isActive)
                        {
                            pod.isActive = false;
                            count--;
                            PlaceKupo.Log("辅助机" + id.ToString("X") + "已施放" + (pod.isLaser ? "激光。" : "重锤。"));
                        }
                    }
                    if (count == 0)
                    {
                        PlaceKupo.WriteWaymark(Preset.Reset);
                        PlaceKupo.Log("场地标点已重置。");
                    }
                }
            }
            catch (Exception e)
            {
                PlaceKupo.Log(e.ToString());
            }
        }

        private void DelegateManager(bool _, LogLineEventArgs logInfo)
        {
            try
            {
                string log = logInfo.logLine;
                if (Regex.IsMatch(log, "^.{14} 00:0044:21O：自我数据:迎击系统启动。 使用激光炮塔消灭敌对生命体。"))
                {
                    count = 0;
                    ActGlobals.oFormActMain.OnLogLineRead += OnCombatantAdded;
                    ActGlobals.oFormActMain.OnLogLineRead += OnCombatantRemoved;
                    PlaceKupo.subscription.ParsedLogLine += OnCastStarted;
                    ActGlobals.oFormActMain.OnLogLineRead += OnCastEnded;
                    PlaceKupo.Log("905P组件加载完成。");
                }
                if (Regex.IsMatch(log, "^.{14} 00:0044:905P：装备重型陆战装置:原来……是这种结局吗……"))
                {
                    count = 0;
                    ActGlobals.oFormActMain.OnLogLineRead -= OnCombatantAdded;
                    ActGlobals.oFormActMain.OnLogLineRead -= OnCombatantRemoved;
                    PlaceKupo.subscription.ParsedLogLine -= OnCastStarted;
                    ActGlobals.oFormActMain.OnLogLineRead -= OnCastEnded;
                    PlaceKupo.Log("905P组件移除完成。");
                }
            }
            catch (Exception e)
            {
                PlaceKupo.Log(e.ToString());
            }
        }

        void IPlaceFunc.AddDelegates()
        {
            ActGlobals.oFormActMain.OnLogLineRead += DelegateManager;
            PlaceKupo.Log("人偶军事基地组件加载完成。");
        }

        void IPlaceFunc.RemoveDelegates()
        {
            ActGlobals.oFormActMain.OnLogLineRead -= DelegateManager;
            ActGlobals.oFormActMain.OnLogLineRead -= OnCombatantAdded;
            ActGlobals.oFormActMain.OnLogLineRead -= OnCombatantRemoved;
            PlaceKupo.subscription.ParsedLogLine -= OnCastStarted;
            ActGlobals.oFormActMain.OnLogLineRead -= OnCastEnded;
            PlaceKupo.Log("人偶军事基地组件移除完成。");
        }
    }
}