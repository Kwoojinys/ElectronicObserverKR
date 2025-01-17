﻿using ElectronicObserver.Utility.Mathematics;
using System;

namespace ElectronicObserver.Utility
{

    /// <summary>
    /// ソフトウェアの情報を保持します。
    /// </summary>
    public static class SoftwareInformation
    {

        /// <summary>
        /// ソフトウェア名(日本語)
        /// </summary>
        public static string SoftwareNameKorean => "74식 전자관측의";

        /// <summary>
        /// バージョン(英語)
        /// </summary>
		public static string VersionKorean => "4.8.2 KRTL_R45";

        /// <summary>
        /// 更新日時
        /// </summary>
        public static DateTime UpdateTime       => DateTimeHelper.CSVStringToTime("2022/06/27 13:00:00");
        public static DateTime MaintenanceTime  = DateTime.Now;

        private static System.Net.WebClient _client;
        private static readonly Uri _uri = new Uri("https://thelokis.github.io/EOTranslation/Translations/softwareversion.txt");

        private static System.Net.WebClient _maintenanceClient;
        private static readonly Uri _maintenanceUri = new Uri("https://thelokis.github.io/EOTranslation/Translations/Maintenance.txt");

        public static void CheckUpdate()
        {
            if (Configuration.Config.Life.CheckUpdateInformation == false)
                return;

            if (_client == null)
            {
                _client = new System.Net.WebClient
                {
                    Encoding = new System.Text.UTF8Encoding(false)
                };

                _client.DownloadStringCompleted += DownloadStringCompleted;
            }

            if (_client.IsBusy == false)
                _client.DownloadStringAsync(_uri);
        }

        public static void CheckMaintenance()
        {
            if (_maintenanceClient == null)
            {
                _maintenanceClient = new System.Net.WebClient
                {
                    Encoding = new System.Text.UTF8Encoding(false)
                };
                _maintenanceClient.DownloadStringCompleted += DownloadTimeCompleted;
            }

            if (_maintenanceClient.IsBusy == false)
                _maintenanceClient.DownloadStringAsync(_maintenanceUri);
        }

        public static string GetMaintenanceTime()
        {
            if (DateTime.Now > MaintenanceTime)
                return "점검 날짜 발표 예정";

            return $"점검까지 : {(int)(MaintenanceTime - DateTime.Now).TotalHours:D2}:" +
                              $"{(int)(MaintenanceTime - DateTime.Now).Minutes:D2}:" +
                              $"{(int)(MaintenanceTime - DateTime.Now).Seconds:D2}";
        }

        private static void DownloadTimeCompleted(object sender, System.Net.DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ErrorReporter.SendErrorReport(e.Error, "점검 정보를 가져오는데 실패했습니다.");
                return;
            }

            if (e.Result.StartsWith("<!DOCTYPE html>") == true)
            {
                Logger.Add(LogType.Alert, "업데이트 정보의 URI가 잘못되었습니다.");
                return;
            }

            try
            {
                using (var sr = new System.IO.StringReader(e.Result))
                {
                    MaintenanceTime = DateTimeHelper.CSVStringToTime(sr.ReadLine());
                }
            }
            catch
            {
                Logger.Add(LogType.Alert, "점검 내역을 불러오는데 실패했습니다.");
            }
        }

        private static void DownloadStringCompleted(object sender, System.Net.DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ErrorReporter.SendErrorReport(e.Error, "업데이트 정보를 가져오는데 실패했습니다.");
                return;
            }

            if (e.Result.StartsWith("<!DOCTYPE html>") == true)
            {
                Logger.Add(LogType.Alert, "업데이트 정보의 URI가 잘못되었습니다.");
                return;
            }

            try
            {
                using (var sr = new System.IO.StringReader(e.Result))
                {
                    DateTime date       = DateTimeHelper.CSVStringToTime(sr.ReadLine());
                    string version      = sr.ReadLine();
                    string description  = sr.ReadToEnd();

                    if (UpdateTime < date)
                    {
                        Logger.Add(LogType.Alert, "새 버전이 출시되었습니다! : " + version);

                        var result = System.Windows.Forms.MessageBox.Show(
                            string.Format("새 버전이 출시되었습니다! : {0}\r\n업데이트 내용 : \r\n{1}\r\n다운로드 페이지로 이동하시겠습니까?？\r\n(취소할경우 이후 표시하지 않습니다.)",
                            version, description),
                            "업데이트 정보", System.Windows.Forms.MessageBoxButtons.YesNoCancel, System.Windows.Forms.MessageBoxIcon.Information,
                            System.Windows.Forms.MessageBoxDefaultButton.Button1);

                        if (result == System.Windows.Forms.DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start("http://thelokis.egloos.com/");
                        }
                        else if (result == System.Windows.Forms.DialogResult.Cancel)
                        {
                            Configuration.Config.Life.CheckUpdateInformation = false;
                        }
                    }
                    else
                    {
                        Logger.Add(LogType.System, "현재 버전이 최신 버전입니다.");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.SendErrorReport(ex, "업데이트에 실패했습니다.");
            }
        }
    }
}
