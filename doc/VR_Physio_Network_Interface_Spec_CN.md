# VR Physio-aware Feedback 网络接口说明（PC Python → VR / Unity）

## 1. 总体流程

本接口用于将 PC 端 Python 脚本计算得到的 leader 生理状态发送给 VR 端。整体流程为：

```text
Polar Verity Sense
    ↓ BLE
PC Python script
    ↓ PPI / HR filtering
    ↓ HR / HRV / baseline / trend inference
    ↓ UDP JSON message
VR / Unity receiver
    ↓ parse static_state + temporal_dynamics
    ↓ select / update physio-aware feedback interface
```

Python 端仍然负责：

- 连接 Polar Verity Sense；
- 接收 BLE PPI / HR 数据；
- 计算 HR、RMSSD、SDNN、PNN50；
- 计算 static state；
- 计算 temporal dynamics；
- 评估 signal quality；
- 通过 UDP JSON 把稳定状态发送给 VR。

VR 端不需要处理原始生理信号，只需要解析 JSON 中的状态字段。

---

## 2. 传输方式

推荐使用：

```text
Protocol: UDP
Encoding: UTF-8 JSON
Default IP: 127.0.0.1
Default Port: 5005
Send rate: 1 message / second
```

### 为什么使用 UDP

本实验中 VR 端只需要获得最新状态，不需要保证每一帧状态都可靠到达。UDP 的优点是：

- Unity / VR 端实现简单；
- 不会阻塞 Polar BLE 数据流；
- 丢失个别状态包影响较小，因为下一秒会发送新的状态；
- 适合实时状态广播。

如果 VR 和 Python 在同一台 PC 上运行，使用：

```text
VR_TARGET_IP = "127.0.0.1"
VR_TARGET_PORT = 5005
```

如果 VR 运行在另一台电脑或独立头显上，需要把 `VR_TARGET_IP` 改成 VR 接收端所在设备的局域网 IP。

---

## 3. Python 端配置项

在 Python 脚本中新增以下配置：

```python
ENABLE_VR_NETWORK = True
VR_TARGET_IP = "127.0.0.1"
VR_TARGET_PORT = 5005
VR_SEND_EVERY_SEC = 1.0
VR_PROTOCOL_VERSION = "physio_aware_feedback_v1"
VR_SOURCE_ID = "polar_verity_sense_pc"
```

VR 开发人员只需要确认：

1. Unity 端监听的 UDP port 与 `VR_TARGET_PORT` 一致；
2. Python 端的 `VR_TARGET_IP` 指向 Unity / VR 接收端设备；
3. 防火墙允许 UDP 端口通信。

---

## 4. JSON Payload Schema

Python 每秒发送一个 JSON object。VR 端建议主要读取以下字段：

```json
{
  "protocol": "physio_aware_feedback_v1",
  "source": "polar_verity_sense_pc",
  "timestamp_unix": 1783140000.123,
  "time_str": "14:21:08",
  "participant_role": "leader",

  "static_state": {
    "final": "Neutral",
    "final_simple": "Neutral",
    "absolute": "Neutral",
    "baseline_shift": "Near personal baseline",
    "baseline_ready": true,
    "z": {
      "hr": 0.12,
      "rmssd": -0.18,
      "sdnn": -0.05
    }
  },

  "temporal_dynamics": {
    "trend": "Stable Pattern",
    "hr_norm_change": 0.03,
    "hrv_norm_change": -0.02
  },

  "co_occurrence": "Neutral + Stable Pattern",

  "metrics": {
    "hr_bpm": 76,
    "rmssd_ms": 34.5,
    "sdnn_ms": 42.1,
    "pnn50_percent": 12.5
  },

  "quality": {
    "level": "medium",
    "hold_used": false,
    "strict": {
      "usable": true,
      "enough_ppi": true,
      "range_ok": true,
      "stability_ok": true,
      "raw_count": 28,
      "clean_count": 21
    },
    "soft": {
      "usable": true,
      "enough_ppi": true,
      "range_ok": true,
      "stability_ok": true,
      "raw_count": 31,
      "clean_count": 25
    }
  },

  "recommended_feedback": {
    "display_mode": "realtime_or_rich",
    "interface_strength": "medium_high",
    "reason": "leader_available_for_feedback"
  }
}
```

---

## 5. VR 端最应该读取的字段

### 5.1 static_state.final_simple

VR 端推荐优先读取：

```json
payload.static_state.final_simple
```

取值范围：

```text
Relaxed
Neutral
Stress
Uncertain
```

这个字段是简化后的稳定枚举，适合 VR 端直接做 UI routing。不要直接解析 `static_state.final` 的完整文本，因为完整文本可能包含补充说明，例如：

```text
Stress (trait-like / near baseline)
Neutral (below baseline)
Uncertain (stress-like vs baseline)
```

### 5.2 temporal_dynamics.trend

VR 端推荐读取：

```json
payload.temporal_dynamics.trend
```

取值范围：

```text
Pressure Increase
Progressive Relaxation
Stable Pattern
Physiological Fluctuation
Dynamics Uncertain
```

含义：

| trend | 含义 | UI 启发 |
|---|---|---|
| Pressure Increase | HR 上升且 HRV 下降，可能压力上升 | 减弱、延迟或只给轻量提示 |
| Progressive Relaxation | HR 下降且 HRV 上升，可能逐渐放松 | 可以呈现更明确的反馈 |
| Stable Pattern | 生理状态较稳定 | 可以使用常规实时反馈 |
| Physiological Fluctuation | 生理状态波动 | 使用轻量或低干扰提示 |
| Dynamics Uncertain | 趋势不确定 | 降低强度或不基于该字段决策 |

