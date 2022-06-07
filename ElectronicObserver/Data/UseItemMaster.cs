﻿using ElectronicObserver.Window;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicObserver.Data
{

	/// <summary>
	/// 消費アイテムのマスターデータを保持します。
	/// </summary>
	public class UseItemMaster : ResponseWrapper, IIdentifiable
	{

		/// <summary>
		/// アイテムID
		/// </summary>
		public int ItemID => (int)this.RawData.api_id;

		/// <summary>
		/// 使用形態
		/// 1=高速修復材, 2=高速建造材, 3=開発資材, 4=資源還元, その他
		/// </summary>
		public int UseType => (int)this.RawData.api_usetype;

		/// <summary>
		/// カテゴリ
		/// </summary>
		public int Category => (int)this.RawData.api_category;

		/// <summary>
		/// アイテム名 번역됨
		/// </summary>
		public string Name
        {
            get { return Utility.ExternalDataReader.Instance.GetTranslation(this.RawData.api_name, Utility.TranslateType.Items, this.ItemID); } 
        }


		/// <summary>
		/// 説明
		/// </summary>
		public string Description => this.RawData.api_description[0];

		//description[1]=家具コインの内容量　省略します


		public int ID => this.ItemID;
		public override string ToString() => $"[{this.ItemID}] {this.Name}";
	}


}
