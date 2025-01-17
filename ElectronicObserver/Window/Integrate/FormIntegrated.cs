﻿using ElectronicObserver.Resource;
using ElectronicObserver.Utility;
using ElectronicObserver.Utility.Storage;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace ElectronicObserver.Window.Integrate
{

    /// <summary>
    /// 取り込むウィンドウのベースとなるフォーム
    /// </summary>
    public partial class FormIntegrate : DockContent
	{

		public readonly static String PREFIX = "FormIntegrated_";

		[DataContract(Name = "MatchControl")]
		public enum MatchControl
		{
			[EnumMember]
			Exact = 0,
			[EnumMember]
			Contains,
			[EnumMember]
			StartEnd,
			[EnumMember]
			Ignore
		}

		[DataContract(Name = "MatchString")]
		public class MatchString
		{

			[DataMember]
			public String Name { get; set; }

			[DataMember]
			public MatchControl MatchControl { get; set; }

			public MatchString(String name, MatchControl match)
			{
                this.Name = name;
                this.MatchControl = match;
			}

			public bool Match(String name)
			{
				switch (this.MatchControl)
				{
					case MatchControl.Exact:
						return (name == this.Name);
					case MatchControl.Contains:
						return name.Contains(this.Name);
					case MatchControl.StartEnd:
						return name.StartsWith(this.Name) || name.EndsWith(this.Name);
					case MatchControl.Ignore:
						return true;
				}
				throw new NotImplementedException("サポートされていないMatchControl");
			}
		}

		[DataContract(Name = "WindowInfo")]
		public sealed class WindowInfo : DataStorage
		{

			[DataMember]
			public String CurrentTitle { get; set; }

			[DataMember]
			public MatchString Title { get; set; }

			[DataMember]
			public MatchString ClassName { get; set; }

			[DataMember]
			public MatchString ProcessFilePath { get; set; }


			public WindowInfo()
			{
                this.Initialize();
			}

			public override void Initialize()
			{
			}

			public bool Match(String title, String className, String filePath)
			{
				return this.Title.Match(title) &&
                    this.ClassName.Match(className) &&
                    this.ProcessFilePath.Match(filePath);
			}
		}

		private static String[] MATCH_COMBO_ITEMS = new String[] {
				  "완전일치",
				  "포함",
				  "전,후 일치",
				  "조건 무시"
		};

		private FormMain parent;

		/// <summary>
		/// 次のウィンドウキャプチャ時に必要な情報
		/// </summary>
		WindowInfo WindowData
		{
			get
			{
				WindowInfo info = new WindowInfo
				{
					CurrentTitle = Text,
					Title = new MatchString(this.titleTextBox.Text,
					(MatchControl)this.titleComboBox.SelectedIndex),
					ClassName = new MatchString(this.classNameTextBox.Text,
					(MatchControl)this.classNameComboBox.SelectedIndex),
					ProcessFilePath = new MatchString(this.fileNameTextBox.Text,
					(MatchControl)this.fileNameComboBox.SelectedIndex)
				};

				return info;
			}
			set
			{
                this.Text = value.CurrentTitle;
                //TabText = value.CurrentTitle.Length > 16 ? value.CurrentTitle.Substring( 0, 16 ) + "..." : value.CurrentTitle;
                this.titleTextBox.Text = value.Title.Name;
                this.titleComboBox.SelectedIndex = (int)value.Title.MatchControl;
                this.classNameTextBox.Text = value.ClassName.Name;
                this.classNameComboBox.SelectedIndex = (int)value.ClassName.MatchControl;
                this.fileNameTextBox.Text = value.ProcessFilePath.Name;
                this.fileNameComboBox.SelectedIndex = (int)value.ProcessFilePath.MatchControl;
			}
		}

		private IntPtr attachingWindow;

		// 戻すときに必要になる情報
		private uint origStyle;
		private IntPtr origOwner;
		private WinAPI.RECT origWindowRect;
		private IntPtr origMenu;

		public FormIntegrate(FormMain parent)
		{
            this.InitializeComponent();

			this.parent = parent;

            this.windowCaptureButton.Image = ResourceManager.Instance.Icons.Images[(int)ResourceManager.IconContent.FormWindowCapture];

            this.titleComboBox.Items.AddRange(MATCH_COMBO_ITEMS);
            this.classNameComboBox.Items.AddRange(MATCH_COMBO_ITEMS);
            this.fileNameComboBox.Items.AddRange(MATCH_COMBO_ITEMS);

            this.TabPageContextMenuStrip = this.tabContextMenu;

            Configuration.Instance.ConfigurationChanged += this.ConfigurationChanged;
            this.ConfigurationChanged();

			parent.WindowCapture.AddCapturedWindow(this);
		}

		void ConfigurationChanged()
		{
            this.Font = Configuration.Config.UI.MainFont;
		}

        private static int RunProgram(WindowInfo info)
        {
            Process P = new Process();

            if (info.ProcessFilePath.Name.Contains("NOTEPAD"))
            {
                int ProgramNameIndex = info.Title.Name.LastIndexOf(".txt");
                string ProgramName = info.Title.Name.Substring(0, ProgramNameIndex) + ".txt";
                P.StartInfo.FileName = "NOTEPAD.EXE";
                P.StartInfo.Arguments = ProgramName;
                P.Start();
                return P.StartTime.Day;
            }
            else
            {
                P.StartInfo.FileName = info.ProcessFilePath.Name;
                P.Start();
                return P.StartTime.Day;
            }
		}


		/// <summary>
		/// PersistStringから復元
		/// </summary>
		public static FormIntegrate FromPersistString(FormMain parent, String str)
		{
			WindowInfo info = new WindowInfo();
			info = (WindowInfo)info.Load(new StringReader(str.Substring(PREFIX.Length)));
			FormIntegrate form = new FormIntegrate(parent)
			{
				WindowData = info
			};

            try
            {
                int Run = RunProgram(info);
            } catch (Exception e)
            {
                Logger.Add(LogType.Error, "윈도우 캡쳐에 실패했습니다. : " + e.Message);
            }

            return form;
		}

		private static string GetMainModuleFilepath(int processId)
		{
			// System.Diagnostics.Processからだと64bit/32bitの壁を超えられないのでWMIで取得
			string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
			using (var searcher = new ManagementObjectSearcher(wmiQueryString))
			{
				using (var results = searcher.Get())
				{
					foreach (ManagementObject mo in results)
					{
						return (string)mo["ExecutablePath"];
					}
				}
			}
			return null;
		}

		/// <summary>
		/// WindowDataにマッチするウィンドウを探す
		/// </summary>
		private IntPtr FindWindow()
		{
			StringBuilder className = new StringBuilder(256);
			StringBuilder windowText = new StringBuilder(256);
			IntPtr result = IntPtr.Zero;
			int currentProcessId = Process.GetCurrentProcess().Id;
			WindowInfo info = this.WindowData;

			WinAPI.EnumWindows((WinAPI.EnumWindowsDelegate)((hWnd, lparam) =>
			{
				WinAPI.GetClassName(hWnd, className, className.Capacity);
				WinAPI.GetWindowText(hWnd, windowText, windowText.Capacity);
				WinAPI.GetWindowThreadProcessId(hWnd, out uint processId);
				if (info.ClassName.Match(className.ToString()) &&
					info.Title.Match(windowText.ToString()) &&
					WinAPI.IsWindowVisible(hWnd) &&
					processId != currentProcessId)
				{
					String fileName = GetMainModuleFilepath((int)processId);
					if (info.ProcessFilePath.Match(fileName))
					{
						result = hWnd;
						return false;
					}
				}
				return true;
			}), IntPtr.Zero);

			return result;
		}

		/// <summary>
		/// ウィンドウを取り込む
		/// </summary>
		public bool Grab()
		{


            if (this.attachingWindow != IntPtr.Zero)
			{
				// 既にアタッチ済み
				return true;
			}
			IntPtr hWnd = this.FindWindow();
			if (hWnd != IntPtr.Zero)
			{
                this.Attach(hWnd, false);
				return true;
			}

            this.infoLabel.Text = "창을 찾을 수 없습니다.";
			return false;
		}

		private static WindowInfo WindowInfoFromHandle(IntPtr hWnd)
		{
			WindowInfo info = new WindowInfo();
			StringBuilder sb = new StringBuilder(256);

			WinAPI.GetClassName(hWnd, sb, sb.Capacity);
			info.ClassName = new MatchString(sb.ToString(), MatchControl.Exact);

			WinAPI.GetWindowText(hWnd, sb, sb.Capacity);
			info.Title = new MatchString(sb.ToString(), MatchControl.Exact);

			WinAPI.GetWindowThreadProcessId(hWnd, out uint processId);
			String fileName = GetMainModuleFilepath((int)processId);
			info.ProcessFilePath = new MatchString(fileName, MatchControl.Exact);

			info.CurrentTitle = info.Title.Name;

            if (info.ProcessFilePath.Name.Contains("NOTEPAD"))
            {
                info.ProcessFilePath.MatchControl = MatchControl.Ignore;
                info.ClassName.MatchControl = MatchControl.Ignore;
            }

                return info;
		}

		private void Attach(IntPtr hWnd, bool showFloating)
		{

			if (this.attachingWindow != IntPtr.Zero)
			{
				if (this.attachingWindow == hWnd)
				{
					// 既にアタッチ済み
					return;
				}
                this.Detach();
			}
			else
			{
                this.settingPanel.Visible = false;
                this.StripMenu_Detach.Enabled = true;
			}

            this.origStyle = unchecked((uint)(long)WinAPI.GetWindowLong(hWnd, WinAPI.GWL_STYLE));
            this.origOwner = WinAPI.GetWindowLong(hWnd, WinAPI.GWL_HWNDPARENT);

			// ターゲットが最大化されていたら戻す
			if ((this.origStyle & WinAPI.WS_MAXIMIZE) != 0)
			{
                this.origStyle &= unchecked((uint)~WinAPI.WS_MAXIMIZE);
				WinAPI.SetWindowLong(hWnd, WinAPI.GWL_STYLE, new IntPtr(unchecked((int)this.origStyle)));
			}

			// キャプションを設定
			StringBuilder stringBuilder = new StringBuilder(32);
			WinAPI.GetWindowText(hWnd, stringBuilder, stringBuilder.Capacity);
            this.Text = stringBuilder.ToString();

			// アイコンを設定
			IntPtr hicon = WinAPI.SendMessage(hWnd, WinAPI.WM_GETICON, (IntPtr)WinAPI.ICON_SMALL, IntPtr.Zero);
			if (hicon == IntPtr.Zero)
			{
				hicon = WinAPI.GeClassLong(hWnd, WinAPI.GCLP_HICON);
			}
			if (hicon != IntPtr.Zero)
			{
				this.Icon = Icon.FromHandle(hicon);
			}

            // メニューを取得
            this.origMenu = WinAPI.GetMenu(hWnd);

			WinAPI.GetWindowRect(hWnd, out this.origWindowRect);

			if (showFloating)
			{
                // このウィンドウの大きさ・位置を設定
                this.Show(this.parent.MainPanel, new Rectangle(
                    this.origWindowRect.left,
                    this.origWindowRect.top,
                    this.origWindowRect.right - this.origWindowRect.left,
                    this.origWindowRect.bottom - this.origWindowRect.top));
			}

			// ターゲットを子ウィンドウに設定
			uint newStyle = this.origStyle;
			newStyle &= unchecked((uint)~(WinAPI.WS_POPUP |
				WinAPI.WS_CAPTION |
				WinAPI.WS_BORDER |
				WinAPI.WS_THICKFRAME |
				WinAPI.WS_MINIMIZEBOX |
				WinAPI.WS_MAXIMIZEBOX));
			newStyle |= WinAPI.WS_CHILD;
			WinAPI.SetWindowLong(hWnd, WinAPI.GWL_STYLE, new IntPtr(unchecked((int)newStyle)));
			WinAPI.SetParent(hWnd, this.Handle);
			WinAPI.MoveWindow(hWnd, 0, 0, this.Width, this.Height, true);

			this.attachingWindow = hWnd;
		}

		private void InternalDetach()
		{
			if (this.attachingWindow != IntPtr.Zero)
			{
				WinAPI.SetParent(this.attachingWindow, IntPtr.Zero);
				WinAPI.SetWindowLong(this.attachingWindow, WinAPI.GWL_STYLE, new IntPtr(unchecked((int)this.origStyle)));
				WinAPI.SetWindowLong(this.attachingWindow, WinAPI.GWL_HWNDPARENT, this.origOwner);
				WinAPI.SetMenu(this.attachingWindow, this.origMenu);
				WinAPI.MoveWindow(this.attachingWindow, this.origWindowRect.left, this.origWindowRect.top,
                    this.origWindowRect.right - this.origWindowRect.left,
                    this.origWindowRect.bottom - this.origWindowRect.top, true);
                this.attachingWindow = IntPtr.Zero;

			}
		}

		/// <summary>
		/// ウィンドウを開放する
		/// </summary>
		public void Detach()
		{
			if (this.attachingWindow != IntPtr.Zero)
			{
                this.InternalDetach();
                this.settingPanel.Visible = true;
                this.StripMenu_Detach.Enabled = false;
                this.infoLabel.Text = "창을 개방 했습니다.";
			}
		}

		/// <summary>
		/// ウィンドウを元の場所を維持しながら取り込む
		/// </summary>
		public void Show(IntPtr hWnd)
		{
            this.Attach(hWnd, true);
            this.WindowData = WindowInfoFromHandle(hWnd);
		}

		private void FormIntegrated_FormClosing(object sender, FormClosingEventArgs e)
		{
            this.InternalDetach();
            Configuration.Instance.ConfigurationChanged -= this.ConfigurationChanged;
		}

		private void FormIntegrated_Resize(object sender, EventArgs e)
		{
			if (this.attachingWindow != IntPtr.Zero)
			{
				Size size = this.ClientSize;
				WinAPI.MoveWindow(this.attachingWindow, 0, 0, size.Width, size.Height, true);
			}
		}

		protected override string GetPersistString()
		{
			StringBuilder stringBuilder = new StringBuilder();
            this.WindowData.Save(stringBuilder);
			return PREFIX + stringBuilder.ToString();
		}

		private void integrateButton_Click(object sender, EventArgs e)
		{
            this.Grab();
		}

		private void windowCaptureButton_WindowCaptured(IntPtr hWnd)
		{

			int capacity = WinAPI.GetWindowTextLength(hWnd) * 2;
			StringBuilder stringBuilder = new StringBuilder(capacity);
			WinAPI.GetWindowText(hWnd, stringBuilder, stringBuilder.Capacity);

			if (MessageBox.Show(stringBuilder.ToString() + "\r\n" + FormWindowCapture.WARNING_MESSAGE,
				"윈도우 캡쳐 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
				== DialogResult.Yes)
			{

                this.Attach(hWnd, false);
                this.WindowData = WindowInfoFromHandle(hWnd);
			}
		}

		private void StripMenu_Detach_Click(object sender, EventArgs e)
		{
            this.Detach();
		}


	}

}
