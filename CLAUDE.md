# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 构建与运行

```bash
# 构建整个解决方案
dotnet build DoipSimulator.slnx

# 单独构建各项目
dotnet build DoipServer/DoipServer.csproj
dotnet build DoipSimulator/DoipSimulator.csproj

# 运行模拟器 UI
dotnet run --project DoipSimulator/DoipSimulator.csproj
```

项目无测试工程，无 lint 配置。

## 架构

两个项目，单向依赖：**DoipSimulator (WinForms UI)** → **DoipServer (类库)**。

### DoipServer 类库 (`DOIPUtils` 命名空间)

- **`DOIP`** — 对外 Facade，暴露 `StartDoipServer()` / `StopDoipServer()` / `SetEthernetData()` 三个静态方法。内嵌 `DOIP.Information` 配置类（IP、VIN、MAC、端口）。
- **`DoIPServer`** (internal sealed) — 核心引擎，启动两条后台 `Thread`：
  - **UDP 线程**：监听广播，响应 `0x0011` 车辆发现请求，返回诊断地址 + MAC + VIN
  - **TCP 线程**：接收诊断仪连接，读 6 字节 DoIP 头 + payload，匹配数据文件中的 Req/Ans 对返回响应
  - 使用 `CancellationTokenSource` 优雅退出，`finally` 块确保 Socket 清理
- **`EthernetDataParser`** — 解析 `Req:` / `Ans:` 格式的文本数据文件，产出 `List<DataGroup>`
- **`LogHelper`** (internal static) — 文件日志，输出到 `log/log_yyyyMMddHHmmss.log`
- **`NetworkHelper`** (internal static) — 枚举本机活跃 IPv4 地址
- **`doipClient.cs`** — 已从编译排除（`.csproj` 中 `<Compile Remove>`），是一个完整 DoIP 客户端的参考实现

### DoipSimulator WinForms 应用

- **`Program.cs`** — 标准 `[STAThread]` 入口
- **`MainWindow`** — 主窗体：TreeView 浏览 `DataDB/` 目录，选择数据文件 → 配置 IP/VIN/MAC/端口 → 点击「连接」启动服务器。`AppendStatus()` 线程安全更新 RichTextBox。

### 协议要点

- **DoIP 头**：6 字节（4 字节 big-endian payload 长度 + 2 字节 payload type），非标准 ISO 13400-2 的 8 字节头
- **Payload 类型**：`0x0011` 车辆发现 | `0x0001` 诊断消息 | `0x0002` ACK | `0x0005` 路由激活
- **UDS 硬编码回退**：`0x22 0xF1 0x50`（读数据）和 `0x22 0xF1 0x90`（读 VIN），其余返回 `0x7F 0x22 0x11`

### 请求匹配优先级

收到 TCP 诊断请求后，**优先**在已加载的 `EthernetData` 列表中按 `SequenceEqual` 精确匹配 `RequestData`，匹配到则依次发送所有 `ResponseData`。文件数据相当于可配置的「剧本」，硬编码 UDS 处理在 `ProcessDiagnosticMessage()` 中作为回退（但当前 TCP 流程未调用该方法，仅数据文件匹配生效）。

### 运行时依赖

- `DataDB/` 目录需存在于可执行文件旁，内含 `Req:`/`Ans:` 格式的文本数据文件
- 默认端口：UDP 6811、TCP 6801
- 默认 VIN：`LBV8A9406GMF25307`
- 解决方案使用 `.slnx` 格式（需 VS 2022+ / MSBuild 17.4+）
