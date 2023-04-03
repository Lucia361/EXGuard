﻿using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EXGuard.Core.EXECProtections
{
    internal static class AntiWebDebuggers_Runtime
    {
		[DllImport("User32.dll", EntryPoint = "MessageBox", CharSet = CharSet.Unicode)]
		static extern int MessageBox(IntPtr h, string m, string c, int type);

		[DllImport("user32.dll", EntryPoint = "SetWindowText")]
		static extern int SetWindowText(IntPtr hWnd, string text);

		[DllImport("User32.dll", EntryPoint = "SendMessage")]
		static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

		[DllImport("user32.dll", EntryPoint = "FindWindowEx")]
		static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		static void Initialize()
		{
			var thread = new Thread(Worker);
			thread.IsBackground = true;
			thread.Start(null);
		}

		static void Worker(object thread)
		{
			var th = thread as Thread;

			if (th == null)
			{
				th = new Thread(Worker);
				th.IsBackground = true;
				th.Start(Thread.CurrentThread);

				Thread.Sleep(500);
			}

			while (true)
			{
				Process[] processList = Process.GetProcesses();

				if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Programs\\Fiddler\\App.ico"))
				{
					Process notepad = Process.Start(new ProcessStartInfo("notepad.exe"));
					if (notepad != null)
					{
						var title = "Web Debugger Detector";
						var message = "Web Debugger has been detected.";

						notepad.WaitForInputIdle();

						if (!string.IsNullOrEmpty(title))
							SetWindowText(notepad.MainWindowHandle, title);

						if (!string.IsNullOrEmpty(message))
						{
							IntPtr child = FindWindowEx(notepad.MainWindowHandle, new IntPtr(0), "Edit", null);
							SendMessage(child, 0x000C, 0, message);
						}
					}

					Thread.Sleep(200);

					Process.GetCurrentProcess().Kill();
				}

				foreach (Process process in processList)
                {
					if (process.ProcessName.Contains("HTTPDebuggerUI") || process.ProcessName.Contains("Fiddler") ||
						process.MainWindowTitle.Contains("HTTPDebuggerUI") || process.MainWindowTitle.Contains("HTTPDebuggerSvc") ||
						process.MainWindowTitle.Contains("HTTP Debugger") || process.MainWindowTitle.Contains("HTTP Debugger") ||
						process.MainWindowTitle.Contains("HTTP HTTP Debugger (32 bit") || process.MainWindowTitle.Contains("HTTP HTTP Debugger (64 bit"))
					{
						Process notepad = Process.Start(new ProcessStartInfo("notepad.exe"));
						if (notepad != null)
						{
							var title = "Web Debugger Detector";
							var message = "Web Debugger has been detected.";

							notepad.WaitForInputIdle();

							if (!string.IsNullOrEmpty(title))
								SetWindowText(notepad.MainWindowHandle, title);

							if (!string.IsNullOrEmpty(message))
							{
								IntPtr child = FindWindowEx(notepad.MainWindowHandle, new IntPtr(0), "Edit", null);
								SendMessage(child, 0x000C, 0, message);
							}
						}

						Thread.Sleep(200);

						Process.GetCurrentProcess().Kill();
					}
				}

				if (!th.IsAlive)
					Process.GetCurrentProcess().Kill();

				Thread.Sleep(5000);
			}
		}
	}
}
