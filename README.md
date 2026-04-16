# CH347 I2C Firmware Engineer Toolkit

這是一個專為韌體工程師設計的 I2C 測試與調試工具，基於 CH347 USB-to-I2C 轉接器開發。

## 1. 核心功能
- **單次命令測試 (Basic Debug)**: 快速讀寫特定寄存器，驗證 ACK/NACK 狀態。
- **序列腳本測試 (Sequence Script)**: 自定義讀寫序列與延遲，模擬複雜的設備初始化流程。
- **全匯流排掃描 (Bus Scan)**: 自動偵測 0x08-0x77 範圍內的所有 I2C 設備。
- **詳細日誌 (Detailed Log)**: 帶有毫秒級時間戳與 16 進位格式化的通訊日誌。

## 2. 腳本語法說明
在 `Sequence Script` 分頁中，您可以使用簡單的指令組合測試腳本：

| 指令 | 格式 | 說明 | 範例 |
| :--- | :--- | :--- | :--- |
| **W** | `W [Addr] [Reg] [Data...]` | 寫入數據 (Hex) | `W 50 00 AA BB` |
| **R** | `R [Addr] [Reg] [Len]` | 讀取數據 (Hex) | `R 50 00 4` |
| **D** | `D [ms]` | 延遲 (毫秒) | `D 100` |
| **#** | `# [Comment]` | 註釋 | `# Init Device` |

### 範例腳本：
```text
# 重置設備並讀取 ID
W 50 00 01
D 50
R 50 0F 1
```

## 3. 開發者技術筆記 (工程師參考)

### 3.1 I2C 物理層細節
- **7-bit 轉 8-bit 地址**: 本工具 API 內部會自動處理。您只需在 UI 輸入 7-bit 地址 (如 `0x50`)，程式會自動左移並填入 R/W 位（寫入時為 `0xA0`，讀取時為 `0xA1`）。
- **時序控制**: `CH347StreamI2C` 採用硬體流控，保證了數據包的完整性。
- **上拉電阻**: 確保硬體引腳已連接適當的上拉電阻（通常為 2.2K-10K）。

### 3.2 專案架構
- **`CH347T_Sharp`**: 底層封裝庫，負責 P/Invoke 調用 `CH347DLL.DLL`。
- **`MainForm.cs`**: 邏輯層，包含腳本解析引擎與異步執行邏輯。
- **`NativeMethods.cs`**: C++ DLL 的原始函式宣告。

## 4. 如何重啟開發
1. 確保電腦已安裝 [CH347 驅動程式](http://wch.cn)。
2. 將 `CH347DLL.DLL` 置於執行檔同目錄下。
3. 使用 Visual Studio 或 `dotnet` CLI 開啟專案。
4. 點擊 `dotnet run` 啟動工具。

---
*Created by Gemini CLI - 2026/04/15*
