﻿using ElectronicObserver.Utility.Mathematics;
using System;
using System.IO;

namespace ElectronicObserver.Utility
{

    public class ErrorReporter
	{

		private const string _basePath = "ErrorReport";


		/// <summary>
		/// エラーレポートを作成します。
		/// </summary>
		/// <param name="ex">発生した例外。</param>
		/// <param name="message">追加メッセージ。</param>
		/// <param name="connectionName">エラーが発生したAPI名。省略可能です。</param>
		/// <param name="connectionData">エラーが発生したAPIの内容。省略可能です。</param>
		public static void SendErrorReport(Exception ex, string message, string connectionName = null, string connectionData = null)
		{

            Logger.Add(LogType.Fatal, string.Format("{0} : {1}", message, ex.Message));

			if (Configuration.Config.Debug.AlertOnError)
				System.Media.SystemSounds.Hand.Play();


			if (!Configuration.Config.Log.SaveErrorReport)
				return;


			string path = _basePath;

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);


			path = string.Format("{0}\\{1}.txt", path, DateTimeHelper.GetTimeStamp());

			try
			{
				using (StreamWriter sw = new StreamWriter(path, false, new System.Text.UTF8Encoding(false)))
				{

					sw.WriteLine("오류 보고서 [ver. {0}] : {1}", SoftwareInformation.VersionKorean, DateTimeHelper.TimeToCSVString(DateTime.Now));
					sw.WriteLine("에러 : {0}", ex.GetType().Name);
					sw.WriteLine(ex.Message);
					sw.WriteLine("추가 정보 : {0}", message);
					sw.WriteLine("스택 트레이스：");
					sw.WriteLine(ex.StackTrace);

					if (connectionName != null && connectionData != null)
					{
						sw.WriteLine();
						sw.WriteLine("통신 내용 : {0}", connectionName);
						sw.WriteLine(connectionData);
					}
				}

			}
			catch (Exception)
			{

                Logger.Add(LogType.Fatal, string.Format("오류 보고서의 작성에 실패했습니다. \r\n{0}\r\n{1}", ex.Message, ex.StackTrace));
			}

		}

    }
}
