﻿namespace ElectronicObserver.Data
{


    /// <summary>
    /// Responseを受信しデータを処理するクラスの基底です。
    /// </summary>
    public abstract class ResponseWrapper
	{

		/// <summary>
		/// 生の受信データ(api_data)
		/// </summary>
		public dynamic RawData { get; private set; }

		/// <summary>
		/// Responseを読み込みます。
		/// </summary>
		/// <param name="apiname">読み込むAPIの名前。</param>
		/// <param name="data">受信したデータ。</param>
		public virtual void LoadFromResponse(string apiname, dynamic data)
		{
            this.RawData = data;
		}

		/// <summary>
		/// 現在のデータが有効かを取得します。
		/// </summary>
		public bool IsAvailable => this.RawData != null;

		public ResponseWrapper()
		{
            this.RawData = null;
		}

	}

}
