using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

public static class ReliableDllInjector
{
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, out IntPtr written);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetModuleHandle(string moduleName);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint stackSize,
        IntPtr lpStartAddress, IntPtr lpParameter, uint creationFlags, out IntPtr threadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr address, uint size, uint freeType);

    const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
    const uint MEM_COMMIT = 0x1000;
    const uint MEM_RESERVE = 0x2000;
    const uint PAGE_READWRITE = 0x04;
    const uint MEM_RELEASE = 0x8000;

    public static bool Inject(string processName, string dllPath)
    {
        try
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
            {
                MessageBox.Show($"❌ Could not find process \"{processName}\".");
                return false;
            }

            Process process = processes[0];
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
            if (hProcess == IntPtr.Zero)
            {
                MessageBox.Show("❌ Failed to open target process.");
                return false;
            }

            byte[] dllBytes = Encoding.Unicode.GetBytes(dllPath + "\0");

            IntPtr allocAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)dllBytes.Length, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (allocAddress == IntPtr.Zero)
            {
                MessageBox.Show("❌ Failed to allocate memory in target process.");
                CloseHandle(hProcess);
                return false;
            }

            if (!WriteProcessMemory(hProcess, allocAddress, dllBytes, (uint)dllBytes.Length, out _))
            {
                MessageBox.Show("❌ Failed to write DLL path into target process.");
                VirtualFreeEx(hProcess, allocAddress, 0, MEM_RELEASE);
                CloseHandle(hProcess);
                return false;
            }

            IntPtr hKernel32 = GetModuleHandle("kernel32.dll");
            IntPtr loadLibraryW = GetProcAddress(hKernel32, "LoadLibraryW");

            if (loadLibraryW == IntPtr.Zero)
            {
                MessageBox.Show("❌ Failed to get LoadLibraryW address.");
                VirtualFreeEx(hProcess, allocAddress, 0, MEM_RELEASE);
                CloseHandle(hProcess);
                return false;
            }

            IntPtr remoteThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryW, allocAddress, 0, out _);
            if (remoteThread == IntPtr.Zero)
            {
                MessageBox.Show("❌ Failed to create remote thread.");
                VirtualFreeEx(hProcess, allocAddress, 0, MEM_RELEASE);
                CloseHandle(hProcess);
                return false;
            }

            // Clean up
            CloseHandle(remoteThread);
            CloseHandle(hProcess);

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Exception: {ex.Message}");
            return false;
        }
    }
}
