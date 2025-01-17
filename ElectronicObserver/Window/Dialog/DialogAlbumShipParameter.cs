﻿using ElectronicObserver.Resource;
using ElectronicObserver.Resource.Record;
using System;
using System.Text;
using System.Windows.Forms;

namespace ElectronicObserver.Window.Dialog
{
    public partial class DialogAlbumShipParameter : Form
	{

		public DialogAlbumShipParameter()
		{
            this.InitializeComponent();
		}

		public DialogAlbumShipParameter(int shipID)
			: this()
		{

            this.InitView(shipID);
		}

		private void DialogAlbumShipParameter_Load(object sender, EventArgs e)
		{
			this.Icon = ResourceManager.ImageToIcon(ResourceManager.Instance.Icons.Images[(int)ResourceManager.IconContent.FormAlbumShip]);
		}


		private void InitView(int shipID)
		{

			var record = RecordManager.Instance.ShipParameter[shipID];

			if (record == null)
			{
				RecordManager.Instance.ShipParameter[shipID] = record = new ShipParameterRecord.ShipParameterElement();
			}


			var keys = RecordManager.Instance.ShipParameter.RecordHeader.Split(',');
			var values = record.SaveLine().Split(',');

            this.ParameterView.Rows.Clear();
			var rows = new DataGridViewRow[keys.Length];

			for (int i = 0; i < rows.Length; i++)
			{
				rows[i] = new DataGridViewRow();
				rows[i].CreateCells(this.ParameterView);
				rows[i].SetValues(keys[i], values[i]);
			}

			rows[0].ReadOnly = rows[1].ReadOnly = true;

            this.ParameterView.Rows.AddRange(rows);

		}



		private void ButtonOK_Click(object sender, EventArgs e)
		{

			try
			{

				var record = new ShipParameterRecord.ShipParameterElement();

				var sb = new StringBuilder();

				foreach (DataGridViewRow row in this.ParameterView.Rows)
				{
					sb.Append(row.Cells[this.ParameterView_Value.Index].Value + ",");
				}
				sb.Remove(sb.Length - 1, 1);

				record.LoadLine(sb.ToString());

				RecordManager.Instance.ShipParameter[record.ShipID] = record;


			}
			catch (Exception ex)
			{

				MessageBox.Show("파라미터 설정에 실패했습니다. \r\n" + ex.Message, "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}


            this.Close();
		}

		private void ButtonCancel_Click(object sender, EventArgs e)
		{
            this.Close();
		}

		private void DialogAlbumShipParameter_FormClosed(object sender, FormClosedEventArgs e)
		{
			ResourceManager.DestroyIcon(this.Icon);
		}
	}
}
