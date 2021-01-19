# 库啵标点工具

[![GitHub last commit](https://img.shields.io/github/last-commit/toolsmax0/PlaceKupo)](https://github.com/toolsmax0/PlaceKupo)

库啵标点工具是一个更底层的Triggernometry的替代品，旨在解决高级触发器面对较复杂计算时程序繁琐不够灵活的问题。
此外本工具还内置了标点和发送指令功能，不需要依赖其他插件。

以人偶军事基地为例，905P使用辅助机程序时可以通过计算判断出安全点并标记。

![GIF 2021-1-18 22-40-49.gif](https://i.loli.net/2021/01/18/wul5v9tRy4ihz2E.gif)

四个大锤的情况是没有标点的，因为我估计大概没人需要（

其实这个全部是激光的情况也没必要标点，不过因为看起来很炫酷我就留下来了

![GIF 2021-1-18 22-42-30.gif](https://i.loli.net/2021/01/18/5G48NWIu3rnYCoi.gif)

激光判定之后标点会自动消失，换言之，如果标点消失了你还没跑到安全点，那么
![を前はも死んだ](https://i.loli.net/2021/01/20/ek4coRnDaryHImz.jpg)

我们再来看看第二次和第三次激光：

![GIF 2021-1-18 22-43-43.gif](https://i.loli.net/2021/01/18/cmMfugowihC6LKz.gif)
![GIF 2021-1-18 22-44-24.gif](https://i.loli.net/2021/01/18/gmvPoWNF2pRKO1B.gif)
![GIF 2021-1-18 22-44-59.gif](https://i.loli.net/2021/01/18/3AZ5Y9ozNSWRvr4.gif)
![GIF 2021-1-18 22-46-14.gif](https://i.loli.net/2021/01/18/IbEnfaFdgs1KZBu.gif)

虽然比起来手动判断可能会慢一点，但是足够跑到安全点了，可以救急用。

不过你可能会想，这个机制也不算太难啊，有必要为了它专门写个轮椅吗？
你说得对，上面不过是一个实验品，真正的重头戏还在下面！为什么呢？
曾经在艾欧泽亚有一位绝*战士，他在5.3版本刚更新的夜里终于进入了尼尔2本。那么这位四十分钟之后就因为拉错了boss位置带着20个人吃了两层激光从而“名留青史”的传奇坦克究竟是谁呢？~~そう、私です！~~

说到这里想必大家已经可以理解这个轮椅的重要之处了。在5.3更新了两个月之后我终于把它写完了。让我们来看看演示。

![GIF 2021-1-18 22-47-00.gif](https://i.loli.net/2021/01/18/vQLOegqUNGPnFXC.gif)

在Boss读条强制传送的瞬间就可以判断出安全点，~~龙骑用了再也放不出LB~~

![GIF 2021-1-18 22-47-36.gif](https://i.loli.net/2021/01/18/6mXqFcOhsWArIZa.gif)

那么说了这么多，这个插件要如何使用呢？

## 下载和安装

[![GitHub all releases](https://img.shields.io/github/downloads/toolsmax0/PlaceKupo/total)](https://github.com/toolsmax0/PlaceKupo/releases)

下载之后在ACT插件列表中添加即可。~~注意该插件必须在游戏启动后运行。~~ 考虑到有的选手需要左手伐绝亚右手绝巴哈，我们特地添加了进程转换功能，再也不用担心双线程打本的时候只有一边有轮椅啦！顺便一提，有了这个功能之后就不需要先打开游戏再加载插件了。

## 开发

目前该插件只适配了一个副本。不过GitHub页面上有简单的开发指南，源码里也有注释，欢迎各路神仙为这个插件添砖加瓦。
有问题也可以直接回复/私信/发issue/托梦。

## 鸣谢

感谢@PrototypeSeiren [![GitHub followers](https://img.shields.io/github/followers/PrototypeSeiren?style=social)](https://github.com/PrototypeSeiren)
提供的莫迪翁传送器以及各位好兄弟的测试反馈。

## 安利

欢迎加入莫迪翁狱友会:868116069
