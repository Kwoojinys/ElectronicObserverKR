﻿using DynaJson;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ElectronicObserver.Data
{

    /// <summary>
    /// 個別の装備データを保持します。
    /// </summary>
    [DebuggerDisplay("[{ID}] : {NameWithLevel}")]
	public class EquipmentData : ResponseWrapper, IIdentifiable
	{

		/// <summary>
		/// 艦載機熟練度の文字列表現
		/// </summary>
		public static readonly string[] AircraftLevelString = {
				"",
				"|",
				"||",
				"|||",
				"/",
				"//",
				"///",
				">>",
			};


		/// <summary>
		/// 装備を一意に識別するID
		/// </summary>
		public int MasterID => (int)this.RawData.api_id;

		/// <summary>
		/// 装備ID
		/// </summary>
		public int EquipmentID => (int)this.RawData.api_slotitem_id;


		/// <summary>
		/// 保護ロック
		/// </summary>
		public bool IsLocked => (int)this.RawData.api_locked != 0;

		public bool IsNoneCounted => this.MasterEquipment.IsSupplies == true ||
									 this.MasterEquipment.IsDamageControl == true ||
									 this.MasterEquipment.IsRation == true;

		/// <summary>
		/// 改修Level
		/// </summary>
		public int Level => (int)this.RawData.api_level;


		/// <summary>
		/// 艦載機熟練度
		/// </summary>
		public int AircraftLevel => this.RawData.api_alv() ? (int)this.RawData.api_alv : 0;



		/// <summary>
		/// 装備のマスターデータへの参照
		/// </summary>
		public EquipmentDataMaster MasterEquipment => KCDatabase.Instance.MasterEquipments[this.EquipmentID];

		/// <summary>
		/// 装備名
		/// </summary>
		public string Name => this.MasterEquipment.Name;

        public string Name_JP => this.MasterEquipment.Name_JP;

		/// <summary>
		/// 装備名(レベルを含む)
		/// </summary>
		public string NameWithLevel
		{
			get
			{
				var sb = new StringBuilder(this.Name);

				if (this.Level > 0)
					sb.Append("+").Append(this.Level);
				if (this.AircraftLevel > 0)
					sb.Append(" ").Append(AircraftLevelString[this.AircraftLevel]);

				return sb.ToString();
			}
		}


		/// <summary>
		/// 配置転換中かどうか
		/// </summary>
		public bool IsRelocated => KCDatabase.Instance.RelocatedEquipments.Keys.Contains(this.MasterID);



		public int ID => this.MasterID;


		public override void LoadFromResponse(string apiname, dynamic data)
		{
			switch (apiname)
			{
				case "api_req_kousyou/createitem":      //不足パラメータの追加
				case "api_req_kousyou/getship":
					data.api_locked = 0;
					data.api_level = 0;
					break;

				case "api_get_member/ship3":            //存在しないアイテムを追加…すると処理に不都合があるので、ID:1で我慢　一瞬だし無問題（？）
					{
						int id = data;
						data = new JsonObject();
						data.api_id = id;
						data.api_slotitem_id = 1;
						data.api_locked = 0;
						data.api_level = 0;
					}
					break;

				default:
					break;
			}

			base.LoadFromResponse(apiname, (object)data);

		}


		public override string ToString() => this.NameWithLevel;


	}


}
