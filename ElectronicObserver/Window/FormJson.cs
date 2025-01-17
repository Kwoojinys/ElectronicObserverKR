﻿using DynaJson;
using ElectronicObserver.Observer;
using ElectronicObserver.Resource;
using ElectronicObserver.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace ElectronicObserver.Window
{
    public partial class FormJson : DockContent
	{


		// yyyyMMdd_hhmmssff[S|Q]@api_path.json
		private static readonly Regex FileNamePattern = new Regex(@"\d{8}_\d{8}([SQ])@(.*)\.json$", RegexOptions.Compiled);
		private const string AutoUpdateDisabledMessage = "<自動更新が無効になっています。Configから有効化してください。>";

		private Regex _apiPattern;

		private string _currentAPIPath;


		public FormJson(FormMain parent)
		{
            this.InitializeComponent();


            this.ConfigurationChanged();

            this.Icon = ResourceManager.ImageToIcon(ResourceManager.Instance.Icons.Images[(int)ResourceManager.IconContent.FormJson]);

            this.ApplyLockLayoutState();
        }

		private void FormJson_Load(object sender, EventArgs e)
		{

			var o = APIObserver.Instance;

			o.RequestReceived += this.RequestReceived;
			o.ResponseReceived += this.ResponseReceived;

			Utility.Configuration.Instance.ConfigurationChanged += this.ConfigurationChanged;
            this.ApplyLockLayoutState();
		}


		void RequestReceived(string apiname, dynamic data)
		{

			if (!this.AutoUpdate.Checked)
				return;

			if (this._apiPattern != null && !this._apiPattern.Match(apiname).Success)
				return;


			LoadRequest(apiname, data);
		}

		void ResponseReceived(string apiname, dynamic data)
		{

			if (!this.AutoUpdate.Checked)
				return;

			if (this._apiPattern != null && !this._apiPattern.Match(apiname).Success)
				return;


			LoadResponse(apiname, data);
		}



		private void LoadRequest(string apiname, Dictionary<string, string> data)
		{

            this.JsonRawData.Text = apiname + " : Request\r\n" + string.Join("\r\n", data.Select(p => p.Key + "=" + p.Value));


			if (!this.UpdatesTree.Checked)
				return;


            this.JsonTreeView.BeginUpdate();


            this.JsonTreeView.Nodes.Clear();
            this.JsonTreeView.Nodes.Add(apiname);

			TreeNode root = new TreeNode("<Request> : {" + data.Count + "}")
			{
				Name = "<Request>"
			};
			root.Nodes.AddRange(data.Select(e => new TreeNode(e.Key + " : " + e.Value)).ToArray());

            this.JsonTreeView.Nodes.Add(root);


            this.JsonTreeView.EndUpdate();
            this._currentAPIPath = apiname;
		}

		private void LoadResponse(string apiname, dynamic data)
		{


            this.JsonRawData.Text = (this._currentAPIPath == apiname ? this.JsonRawData.Text + "\r\n\r\n" : "") + apiname + " : Response\r\n" + (data == null ? "" : data.ToString());

			if (!this.UpdatesTree.Checked)
				return;


            this.JsonTreeView.BeginUpdate();


			if (this.JsonTreeView.Nodes.Count == 0 || this.JsonTreeView.Nodes[0].Text != apiname)
			{
                this.JsonTreeView.Nodes.Clear();
                this.JsonTreeView.Nodes.Add(apiname);
			}

			var node = CreateNode("<Response>", data);
			CreateChildNode(node);
            this.JsonTreeView.Nodes.Add(node);


            this.JsonTreeView.EndUpdate();
            this._currentAPIPath = apiname;
		}





		private void LoadFromFile(string path)
		{


			var match = FileNamePattern.Match(path);

			if (match.Success)
			{

				try
				{

					using (var sr = new StreamReader(path))
					{

						string data = sr.ReadToEnd();


						if (match.Groups[1].Value == "Q")
						{
							// request

							var parsedData = new Dictionary<string, string>();
							data = System.Web.HttpUtility.UrlDecode(data);

							foreach (string unit in data.Split("&".ToCharArray()))
							{
								string[] pair = unit.Split("=".ToCharArray());
								parsedData.Add(pair[0], pair[1]);
							}

                            this.LoadRequest(match.Groups[2].Value.Replace('@', '/'), parsedData);


						}
						else if (match.Groups[1].Value == "S")
						{
							//response

							int head = data.IndexOfAny("[{".ToCharArray());
							if (head == -1)
								throw new ArgumentException("JSON 의 시작 문자를 찾을 수 없습니다.");
							data = data.Substring(head);

							LoadResponse(match.Groups[2].Value.Replace('@', '/'), JsonObject.Parse(data));

						}

					}

				}
				catch (Exception ex)
				{

					Utility.ErrorReporter.SendErrorReport(ex, "JSON 데이터 로딩에 실패했습니다.");
				}


			}
		}


		private void CreateChildNode(TreeNode root)
		{
			dynamic json = root.Tag;

			if (json == null || !(json is JsonObject))
			{
				return;

			}
			else if (json.IsArray)
			{
				foreach (var elem in json)
				{
					root.Nodes.Add(CreateNode("", elem));
				}

			}
			else if (json.IsObject)
			{
				foreach (KeyValuePair<string, dynamic> elem in json)
				{
					root.Nodes.Add(CreateNode(elem.Key, elem.Value));
				}
			}
		}

		private TreeNode CreateNode(string name, dynamic data)
		{
			TreeNode node = new TreeNode
			{
				Tag = data,
				Name = name,
				Text = string.IsNullOrEmpty(name) ? "" : (name + " : ")
			};

			if (data == null)
			{
				node.Text += "null";

			}
			else if (data is string)
			{
				node.Text += "\"" + data + "\"";

			}
			else if (data is bool || data is double)
			{
				node.Text += data.ToString();

			}
			else if (data.IsArray)
			{
				int count = 0;
				foreach (var elem in data)
					count++;
				node.Text += "[" + count + "]";

			}
			else if (data.IsObject)
			{
				int count = 0;
				foreach (var elem in data)
					count++;
				node.Text += "{" + count + "}";

			}
			else
			{
				throw new NotImplementedException();

			}

			return node;
		}


		// ノードが展開されたときに、孫ノードを生成する(子ノードは生成済みという前提)
		// 子ノードまでだと [+] [-] が表示されないため孫まで生成しておく
		private void JsonTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{

			// Checked は展開済みフラグ :(
			if (e.Node.Checked)
				return;

			foreach (TreeNode child in e.Node.Nodes)
			{
                this.CreateChildNode(child);
			}

			e.Node.Checked = true;
		}



		void ConfigurationChanged()
		{

			var c = Utility.Configuration.Config;

            this.Font = this.tabControl1.Font = c.UI.MainFont;

            this.AutoUpdate.Checked = c.FormJson.AutoUpdate;
            this.UpdatesTree.Checked = c.FormJson.UpdatesTree;
            this.AutoUpdateFilter.Text = c.FormJson.AutoUpdateFilter;

            this.ForeColor = Utility.ThemeManager.GetColor(Utility.ThemeColors.MainFontColor);
            this.BackColor = Utility.ThemeManager.GetColor(Utility.ThemeColors.BackgroundColor);

            this.JsonTreeView.Nodes.Clear();

			if (!this.AutoUpdate.Checked || !this.UpdatesTree.Checked)
                this.JsonTreeView.Nodes.Add(AutoUpdateDisabledMessage);


			try
			{
                this._apiPattern = new Regex(c.FormJson.AutoUpdateFilter);
                this.AutoUpdateFilter.BackColor = SystemColors.Window;

			}
			catch (Exception)
			{
                this.AutoUpdateFilter.BackColor = Color.MistyRose;
                this._apiPattern = null;
			}

            this.ApplyLockLayoutState();
        }


		private void tabControl1_DragEnter(object sender, DragEventArgs e)
		{

			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Copy;
			else
				e.Effect = DragDropEffects.None;

		}

		private void tabControl1_DragDrop(object sender, DragEventArgs e)
		{

			foreach (string path in ((string[])e.Data.GetData(DataFormats.FileDrop)).OrderBy(s => s))
                this.LoadFromFile(path);

		}


		private void UpdatesTree_CheckedChanged(object sender, EventArgs e)
		{

            this.JsonTreeView.Nodes.Clear();

			if (!this.AutoUpdate.Checked || !this.UpdatesTree.Checked)
                this.JsonTreeView.Nodes.Add(AutoUpdateDisabledMessage);


			Utility.Configuration.Config.FormJson.UpdatesTree = this.UpdatesTree.Checked;
		}

		private void AutoUpdate_CheckedChanged(object sender, EventArgs e)
		{

            this.JsonTreeView.Nodes.Clear();

			if (!this.AutoUpdate.Checked || !this.UpdatesTree.Checked)
                this.JsonTreeView.Nodes.Add(AutoUpdateDisabledMessage);


			Utility.Configuration.Config.FormJson.AutoUpdate = this.AutoUpdate.Checked;
		}


		private void AutoUpdateFilter_Validated(object sender, EventArgs e)
		{
			var c = Utility.Configuration.Config.FormJson;
			c.AutoUpdateFilter = this.AutoUpdateFilter.Text;

			try
			{
                this._apiPattern = new Regex(c.AutoUpdateFilter);
                this.AutoUpdateFilter.BackColor = SystemColors.Window;

			}
			catch (Exception)
			{
                this.AutoUpdateFilter.BackColor = Color.MistyRose;
                this._apiPattern = null;
			}
		}


		private void TreeContextMenu_Expand_Click(object sender, EventArgs e)
		{
            this.JsonTreeView.SelectedNode.ExpandAll();
		}
		private void TreeContextMenu_Shrink_Click(object sender, EventArgs e)
		{
            this.JsonTreeView.SelectedNode.Collapse();
		}
		private void TreeContextMenu_ShrinkParent_Click(object sender, EventArgs e)
		{
			var node = this.JsonTreeView.SelectedNode.Parent;
			if (node != null)
				node.Collapse();
		}


		private void TreeContextMenu_Opening(object sender, CancelEventArgs e)
		{

			var root = this.JsonTreeView.SelectedNode;
			dynamic json = root.Tag;

			// root is array, children > 0, root[0](=child) is object or array
			if (
				root.GetNodeCount(false) > 0 &&
				json != null && json is JsonObject && json.IsArray &&
				root.FirstNode.Tag != null && root.FirstNode.Tag is JsonObject && (((dynamic)root.FirstNode.Tag).IsArray || ((dynamic)root.FirstNode.Tag).IsObject))
			{
                this.TreeContextMenu_OutputCSV.Enabled = true;

			}
			else
			{
                this.TreeContextMenu_OutputCSV.Enabled = false;
			}

		}

		private void TreeContextMenu_OutputCSV_Click(object sender, EventArgs e)
		{

			if (this.CSVSaver.ShowDialog() == DialogResult.OK)
			{

				try
				{

					using (var sw = new StreamWriter(this.CSVSaver.FileName, false, Utility.Configuration.Config.Log.FileEncoding))
					{

						var root = this.JsonTreeView.SelectedNode;

						sw.WriteLine(BuildCSVHeader(new StringBuilder(), "", ((dynamic)root.Tag)[0]).ToString());

						foreach (dynamic elem in (dynamic)root.Tag)
						{
							sw.WriteLine(BuildCSVContent(new StringBuilder(), elem).ToString());
						}
					}


				}
				catch (Exception ex)
				{
					Utility.ErrorReporter.SendErrorReport(ex, "JSON: CSV 출력에 실패했습니다.");
					MessageBox.Show("JSON: CSV 출력에 실패했습니다.\r\n" + ex.Message, "출력에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}


			}

		}


		private StringBuilder BuildCSVHeader(StringBuilder sb, string currentPath, dynamic data)
		{

			if (data is JsonObject)
			{

				if (data.IsObject)
				{
                    foreach (KeyValuePair<string, dynamic> p in data)
                    {
                        BuildCSVHeader(sb, currentPath + "." + p.Key, p.Value);
                    }
                    return sb;

				}
				else if (data.IsArray)
				{
					int index = 0;
					foreach (dynamic elem in data)
					{
						BuildCSVHeader(sb, currentPath + "[" + index + "]", elem);
						index++;
					}
					return sb;

				}
			}

			sb.Append(currentPath).Append(",");
			return sb;
		}

		private StringBuilder BuildCSVContent(StringBuilder sb, dynamic data)
		{

			if (data is JsonObject)
			{

				if (data.IsObject)
				{
					foreach (string p in data.GetDynamicMemberNames())
					{
						BuildCSVContent(sb, data[p]);
					}
					return sb;

				}
				else if (data.IsArray)
				{
					foreach (dynamic elem in data)
					{
						BuildCSVContent(sb, elem);
					}
					return sb;

				}
			}

			sb.Append(data).Append(",");
			return sb;
		}


		private void TreeContextMenu_CopyToClipboard_Click(object sender, EventArgs e)
		{
			if (this.JsonTreeView.SelectedNode != null && this.JsonTreeView.SelectedNode.Tag != null)
			{
				Clipboard.SetData(DataFormats.StringFormat, this.JsonTreeView.SelectedNode.Tag.ToString());
			}
			else
			{
				System.Media.SystemSounds.Exclamation.Play();
			}
		}


		private StringBuilder BuildDocument(dynamic data)
		{
			return BuildDocumentContent(new StringBuilder(), data, 0);
		}

		private StringBuilder BuildDocumentContent(StringBuilder sb, dynamic data, int indentLevel)
		{
			if (data is JsonObject)
			{
				if (data.IsObject)
				{
					foreach (string p in data.GetDynamicMemberNames())
					{
						sb.AppendLine();
						for (int i = 0; i < indentLevel; i++)
							sb.Append("\t");
						sb.Append(p);

						int tab = (int)Math.Ceiling( (24 - (p.Length /*+ indentLevel * 4*/)) / 4.0);
						for (int i = 0; i < tab; i++)
							sb.Append("\t");
						sb.Append("：");

						BuildDocumentContent(sb, data[p], indentLevel + 1);
					}
				}
				else if (data.IsArray)
				{
					sb.Append($"[{((dynamic[])data).Length}]");

					foreach (dynamic elem in data)
					{
						if (elem is JsonObject && (elem.IsObject || elem.IsArray))
						{
							BuildDocumentContent(sb, elem, indentLevel);

							break;
						}
					}
				}
			}

			return sb;
		}

		private void TreeContextMenu_CopyAsDocument_Click(object sender, EventArgs e)
		{
			if (this.JsonTreeView.SelectedNode != null && this.JsonTreeView.SelectedNode.Tag != null)
			{
				Clipboard.SetData(DataFormats.StringFormat, this.BuildDocument(this.JsonTreeView.SelectedNode.Tag));
			}
			else
			{
				System.Media.SystemSounds.Exclamation.Play();
			}
		}



		// 右クリックでも選択するように
		private void JsonTreeView_MouseClick(object sender, MouseEventArgs e)
		{
			var node = this.JsonTreeView.GetNodeAt(e.Location);
			if (node != null)
			{
                this.JsonTreeView.SelectedNode = node;
			}
		}



		protected override string GetPersistString()
		{
			return "Json";
		}


	}
}
