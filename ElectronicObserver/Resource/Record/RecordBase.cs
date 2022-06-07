using ElectronicObserver.Utility.Mathematics;
using ElectronicObserver.Utility;
using System;
using System.IO;
using System.Windows.Forms;

namespace ElectronicObserver.Resource.Record
{

	/// <summary>
	/// レコードの基底です。
	/// </summary>
	public abstract class RecordBase
	{


		/// <summary>
		/// レコードの要素の基底です。
		/// </summary>
		public abstract class RecordElementBase
		{

			public abstract void LoadLine(string line);
			public abstract string SaveLine();

		}



		/// <summary>
		/// ファイルからレコードを読み込みます。
		/// </summary>
		/// <param name="path">ファイルが存在するフォルダのパス。</param>
		public virtual bool Load(string path)
		{

			path = this.GetFilePath(path);

			try
			{
				bool hasError = false;

				using (StreamReader sr = new StreamReader(path, Configuration.Config.Log.FileEncoding))
				{
                    this.ClearRecord();
					bool ignoreError = false;
					string line;
					sr.ReadLine();          //ヘッダを読み飛ばす

					while ((line = sr.ReadLine()) != null)
					{
                        try
                        {
                            this.LoadLine(line);

                        }
                        catch (Exception ex)
                        {
                            if (ignoreError)
                                continue;

                            hasError = true;
                            ErrorReporter.SendErrorReport(ex, $"기록 {Path.GetFileName(path)} 의 손상을 감지했습니다.");

                            switch (MessageBox.Show($"기록 {Path.GetFileName(path)} 에서 손상된 데이터를 감지했습니다.\r\n\r\n[중단]: 읽기를 중지합니다. 데이터를 잃을 수 있습니다.\r\n[재시도]: (권장)로드를 계속합니다.\r\n[무시]: 로드를 계속합니다.(이후 추가확인하지않습니다.)",
								"기록 손상 감지", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2))
                            {
                                case DialogResult.Abort:
                                    throw;

                                case DialogResult.Retry:
                                    // do nothing
                                    break;

                                case DialogResult.Ignore:
                                    ignoreError = true;
                                    break;
                            }
                        }
                    }

				}

                if (hasError == true)
                {
                    string backupDestination = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "_backup_" + DateTimeHelper.GetTimeStamp() + Path.GetExtension(path));
                    File.Copy(path, backupDestination);
                    Logger.Add(LogType.Error, $"복구전에 기록을 {backupDestination} 에 백업했습니다. 복구에 실패했을경우, 이 파일을 다시 이용해주세요.");

                    this.SaveAll(RecordManager.Instance.MasterPath);
                }

                this.UpdateLastSavedIndex();
				return true;

			}
			catch (FileNotFoundException)
			{
                Logger.Add(LogType.Error, "기록 " + path + " 은 존재하지않습니다.");

			}
			catch (Exception ex)
			{
                ErrorReporter.SendErrorReport(ex, "기록 " + path + " 로드에 실패했습니다.");
			}

			return false;
		}


		/// <summary>
		/// ファイルに全てのレコードを書き込みます。
		/// </summary>
		/// <param name="path">ファイルが存在するフォルダのパス。</param>
		public virtual bool SaveAll(string path)
		{

			path = this.GetFilePath(path);

			try
			{

				using (StreamWriter sw = new StreamWriter(path, false, Configuration.Config.Log.FileEncoding))
				{

					sw.WriteLine(this.RecordHeader);
					sw.Write(this.SaveLinesAll());

				}

                this.UpdateLastSavedIndex();
				return true;

			}
			catch (Exception ex)
			{
                ErrorReporter.SendErrorReport(ex, "기록 " + path + " 저장에 실패했습니다.");
			}

			return false;
		}


		/// <summary>
		/// ファイルに前回からの差分を追記します。
		/// </summary>
		/// <param name="path">ファイルが存在するフォルダのパス。</param>
		public virtual bool SavePartial(string path)
		{

			if (!this.SupportsPartialSave)
				return false;


			path = this.GetFilePath(path);
			bool exists = File.Exists(path);

			try
			{

				using (StreamWriter sw = new StreamWriter(path, true, Configuration.Config.Log.FileEncoding))
				{

					if (!exists)
						sw.WriteLine(this.RecordHeader);

					sw.Write(this.SaveLinesPartial());
				}

                this.UpdateLastSavedIndex();
				return true;

			}
			catch (Exception ex)
			{
                ErrorReporter.SendErrorReport(ex, "기록 " + path + " 저장에 실패했습니다.");
			}

			return false;
		}


		protected string GetFilePath(string path)
		{
			return path.Trim(@" \\""".ToCharArray()) + "\\" + this.FileName;
		}


		/// <summary>
		/// ファイルから読み込んだデータを解析し、レコードに追加します。
		/// </summary>
		/// <param name="line">読み込んだ一行分のデータ。</param>
		protected abstract void LoadLine(string line);

		/// <summary>
		/// レコードのデータをファイルに書き込める文字列に変換します。
		/// </summary>
		protected abstract string SaveLinesAll();

		/// <summary>
		/// レコードの差分データをファイルに書き込める文字列に変換します。
		/// </summary>
		protected abstract string SaveLinesPartial();

		public abstract bool SupportsPartialSave { get; }


		/// <summary>
		/// レコードをクリアします。ロード直前に呼ばれます。
		/// </summary>
		protected abstract void ClearRecord();

		protected abstract void UpdateLastSavedIndex();

		public abstract bool NeedToSave { get; }


		public abstract void RegisterEvents();


		/// <summary>
		/// レコードのヘッダを取得します。
		/// </summary>
		public abstract string RecordHeader { get; }

		/// <summary>
		/// 保存するファイル名を取得します。
		/// </summary>
		public abstract string FileName { get; }

	}

}
