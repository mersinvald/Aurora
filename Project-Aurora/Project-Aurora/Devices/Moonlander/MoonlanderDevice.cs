using Aurora.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipes;
using Aurora.Settings;
using System.ComponentModel;
using System.Web.Caching;
using CSScriptLibrary;
using System.Runtime.InteropServices;
using Aurora.Devices;
using Corale.Colore.Razer.Keyboard;

namespace Aurora.Devices.Moonlander
{
    class MoonlanderDevice : Device
    {
        // Generic Variables
        private string devicename = "Moonlander MK1";
        private bool isInitialized = false;

        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        private Dictionary<DeviceKeys, Color> cachedKeyColors = new Dictionary<DeviceKeys, Color>();

        private NamedPipeClientStream bridge;

        public string GetDeviceName()
        {
            return devicename;
        }

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                return devicename + ": Initialized";
            }
            else
            {
                return devicename + ": Not initialized";
            }
        }

        public bool Initialize()
        {
            if (!isInitialized)
            {
                try
                {
                    bridge = new NamedPipeClientStream(".", "hid-server\\aurora-bridge", PipeDirection.Out);
                    bridge.Connect(5000);

                    // Mark Initialized = TRUE
                    isInitialized = true;
                    return true;
                }
                catch (Exception ex)
                {
                    Global.logger.Error(devicename + " device, Exception! Message:" + ex);
                }

                // Mark Initialized = FALSE
                isInitialized = false;
                return false;
            }

            return isInitialized;

        }

        public void Shutdown()
        {
            if (this.IsInitialized())
            {
                bridge.Dispose();
            }
        }

        public void Reset()
        {
            cachedKeyColors.Clear();
            Reconnect();
        }

        public bool Reconnect()
        {
            bool success = false;
            try
            {
                bridge.Dispose();
                bridge = new NamedPipeClientStream(".", "hid-server\\aurora-bridge", PipeDirection.Out);
                bridge.Connect(5000);
                success = true;
            }
            catch (Exception ex)
            {
                Global.logger.Error(devicename + " device, Exception! Message:" + ex);
            }
            return success;
        }

        public bool IsInitialized()
        {
            return isInitialized;
        }

        public bool IsConnected()
        {
            return bridge.IsConnected;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false) // Is this necessary?
        {
            bool update_result = false;

            if (e.Cancel) return false;
            try
            {
                foreach (KeyValuePair<DeviceKeys, Color> pair in keyColors)
                {
                    SendColorsToKeyboard(pair.Key, pair.Value);
                }

                bridge.Flush();

                update_result = true;
            }
            catch (Exception exception)
            {
                Global.logger.Error(devicename + " device, error when updating device. Error: " + exception);
                update_result = false;
            }

            return update_result;
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            bool update_result = UpdateDevice(colorComposition.keyColors, e, forced);

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return update_result;
        }

        private void SendColorsToKeyboard(DeviceKeys key, Color color, bool forced = false)
        {
            bool needsUpdate = true;

            // Ignore None and Peripheral
            if ((int)key < 1)
            {
                return;
            }

            Color cached;
            bool found = cachedKeyColors.TryGetValue(key, out cached);

            if (found)
            {
                needsUpdate = cached != color;
            }

            if (forced || needsUpdate)
            {
                cachedKeyColors[key] = color;
                KeyColorConverter bytes = new KeyColorConverter(key, color);
                bridge.WriteByte(bytes.Byte0);
                bridge.WriteByte(bytes.Byte1);
                bridge.WriteByte(bytes.Byte2);
                bridge.WriteByte(bytes.Byte3);
                bridge.WriteByte(bytes.Byte4);
                bridge.WriteByte(bytes.Byte5);
                bridge.WriteByte(bytes.Byte6);
                bridge.WriteByte(bytes.Byte7);
            }
        }

        // Device Status Methods
        public bool IsKeyboardConnected()
        {
            return isInitialized;
        }

        public bool IsPeripheralConnected()
        {
            return isInitialized;
        }

        public string GetDeviceUpdatePerformance()
        {
            return (isInitialized ? lastUpdateTime + " ms" : "");
        }

        public VariableRegistry GetRegisteredVariables()
        {
            return new VariableRegistry();
        }
    }
}

[StructLayout(LayoutKind.Explicit)]
struct KeyColorConverter
{
    [FieldOffset(0)] public Int32 key;
    [FieldOffset(0)] public byte Byte0;
    [FieldOffset(1)] public byte Byte1;
    [FieldOffset(2)] public byte Byte2;
    [FieldOffset(3)] public byte Byte3;
    [FieldOffset(4)] public UInt32 color;
    [FieldOffset(4)] public byte Byte4;
    [FieldOffset(5)] public byte Byte5;
    [FieldOffset(6)] public byte Byte6;
    [FieldOffset(7)] public byte Byte7;

    public KeyColorConverter(DeviceKeys k, Color c)
    {
        Byte0 = Byte1 = Byte2 = Byte3 = 0;
        color = 0;
        key = (Int32)k;
        Byte4 = c.R;
        Byte5 = c.G;
        Byte6 = c.B;
        Byte7 = c.A;
    }
}