### 5.3 quality.level

VR 端应读取：

```json
payload.quality.level
```

取值范围：

```text
high
medium
low
very_low
```

推荐逻辑：

```text
high / medium: 可以使用生理状态驱动 adaptive feedback
low: 只使用轻量提示或 summary
very_low: 不使用生理状态触发强 UI，fallback 到普通反馈策略
```

### 5.4 recommended_feedback.display_mode

Python 端会额外给出一个可选推荐：

```json
payload.recommended_feedback.display_mode
```

取值示例：

```text
realtime_or_rich
subtle_realtime
delayed_summary_or_subtle
subtle_or_summary
suppress_or_summary
```

VR 端可以直接使用，也可以忽略该字段，自己根据 `static_state.final_simple`、`temporal_dynamics.trend` 和 `quality.level` 做规则判断。

---

## 6. 推荐 VR 端 UI Routing 逻辑

推荐 VR 端采用以下优先级：

```text
1. 先检查 quality.level
2. 再看 static_state.final_simple
3. 再看 temporal_dynamics.trend
4. 最后决定 interface mode
```

伪代码：

```csharp
if (quality.level == "very_low" || quality.level == "low") {
    interfaceMode = "summary_or_fallback";
}
else if (staticState == "Stress" || trend == "Pressure Increase") {
    interfaceMode = "subtle_or_delayed";
}
else if (trend == "Physiological Fluctuation") {
    interfaceMode = "subtle_realtime";
}
else if ((staticState == "Relaxed" || staticState == "Neutral") &&
         (trend == "Stable Pattern" || trend == "Progressive Relaxation")) {
    interfaceMode = "realtime_or_rich";
}
else {
    interfaceMode = "subtle_or_summary";
}
```

对应 UI 含义：

| interfaceMode | 建议 UI 行为 | 推荐interface |
|---|---|---|
| realtime_or_rich | 可以显示较明确的实时 speaking-intention cue | GradeHalo |
| subtle_realtime | 显示轻量 peripheral cue 或低强度 cue | DirectionalPeripheralHalo |
| subtle_or_delayed | 降低即时反馈强度，必要时推迟到 summary |BinaryHalo |
| summary_or_fallback | 不依赖生理状态，只使用普通 summary 或默认界面 | Repeat Attempt Dashboard |
| subtle_or_summary | 使用保守策略 | TimelineDashboard |

---

## 7. Unity 端 UDP 接收逻辑示例

下面是 Unity 端的最小接收逻辑示例。正式项目中建议把 JSON class 定义完整，并把 UI 更新放到 Unity main thread 中执行。

```csharp
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class PhysioUdpReceiver : MonoBehaviour
{
    public int listenPort = 5005;
    private UdpClient udpClient;
    private Thread receiveThread;
    private volatile bool running = true;

    private string latestJson = null;

    void Start()
    {
        udpClient = new UdpClient(listenPort);
        receiveThread = new Thread(ReceiveLoop);
        receiveThread.IsBackground = true;
        receiveThread.Start();
        Debug.Log("Physio UDP Receiver started on port " + listenPort);
    }

    void ReceiveLoop()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, listenPort);
        while (running)
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEP);
                latestJson = Encoding.UTF8.GetString(data);
            }
            catch (Exception e)
            {
                Debug.LogWarning("UDP receive error: " + e.Message);
            }
        }
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(latestJson))
        {
            string json = latestJson;
            latestJson = null;

            // TODO: parse JSON here.
            // Recommended fields:
            // static_state.final_simple
            // temporal_dynamics.trend
            // quality.level
            // recommended_feedback.display_mode

            Debug.Log("Received physio payload: " + json);
        }
    }

    void OnDestroy()
    {
        running = false;
        udpClient?.Close();
        receiveThread?.Abort();
    }
}
```

---

## 8. 测试方式

### 8.1 Python 端

先确认配置：

```python
ENABLE_VR_NETWORK = True
VR_TARGET_IP = "127.0.0.1"
VR_TARGET_PORT = 5005
```

如果 Unity Editor 和 Python 在同一台电脑上，保持 `127.0.0.1` 即可。  
如果 VR 端在另一台设备，改成该设备的局域网 IP。

### 8.2 Unity 端

1. 在 Unity 中挂载 `PhysioUdpReceiver`；
2. 设置 `listenPort = 5005`；
3. 运行场景；
4. 启动 Python 脚本；
5. Unity Console 应每秒收到一条 JSON message。

---

## 9. VR 开发侧注意事项

1. 不要使用 raw HR / RMSSD / SDNN 直接控制强 UI。
2. 优先使用 `static_state.final_simple` 和 `temporal_dynamics.trend`。
3. 如果 `quality.level` 是 `low` 或 `very_low`，不要做强烈实时反馈。
4. `recommended_feedback` 是 Python 侧的推荐，不是必须遵守的强制命令。
5. UI 不应该显示原始生理数值给参与者；建议只用于后台 adaptive routing。
6. 如果超过 3 秒没有收到 UDP 包，VR 端应回退到默认反馈策略。

---

## 10. 最小可用字段总结

VR 端最低只需要以下字段即可完成 physio-aware feedback：

```json
{
  "static_state": {
    "final_simple": "Neutral"
  },
  "temporal_dynamics": {
    "trend": "Stable Pattern"
  },
  "quality": {
    "level": "medium"
  },
  "recommended_feedback": {
    "display_mode": "realtime_or_rich"
  }
}
```

这四个字段足够支持 VR 侧选择不同强度的 interface。
