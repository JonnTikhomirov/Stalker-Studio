using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Stalker_Studio.Common;
using System.Diagnostics;
using System;
using System.Windows.Forms.Integration;
using System.Threading;
using Stalker_Studio.StalkerClass;

namespace Stalker_Studio.ViewModel
{
	class OGFViewModel : FileViewModel
	{
        private IntPtr _windowHandle = IntPtr.Zero;

        public OGFViewModel(OGFFile file) : base(file) 
        {
            Initialize();
        }
		public OGFViewModel(string fullName) : this(new OGFFile(fullName)) { }
		public OGFViewModel() : this(new OGFFile()) { }

        public override string Title
        {
            get 
            {
                return "OGF Tool "+ base.Title;
            }
        }

        protected override void OnSetMainControl()
        {
            if(_mainControl is WindowsFormsHost)
                _mainControl.Loaded += MainControl_Loaded;
        }

        private void MainControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_windowHandle != IntPtr.Zero)
                return;
            string pathEXE = $"{System.Windows.Forms.Application.StartupPath}\\AddonSoft\\OGF_Editor\\OGF tool.exe";
            Process process = Process.Start(pathEXE, $"\"{_file.FullName}\"");
            if (process == null)
                return;
            while (!SetHandle(process))
            {
                System.Threading.Thread.Sleep(50);
            }

            int wid = (int)(_mainControl as WindowsFormsHost).RenderSize.Width;
            int hei = (int)(_mainControl as WindowsFormsHost).RenderSize.Height;

            Thread th = new Thread(() =>
            {
                ApiWin.Native.SetWindowPos(_windowHandle, 0, -10, -30, wid + 20, hei + 39, ApiWin.Native.SWP_NOZORDER | ApiWin.Native.SWP_SHOWWINDOW);
            });
            th.Start();

            (sender as WindowsFormsHost).Loaded -= MainControl_Loaded;
        }

        private bool SetHandle(Process process)
        {          
            WindowsFormsHost panel_ogf = _mainControl as WindowsFormsHost;
            IntPtr studioHandle = panel_ogf.Child.Handle;

            if (process.MainWindowHandle != IntPtr.Zero)
            {
                if (ApiWin.Native.GetTitle(process.MainWindowHandle) == process.MainWindowTitle && !string.IsNullOrWhiteSpace(process.MainWindowTitle))
                {
                    ApiWin.Native.SetParent(process.MainWindowHandle, studioHandle);
                    _windowHandle = process.MainWindowHandle;

                    return true;
                }
            }
            return false;
        }

		private void Initialize() 
		{
            _commands.Clear();
		}
	}
}
