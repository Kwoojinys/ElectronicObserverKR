﻿using ElectronicObserver.Data.Battle.Detail;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectronicObserver.Data.Battle.Phase
{

    /// <summary>
    /// 戦闘フェーズの基底クラスです。
    /// </summary>
    public abstract class PhaseBase
	{
		protected BattleData Battle;
		public List<BattleDetail> BattleDetails { get; protected set; }
		public readonly string Title;


		protected PhaseBase(BattleData battle, string title)
		{
            this.Battle = battle;
            this.BattleDetails = new List<BattleDetail>();
            this.Title = title;
		}


		protected dynamic RawData => this.Battle.RawData;


		protected static bool IsIndexFriend(int index) => 0 <= index && index < 12;
		protected static bool IsIndexEnemy(int index) => 12 <= index && index < 24;


		/// <summary>
		/// 被ダメージ処理を行います。
		/// </summary>
		/// <param name="hps">各艦のHPリスト。</param>
		/// <param name="index">ダメージを受ける艦のインデックス。</param>
		/// <param name="damage">ダメージ。</param>
		protected void AddDamage(int[] hps, int index, int damage)
		{

			hps[index] -= Math.Max(damage, 0);

			// 自軍艦の撃沈が発生した場合(ダメコン処理)
			if (hps[index] <= 0 && IsIndexFriend(index) && !this.Battle.IsPractice)
			{
				var ship = this.Battle.Initial.GetFriendShip(index);
				if (ship == null)
					return;

				int id = ship.DamageControlID;

				if (id == 42)
					hps[index] = (int)(ship.HPMax * 0.2);

				else if (id == 43)
					hps[index] = ship.HPMax;

			}
		}


		protected virtual IEnumerable<BattleDetail> SearchBattleDetails(int index)
		{
			return this.BattleDetails.Where(d => d.AttackerIndex == index || d.DefenderIndex == index);
		}
		public virtual string GetBattleDetail(int index)
		{
			IEnumerable<BattleDetail> list;
			if (index == -1)
				list = this.BattleDetails;
			else
				list = this.SearchBattleDetails(index);

			if (list.Any())
			{
				return string.Join("\r\n", list) + "\r\n";
			}
			else return null;
		}
		public virtual string GetBattleDetail() { return this.GetBattleDetail(-1); }


		public override string ToString() => string.Join(" / \r\n", this.BattleDetails);



		/// <summary>
		/// データが有効かどうかを示します。
		/// </summary>
		public abstract bool IsAvailable { get; }

		/// <summary>
		/// 戦闘をエミュレートします。
		/// </summary>
		/// <param name="hps">各艦のHPリスト。</param>
		/// <param name="damages">各艦の与ダメージリスト。</param>
		public abstract void EmulateBattle(int[] hps, int[] damages);


	}
}
