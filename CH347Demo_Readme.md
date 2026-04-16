# CH347 / CH347T / CH347F / CH339W 開發筆記 (C# / C++)

此筆記整理自官方 `CH347Demo` 範例程式碼，旨在為使用 `CH347T_Sharp` 或原生 C++ DLL 開發應用程式提供參考。

## 1. 基礎架構與初始化

### 1.1 設備發現與開啟
- **設備枚舉**：CH347 支援多個設備同時連接。通常透過循環索引 `0` 到 `15` 調用 `CH347OpenDevice(index)` 來發現設備。
- **晶片模式 (ChipMode)**：
  - `Mode 0`: UART0 & UART1 (VCP/HID)
  - `Mode 1`: UART1 + SPI + I2C (Vendor)
  - `Mode 2`: UART1 + SPI + I2C (HID)
  - `Mode 3`: UART1 + JTAG + I2C (Vendor)
- **熱插拔處理**：建議使用 `CH347SetDeviceNotify` 註冊回呼函數，監聽 `CH347_DEVICE_ARRIVAL` (3) 和 `CH347_DEVICE_REMOVE` (0) 事件。

### 1.2 通用設置
- **逾時設置**：開啟設備後應立即調用 `CH347SetTimeout` 設定讀寫逾時（預設通常為 500ms）。

---

## 2. SPI 介面開發要點

### 2.1 初始化流程
1. 調用 `CH347SPI_SetFrequency` 設定時鐘頻率（支援 60MHz, 30MHz, 15MHz 等）。
2. (可選) 調用 `CH347SPI_SetDataBits` 設定位寬（8-bit 或 16-bit）。
3. 調用 `CH347SPI_Init` 並傳入 `mSpiCfgS` 結構完成配置（包含 Mode 0-3、片選極性、自動撤銷片選等）。

### 2.2 數據傳輸
- **`CH347StreamSPI4`**: 4 線制標準 SPI，同時進行讀寫。輸入緩衝區在調用後會被替換為讀取的數據。
- **`CH347SPI_Read / Write`**: 針對大數據塊優化的 API。`Read` 操作效率高於 `WriteRead` 後棄掉寫入部分的做法。
- **片選控制**：
  - 如果 `mSpiCfgS.iChipSelect` 的最高位 (Bit 7) 為 `1`，API 會在傳輸時自動控制 CS。
  - 也可以手動使用 `CH347SPI_ChangeCS` 或 `CH347SPI_SetChipSelect` 控制。

---

## 3. I2C 介面開發要點

### 3.1 初始化與配置
- **速度等級**：使用 `CH347I2C_Set` 設定，支援 20KHz (0), 100KHz (1), 400KHz (2), 750KHz (3)。
- **時鐘拉伸 (Clock Stretch)**：若從設備較慢，可調用 `CH347I2C_SetStretch(index, TRUE)`。
- **延遲設定**：`CH347I2C_SetDelaymS` 可在流操作指令間加入硬體級延遲。

### 3.2 讀寫操作
- **`CH347StreamI2C`**: 統一的流操作介面。
  - **純寫入**：`iReadLength` 設為 0。
  - **複合操作 (先寫後讀)**：常用於讀取寄存器。`iWriteBuffer` 第一個位元組通常是 `(DeviceAddr << 1)`，後面接寄存器地址。

---

## 4. GPIO 介面開發要點

### 4.1 基本操控
- **`CH347GPIO_Get`**: 獲取 8 個 GPIO 的方向和當前電平。
- **`CH347GPIO_Set`**: 設定方向和輸出電平。使用 `iEnable` 掩碼位元組來決定哪些引腳受此次調用影響。

### 4.2 中斷處理
- CH347 支援 GPIO 中斷。
- 調用 `CH347SetIntRoutine` 指定引腳和觸發方式（上升沿、下降沿、雙邊沿）。
- 在回呼函數中處理事件。注意回呼發生在獨立執行緒，若要更新 UI 需進行同步。

---

## 5. UART 介面開發要點

### 5.1 獨立 API
- UART 使用獨立的 API 前綴 `CH347Uart_`。
- 開啟設備需使用 `CH347Uart_Open`。
- **波特率**：支援非標準波特率，最高可達 9Mbps 或更高（取決於晶片型號）。

### 5.2 讀寫建議
- **讀取**：由於 USB 傳輸特性，建議在獨立執行緒中循環調用 `CH347Uart_Read`。
- **效能**：大數據傳輸時，增加單次讀寫的 Buffer 大小（如 4KB 或以上）可提高輸送量。

---

## 6. JTAG 介面開發要點

### 6.1 初始化
- 使用 `CH347Jtag_INIT` 設定時鐘等級 (0-5)。

### 6.2 狀態機操作
- 支援透過 `CH347Jtag_SwitchTapState` 直接切換 TAP 狀態。
- 提供 `CH347Jtag_ByteWriteDR`, `CH347Jtag_ByteReadDR` 等位元組級 API，方便進行固件燒錄等大數據操作。
- `CH347Jtag_IoScan`: 處理 IR/DR 掃描的基礎 API。

---

## 7. 開發陷阱與建議

1. **多執行緒安全**：雖然 DLL 內部有一定的保護，但建議在應用層對同一個設備實例的併發存取進行加鎖。
2. **Buffer 對齊**：傳遞給 API 的 Byte 陣列應確保生命週期在調用期間有效，C# 中使用 `fixed` 或讓 GC 暫停回收（封送處理已自動處理此點）。
3. **錯誤偵測**：API 大多返回 `BOOL`。若失敗，可檢查連接狀態或參數範圍。
4. **Library 切換**：
   - 32 位元應用請連結 `CH347DLL.lib`。
   - 64 位元應用請連結 `amd64\CH347DLLA64.lib` 並確保執行目錄有 `CH347DLLA64.dll` (在 C# 封裝中需注意 DLL 名稱對應)。
