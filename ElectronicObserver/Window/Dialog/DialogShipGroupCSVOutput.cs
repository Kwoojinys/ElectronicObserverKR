﻿using System;
using System.Windows.Forms;

namespace ElectronicObserver.Window.Dialog
{
    public partial class DialogShipGroupCSVOutput : Form
	{

		/// <summary>
		/// 出力フィルタを指定します。
		/// </summary>
		public enum FilterModeConstants
		{

			/// <summary>全て出力</summary>
			All,

			/// <summary>表示されている行のみ出力</summary>
			VisibleColumnOnly,
		}

		/// <summary>
		/// 出力フォーマットを指定します。
		/// </summary>
		public enum OutputFormatConstants
		{

			/// <summary>閲覧用</summary>
			User,

			/// <summary>データ用</summary>
			Data,
		}


		/// <summary>
		/// 出力ファイルのパス
		/// </summary>
		public string OutputPath
		{
			get { return this.TextOutputPath.Text; }
			set { this.TextOutputPath.Text = value; }
		}

		/// <summary>
		/// 出力フィルタ
		/// </summary>
		public FilterModeConstants FilterMode
		{
			get
			{
				if (this.RadioOutput_All.Checked)
					return FilterModeConstants.All;
				else
					return FilterModeConstants.VisibleColumnOnly;
			}
			set
			{
				switch (value)
				{
					case FilterModeConstants.All:
                        this.RadioOutput_All.Checked = true; break;

					case FilterModeConstants.VisibleColumnOnly:
                        this.RadioOutput_VisibleColumnOnly.Checked = true; break;
				}
			}
		}

		/// <summary>
		/// 出力フォーマット
		/// </summary>
		public OutputFormatConstants OutputFormat
		{
			get
			{
				if (this.RadioFormat_User.Checked)
					return OutputFormatConstants.User;
				else
					return OutputFormatConstants.Data;
			}
			set
			{
				switch (value)
				{
					case OutputFormatConstants.User:
                        this.RadioFormat_User.Checked = true; break;

					case OutputFormatConstants.Data:
                        this.RadioFormat_Data.Checked = true; break;
				}
			}
		}



		public DialogShipGroupCSVOutput()
		{
            this.InitializeComponent();

            this.DialogSaveCSV.InitialDirectory = Utility.Configuration.Config.Connection.SaveDataPath;

		}

		private void DialogShipGroupCSVOutput_Load(object sender, EventArgs e)
		{


		}

		private void ButtonOutputPathSearch_Click(object sender, EventArgs e)
		{

			if (this.DialogSaveCSV.ShowDialog() == DialogResult.OK)
			{

                this.TextOutputPath.Text = this.DialogSaveCSV.FileName;

			}

            this.DialogSaveCSV.InitialDirectory = null;

		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
            this.DialogResult = DialogResult.OK;
		}

		private void ButtonCancel_Click(object sender, EventArgs e)
		{
            this.DialogResult = DialogResult.Cancel;
		}

	}
}
