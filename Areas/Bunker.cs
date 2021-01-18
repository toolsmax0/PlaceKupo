using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Zodiark.Namazu;

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

        private enum States { Idle, In905P, In2P };

        private States state;
        private readonly Dictionary<uint, int> map;
        private Pod[] pods;
        private readonly Point center = new Point(200, -500, -100);
        private int count;
        private readonly object lock1 = new object();
        private readonly object lock2 = new object();
        private readonly LogLineEventDelegate R010;
        private readonly LogLineEventDelegate R011;
        private readonly StreamWriter sw;
        private int[] Xnum;
        private int[] Znum;
        private readonly float[] Xenum = { 177.5F, 192.5F, 207.5F, 222.5F };
        private readonly float[] Zenum = { -722.5F, -707.5F, -692.5F, -677.5F };
        private System.Collections.ObjectModel.ReadOnlyCollection<FFXIV_ACT_Plugin.Common.Models.Combatant> list;
#if DEBUG
        private Regex On905P = new Regex(@".{14} 00:0044:21O：自我数据:迎击系统启动。 使用激光炮塔消灭敌对生命体。", RegexOptions.Compiled);
        private Regex On905PDead = new Regex(".{14} 00:0044:905P：装备重型陆战装置:原来……是这种结局吗……", RegexOptions.Compiled);
        private Regex On2P = new Regex(".{14} 00:0044:2P：融合体:辅助机！ 执行激光程序！", RegexOptions.Compiled);
        private Regex OnAdded = new Regex(@".{14} 03:(.*?):Added new combatant 辅助机.*Pos: \((.*?),(.*?),(.*?)\)", RegexOptions.Compiled);
        private Regex OnRemoved = new Regex(@".{14} 04:(.*?):Removing combatant 辅助机", RegexOptions.Compiled);
        private Regex OnCast1 = new Regex(@"([^❘]*)❘辅助机❘([^❘]*)❘", RegexOptions.Compiled);
        private Regex OnCast2 = new Regex(@".{14} 1[56]:(.*?):辅助机", RegexOptions.Compiled);
        private Regex OnAdded2 = new Regex(@".{14} 03:[^:]*:Added new combatant 辅助机：融合体.*?Level: 80.*?Pos: \((.*?),(.*?),(.*?)\)", RegexOptions.Compiled);
        private Regex OnTeleport = new Regex(@".{14} 23:([^:]*):[^:]*:([^:]*):[^:]*:E000:0000:0074", RegexOptions.Compiled);

#else
        private Regex On905P = new Regex(@"^.{14} 00:0044:21O：自我数据:迎击系统启动。 使用激光炮塔消灭敌对生命体。", RegexOptions.Compiled);
        private Regex On905PDead = new Regex("^.{14} 00:0044:905P：装备重型陆战装置:原来……是这种结局吗……", RegexOptions.Compiled);
        private Regex On2P = new Regex("^.{14} 00:0044:2P：融合体:辅助机！ 执行激光程序！", RegexOptions.Compiled);
        private Regex OnAdded = new Regex(@"^.{14} 03:(.*?):Added new combatant 辅助机.*Pos: \((.*?),(.*?),(.*?)\)", RegexOptions.Compiled);
        private Regex OnRemoved = new Regex(@"^.{14} 04:(.*?):Removing combatant 辅助机", RegexOptions.Compiled);
        private Regex OnCast1 = new Regex(@"^([^\|]*)\|辅助机\|([^\|]*)\|", RegexOptions.Compiled);
        private Regex OnCast2 = new Regex(@"^.{14} 1[56]:(.*?):辅助机", RegexOptions.Compiled);
        private Regex OnAdded2 = new Regex(@"^.{14} 03:[^:]*:Added new combatant 辅助机：融合体.*?Level: 80.*?Pos: \((.*?),(.*?),(.*?)\)", RegexOptions.Compiled);
        private Regex OnTeleport = new Regex(@"^.{14} 23:([^:]*):[^:]*:([^:]*):[^:]*:E000:0000:0074", RegexOptions.Compiled);
