﻿using ElectronicObserver.Utility.Mathematics;
using System;
using System.Collections.Generic;

namespace ElectronicObserver.Data
{

    /// <summary>
    /// 提督および司令部の情報を保持します。
    /// </summary>
    public class AdmiralData : APIWrapper
	{

		/// <summary>
		/// 提督ID
		/// </summary>
		public int AdmiralID
		{
			get
			{
				if (this.RawData.api_member_id is string)
					return int.Parse(this.RawData.api_member_id);
				else
					return (int)this.RawData.api_member_id;
			}
		}

		/// <summary>
		/// 提督名
		/// </summary>
		public string AdmiralName => this.RawData.api_nickname;

		/// <summary>
		/// 起動日時
		/// </summary>
		public DateTime StartTime => DateTimeHelper.FromAPITime((long)this.RawData.api_starttime);

		/// <summary>
		/// 艦隊司令部Level.
		/// </summary>
		public int Level => (int)this.RawData.api_level;

		/// <summary>
		/// 階級
		/// </summary>
		public int Rank => (int)this.RawData.api_rank;

		/// <summary>
		/// 提督経験値
		/// </summary>
		public int Exp => (int)this.RawData.api_experience;

		/// <summary>
		/// 提督コメント
		/// </summary>
		public string Comment => this.RawData.api_comment;

		/// <summary>
		/// 最大保有可能艦娘数
		/// </summary>
		public int MaxShipCount => (int)this.RawData.api_max_chara;

		/// <summary>
		/// 最大保有可能装備数
		/// </summary>
		public int MaxEquipmentCount => (int)this.RawData.api_max_slotitem;

		/// <summary>
		/// 最大保有可能艦隊数
		/// </summary>
		public int FleetCount => (int)this.RawData.api_count_deck;

		/// <summary>
		/// 工廠ドック数
		/// </summary>
		public int ArsenalCount => (int)this.RawData.api_count_kdock;

		/// <summary>
		/// 入渠ドック数
		/// </summary>
		public int DockCount => (int)this.RawData.api_count_ndock;


		/// <summary>
		/// 家具コイン
		/// </summary>
		public int FurnitureCoin => (int)this.RawData.api_fcoin;

		/// <summary>
		/// 出撃の勝数
		/// </summary>
		public int SortieWin => (int)this.RawData.api_st_win;

		/// <summary>
		/// 出撃の敗数
		/// </summary>
		public int SortieLose => (int)this.RawData.api_st_lose;

		/// <summary>
		/// 遠征の回数
		/// </summary>
		public int MissionCount => (int)this.RawData.api_ms_count;

		/// <summary>
		/// 遠征の成功数
		/// </summary>
		public int MissionSuccess => (int)this.RawData.api_ms_success;

		/// <summary>
		/// 演習の勝数
		/// </summary>
		public int PracticeWin => (int)this.RawData.api_pt_win;

		/// <summary>
		/// 演習の敗数
		/// </summary>
		public int PracticeLose => (int)this.RawData.api_pt_lose;

		/// <summary>
		/// 甲種勲章保有数
		/// </summary>
		public int Medals
		{
			get { return (int)this.RawData.api_medals; }
		}


		/// <summary>
		/// 資源の自然回復上限
		/// </summary>
		public int MaxResourceRegenerationAmount => this.Level * 250 + 750;


		public override void LoadFromRequest(string apiname, Dictionary<string, string> data)
		{
			base.LoadFromRequest(apiname, data);

			if (apiname == "api_req_member/updatecomment")
			{
				if (this.RawData != null)
                    this.RawData.api_comment = data["api_cmt"];
			}
		}
	}


}
