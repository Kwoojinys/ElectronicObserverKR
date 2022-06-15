﻿using DynaJson;
using ElectronicObserver.Utility.Storage;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace ElectronicObserver.Window.Support
{



    public static class WindowPlacementManager
	{

		#region 各種宣言

		[DllImport("user32.dll")]
		private static extern bool SetWindowPlacement(
			IntPtr hWnd,
			[In] ref WINDOWPLACEMENT lpwndpl);

		[DllImport("user32.dll")]
		private static extern bool GetWindowPlacement(
			IntPtr hWnd,
			out WINDOWPLACEMENT lpwndpl);


		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		public struct WINDOWPLACEMENT
		{
			public int length;
			public int flags;
			public SW showCmd;
			public POINT minPosition;
			public POINT maxPosition;
			public RECT normalPosition;
		}

		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;

			public POINT(int x, int y)
			{
				this.X = x;
				this.Y = y;
			}
		}

		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;

			public RECT(int left, int top, int right, int bottom)
			{
				this.Left = left;
				this.Top = top;
				this.Right = right;
				this.Bottom = bottom;
			}
		}

		public enum SW
		{
			HIDE = 0,
			SHOWNORMAL = 1,
			SHOWMINIMIZED = 2,
			SHOWMAXIMIZED = 3,
			SHOWNOACTIVATE = 4,
			SHOW = 5,
			MINIMIZE = 6,
			SHOWMINNOACTIVE = 7,
			SHOWNA = 8,
			RESTORE = 9,
			SHOWDEFAULT = 10,
		}



		[DataContract(Name = "WindowPlacementWrapper")]
		public sealed class WindowPlacementWrapper : DataStorage
		{

			[IgnoreDataMember]
			public WINDOWPLACEMENT RawData;

			[DataMember]
			public int flags
			{
				get { return this.RawData.flags; }
				set { this.RawData.flags = value; }
			}

			[DataMember]
			public int showCmd
			{
				get { return (int)this.RawData.showCmd; }
				set { this.RawData.showCmd = (SW)value; }
			}

			[DataMember]
			public int minPositionX
			{
				get { return this.RawData.minPosition.X; }
				set { this.RawData.minPosition.X = value; }
			}

			[DataMember]
			public int minPositionY
			{
				get { return this.RawData.minPosition.Y; }
				set { this.RawData.minPosition.Y = value; }
			}

			[DataMember]
			public int maxPositionX
			{
				get { return this.RawData.maxPosition.X; }
				set { this.RawData.maxPosition.X = value; }
			}

			[DataMember]
			public int maxPositionY
			{
				get { return this.RawData.maxPosition.Y; }
				set { this.RawData.maxPosition.Y = value; }
			}

			[DataMember]
			public int normalPositionLeft
			{
				get { return this.RawData.normalPosition.Left; }
				set { this.RawData.normalPosition.Left = value; }
			}

			[DataMember]
			public int normalPositionTop
			{
				get { return this.RawData.normalPosition.Top; }
				set { this.RawData.normalPosition.Top = value; }
			}

			[DataMember]
			public int normalPositionRight
			{
				get { return this.RawData.normalPosition.Right; }
				set { this.RawData.normalPosition.Right = value; }
			}

			[DataMember]
			public int normalPositionBottom
			{
				get { return this.RawData.normalPosition.Bottom; }
				set { this.RawData.normalPosition.Bottom = value; }
			}


			public WindowPlacementWrapper() : base()
			{
                this.Initialize();
			}

			public override void Initialize()
			{
                this.RawData = new WINDOWPLACEMENT();
                this.RawData.length = Marshal.SizeOf(this.RawData);
                this.RawData.flags = 0;
			}
		}
		#endregion



		public static string WindowPlacementConfigPath => @"Settings\WindowPlacement.json";


		[Obsolete]
		public static void LoadWindowPlacement(FormMain form, string path)
		{

			try
			{

				if (File.Exists(path))
				{

					string settings;

					using (StreamReader sr = new StreamReader(path))
					{
						settings = sr.ReadToEnd();
					}


					WindowPlacementWrapper wp = JsonObject.Parse(settings);

					if (wp.RawData.showCmd == SW.SHOWMINIMIZED)
						wp.RawData.showCmd = SW.SHOWNORMAL;

					SetWindowPlacement(form.Handle, ref wp.RawData);

				}

			}
			catch (Exception ex)
			{

				Utility.ErrorReporter.SendErrorReport(ex, "윈도우 상태의 복원에 실패했습니다.");

			}

		}


		public static void LoadWindowPlacement(FormMain form, Stream stream)
		{

			try
			{
				var wp = new WindowPlacementWrapper();
				wp = (WindowPlacementWrapper)wp.Load(stream);

				if (wp.RawData.showCmd == SW.SHOWMINIMIZED)
					wp.RawData.showCmd = SW.SHOWNORMAL;

				SetWindowPlacement(form.Handle, ref wp.RawData);


			}
			catch (Exception ex)
			{

				Utility.ErrorReporter.SendErrorReport(ex, "윈도우 상태의 복원에 실패했습니다.");
			}

		}


		[Obsolete]
		public static void SaveWindowPlacement(FormMain form, string path)
		{


			try
			{

				string parent = Directory.GetParent(path).FullName;
				if (!Directory.Exists(parent))
				{
					Directory.CreateDirectory(parent);
				}


				var wp = new WindowPlacementWrapper();



				GetWindowPlacement(form.Handle, out wp.RawData);


				string settings = JsonObject.Serialize(wp);

				using (StreamWriter sw = new StreamWriter(path))
				{

					sw.Write(settings);

				}


			}
			catch (Exception ex)
			{

				Utility.ErrorReporter.SendErrorReport(ex, "윈도우 상태의 저장에 실패했습니다.");
			}
		}



		public static void SaveWindowPlacement(FormMain form, Stream stream)
		{

			try
			{
				var wp = new WindowPlacementWrapper();

				GetWindowPlacement(form.Handle, out wp.RawData);

				wp.Save(stream);

			}
			catch (Exception ex)
			{

				Utility.ErrorReporter.SendErrorReport(ex, "윈도우 상태의 저장에 실패했습니다.");
			}
		}

	}



}
