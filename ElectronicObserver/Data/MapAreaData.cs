﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectronicObserver.Window;

namespace ElectronicObserver.Data
{

	public class MapAreaData : APIWrapper, IIdentifiable
	{

		/// <summary>
		/// 海域カテゴリID
		/// </summary>
		public int MapAreaID => (int)this.RawData.api_id;

		/// <summary>
		/// 海域カテゴリ名
		/// </summary>
		public string Name
        {
            get { return FormMain.Instance.Translator.GetTranslation(this.RawData.api_name, Utility.DataType.OperationMap); }
        }

		/// <summary>
		/// 海域タイプ　0=通常, 1=イベント
		/// </summary>
		public int MapType => (int)this.RawData.api_type;



		public int ID => this.MapAreaID;
		public override string ToString() => $"[{this.MapAreaID}] {this.Name}";
	}

}