#endif

        private readonly Dictionary<float, int> Xpos;

        private readonly Dictionary<float, int> Zpos;

        private int teleport;

        public Bunker()
        {
            map = new Dictionary<uint, int>();
            state = States.Idle;
            pods = new Pod[10];
            Xnum = new int[4];
            Znum = new int[4];
            sw = new StreamWriter("pos.log", true);

            Xpos = new Dictionary<float, int>
            {
                {177.5F,0},
                {192.5F,1},
                {207.5F,2},
                {222.5F,3}
            };
            Zpos = new Dictionary<float, int>
            {
                {-722.5F,0},
                {-707.5F,1},
                {-692.5F,2},
                {-677.5F,3}
            };
            R010 = new LogLineEventDelegate(OnCombatantAdded);
            R010 += OnCombatantRemoved;
            R010 += OnCastEnded;
            R011 = new LogLineEventDelegate(OnCombatantAdded2);
            R011 += OnTeleportCasted;
        }

        private int GetLoc(Pod x)
        {
            float angle = (float)(Point.GetAngle(center, x.Loc) * 4 / Math.PI);
            int a = (int)Math.Round(angle, 0, 0);
            if (a < 0) a += 8;
            return a;
        }

        private void MarkExistence(float x, float z, int mark)
        {
            if (x == 164.9F || x == 234.9F)
            {
                if (Zpos.ContainsKey(z))
                {
                    Znum[Zpos[z]] += mark;
                }
                else
                {
                    PlaceKupo.Log("Error: Invalid Position of Pod: z: " + z.ToString());
                }
            }
            if (z == -665F || z == -735F)
            {
                if (Xpos.ContainsKey(x))
                {
                    Xnum[Xpos[x]] += mark;
                }
                else
                {
                    PlaceKupo.Log("Error: Invalid Position of Pod: x: " + x.ToString());
                }
            }
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
            if (state != States.In905P) return;
            string log = logInfo.logLine;
            Match m = OnAdded.Match(log);
            if (m.Success)
            {
                Pod pod = new Pod
                {
                    Id = Convert.ToUInt32(m.Groups[1].Value, 16)
                };
                float x = float.Parse(m.Groups[2].Value);
                float y = float.Parse(m.Groups[4].Value);
                float z = float.Parse(m.Groups[3].Value);
                pod.Loc = new Point(x, y, z);
                pod.isActive = false;
                int index = GetLoc(pod);
                pods[index] = pod;
                map.Add(pod.Id, index);
                PlaceKupo.Log("已添加辅助机: " + pod.Id.ToString("X") + ", 位置: " + pod.Loc.ToString() + ", 编号: " + index);
            }
        }

        private void OnCombatantRemoved(bool _, LogLineEventArgs logInfo)
        {
            if (state != States.In905P) return;
            string log = logInfo.logLine;
            Match m = OnRemoved.Match(log);
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

        private void OnCastStarted(uint _, int type, string log)
        {
            if (state != States.In905P) return;
            if (type != 20) return;//20
            Match m = OnCast1.Match(log);
            if (m.Success)
            {
                lock (lock1)
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
                        int cnt = 0;
                        for (int i = 0; i < 8; i++)
                        {
                            var x = pods[i];
                            var y = pods[(i + 1) % 8];
                            if (x.isActive && y.isActive && x.isLaser && y.isLaser)
                            {
                                Point mid = Point.Midpoint(x.Loc, y.Loc);
                                PlaceKupo.WriteWaymark(new Waymark(mid, 0, true), cnt++);
                                PlaceKupo.Log("已标记安全点: " + mid.ToString());
                            }
                        }
                    }
                }
            }
        }

        private void OnCastEnded(bool _, LogLineEventArgs logInfo)
        {
            if (state != States.In905P) return;
            string log = logInfo.logLine;
            Match m = OnCast2.Match(log);
            if (m.Success)
            {
                lock (lock1)
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
        }

        private void OnCombatantAdded2(bool _, LogLineEventArgs logInfo)
        {
            if (state != States.In2P) return;
            string log = logInfo.logLine;
            Match m = OnAdded2.Match(log);
            if (m.Success)
            {
                float x = float.Parse(m.Groups[1].Value);
                float z = float.Parse(m.Groups[2].Value);
                MarkExistence(x, z, 1);
                PlaceKupo.Log(string.Format("已添加辅助机: ({0},-480,{1})", x, z));
            }
        }

        private void OnTeleportCasted(bool _, LogLineEventArgs logInfo)
        {
            if (state != States.In2P) return;
            string log = logInfo.logLine;
            Match m = OnTeleport.Match(log);
            if (m.Success)
            {
                lock (lock2)
                {
                    uint id1 = Convert.ToUInt32(m.Groups[1].Value, 16);
                    uint id2 = Convert.ToUInt32(m.Groups[2].Value, 16);
                    if (list == null)
                    {
                        list = PlaceKupo.repository.GetCombatantList();
                    }
                    var cmb = list.FirstOrDefault(x => x.ID == id1);
                    if (cmb != null)
                    {
                        MarkExistence(cmb.PosX, cmb.PosY, 1);
                        PlaceKupo.Log(string.Format("已添加 {3}({4}): ({0},{1},{2})", cmb.PosX.ToString(), cmb.PosZ.ToString(), cmb.PosY.ToString(), cmb.Name, cmb.ID.ToString("X")));
                    }
                    cmb = list.FirstOrDefault(x => x.ID == id2);
                    if (cmb != null)
                    {
                        MarkExistence(cmb.PosX, cmb.PosY, -1);
                        PlaceKupo.Log(string.Format("已移除 {3}({4}): ({0},{1},{2})", cmb.PosX.ToString(), cmb.PosZ.ToString(), cmb.PosY.ToString(), cmb.Name, cmb.ID.ToString("X")));
                    }
                    teleport++;
                    if (teleport == 2)
                    {
                        int cnt = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            if (Xnum[i] == 0)
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    if (Znum[j] == 0)
                                    {
                                        PlaceKupo.WriteWaymark(new Waymark(Xenum[i], -480F, Zenum[j], 0, true), cnt++);
                                        PlaceKupo.Log(string.Format("已标记安全点: ({0},-480,{1})。", Xenum[i], Zenum[j]));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DelegateManager(bool _, LogLineEventArgs logInfo)
        {
            string log = logInfo.logLine;
            if (PlaceKupo.Wipe.IsMatch(log))
            {
                state = States.Idle;
                map.Clear();
                pods = new Pod[10];
                Xnum = new int[4];
                Znum = new int[4];
                Zpos.Clear();
                Xpos.Clear();
                count = 0;
                teleport = 0;
                list = null;
                return;
            }
            if (On905P.IsMatch(log))
            {
                count = 0;
                state = States.In905P;
                PlaceKupo.Log("905P组件加载完成。");
                return;
            }
            if (On905PDead.IsMatch(log))
            {
                count = 0;
                state = States.Idle;
                PlaceKupo.Log("905P组件移除完成。");
                return;
            }
            if (On2P.IsMatch(log))
            {
                teleport = 0;
                state = States.In2P;
                PlaceKupo.Log("2P融合体组件加载完成。");
                return;
            }
        }

        void IPlaceFunc.AddDelegates()
        {
            ActGlobals.oFormActMain.OnLogLineRead += DelegateManager;
            ActGlobals.oFormActMain.OnLogLineRead += R010;
            ActGlobals.oFormActMain.OnLogLineRead += R011;
            PlaceKupo.subscription.ParsedLogLine += OnCastStarted;
            PlaceKupo.Log("人偶军事基地组件加载完成。");
        }

        void IPlaceFunc.RemoveDelegates()
        {
            ActGlobals.oFormActMain.OnLogLineRead -= R010;
            ActGlobals.oFormActMain.OnLogLineRead -= R011;
            PlaceKupo.subscription.ParsedLogLine -= OnCastStarted;
            ActGlobals.oFormActMain.OnLogLineRead -= DelegateManager;
            PlaceKupo.Log("人偶军事基地组件移除完成。");
            sw.Close();
        }
    }
}