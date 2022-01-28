using rustbox.Features;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rustbox
{
    public static class DMAController
    {
        public static uint pid = 0;
        public static FormMain formMain;
        public static vmm.MAP_MODULEENTRY gameAssembly;
        public static vmm.MAP_MODULEENTRY unityplayer;
        public static bool isAttached = false;

        public static Thread updateThread;
        public static Thread entityLoopThread;

        public static string ReadString(ulong address)
        {
            byte[] byteArray = vmm.MemRead(pid, address, 128);
            string convertedString = System.Text.Encoding.ASCII.GetString(byteArray);
            if (convertedString.Contains('\0'))
                convertedString = convertedString.Split('\0')[0];
            return convertedString;
        }

        public static string ReadClassName(ulong address)
        {
            byte[] byteArray = vmm.MemRead(pid, address, 64);
            string convertedString = System.Text.Encoding.ASCII.GetString(byteArray);
            if (convertedString.Contains('\0'))
                convertedString = convertedString.Split('\0')[0];
            return convertedString;
        }

        public static void WriteMemory<T>(ulong address, T value)
        {
            if (address != 0)
            {
                byte[] buffer = StructureToBytes<T>(value);
                vmm.MemWrite(pid, address, buffer);
            }
        }

        public static byte[] StructureToBytes<T>(T structure)
        {
            int size = Marshal.SizeOf(structure);
            byte[] buffer = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(structure, ptr, true);
            Marshal.Copy(ptr, buffer, 0, size);
            Marshal.FreeHGlobal(ptr);
            return buffer;
        }

        public static T ReadMemory<T>(ulong address)
        {
            if (address != 0)
            {
                uint size = (uint)Marshal.SizeOf(typeof(T));
                byte[] buffer = vmm.MemRead(pid, address, size);
                T result = default(T);
                result = BytesToStructure<T>(buffer);
                return result;
            }
            else
            {
                return default(T);
            }
        }

        public static T BytesToStructure<T>(byte[] buffer)
        {
            T result = default(T);
            int size = buffer.Length;
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(buffer, 0, ptr, size);
            result = (T)Marshal.PtrToStructure(ptr, result.GetType());

            Marshal.FreeHGlobal(ptr);
            return result;
        }

        public static void Cleanup()
        {
            try
            {
                if (updateThread != null)
                {
                    if (updateThread.IsAlive)
                    {
                        updateThread.Abort();
                        updateThread = null;
                    }
                }

                isAttached = false;
                vmm.Close();
                formMain.Invoke(new MethodInvoker(delegate () { formMain.labelGameAssembly.Text = "-"; }));
                formMain.Invoke(new MethodInvoker(delegate () { formMain.labelRustPID.Text = "-"; }));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[EASYOFFLINE] FATAL error while closing connection to DMA device. \n" + ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void Attach()
        {
            try
            {
                Console.Clear();

                Console.WriteLine("[EASYOFFLINE] Initializing DMA Device");
                formMain.Invoke(new MethodInvoker(delegate () { formMain.labelStatus.Text = "Initializing"; }));
                formMain.Invoke(new MethodInvoker(delegate () { formMain.labelStatus.ForeColor = Color.Wheat; }));
                vmm.Initialize("-printf", "-device", "fpga");

                Console.WriteLine("[EASYOFFLINE] Getting Game PID");
                formMain.Invoke(new MethodInvoker(delegate () { formMain.labelStatus.Text = "Retrieving PID"; }));
                formMain.Invoke(new MethodInvoker(delegate () { formMain.labelStatus.ForeColor = Color.Yellow; }));
                while (!vmm.PidGetFromName("RustClient.exe", out pid)) { }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[EASYOFFLINE] Got PID: " + pid);
                formMain.Invoke(new MethodInvoker(delegate () { formMain.labelRustPID.Text = pid.ToString(); }));

                Console.WriteLine("[EASYOFFLINE] Getting Module");
                gameAssembly = vmm.Map_GetModuleFromName(pid, "GameAssembly.dll");
                unityplayer = vmm.Map_GetModuleFromName(pid, "UnityPlayer.dll");

                if (gameAssembly.vaBase != 0)
                {
                    Console.WriteLine("[EASYOFFLINE] Got Modules");
                    formMain.Invoke(new MethodInvoker(delegate () { formMain.labelStatus.Text = "Attached"; }));
                    formMain.Invoke(new MethodInvoker(delegate () { formMain.labelStatus.ForeColor = Color.Green; }));
                    formMain.Invoke(new MethodInvoker(delegate () { formMain.buttonDetach.Enabled = true; }));
                    formMain.Invoke(new MethodInvoker(delegate () { formMain.labelGameAssembly.Text = string.Format("0x{0:X}", gameAssembly.vaBase); }));

                    isAttached = true;

                    entityLoopThread = new Thread(UpdateLoop.RunEntityLoop);
                    entityLoopThread.Start();

                    updateThread = new Thread(UpdateLoop.RunFeatures);
                    updateThread.Start();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[EASYOFFLINE] ERROR could not get Module GameAssembly.dll.");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[EASYOFFLINE] FATAL error when initializing DMA device. \n" + ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
