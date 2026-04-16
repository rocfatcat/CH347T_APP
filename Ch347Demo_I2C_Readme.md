# CH347 I2C 庫 (Library) API 詳解指南

本筆記旨在詳盡說明 CH347 DLL 中提供的 I2C 相關 API 介面、參數定義及使用邏輯，方便開發者直接進行程式撰寫。

---

## 1. 核心控制 API (Core Control)

### 1.1 `CH347I2C_Set` - 設定時鐘速率
```cpp
BOOL WINAPI CH347I2C_Set(ULONG iIndex, ULONG iMode);
```
- **功能**: 初始化 I2C 介面速度。
- **參數**:
  - `iIndex`: 設備索引 (0-15)。
  - `iMode`: 速率選擇 (Bit 1-0)。
    - `00` (0): 20KHz (低速)
    - `01` (1): 100KHz (標準，預設)
    - `10` (2): 400KHz (快速)
    - `11` (3): 750KHz (高速)
- **備註**: 必須在進行任何數據交換前呼叫。

### 1.2 `CH347I2C_SetStretch` - 時鐘拉伸
```cpp
BOOL WINAPI CH347I2C_SetStretch(ULONG iIndex, BOOL iEnable);
```
- **功能**: 開啟或關閉 I2C Clock Stretch。
- **參數**: `iEnable` (1:開啟, 0:關閉)。
- **用途**: 當從設備 (Slave) 處理不過來時，會拉低 SCL 訊號線以暫停傳輸。若通訊失敗頻繁，建議開啟。

### 1.3 `CH347I2C_SetDriverMode` - 驅動模式
```cpp
BOOL WINAPI CH347I2C_SetDriverMode(ULONG iIndex, UCHAR iMode);
```
- **功能**: 設定 I2C 引腳 (SCL/SDA) 的輸出驅動模式。
- **參數**:
  - `0`: Open-Drain (開漏，最常見，需外部上拉)。
  - `1`: Push-Pull (推挽)。

---

## 2. 數據傳輸 API (Data Transfer)

### 2.1 `CH347StreamI2C` - 流式數據交換
這是與 I2C 設備通訊的最核心、最強大的 API。它封裝了 START、地址發送、數據讀寫及 STOP 流程。
```cpp
BOOL WINAPI CH347StreamI2C(
    ULONG iIndex, 
    ULONG iWriteLength, 
    PVOID iWriteBuffer, 
    ULONG iReadLength, 
    PVOID oReadBuffer
);
```
- **工作邏輯**:
  1. 發送 START 訊號。
  2. 寫入 `iWriteBuffer` 中的所有數據。
  3. 如果 `iReadLength` > 0，則發送 REPEATED START 並切換到讀取模式，將數據存入 `oReadBuffer`。
  4. 最後發送 STOP 訊號。
- **參數關鍵**:
  - `iWriteBuffer`: 第一個位元組必須是 `(DeviceAddr << 1)`。例如設備地址為 `0x50`，寫入時傳 `0xA0`。
  - **純寫入**: `iReadLength` 設為 0，`oReadBuffer` 設為 NULL。
  - **純讀取**: `iWriteLength` 設為 1 (只發送帶讀取位的地址位元組)，`iWriteBuffer` 內容為 `(DeviceAddr << 1 | 0x01)`。

### 2.2 `CH347StreamI2C_RetACK` - 帶 ACK 反饋的傳輸
```cpp
BOOL WINAPI CH347StreamI2C_RetACK(..., PULONG rAckCount);
```
- **功能**: 與 `CH347StreamI2C` 相同，但多了一個 `rAckCount` 指針，返回通訊過程中收到的 ACK 數量。
- **用途**: 偵錯、確認設備是否存在。

---

## 3. EEPROM 專用 API

這組 API 對 24C 系列 EEPROM 進行了優化，自動處理 16-bit 寄存器地址和頁面讀寫。

### 3.1 `CH347ReadEEPROM` / `CH347WriteEEPROM`
```cpp
BOOL WINAPI CH347ReadEEPROM(ULONG iIndex, EEPROM_TYPE iEepromID, ULONG iAddr, ULONG iLength, PUCHAR oBuffer);
BOOL WINAPI CH347WriteEEPROM(ULONG iIndex, EEPROM_TYPE iEepromID, ULONG iAddr, ULONG iLength, PUCHAR iBuffer);
```
- **`iEepromID`**: 使用 `EEPROM_TYPE` 列舉（如 `ID_24C02`, `ID_24C64` 等）。
- **`iAddr`**: EEPROM 內部的數據單元地址。
- **`iLength`**: 讀寫長度。

---

## 4. 進階微調 API (僅限 CH347T)

### 4.1 `CH347I2C_SetIgnoreNack`
- **功能**: 設定是否忽略設備的 NACK。
- **參數**: `0` (收到 NACK 即停止傳輸), `1` (忽略 NACK 繼續發送)。

### 4.2 `CH347I2C_SetAckClk_DelayuS`
- **功能**: 設定第 8 個時鐘脈衝（ACK/NACK 位）的低平延遲。
- **用途**: 針對特殊的慢速 I2C 協議進行微調。

---

## 5. C# 封裝與開發總結

在 `CH347T_Sharp` 中，我們建議使用 `CH347Device` 類別進行包裝：

| C++ API | C# 封裝方法 | 說明 |
| :--- | :--- | :--- |
| `CH347I2C_Set` | `device.I2CSet(mode)` | 初始化速度 |
| `CH347StreamI2C` | `device.I2CStream(wBuf, rLen)` | 返回 `byte[]` 或 `null` |

### 開發範例：
```csharp
// 1. 設定 400KHz 快速模式
device.I2CSet(2);

// 2. 寫入 2 位元組數據到 0x50 設備
byte[] data = new byte[] { 0xA0, 0x01, 0xFF }; // 地址 + 寄存器 + 數據
device.I2CStream(data, 0); 
```

### 常見錯誤代碼與狀態：
- **傳回 `false`**: 通常是硬體層級的錯誤，例如接線斷路或從設備無響應。
- **`oReadBuffer` 全為 0xFF**: SDA 線被拉高，通常表示從設備未正確驅動數據線或地址錯誤。
