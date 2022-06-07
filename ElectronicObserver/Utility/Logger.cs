using ElectronicObserver.Utility.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ElectronicObserver.Utility.UtilExtension;

namespace ElectronicObserver.Utility
{


	public delegate void LogAddedEventHandler(Logger.LogData data);
    public enum LogType
    {
		Fatal		= 0,
        Error		= 1,
        Alert		= 2,
        System		= 3,
        Browser		= 4,
        PowerUp		= 5,
        Arsenal		= 6,
        Repair		= 7,
        Synergy		= 8,
        Battle		= 9,
        LevelUp		= 10,
        Resupply	= 11,
        Expedition	= 12,
        Gimick		= 13,
        GetItem		= 14,
		ShipDrop	= 15,
        None,
    }

    public sealed class Logger
	{

		#region Singleton

		private static readonly Logger instance = new Logger();

		public static Logger Instance => instance;

        #endregion

        public static IEnumerable<string> GetLogTypeNames()
        {
            yield return "치명적오류"; // 0
            yield return "오류";
            yield return "경고 및 알림"; // 2
            yield return "시스템메시지";
            yield return "브라우저 통신";  // 4
            yield return "근대화 개수 / 개장";
            yield return "공창 관련"; // 6
            yield return "수리";
            yield return "장비 시너지"; // 8 
            yield return "전투 관련";
            yield return "레벨업";  // 10
            yield return "보급 관련";
            yield return "원정 관련"; // 12
            yield return "기믹 관련";
            yield return "출격시 자원/장비 획득"; // 14
			yield return "칸무스 드랍";
        }


        /// <summary>
        /// ログが追加された時に発生します。
        /// </summary>
        public event LogAddedEventHandler LogAdded = delegate { };


		public class LogData
		{

			/// <summary>
			/// 書き込み時刻
			/// </summary>
			public readonly DateTime Time;


			public readonly LogType Type;
			/// <summary>
			/// ログ内容
			/// </summary>
			public readonly string Message;

			public LogData(DateTime time, LogType type, string message)
			{
				this.Time = time;
				this.Type = type;
				this.Message = message;
			}


			public override string ToString() => $"[{DateTimeHelper.TimeToCSVString(this.Time)}] : {this.Message}";
		}

		private List<LogData>	_log			{ get; set; }
		private bool			_toDebugConsole { get; set; }
		private int				_lastSavedCount { get; set; }

		private Logger()
		{
            this._log = new List<LogData>();
            this._toDebugConsole = true;
            this._lastSavedCount = 0;
		}


		public static IReadOnlyList<LogData> Log
		{
			get
			{
				lock (Instance)
				{
					return Instance._log.AsReadOnly();
				}
			}
		}

		/// <summary>
		/// ログを追加します。
		/// </summary>
		/// <param name="priority">優先度。</param>
		/// <param name="message">ログ内容。</param>
		public static void Add(LogType type, string message)
		{
			LogData data = new LogData(DateTime.Now, type, message);

			lock (Instance)
			{
                Instance._log.Add(data);
			}

			if (Configuration.Config.Log.SaveLogFlag && Configuration.Config.Log.SaveLogImmediately)
				Save();

			if (Configuration.Config.Log.VisibleLogList[(int)type] == true)
			{
				if (Instance._toDebugConsole)
				{
					System.Diagnostics.Debug.WriteLine(data.ToString());
				}

				try
				{
                    Instance.LogAdded(data);

				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.Message);
				}
			}
		}

		/// <summary>
		/// ログをすべて消去します。
		/// </summary>
		public static void Clear()
		{
			lock (Instance)
			{
                instance._log.Clear();
                instance._lastSavedCount = 0;
			}
		}

		public static readonly string DefaultPath = @"eolog.log";

		public static void Save()
		{
			Save(DefaultPath);
		}

		/// <summary>
		/// ログを保存します。
		/// </summary>
		/// <param name="path">保存先のファイル。</param>
		public static void Save(string path)
		{
			try
			{
				lock (Instance)
				{
					var log = instance;

					using (StreamWriter sw = new StreamWriter(path, true, Configuration.Config.Log.FileEncoding))
					{
						foreach (var l in log._log.Skip(log._lastSavedCount).Where(l => Configuration.Config.Log.VisibleLogList[(int) l.Type] == true))
						{
							sw.WriteLine(l.ToString());
						}

						log._lastSavedCount = log._log.Count;
					}
				}
			}
			catch (Exception)
			{

				// に ぎ り つ ぶ す
			}

		}

	}
}
