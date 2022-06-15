﻿using ElectronicObserver.Data.Battle.Detail;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectronicObserver.Data.Battle.Phase
{
    /// <summary>
    /// 夜戦における友軍艦隊攻撃フェーズの処理を行います。
    /// </summary>
    public class PhaseFriendlyShelling : PhaseBase
	{
		public PhaseFriendlyShelling(BattleData battle, string title)
			: base(battle, title)
		{
			if (!this.IsAvailable)
				return;

			// battle translation

			int[] fleetflag = (int[])this.ShellingData.api_at_eflag;
			int[] attackers = (int[])this.ShellingData.api_at_list;
			int[] nightAirAttackFlags = (int[])this.ShellingData.api_n_mother_list;
			int[] attackTypes = (int[])this.ShellingData.api_sp_list;
			int[][] defenders = ((dynamic[])this.ShellingData.api_df_list).Select(elem => ((int[])elem).Where(e => e != -1).ToArray()).ToArray();
			int[][] attackEquipments = ((dynamic[])this.ShellingData.api_si_list).Select(elem => ((dynamic[])elem).Select<dynamic, int>(ch => ch is string ? int.Parse(ch) : (int)ch).ToArray()).ToArray();
			int[][] criticals = ((dynamic[])this.ShellingData.api_cl_list).Select(elem => ((int[])elem).Where(e => e != -1).ToArray()).ToArray();
			double[][] rawDamages = ((dynamic[])this.ShellingData.api_damage).Select(elem => ((double[])elem).Where(e => e != -1).ToArray()).ToArray();

            this.Attacks = new List<PhaseFriendlySupportAttack>();



			for (int i = 0; i < attackers.Length; i++)
			{
				var attack = new PhaseFriendlySupportAttack
				{
					Attacker = new BattleIndex(attackers[i] + (fleetflag[i] == 0 ? 0 : 12), false, this.Battle.IsEnemyCombined),
					NightAirAttackFlag = nightAirAttackFlags[i] == -1,
					AttackType = attackTypes[i],
					EquipmentIDs = attackEquipments[i],
				};
				for (int k = 0; k < defenders[i].Length; k++)
				{
					var defender = new PhaseFriendlySupportDefender
					{
						Defender = new BattleIndex(defenders[i][k] + (fleetflag[i] == 0 ? 12 : 0), false, this.Battle.IsEnemyCombined),
						CriticalFlag = criticals[i][k],
						RawDamage = rawDamages[i][k]
					};
					attack.Defenders.Add(defender);
				}

                this.Attacks.Add(attack);
			}
		}

		public override bool IsAvailable => this.RawData.api_friendly_battle();


		public override void EmulateBattle(int[] hps, int[] damages)
		{
			if (!this.IsAvailable)
				return;

            // note: HP計算が正しくできない - 送られてくるHPにはすでに友軍支援のダメージが適用済みであるため、昼戦終了時のHPを参照しなければならない
            int[] friendhps = this.Battle.FriendlySupportInfo.FriendlyInitialHPs;

            foreach (var attack in this.Attacks)
            {
                foreach (var defs in attack.Defenders.GroupBy(d => d.Defender))
                {
                    this.BattleDetails.Add(new BattleFriendlyShellingDetail(
                        (BattleNight)this.Battle,
                        attack.Attacker,
                        defs.Key,
                        defs.Select(d => d.RawDamage).ToArray(),
                        defs.Select(d => d.CriticalFlag).ToArray(),
                        attack.AttackType,
                        attack.EquipmentIDs,
                        attack.NightAirAttackFlag,
                        defs.Key.IsFriend ? friendhps[defs.Key] : hps[defs.Key]));
                    if (defs.Key.IsFriend)
                        friendhps[defs.Key] -= Math.Max(defs.Sum(d => d.Damage), 0);
                    else
                        this.AddDamage(hps, defs.Key, defs.Sum(d => d.Damage));
                }
			}

		}

		/// <summary>
		/// 戦闘データ
		/// </summary>
		public dynamic BattleData => this.RawData.api_friendly_battle;

		/// <summary>
		/// 砲撃戦データ
		/// </summary>
		public dynamic ShellingData => this.RawData.api_friendly_battle.api_hougeki;


		// api_voice_id
		// api_voice_p_no


		/// <summary>
		/// 自軍照明弾投射艦インデックス
		/// </summary>
		public int FlareIndexFriend => (int)this.BattleData.api_flare_pos[0];

		/// <summary>
		/// 敵軍照明弾投射艦インデックス
		/// </summary>
		public int FlareIndexEnemy => (int)this.BattleData.api_flare_pos[1];


		/// <summary>
		/// 自軍照明弾投射艦
		/// </summary>
		public ShipDataMaster FlareFriendInstance
		{
			get
			{
				int index = this.FlareIndexFriend;
				if (0 <= index && index < this.Battle.FriendlySupportInfo.FriendlyMembersInstance.Length)
					return this.Battle.FriendlySupportInfo.FriendlyMembersInstance[index];
				return null;
			}
		}

		/// <summary>
		/// 敵軍照明弾投射艦
		/// </summary>
		public ShipDataMaster FlareEnemyInstance
		{
			get
			{
				int index = this.FlareIndexEnemy;
				var nightinitial = (this.Battle as BattleNight)?.NightInitial;

				if (nightinitial != null &&
					0 <= index && index < nightinitial.EnemyMembersInstance.Length)
					return nightinitial.EnemyMembersInstance[index];
				return null;
			}
		}


		/// <summary>
		/// 自軍探照灯照射艦番号
		/// </summary>
		public int SearchlightIndexFriend
		{
			get
			{
				int index = -1;
				var eqmaster = KCDatabase.Instance.MasterEquipments;

				for ( int i = 0; i < this.Battle.FriendlySupportInfo.FriendlyMembersInstance.Length; i++)
				{
					if(this.Battle.FriendlySupportInfo.FriendlyMembers[i] != -1 && this.Battle.FriendlySupportInfo.FriendlyInitialHPs[i] > 1)
					{
						if (this.Battle.FriendlySupportInfo.FriendlySlots[i].Any(id => eqmaster[id]?.CategoryType == EquipmentTypes.SearchlightLarge))
							return i;
						else if (this.Battle.FriendlySupportInfo.FriendlySlots[i].Any(id => eqmaster[id]?.CategoryType == EquipmentTypes.Searchlight) && index == -1)
							index = i;
					}
				}

				return index;
			}
		}


		/// <summary>
		/// 敵軍探照灯照射艦番号
		/// 厳密には異なるが(友軍の攻撃で探照灯所持艦の HP が 1 になった場合 -1 になる)、めったに起こるものでもないので気にしないことにする
		/// </summary>
		public int SearchlightIndexEnemy => (this.Battle as BattleNight)?.NightInitial?.SearchlightIndexEnemy ?? -1;


		/// <summary>
		/// 自軍探照灯照射艦
		/// </summary>
		public ShipDataMaster SearchlightFriendInstance
		{
			get
			{
				int index = this.SearchlightIndexFriend;
				if (0 <= index && index < this.Battle.FriendlySupportInfo.FriendlyMembersInstance.Length)
					return this.Battle.FriendlySupportInfo.FriendlyMembersInstance[index];
				return null;
			}
		}

		/// <summary>
		/// 敵軍探照灯投射艦
		/// </summary>
		public ShipDataMaster SearchlightEnemyInstance
		{
			get
			{
				int index = this.SearchlightIndexEnemy;
				var nightinitial = (this.Battle as BattleNight)?.NightInitial;

				if (nightinitial != null &&
					0 <= index && index < nightinitial.EnemyMembersInstance.Length)
					return nightinitial.EnemyMembersInstance[index];
				return null;
			}
		}



		public List<PhaseFriendlySupportAttack> Attacks { get; private set; }


		public class PhaseFriendlySupportAttack
		{
			public BattleIndex Attacker;
			public int AttackType;
			public bool NightAirAttackFlag;
			public List<PhaseFriendlySupportDefender> Defenders;
			public int[] EquipmentIDs;

			public PhaseFriendlySupportAttack()
			{
                this.Defenders = new List<PhaseFriendlySupportDefender>();
			}
		}

		public class PhaseFriendlySupportDefender
		{
			public BattleIndex Defender;
			public int CriticalFlag;
			public double RawDamage;
			public bool GuardsFlagship => this.RawDamage != Math.Floor(this.RawDamage);
			public int Damage => (int)this.RawDamage;
		}




		protected static int[] FixedArray(int[] array, int length, int defaultValue = -1)
		{
			var ret = new int[length];
			int l = Math.Min(length, array.Length);
			Array.Copy(array, ret, l);
			if (l < length)
			{
				for (int i = l; i < length; i++)
					ret[i] = defaultValue;
			}

			return ret;
		}

	}
}
