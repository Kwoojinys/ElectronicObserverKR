﻿using ElectronicObserver.Resource;
using ElectronicObserver.Utility;
using System;
using System.Windows.Forms;

namespace ElectronicObserver.Window.Dialog
{
    public partial class DialogVersion : Form
	{
		public DialogVersion()
		{
            this.InitializeComponent();

            this.TextVersion.Text = string.Format("{0} (ver. {1} - {2} Release)", SoftwareInformation.SoftwareNameKorean, SoftwareInformation.VersionKorean, SoftwareInformation.UpdateTime.ToString("d"));
		}

		private void TextAuthor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{

			System.Diagnostics.Process.Start("https://twitter.com/andanteyk");

		}

		private void ButtonClose_Click(object sender, EventArgs e)
		{

			this.Close();

		}

		private void TextInformation_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{

			System.Diagnostics.Process.Start("http://electronicobserver.blog.fc2.com/");

		}

        private void TextTranslator_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            System.Diagnostics.Process.Start("http://thelokis.egloos.com/");

        }

        private void Gall_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            System.Diagnostics.Process.Start("http://gall.dcinside.com/board/lists/?id=kancolle");

        }


        private void DialogVersion_Load(object sender, EventArgs e)
		{

			this.Icon = ResourceManager.Instance.AppIcon;
		}

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
