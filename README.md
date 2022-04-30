# 战地1服务器管理工具

一个方便服主和管理员管理服务器的工具  
该版本是新版服管工具，增加了功能提高稳定性

- `BF1.ServerAdminTools.ASP`:web服务器接口（不在计划中）
- `BF1.ServerAdminTools.Common`:工具核心，抓取游戏内存
- `BF1.ServerAdminTools.GameImage`:判断游戏窗口状态，进入服务器
- `BF1.ServerAdminTools.Netty`:工具Netty接口服务器
- `BF1.ServerAdminTools.SDK`:一个C#的简单netty接口客户端
- `BF1.ServerAdminTools.Test`:测试项目
- `BF1.ServerAdminTools.Wpf`:主界面

## 更新日志
### 2022.4.30
- 修复崩溃问题
- 修复切图不生效问题
- 修复白名单问题
- 优化发送文本

### 2022.4.29
- 修复单边规则
- 优化进程钩子
- 调整UI

### 2022.4.28
- 修复bug提升稳定性
- 修复程序无法正常退出问题
- 增加队伍规则

### 2022.4.27
- 修复地图规则强制生效问题
- 修复配置文件读取崩溃
- 修复黑名单订阅获取失败清空数据问题
- 修复强制保存规则问题
- 完善netty接口
- 新增白名单控制选项
- 新增1024x768分辨率支持

### 2022.4.26
- 修复多个`bf1.exe`问题
- 新增地图规则
- 新增黑名单订阅

### 2022.4.23
- 修复生涯问题
- 修改逻辑
- 修改登录
- 修改GUI

### 2022.4.20
- 修复生涯问题
- 修复换图问题

### 2022.4.14
- 修复流量占用错误
- 增加netty接口
- 增加断线重连功能

### 2022.4.13
- 修复崩溃错误
- 修复数值转换错误

### 2022.4.12
- 修复劣势方规则不生效
- 增加劣势方分数达到一定程度才开始换图
- 调整换图方式策列

### 2022.4.11
- 修改外观样式
- 新增可更换背景
- 新增劣势方不同规则
- 修复踢出bug

### 2022.4.8
- 修复接口bug
- 新增对接[BotBattlefield](https://github.com/Coloryr/BotBattlefield)

### 2022.4.7
- 完成Netty接口
- 拆分核心

### 2022.4.3
- 修改版本号
- 修改界面样式
- 修改数据库性能
- 修改游戏钩子
- 提升软件性能
- 提升软件稳定性
- 增加预览版信息
- 增加多个规则设定
- 增加白名单黑名单玩家导入
- 重写踢人逻辑
- 重写跳边检测
- 重写配置文件数据结构
