﻿using System.Collections.Generic;

namespace ElectronicObserver.Observer
{

    public delegate void APIReceivedEventHandler(string apiname, dynamic data);

	/// <summary>
	/// API処理部の基底となるクラスです。
	/// </summary>
	public abstract class APIBase
	{

		/// <summary>
		/// Requestの処理が完了した時に発生します。
		/// </summary>
		public event APIReceivedEventHandler RequestReceived = delegate { };

		/// <summary>
		/// Responseの処理が完了した時に発生します。
		/// </summary>
		public event APIReceivedEventHandler ResponseReceived = delegate { };


		/// <summary>
		/// Requestを処理し、RequestReceivedを起動します。
		/// 継承時は最後に呼ぶようにして下さい。
		/// </summary>
		/// <param name="data">処理するデータ。</param>
		public virtual void OnRequestReceived(Dictionary<string, string> data)
		{
           // Utility.Logger.Add(2, APIName + ":" + data);
			RequestReceived(this.APIName, data);
		}

		/// <summary>
		/// Responseを処理し、ResponseReceivedを起動します。
		/// 継承時は最後に呼ぶようにして下さい。
		/// </summary>
		/// <param name="data">処理するデータ。</param>
		public virtual void OnResponseReceived(dynamic data)
		{
            //Utility.Logger.Add(2, APIName + ":" + data);
            ResponseReceived(this.APIName, data);
		}


		/// <summary>
		/// Requestの処理をサポートしているかを取得します。
		/// </summary>
		public virtual bool IsRequestSupported => false;

		/// <summary>
		/// Responseの処理をサポートしているかを取得します。
		/// </summary>
		public virtual bool IsResponseSupported => true;



		/// <summary>
		/// API名を取得します。
		/// </summary>
		public abstract string APIName { get; }

	}

}
