// Note by the Author: 张菁菁
// This file is modified to use for Nobunaga's Ambition Online MultiLaunch. 
// 
// Source: http://social.msdn.microsoft.com/Forums/vstudio/en-US/c02fc640-a0a1-4e0d-a425-a37899c5e5d3/how-can-i-find-the-address-of-active-window-in-explorer

// The original Licence information：

//Guild Wars MultiLaunch - Safe and efficient way to launch multiple GWs.
//The Guild Wars executable is never modified, keeping you inline with the tos.
//
//Copyright (C) 2010 IMKey@GuildWarsGuru

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NolMultiLauncherAllServer
{
    public class HandleManager
    {
        #region Win32 API to be called.

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DuplicateHandle(IntPtr hSourceProcessHandle,
            IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle,
            uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, DuplicateOptions dwOption);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess,
          [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, UInt32 dwProcessID);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern NTSTATUS NtQueryInformationFile(IntPtr FileHandle,
          ref IO_STATUS_BLOCK IoStatusBlock, IntPtr FileInformation, int FileInformationLength,
          FILE_INFORMATION_CLASS FileInformationClass);

        [DllImport("ntdll.dll")]
        private static extern NTSTATUS NtQueryObject(IntPtr ObjectHandle, OBJECT_INFORMATION_CLASS ObjectInformationClass,
          IntPtr ObjectInformation, int ObjectInformationLength, out int ReturnLength);

        [DllImport("ntdll.dll")]
        private static extern NTSTATUS NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS SystemInformationClass,
          IntPtr SystemInformation, int SystemInformationLength, out int ReturnLength);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);
        #endregion

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, [MarshalAs(UnmanagedType.Bool)] bool bRepaint);
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        #region Structures

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct SYSTEM_HANDLE_INFORMATION
        {
            public UInt32 OwnerPID;
            public Byte ObjectType;
            public Byte HandleFlags;
            public UInt16 HandleValue;
            public UIntPtr ObjectPointer;
            public IntPtr AccessMask;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OBJECT_BASIC_INFORMATION
        {
            public UInt32 Attributes;
            public UInt32 GrantedAccess;
            public UInt32 HandleCount;
            public UInt32 PointerCount;
            public UInt32 PagedPoolUsage;
            public UInt32 NonPagedPoolUsage;
            public UInt32 Reserved1;
            public UInt32 Reserved2;
            public UInt32 Reserved3;
            public UInt32 NameInformationLength;
            public UInt32 TypeInformationLength;
            public UInt32 SecurityDescriptorLength;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreateTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IO_STATUS_BLOCK
        {
            public UInt32 Status;
            public UInt64 Information;
        }

        #endregion

        #region Enumerations

        /// <summary>
        ///     Special window handles
        /// </summary>
        public enum SpecialWindowHandles
        {
            // ReSharper disable InconsistentNaming
            /// <summary>
            ///     Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost window, the window loses its topmost status and is placed at the bottom of all other windows.
            /// </summary>
            HWND_TOP = 0,
            /// <summary>
            ///     Places the window above all non-topmost windows (that is, behind all topmost windows). This flag has no effect if the window is already a non-topmost window.
            /// </summary>
            HWND_BOTTOM = 1,
            /// <summary>
            ///     Places the window at the top of the Z order.
            /// </summary>
            HWND_TOPMOST = -1,
            /// <summary>
            ///     Places the window above all non-topmost windows. The window maintains its topmost position even when it is deactivated.
            /// </summary>
            HWND_NOTOPMOST = -2
            // ReSharper restore InconsistentNaming
        }

        [Flags]
        public enum SetWindowPosFlags : uint
        {
            // ReSharper disable InconsistentNaming

            /// <summary>
            ///     If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
            /// </summary>
            SWP_ASYNCWINDOWPOS = 0x4000,

            /// <summary>
            ///     Prevents generation of the WM_SYNCPAINT message.
            /// </summary>
            SWP_DEFERERASE = 0x2000,

            /// <summary>
            ///     Draws a frame (defined in the window's class description) around the window.
            /// </summary>
            SWP_DRAWFRAME = 0x0020,

            /// <summary>
            ///     Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
            /// </summary>
            SWP_FRAMECHANGED = 0x0020,

            /// <summary>
            ///     Hides the window.
            /// </summary>
            SWP_HIDEWINDOW = 0x0080,

            /// <summary>
            ///     Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
            /// </summary>
            SWP_NOACTIVATE = 0x0010,

            /// <summary>
            ///     Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
            /// </summary>
            SWP_NOCOPYBITS = 0x0100,

            /// <summary>
            ///     Retains the current position (ignores X and Y parameters).
            /// </summary>
            SWP_NOMOVE = 0x0002,

            /// <summary>
            ///     Does not change the owner window's position in the Z order.
            /// </summary>
            SWP_NOOWNERZORDER = 0x0200,

            /// <summary>
            ///     Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
            /// </summary>
            SWP_NOREDRAW = 0x0008,

            /// <summary>
            ///     Same as the SWP_NOOWNERZORDER flag.
            /// </summary>
            SWP_NOREPOSITION = 0x0200,

            /// <summary>
            ///     Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
            /// </summary>
            SWP_NOSENDCHANGING = 0x0400,

            /// <summary>
            ///     Retains the current size (ignores the cx and cy parameters).
            /// </summary>
            SWP_NOSIZE = 0x0001,

            /// <summary>
            ///     Retains the current Z order (ignores the hWndInsertAfter parameter).
            /// </summary>
            SWP_NOZORDER = 0x0004,

            /// <summary>
            ///     Displays the window.
            /// </summary>
            SWP_SHOWWINDOW = 0x0040,

            // ReSharper restore InconsistentNaming
        }

        //DuplicateHandle
        [Flags]
        private enum DuplicateOptions : uint
        {
            DUPLICATE_CLOSE_SOURCE = 0x00000001,
            DUPLICATE_SAME_ACCESS = 0x00000002
        }

        //OpenProcess
        [Flags]
        private enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        //NtQueryObject and NtQuerySystemInformation
        [Flags]
        private enum NTSTATUS : uint
        {
            STATUS_SUCCESS = 0x00000000,
            STATUS_INFO_LENGTH_MISMATCH = 0xC0000004
        } //partial enum, actual set is huge, google ntstatus.h

        //NtQueryObject
        [Flags]
        private enum OBJECT_INFORMATION_CLASS : uint
        {
            ObjectBasicInformation = 0,
            ObjectNameInformation = 1,
            ObjectTypeInformation = 2,
            ObjectAllTypesInformation = 3,
            ObjectHandleInformation = 4
        }

        //NtQuerySystemInformation
        [Flags]
        private enum SYSTEM_INFORMATION_CLASS : uint
        {
            SystemHandleInformation = 16
        } //partial enum, actual set is huge, google SYSTEM_INFORMATION_CLASS

        //NtQueryInformationFile
        [Flags]
        private enum FILE_INFORMATION_CLASS
        {
            FileNameInformation = 9
        } //partial enum, actual set is huge, google SYSTEM_INFORMATION_CLASS

        #endregion

        #region const variables
        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_SHOWMINNOACTIVATE = 7;
        private const int SW_RESTORE = 9;
        private const int SW_SHOWDEFAULT = 10;
        #endregion

        #region Functions

        /// <summary>
        /// Clears file locks on the GW_DAT file located at basePath.
        /// </summary>
        /// <param name="basePath">Full patch to gw.dat file</param>
        /// <returns></returns>
        /* Removed by Wei
         * This function is only for Guild War, it won't be used in Nobunaga Online.
        public static bool ClearDatLock(string basePath)
        {
            bool success = false;

            //take off the drive portion due to limitation in how killhandle works for file name
            string root = Directory.GetDirectoryRoot(basePath).Substring(0, 2);
            basePath = basePath.Replace(root, string.Empty);
            string fileToUnlock = basePath + "\\" + Program.GW_DAT;

            //get list of currently running system processes
            Process[] processList = Process.GetProcesses();

            foreach (Process i in processList)
            {
                //filter for guild wars ones
                if (i.ProcessName.Equals(Program.NOL_PROCESS_NAME, StringComparison.OrdinalIgnoreCase))
                {
                    if (HandleManager.KillHandle(i, fileToUnlock, true))
                    {
                        success = true;
                    }
                }
            }
            
            return success;
        }
        */
        /// <summary>
        /// Minimize all Nobunaga Online window
        /// </summary>
        /// 
        /// <returns></returns>
        public static bool MinimizeWindow(IntPtr hWnd)
        {
            if (!hWnd.Equals(IntPtr.Zero))
            {
                return ShowWindow(hWnd, SW_SHOWMINNOACTIVATE);
            }
            return false;   
        }
        /// <summary>
        /// Close all Nobunaga Online window
        /// </summary>
        /// 
        /// <returns></returns>
        public static bool CloseAllWindow()
        {
            bool success = false;

            //get list of currently running system processes
            Process[] processList = Process.GetProcesses();

            foreach(Process i in processList)
            {
                if(i.ProcessName.Equals(Program.NOL_SD_PROCESS_NAME, StringComparison.OrdinalIgnoreCase) ||
                    i.ProcessName.Equals(Program.NOL_HD_PROCESS_NAME, StringComparison.OrdinalIgnoreCase)||
                    i.ProcessName.Equals(Program.NOL_MOVIE_OVERLAY,StringComparison.OrdinalIgnoreCase))
                {
                    HandleManager.TerminateProcess(i.Handle, 0);
                }
            }

            return success;
        }
        /// <summary>
        /// Terminate process by handle.
        /// </summary>
        /// <returns></returns>
        public static bool TerminateProcessByHandle(IntPtr hProcess)
        {
            return HandleManager.TerminateProcess(hProcess, 0);
        }
        /// <summary>
        /// Kills NOL mutex is active processes.
        /// </summary>
        /// <returns></returns>
        public static bool ClearMutex()
        {
            //get list of currently running system processes
            Process[] processList = Process.GetProcesses();

            foreach (Process i in processList)
            {
                //filter for guild wars ones
                if (i.ProcessName.Equals(Program.NOL_SD_PROCESS_NAME, StringComparison.OrdinalIgnoreCase)||
                    i.ProcessName.Equals(Program.NOL_HD_PROCESS_NAME, StringComparison.OrdinalIgnoreCase)||
                    i.ProcessName.Equals(Program.NOL_MOVIE_OVERLAY, StringComparison.OrdinalIgnoreCase))
                {
                    return HandleManager.KillHandle(i, Program.NOL_MUTEX_NAME, false);
                }
            }

            return false;
        }
        public static bool CLearMutexInProc(Process p)
        {
            if (p != null)
            {
                return HandleManager.KillHandle(p, Program.NOL_MUTEX_NAME, false);
            }
            return false;
        }
        /// <summary>
        /// Kills the handle whose name contains the nameFragment.
        /// </summary>
        /// <param name="targetProcess"></param>
        /// <param name="handleName"></param>
        public static bool KillHandle(Process targetProcess, string handleName, bool isFile)
        {
            bool success = false;

            //pSysInfoBuffer is a pointer to unmanaged memory
            IntPtr pSysHandles = GetAllHandles();

            //sanity check
            if (pSysHandles == IntPtr.Zero) return success;

            //Assemble list of SYSTEM_HANDLE_INFORMATION for the specified target process
            List<SYSTEM_HANDLE_INFORMATION> processHandles = GetHandles(targetProcess, pSysHandles);

            //free pSysInfoBuffer buffer
            Marshal.FreeHGlobal(pSysHandles);

            //Iterate through handles which belong to target process and kill
            IntPtr hProcess = OpenProcess(ProcessAccessFlags.DupHandle, false, (UInt32)targetProcess.Id);

            foreach (SYSTEM_HANDLE_INFORMATION handleInfo in processHandles)
            {
                string name;

                if (isFile)
                {
                    name = GetFileName(handleInfo, hProcess);
                }
                else
                {
                    name = GetHandleName(handleInfo, hProcess);
                }

                if (name.Contains(handleName))
                {
                    if (CloseHandleEx(handleInfo.OwnerPID, new IntPtr(handleInfo.HandleValue)))
                    {
                        success = true;
                    }
                }
            }
            CloseHandle(hProcess);

            return success;
        }

        /// <summary>
        /// Closes a handle that is owned by another process.
        /// </summary>
        /// <param name="processID"></param>
        /// <param name="handleToClose"></param>
        private static bool CloseHandleEx(UInt32 processID, IntPtr handleToClose)
        {
            IntPtr hProcess = OpenProcess(ProcessAccessFlags.All, false, processID);

            //Kills handle by DUPLICATE_CLOSE_SOURCE option, source is killed while destinationHandle goes to null
            IntPtr x;
            bool success = DuplicateHandle(hProcess, handleToClose, IntPtr.Zero,
              out x, 0, false, DuplicateOptions.DUPLICATE_CLOSE_SOURCE);

            CloseHandle(hProcess);

            return success;
        }

        /// <summary>
        /// Convert UNICODE_STRING located at pStringBuffer to a managed string.
        /// </summary>
        /// <param name="pStringBuffer">Pointer to start of UNICODE_STRING struct.</param>
        /// <returns>Managed string.</returns>
        private static string ConvertToString(IntPtr pStringBuffer)
        {
            long baseAddress = pStringBuffer.ToInt64();

            //don't know why, 8 bytes for 32 bit platforms and 16 bytes for 64 bit
            int offset = IntPtr.Size * 2;

            string handleName = Marshal.PtrToStringUni(new IntPtr(baseAddress + offset));

            return handleName;
        }

        /// <summary>
        /// Retrieves all currently active handles for all system processes.
        /// There currently isn't a way to only get it for a specific process.
        /// This relies on NtQuerySystemInformation which exists in ntdll.dll.
        /// </summary>
        /// <returns>Unmanaged IntPtr to the handles (raw data, must be processed)</returns>
        private static IntPtr GetAllHandles()
        {
            int bufferSize = 0x10000;  //initial buffer size of 65536 bytes (initial estimate)
            int actualSize;       //will store size of actual data written to buffer

            //initial allocation
            IntPtr pSysInfoBuffer = Marshal.AllocHGlobal(bufferSize);

            //query for handles (priming call, since initial buffer size will probably not be big enough)
            NTSTATUS queryResult = NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemHandleInformation,
              pSysInfoBuffer, bufferSize, out actualSize);

            // Keep calling until buffer is large enough to fit all handles
            while (queryResult == NTSTATUS.STATUS_INFO_LENGTH_MISMATCH)
            {
                //deallocate space since we couldn't fit all the handles in
                Marshal.FreeHGlobal(pSysInfoBuffer);

                //double buffer size (we can't just use actualSize from last call since # of handles vary in time)
                bufferSize = bufferSize * 2;

                //allocate memory with increase buffer size
                pSysInfoBuffer = Marshal.AllocHGlobal(bufferSize); // It may cause a OutOfMemoryException, need to find out why By 张菁菁

                //query for handles
                queryResult = NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemHandleInformation,
                  pSysInfoBuffer, bufferSize, out actualSize);
            }

            if (queryResult == NTSTATUS.STATUS_SUCCESS)
            {
                return pSysInfoBuffer; //pSystInfoBuffer will be freed later
            }
            else
            {
                //other NTSTATUS, shouldn't happen
                Marshal.FreeHGlobal(pSysInfoBuffer);
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Filter out handles which belong to targetProcess.
        /// </summary>
        /// <param name="targetProcess">The process whose handles you want.</param>
        /// <param name="pAllHandles">Pointer to all the system handles.</param>
        /// <returns>List of handles owned by the targetProcess</returns>
        private static List<SYSTEM_HANDLE_INFORMATION> GetHandles(Process targetProcess, IntPtr pSysHandles)
        {
            List<SYSTEM_HANDLE_INFORMATION> processHandles = new List<SYSTEM_HANDLE_INFORMATION>();

            Int64 pBaseLocation = pSysHandles.ToInt64();  //base address
            Int64 currentOffset;              //offset from pBaseLocation
            IntPtr pLocation;                //current address

            SYSTEM_HANDLE_INFORMATION currentHandleInfo;

            //number of total system handles (should be okay for 64bit version too)
            int nHandles = Marshal.ReadInt32(pSysHandles);

            // Iterate through all system handles
            for (int i = 0; i < nHandles; i++)
            {
                //first (IntPtr.Size) bytes stores number of handles
                //data follows, each set is size of SYSTEM_HANDLE_INFORMATION
                currentOffset = IntPtr.Size + i * Marshal.SizeOf(typeof(SYSTEM_HANDLE_INFORMATION));

                //calculate intptr to new location
                pLocation = new IntPtr(pBaseLocation + currentOffset);

                // Create structure out of the memory block
                currentHandleInfo = (SYSTEM_HANDLE_INFORMATION)
                  Marshal.PtrToStructure(pLocation, typeof(SYSTEM_HANDLE_INFORMATION));

                // Add only handles which match the target process id
                if (currentHandleInfo.OwnerPID == (UInt32)targetProcess.Id)
                {
                    processHandles.Add(currentHandleInfo);
                }
            }

            return processHandles;
        }

        /// <summary>
        /// Queries for file name associated with handle.
        /// </summary>
        /// <param name="handleInfo">The handle info.</param>
        /// <param name="hProcess">Open handle to the process which owns that handle.</param>
        /// <returns></returns>
        private static string GetFileName(SYSTEM_HANDLE_INFORMATION handleInfo, IntPtr hProcess)
        {
            try
            {
                IntPtr thisProcess = Process.GetCurrentProcess().Handle;
                IntPtr handle;

                // Need to duplicate handle in this process to be able to access name
                DuplicateHandle(hProcess, new IntPtr(handleInfo.HandleValue), thisProcess,
                  out handle, 0, false, DuplicateOptions.DUPLICATE_SAME_ACCESS);

                // Setup buffer to store unicode string
                int bufferSize = 0x200; //512 bytes

                // Allocate unmanaged memory to store name
                IntPtr pFileNameBuffer = Marshal.AllocHGlobal(bufferSize);
                IO_STATUS_BLOCK ioStat = new IO_STATUS_BLOCK();

                NtQueryInformationFile(handle, ref ioStat, pFileNameBuffer, bufferSize, FILE_INFORMATION_CLASS.FileNameInformation);

                // Close this handle
                CloseHandle(handle);  //super important... almost missed this

                // offset=4 seems to work...
                int offset = 4;
                long pBaseAddress = pFileNameBuffer.ToInt64();

                // Do the conversion to managed type
                string fileName = Marshal.PtrToStringUni(new IntPtr(pBaseAddress + offset));

                // Release
                Marshal.FreeHGlobal(pFileNameBuffer);

                return fileName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Queries for name of handle.
        /// </summary>
        /// <param name="targetHandleInfo">The handle info.</param>
        /// <param name="hProcess">Open handle to the process which owns that handle.</param>
        /// <returns></returns>
        private static string GetHandleName(SYSTEM_HANDLE_INFORMATION targetHandleInfo, IntPtr hProcess)
        {
            //skip special NamedPipe handle (this may cause hang up with NtQueryObject function)
            if (targetHandleInfo.AccessMask.ToInt64() == 0x0012019F)
            {
                return String.Empty;
            }

            IntPtr thisProcess = Process.GetCurrentProcess().Handle;
            IntPtr handle;

            // Need to duplicate handle in this process to be able to access name
            DuplicateHandle(hProcess, new IntPtr(targetHandleInfo.HandleValue), thisProcess,
              out handle, 0, false, DuplicateOptions.DUPLICATE_SAME_ACCESS);

            // Setup buffer to store unicode string
            int bufferSize = GetHandleNameLength(handle);

            // Allocate unmanaged memory to store name
            IntPtr pStringBuffer = Marshal.AllocHGlobal(bufferSize);

            // Query to fill string buffer with name 
            NtQueryObject(handle, OBJECT_INFORMATION_CLASS.ObjectNameInformation, pStringBuffer, bufferSize, out bufferSize);

            // Close this handle
            CloseHandle(handle);  //super important... almost missed this

            // Do the conversion to managed type
            string handleName = ConvertToString(pStringBuffer);

            // Release
            Marshal.FreeHGlobal(pStringBuffer);

            return handleName;
        }

        /// <summary>
        /// Get size of the name info block for that handle.
        /// </summary>
        /// <param name="handle">Handle to process.</param>
        /// <returns></returns>
        private static int GetHandleNameLength(IntPtr handle)
        {
            int infoBufferSize = Marshal.SizeOf(typeof(OBJECT_BASIC_INFORMATION)); //size of OBJECT_BASIC_INFORMATION struct
            IntPtr pInfoBuffer = Marshal.AllocHGlobal(infoBufferSize);       //allocate

            // Query for handle's OBJECT_BASIC_INFORMATION
            NtQueryObject(handle, OBJECT_INFORMATION_CLASS.ObjectBasicInformation, pInfoBuffer, infoBufferSize, out infoBufferSize);

            // Map memory to structure
            OBJECT_BASIC_INFORMATION objInfo =
              (OBJECT_BASIC_INFORMATION)Marshal.PtrToStructure(pInfoBuffer, typeof(OBJECT_BASIC_INFORMATION));

            Marshal.FreeHGlobal(pInfoBuffer);  //release

            // If the handle has an empty name, we still need to give the buffer a size to map the UNICODE_STRING struct to.
            if (objInfo.NameInformationLength == 0)
            {
                return 0x100;  //reserve 256 bytes, since nameinfolength = 0 for filenames
            }
            else
            {
                return (int)objInfo.NameInformationLength;
            }
        }

        #endregion
    }
}
