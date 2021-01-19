# PlaceKupo

一个基于Zodiark/PaisleyPark的自动标点工具

## 支持副本

<details>

* 人偶军事基地

  * 905P激光安全点

  * 2P融合体传送激光安全点

</details>

## 开发

### 引用项

* `Advanced Combat Tracker.dll` 可在ACT根目录找到
* `FFXIV_ACT_Plugin.Common.dll` 可在[解析插件发布页](https://github.com/ravahn/FFXIV_ACT_Plugin/tree/master/Releases)下载SDK并解压后得到
* `Zodiark.Namazu.dll` [下载](https://github.com/PrototypeSeiren/Zodiark)并手动编译  
  其余引用项可在Nuget中安装

### 模型

* `Zodiark.Namazu.Point` 三维坐标系上的一点
* `Zodiark.Namazu.Preset` 标点预设
* `Zodiark.Namazu.Waymark` 单个标点  
  详细信息可参考源码

### 实现

 1. 在 `PlaceKupo.Areas` 下新建类并实现 `IPlaceFunc` 接口
 2. 在 `PlaceKupo.map` 中添加类

### 事件

* `PlaceKupo.subscription.ParsedLogLine` 网络日志行
* `ActGlobals.oFormActMain.BeforeLogLineRead` ACT日志行

    其余可参阅 `FFXIV_ACT_Plugin.Common.dll` 以及 [ACT API文档](https://advancedcombattracker.com/apidoc/html/T_Advanced_Combat_Tracker_FormActMain.htm)

### 方法

* `PlaceKupo.Log(string)` 记录一条日志
* `PlaceKupo.SendCommand(string)` 向聊天框发送一条指令
* `PlaceKupo.TTS(string)` 文字转语音
* `PlaceKupo.WriteWaymark(Zodiark.Namazu.Preset)` 写入一套标点预设
* `PlaceKupo.WriteWaymark(Zodiark.Namazu.Waymark, int)` 写入单个标点
* `PlaceKupo.ReadWaymark()`读取游戏内标点
