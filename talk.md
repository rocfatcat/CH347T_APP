# CH347T 開發紀錄 (C# / WinForm)

本文件紀錄了從原始 C++ SDK 轉換至 C# 封裝庫，並開發專業 I2C/SMBus 調試工具的全過程。

## 1. 核心開發里程碑

### 1.1 C# 封裝庫 (`CH347T_Sharp`)
- **目標**: 將 WCH 官方提供之 `CH347DLL.DLL` 封裝為易於使用的 .NET 類別庫。
- **實作內容**:
    - **`NativeMethods.cs`**: 包含 SPI, I2C, GPIO, UART, JTAG 的 P/Invoke 宣告。
    - **`Structures.cs` / `Enums.cs`**: 嚴格按照 C++ Header 定義結構佈局 (`Pack=1`)。
    - **`CH347Device.cs`**: 物件導向包裝，管理設備 Handle 與生命週期。
- **相容性修正**: 統一專案目標框架為 `.NET 8.0`，並處理了 Nullable Reference Types 的編譯警告。

### 1.2 技術筆記整理
- **`CH347Demo_Readme.md`**: 彙整官方 C++ Demo 的核心邏輯。
- **`Ch347Demo_I2C_Readme.md`**: 專門針對 I2C API 的詳細參數說明與通訊流程建議。

### 1.3 專業調試工具 (`CH347T_WinForm`)
- **介面設計**:
    - 採用 **自適應佈局 (Responsive UI)**，結合 `TableLayoutPanel` 與 `SplitContainer`。
    - 增加功能區域填充空間 (20px Padding)，提升視覺層次感。
- **功能開發**:
    - **I2C 掃描**: 自動遍歷 0x08-0x77 位址並偵測 ACK。
    - **序列腳本 (Sequence Script)**: 支援 `W` (寫), `R` (讀), `D` (延遲) 指令，方便模擬初始化序列。
    - **SMBus 擴充**: 增加 `SWB`, `SRB`, `SWW`, `SRW` 指令，符合標準 SMBus 協議格式。
    - **熱插拔監測 (PnP)**: 整合 `CH347SetDeviceNotify`，支援裝置拔除自動中斷與重新插入提醒。
    - **日誌系統**: 黑色後台風格，帶有毫秒級時間戳與 ACK/NACK 狀態偵測。

## 2. 解決的關鍵問題

1. **.NET 版本不相容**: 修正了 `net10.0` 與 `net8.0` 混用的錯誤，統一為 `net8.0`。
2. **UI 重疊問題**: 從絕對座標改為容器佈局管理，支援視窗自由縮放。
3. **PnP 委派回收**: 解決了 `mPCH347_NOTIFY_ROUTINE` 因為沒有被長久持有而被 GC 回收導致的閃退風險。
4. **地址位移**: 在 API 層次自動處理 7-bit 位址左移 1 位並填入 R/W 位的邏輯。
5. **編譯警告**: 修正了 CS8618 (欄位未初始化) 與 CS8603 (可能回傳 Null) 等 .NET 安全警告。

## 3. 未來擴充方向
- **JTAG/SPI 燒錄分頁**: 目前架構已預留 TabControl 空間，可依樣畫葫蘆增加 SPI Flash 或 JTAG 燒錄介面。
- **腳本儲存**: 增加將測試序列存為 `.txt` 或 `.json` 的功能。
- **波形模擬**: 雖然 CH347 不是邏輯分析儀，但可以透過 GPIO 高頻取樣做簡單的訊號監測。

---
**Gemini CLI 協助開發**
日期: 2026/04/15
OS: Win32 (Windows)
Runtime: .NET 8.0
