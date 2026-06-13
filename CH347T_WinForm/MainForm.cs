using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using CH347T_Sharp;

namespace CH347T_WinForm
{
    public partial class MainForm : Form
    {
        private CH347Device? device;
        private Label lblStatus = null!;
        private ComboBox cbSpeed = null!;
        private TextBox txtLog = null!;
        private TextBox txtScript = null!;
        private TextBox txtDevAddr = null!;
        private TextBox txtRegAddr = null!;
        private TextBox txtWriteData = null!;
        private NumericUpDown numReadLen = null!;
        private Button btnOpen = null!;
        private Button btnWrite = null!;
        private Button btnRead = null!;
        private Button btnScan = null!;
        private Button btnRunScript = null!;

        private NativeMethods.mPCH347_NOTIFY_ROUTINE? notifyHandler;

        public MainForm()
        {
            SetupUI();
            InitializePnP();
        }

        private void InitializePnP()
        {
            notifyHandler = new NativeMethods.mPCH347_NOTIFY_ROUTINE(OnDeviceNotify);
            NativeMethods.CH347SetDeviceNotify(0, "VID_1A86&PID_55", notifyHandler);
        }

        private void OnDeviceNotify(uint status)
        {
            if (status == 0)
            {
                this.BeginInvoke(new Action(() => {
                    if (device != null && device.IsOpen)
                    {
                        Log("!!! CRITICAL: DEVICE UNPLUGGED !!!", true);
                        DisconnectDevice();
                        MessageBox.Show("The I2C device has been disconnected.", "Hardware Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));
            }
            else if (status == 3)
            {
                this.BeginInvoke(new Action(() => {
                    Log("✔ Device connection detected. You can now open the device.");
                }));
            }
        }

        private void SetupUI()
        {
            this.Text = "CH347 I2C Firmware Engineer Toolkit";
            this.Size = new Size(950, 750);
            this.MinimumSize = new Size(700, 600);

            FlowLayoutPanel pnlTop = new FlowLayoutPanel { 
                Dock = DockStyle.Top, 
                Height = 60, 
                BackColor = Color.FromArgb(220, 225, 230),
                Padding = new Padding(15, 15, 0, 0)
            };
            this.Controls.Add(pnlTop);

            Panel pnlMainContainer = new Panel { 
                Dock = DockStyle.Fill, 
                Padding = new Padding(15, 20, 15, 15)
            };
            this.Controls.Add(pnlMainContainer);
            pnlMainContainer.BringToFront();

            SplitContainer mainSplit = new SplitContainer { 
                Dock = DockStyle.Fill, 
                Orientation = Orientation.Horizontal,
                SplitterDistance = 380 
            };
            pnlMainContainer.Controls.Add(mainSplit);

            lblStatus = new Label { Text = "Disconnected", AutoSize = true, Margin = new Padding(0, 5, 20, 0), Font = new Font(this.Font, FontStyle.Bold) };
            cbSpeed = new ComboBox { Width = 100, Margin = new Padding(0, 2, 10, 0) };
            cbSpeed.Items.AddRange(new object[] { "20KHz", "100KHz", "400KHz", "750KHz" });
            cbSpeed.SelectedIndex = 1;
            btnOpen = new Button { Text = "Open CH347", Width = 100 };
            btnScan = new Button { Text = "Scan Bus", Width = 100, Enabled = false };
            
            pnlTop.Controls.Add(lblStatus);
            pnlTop.Controls.Add(cbSpeed);
            pnlTop.Controls.Add(btnOpen);
            pnlTop.Controls.Add(btnScan);

            btnOpen.Click += BtnOpen_Click;
            btnScan.Click += BtnScan_Click;

            TabControl tabs = new TabControl { Dock = DockStyle.Fill };
            mainSplit.Panel1.Controls.Add(tabs);

            TabPage tabBasic = new TabPage("Basic Debug");
            TabPage tabScript = new TabPage("Sequence Script");
            tabs.TabPages.Add(tabBasic);
            tabs.TabPages.Add(tabScript);

            TableLayoutPanel gridBasic = new TableLayoutPanel { 
                Dock = DockStyle.Fill, 
                ColumnCount = 3, 
                RowCount = 5,
                Padding = new Padding(20)
            };
            gridBasic.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            gridBasic.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            gridBasic.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

            gridBasic.Controls.Add(new Label { Text = "Dev Addr (7-bit Hex):", Anchor = AnchorStyles.Left }, 0, 0);
            txtDevAddr = new TextBox { Text = "50", Width = 80 };
            gridBasic.Controls.Add(txtDevAddr, 1, 0);

            gridBasic.Controls.Add(new Label { Text = "Reg Addr (Hex):", Anchor = AnchorStyles.Left }, 0, 1);
            txtRegAddr = new TextBox { Text = "00", Width = 80 };
            gridBasic.Controls.Add(txtRegAddr, 1, 1);

            gridBasic.Controls.Add(new Label { Text = "Write Bytes (Space split):", Anchor = AnchorStyles.Left }, 0, 2);
            txtWriteData = new TextBox { Text = "AA BB CC", Dock = DockStyle.Fill };
            btnWrite = new Button { Text = "I2C Write", Dock = DockStyle.Fill, Enabled = false };
            gridBasic.Controls.Add(txtWriteData, 1, 2);
            gridBasic.Controls.Add(btnWrite, 2, 2);

            gridBasic.Controls.Add(new Label { Text = "Read Length:", Anchor = AnchorStyles.Left }, 0, 3);
            numReadLen = new NumericUpDown { Value = 1, Width = 80 };
            btnRead = new Button { Text = "I2C Read", Dock = DockStyle.Fill, Enabled = false };
            gridBasic.Controls.Add(numReadLen, 1, 3);
            gridBasic.Controls.Add(btnRead, 2, 3);

            tabBasic.Controls.Add(gridBasic);
            btnWrite.Click += BtnWrite_Click;
            btnRead.Click += BtnRead_Click;

            txtScript = new TextBox { 
                Multiline = true, Dock = DockStyle.Fill, 
                Font = new Font("Consolas", 11),
                Text = "# I2C: W/R [Addr] [Reg] [Data/Len]\r\n# SMBus: SWB/SRB/SWW/SRW [Addr] [Cmd]\r\n# W 50 00 AA BB\r\n# SRB 50 0F\r\n# D 100\r\n",
                ScrollBars = ScrollBars.Vertical 
            };
            btnRunScript = new Button { Text = "Run Sequence", Dock = DockStyle.Bottom, Height = 40, Enabled = false, BackColor = Color.LightSteelBlue };
            tabScript.Controls.Add(txtScript);
            tabScript.Controls.Add(btnRunScript);
            btnRunScript.Click += BtnRunScript_Click;

            GroupBox grpLog = new GroupBox { Text = "Communication Log", Dock = DockStyle.Fill, Padding = new Padding(5) };
            txtLog = new TextBox { 
                Multiline = true, Dock = DockStyle.Fill, 
                ReadOnly = true, ScrollBars = ScrollBars.Vertical, 
                BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.LimeGreen,
                Font = new Font("Consolas", 10)
            };
            grpLog.Controls.Add(txtLog);
            mainSplit.Panel2.Controls.Add(grpLog);
        }

        private void Log(string msg, bool isError = false) 
        {
            if (txtLog.InvokeRequired) { txtLog.Invoke(new Action(() => Log(msg, isError))); return; }
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss.fff}] {(isError ? "✘ " : "✔ ")}{msg}\r\n");
        }

        private void BtnOpen_Click(object? sender, EventArgs e)
        {
            try {
                if (device != null && device.IsOpen) {
                    DisconnectDevice();
                } else {
                    device = new CH347Device(0);
                    if (device.Open()) {
                        device.I2CSet((uint)cbSpeed.SelectedIndex);
                        
                        var info = device.GetDeviceInfo();
                        string prodName = CH347Device.BytesToString(info.ProductString);
                        string speed = info.UsbSpeedType == 1 ? "High-Speed" : (info.UsbSpeedType == 2 ? "Super-Speed" : "Full-Speed");
                        
                        lblStatus.Text = $"Connected: {prodName} ({speed})";
                        lblStatus.ForeColor = Color.DarkGreen;
                        
                        Log("========================================");
                        Log($"✔ Device Connected Successfully!");
                        Log($"Product  : {prodName}");
                        Log($"Firmware : v{info.FirewareVer}");
                        Log($"USB Bus  : {speed}");
                        Log($"Interface: Mode {info.ChipMode}");
                        Log("========================================");

                        btnOpen.Text = "Close Device";
                        ToggleControls(true);
                    } else {
                        Log("Failed to open device. Device not found or occupied.", true);
                        MessageBox.Show("Could not find CH347 device.\n\nPlease check:\n1. Hardware connection\n2. Driver installation\n3. Chip Mode switch setting", 
                                        "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            } catch (Exception ex) { Log(ex.Message, true); }
        }

        private void DisconnectDevice()
        {
            if (device != null)
            {
                device.Dispose();
                device = null;
            }
            lblStatus.Text = "Disconnected";
            lblStatus.ForeColor = Color.DarkRed;
            btnOpen.Text = "Open CH347";
            ToggleControls(false);
            Log("Device connection closed.");
        }

        private void ToggleControls(bool enabled)
        {
            btnWrite.Enabled = btnRead.Enabled = btnScan.Enabled = btnRunScript.Enabled = enabled;
        }

        private void BtnScan_Click(object? sender, EventArgs e)
        {
            if (device == null) return;
            Log("Scanning I2C Bus (0x08 - 0x77)...");
            Task.Run(() => {
                int count = 0;
                for (byte i = 0x08; i <= 0x77; i++) {
                    if (device == null) break;
                    if (device.I2CWriteAndCheckAck(new byte[] { (byte)(i << 1) })) {
                        Log($">> Device detected at 0x{i:X2}");
                        count++;
                    }
                }
                Log($"Scan complete. {count} devices found.");
            });
        }

        private void BtnWrite_Click(object? sender, EventArgs e)
        {
            try {
                byte addr = Convert.ToByte(txtDevAddr.Text, 16);
                byte reg = Convert.ToByte(txtRegAddr.Text, 16);
                byte[] data = txtWriteData.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToByte(x, 16)).ToArray();
                ExecuteWrite(addr, reg, data);
            } catch (Exception ex) { Log(ex.Message, true); }
        }

        private void BtnRead_Click(object? sender, EventArgs e)
        {
            try {
                byte addr = Convert.ToByte(txtDevAddr.Text, 16);
                byte reg = Convert.ToByte(txtRegAddr.Text, 16);
                ExecuteRead(addr, reg, (uint)numReadLen.Value);
            } catch (Exception ex) { Log(ex.Message, true); }
        }

        private bool ExecuteWrite(byte addr, byte reg, byte[] data)
        {
            if (device == null) return false;
            byte[] buf = new byte[data.Length + 2];
            buf[0] = (byte)(addr << 1);
            buf[1] = reg;
            Array.Copy(data, 0, buf, 2, data.Length);
            bool success = device.I2CStream(buf, 0) != null;
            Log($"WRITE 0x{addr:X2} REG:0x{reg:X2} DATA:[{BitConverter.ToString(data)}] -> {(success ? "ACK" : "NACK")}", !success);
            return success;
        }

        private byte[]? ExecuteRead(byte addr, byte reg, uint len)
        {
            if (device == null) return null;
            byte[] select = new byte[] { (byte)(addr << 1), reg };
            byte[]? data = device.I2CStream(select, len);
            if (data != null) Log($"READ  0x{addr:X2} REG:0x{reg:X2} LEN:{len} DATA:[{BitConverter.ToString(data)}]");
            else Log($"READ  0x{addr:X2} REG:0x{reg:X2} -> FAILED (NACK)", true);
            return data;
        }

        private async void BtnRunScript_Click(object? sender, EventArgs e)
        {
            btnRunScript.Enabled = false;
            string[] lines = txtScript.Lines;
            Log("--- Starting Sequence Execution ---");

            await Task.Run(async () => {
                foreach (var line in lines) {
                    if (device == null) break;
                    string l = line.Trim();
                    if (string.IsNullOrWhiteSpace(l) || l.StartsWith("#")) continue;
                    try {
                        string[] parts = l.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        string cmd = parts[0].ToUpper();
                        byte addr, reg;
                        switch (cmd) {
                            case "W":
                                ExecuteWrite(Convert.ToByte(parts[1], 16), Convert.ToByte(parts[2], 16), parts.Skip(3).Select(x => Convert.ToByte(x, 16)).ToArray());
                                break;
                            case "R":
                                ExecuteRead(Convert.ToByte(parts[1], 16), Convert.ToByte(parts[2], 16), Convert.ToUInt32(parts[3]));
                                break;
                            case "SWB": // SMBus Write Byte
                                addr = Convert.ToByte(parts[1], 16);
                                reg = Convert.ToByte(parts[2], 16);
                                byte bData = Convert.ToByte(parts[3], 16);
                                bool swbSucc = device.SMBusWriteByte(addr, reg, bData);
                                Log($"SMBus SWB 0x{addr:X2} CMD:0x{reg:X2} DATA:0x{bData:X2} -> {(swbSucc ? "ACK" : "NACK")}", !swbSucc);
                                break;
                            case "SRB": // SMBus Read Byte
                                addr = Convert.ToByte(parts[1], 16);
                                reg = Convert.ToByte(parts[2], 16);
                                byte[]? srbData = device.SMBusReadByte(addr, reg);
                                if (srbData != null) Log($"SMBus SRB 0x{addr:X2} CMD:0x{reg:X2} -> 0x{srbData[0]:X2}");
                                else Log($"SMBus SRB 0x{addr:X2} CMD:0x{reg:X2} -> FAILED", true);
                                break;
                            case "SWW": // SMBus Write Word
                                addr = Convert.ToByte(parts[1], 16);
                                reg = Convert.ToByte(parts[2], 16);
                                ushort wData = Convert.ToUInt16(parts[3], 16);
                                bool swwSucc = device.SMBusWriteWord(addr, reg, wData);
                                Log($"SMBus SWW 0x{addr:X2} CMD:0x{reg:X2} DATA:0x{wData:X4} -> {(swwSucc ? "ACK" : "NACK")}", !swwSucc);
                                break;
                            case "SRW": // SMBus Read Word
                                addr = Convert.ToByte(parts[1], 16);
                                reg = Convert.ToByte(parts[2], 16);
                                byte[]? srwData = device.SMBusReadWord(addr, reg);
                                if (srwData != null && srwData.Length == 2) {
                                    ushort val = (ushort)(srwData[0] | (srwData[1] << 8));
                                    Log($"SMBus SRW 0x{addr:X2} CMD:0x{reg:X2} -> 0x{val:X4}");
                                }
                                else Log($"SMBus SRW 0x{addr:X2} CMD:0x{reg:X2} -> FAILED", true);
                                break;
                            case "D":
                                int ms = Convert.ToInt32(parts[1]);
                                Log($"WAIT {ms}ms...");
                                await Task.Delay(ms);
                                break;
                        }
                    } catch (Exception ex) { Log($"Line Error '{l}': {ex.Message}", true); }
                }
            });

            Log("--- Sequence Complete ---");
            btnRunScript.Enabled = true;
        }
    }
}
