﻿using ElectronicObserver.Utility.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ElectronicObserver.Data
{


    /// <summary>
    /// 個別の艦娘データを保持します。
    /// </summary>
    public class ShipData : APIWrapper, IIdentifiable
    {



        /// <summary>
        /// 艦娘を一意に識別するID
        /// </summary>
        public int MasterID => (int)this.RawData.api_id;

        /// <summary>
        /// 並べ替えの順番
        /// </summary>
        public int SortID => (int)this.RawData.api_sortno;

        /// <summary>
        /// 艦船ID
        /// </summary>
        public int ShipID => (int)this.RawData.api_ship_id;

        /// <summary>
        /// レベル
        /// </summary>
        public int Level => (int)this.RawData.api_lv;

        /// <summary>
        /// 累積経験値
        /// </summary>
        public int ExpTotal => (int)this.RawData.api_exp[0];

        /// <summary>
        /// 次のレベルに達するために必要な経験値
        /// </summary>
        public int ExpNext => (int)this.RawData.api_exp[1];


        /// <summary>
        /// 耐久現在値
        /// </summary>
        public int HPCurrent { get; internal set; }

        /// <summary>
        /// 耐久最大値
        /// </summary>
        public int HPMax => (int)this.RawData.api_maxhp;


        /// <summary>
        /// 速力
        /// </summary>
        public int Speed => this.RawData.api_soku() ? (int)this.RawData.api_soku : this.MasterShip.Speed;

        /// <summary>
        /// 射程
        /// </summary>
        public int Range => (int)this.RawData.api_leng;


        /// <summary>
        /// 装備スロット(ID)
        /// </summary>
        public ReadOnlyCollection<int> Slot { get; private set; }


        /// <summary>
        /// 装備スロット(マスターID)
        /// </summary>
        public ReadOnlyCollection<int> SlotMaster => Array.AsReadOnly(this.Slot.Select(id => KCDatabase.Instance.Equipments[id]?.EquipmentID ?? -1).ToArray());

        /// <summary>
        /// 装備スロット(装備データ)
        /// </summary>
        public ReadOnlyCollection<EquipmentData> SlotInstance => Array.AsReadOnly(this.Slot.Select(id => KCDatabase.Instance.Equipments[id]).ToArray());

        /// <summary>
        /// 装備スロット(装備マスターデータ)
        /// </summary>
        public ReadOnlyCollection<EquipmentDataMaster> SlotInstanceMaster => Array.AsReadOnly(this.Slot.Select(id => KCDatabase.Instance.Equipments[id]?.MasterEquipment).ToArray());


        /// <summary>
        /// 補強装備スロット(ID)
        /// 0=未開放, -1=装備なし 
        /// </summary>
        public int ExpansionSlot { get; private set; }

        /// <summary>
        /// 補強装備スロット(マスターID)
        /// </summary>
        public int ExpansionSlotMaster => this.ExpansionSlot == 0 ? 0 : (KCDatabase.Instance.Equipments[this.ExpansionSlot]?.EquipmentID ?? -1);

        /// <summary>
        /// 補強装備スロット(装備データ)
        /// </summary>
        public EquipmentData ExpansionSlotInstance => KCDatabase.Instance.Equipments[this.ExpansionSlot];

        /// <summary>
        /// 補強装備スロット(装備マスターデータ)
        /// </summary>
        public EquipmentDataMaster ExpansionSlotInstanceMaster => KCDatabase.Instance.Equipments[this.ExpansionSlot]?.MasterEquipment;


        /// <summary>
        /// 全てのスロット(ID)
        /// </summary>
        public ReadOnlyCollection<int> AllSlot => Array.AsReadOnly(this.Slot.Concat(new[] { this.ExpansionSlot }).ToArray());

        /// <summary>
        /// 全てのスロット(マスターID)
        /// </summary>
        public ReadOnlyCollection<int> AllSlotMaster => Array.AsReadOnly(this.AllSlot.Select(id => KCDatabase.Instance.Equipments[id]?.EquipmentID ?? -1).ToArray());

        /// <summary>
        /// 全てのスロット(装備データ)
        /// </summary>
        public ReadOnlyCollection<EquipmentData> AllSlotInstance => Array.AsReadOnly(this.AllSlot.Select(id => KCDatabase.Instance.Equipments[id]).ToArray());

        /// <summary>
        /// 全てのスロット(装備マスターデータ)
        /// </summary>
        public ReadOnlyCollection<EquipmentDataMaster> AllSlotInstanceMaster => Array.AsReadOnly(this.AllSlot.Select(id => KCDatabase.Instance.Equipments[id]?.MasterEquipment).ToArray());



        private int[] _aircraft;
        /// <summary>
        /// 各スロットの航空機搭載量
        /// </summary>
        public ReadOnlyCollection<int> Aircraft => Array.AsReadOnly(this._aircraft);


        /// <summary>
        /// 現在の航空機搭載量
        /// </summary>
        public int AircraftTotal => this._aircraft.Sum(a => Math.Max(a, 0));


        /// <summary>
        /// 搭載燃料
        /// </summary>
        public int Fuel { get; internal set; }

        /// <summary>
        /// 搭載弾薬
        /// </summary>
        public int Ammo { get; internal set; }


        /// <summary>
        /// スロットのサイズ
        /// </summary>
        public int SlotSize => !this.RawData.api_slotnum() ? 0 : (int)this.RawData.api_slotnum;

        /// <summary>
        /// 入渠にかかる時間(ミリ秒)
        /// </summary>
        public int RepairTime => (int)this.RawData.api_ndock_time;

        /// <summary>
        /// 入渠にかかる鋼材
        /// </summary>
        public int RepairSteel => (int)this.RawData.api_ndock_item[1];

        /// <summary>
        /// 入渠にかかる燃料
        /// </summary>
        public int RepairFuel => (int)this.RawData.api_ndock_item[0];

        /// <summary>
        /// コンディション
        /// </summary>
        public int Condition { get; internal set; }


        #region Parameters

        /********************************************************
		 * 強化値：近代化改修・レベルアップによって上昇した数値
		 * 総合値：装備込みでのパラメータ
		 * 基本値：装備なしでのパラメータ(初期値+強化値)
		 ********************************************************/

        private int[] _modernized;
        /// <summary>
        /// 火力強化値
        /// </summary>
        public int FirepowerModernized => this._modernized.Length >= 5 ? this._modernized[0] : 0;

        /// <summary>
        /// 雷装強化値
        /// </summary>
        public int TorpedoModernized => this._modernized.Length >= 5 ? this._modernized[1] : 0;

        /// <summary>
        /// 対空強化値
        /// </summary>
        public int AAModernized => this._modernized.Length >= 5 ? this._modernized[2] : 0;

        /// <summary>
        /// 装甲強化値
        /// </summary>
        public int ArmorModernized => this._modernized.Length >= 5 ? this._modernized[3] : 0;

        /// <summary>
        /// 運強化値
        /// </summary>
        public int LuckModernized => this._modernized.Length >= 5 ? this._modernized[4] : 0;

        /// <summary>
        /// 耐久強化値
        /// </summary>
        public int HPMaxModernized => this._modernized.Length >= 7 ? this._modernized[5] : 0;

        /// <summary>
        /// 対潜強化値
        /// </summary>
        public int ASWModernized => this._modernized.Length >= 7 ? this._modernized[6] : 0;


        /// <summary>
        /// 火力改修残り
        /// </summary>
        public int FirepowerRemain => (this.MasterShip.FirepowerMax - this.MasterShip.FirepowerMin) - this.FirepowerModernized;

        /// <summary>
        /// 雷装改修残り
        /// </summary>
        public int TorpedoRemain => (this.MasterShip.TorpedoMax - this.MasterShip.TorpedoMin) - this.TorpedoModernized;

        /// <summary>
        /// 対空改修残り
        /// </summary>
        public int AARemain => (this.MasterShip.AAMax - this.MasterShip.AAMin) - this.AAModernized;

        /// <summary>
        /// 装甲改修残り
        /// </summary>
        public int ArmorRemain => (this.MasterShip.ArmorMax - this.MasterShip.ArmorMin) - this.ArmorModernized;

        /// <summary>
        /// 運改修残り
        /// </summary>
        public int LuckRemain => (this.MasterShip.LuckMax - this.MasterShip.LuckMin) - this.LuckModernized;

        /// <summary>
        /// 耐久改修残り
        /// </summary>
        public int HPMaxRemain => (this.IsMarried ? this.MasterShip.HPMaxMarriedModernizable : this.MasterShip.HPMaxModernizable) - this.HPMaxModernized;

        /// <summary>
        /// 対潜改修残り
        /// </summary>
        public int ASWRemain => this.ASWMax <= 0 ? 0 : this.MasterShip.ASWModernizable - this.ASWModernized;


        /// <summary>
        /// 火力総合値
        /// </summary>
        public int FirepowerTotal => (int)this.RawData.api_karyoku[0];

        /// <summary>
        /// 화뇌합
        /// </summary>
        public int SumTorPower => this.FirepowerBase + this.TorpedoBase;

        /// <summary>
        /// 雷装総合値
        /// </summary>
        public int TorpedoTotal => (int)this.RawData.api_raisou[0];

        /// <summary>
        /// 対空総合値
        /// </summary>
        public int AATotal => (int)this.RawData.api_taiku[0];

        /// <summary>
        /// 装甲総合値
        /// </summary>
        public int ArmorTotal => (int)this.RawData.api_soukou[0];

        /// <summary>
        /// 回避総合値
        /// </summary>
        public int EvasionTotal => (int)this.RawData.api_kaihi[0];

        /// <summary>
        /// 対潜総合値
        /// </summary>
        public int ASWTotal => (int)this.RawData.api_taisen[0];

        /// <summary>
        /// 索敵総合値
        /// </summary>
        public int LOSTotal => (int)this.RawData.api_sakuteki[0];

        /// <summary>
        /// 運総合値
        /// </summary>
        public int LuckTotal => (int)this.RawData.api_lucky[0];

        /// <summary>
        /// 爆装総合値
        /// </summary>
        public int BomberTotal => this.AllSlotInstanceMaster.Sum(eq => eq?.Bomber ?? 0);

        /// <summary>
        /// 命中総合値
        /// </summary>
        public int AccuracyTotal => this.AllSlotInstanceMaster.Sum(eq => eq?.Accuracy ?? 0);


        /// <summary>
        /// 火力基本値
        /// </summary>
        public int FirepowerBase => this.MasterShip.FirepowerMin + this.FirepowerModernized;


        /// <summary>
        /// 雷装基本値
        /// </summary>
        public int TorpedoBase => this.MasterShip.TorpedoMin + this.TorpedoModernized;


        /// <summary>
        /// 対空基本値
        /// </summary>
        public int AABase => this.MasterShip.AAMin + this.AAModernized;


        /// <summary>
        /// 装甲基本値
        /// </summary>
        public int ArmorBase => this.MasterShip.ArmorMin + this.ArmorModernized;


        /// <summary>
        /// 回避基本値
        /// </summary>
        public int EvasionBase
        {
            get
            {
                if (this.MasterShip.Evasion.IsDetermined)
                    return this.MasterShip.Evasion.GetParameter(this.Level);

                // パラメータ上限下限が分かっていれば上ので確実に取れる
                // 不明な場合は以下で擬似計算（装備分を引くだけ）　装備シナジーによって上昇している場合誤差が発生します
                return this.EvasionTotal - this.AllSlotInstance.Sum(eq => eq?.MasterEquipment?.Evasion ?? 0);
            }
        }

        /// <summary>
        /// 対潜基本値
        /// </summary>
        public int ASWBase
        {
            get
            {
                if (this.MasterShip.ASW.IsDetermined)
                    return this.MasterShip.ASW.GetParameter(this.Level) + this.ASWModernized;

                return this.ASWTotal - this.AllSlotInstance.Sum(eq => eq?.MasterEquipment?.ASW ?? 0);
            }
        }

        /// <summary>
        /// 索敵基本値
        /// </summary>
        public int LOSBase
        {
            get
            {
                if (this.MasterShip.LOS.IsDetermined)
                    return this.MasterShip.LOS.GetParameter(this.Level);

                return this.LOSTotal - this.AllSlotInstance.Sum(eq => eq?.MasterEquipment?.LOS ?? 0);
            }
        }

        /// <summary>
        /// 運基本値
        /// </summary>
        public int LuckBase => this.MasterShip.LuckMin + this.LuckModernized;



        /// <summary>
        /// 回避最大値
        /// </summary>
        public int EvasionMax => (int)this.RawData.api_kaihi[1];

        /// <summary>
        /// 対潜最大値
        /// </summary>
        public int ASWMax => (int)this.RawData.api_taisen[1];

        /// <summary>
        /// 索敵最大値
        /// </summary>
        public int LOSMax => (int)this.RawData.api_sakuteki[1];

        #endregion


        /// <summary>
        /// 保護ロックの有無
        /// </summary>
        public bool IsLocked => (int)this.RawData.api_locked != 0;

        /// <summary>
        /// 装備による保護ロックの有無
        /// </summary>
        public bool IsLockedByEquipment => (int)this.RawData.api_locked_equip != 0;


        /// <summary>
        /// 出撃海域
        /// </summary>
        public int SallyArea => this.RawData.api_sally_area() ? (int)this.RawData.api_sally_area : -1;



        /// <summary>
        /// 艦船のマスターデータへの参照
        /// </summary>
        public ShipDataMaster MasterShip => KCDatabase.Instance.MasterShips[this.ShipID];


        /// <summary>
        /// 入渠中のドックID　非入渠時は-1
        /// </summary>
        public int RepairingDockID => KCDatabase.Instance.Docks.Values.FirstOrDefault(dock => dock.ShipID == this.MasterID)?.DockID ?? -1;


        /// <summary>
        /// 所属艦隊　-1=なし
        /// </summary>
        public int Fleet => KCDatabase.Instance.Fleet.Fleets.Values.FirstOrDefault(f => f.Members.Contains(this.MasterID))?.FleetID ?? -1;



        /// <summary>
        /// 所属艦隊及びその位置
        /// ex. 1-3 (位置も1から始まる)
        /// 所属していなければ 空文字列
        /// </summary>
        public string FleetWithIndex
        {
            get
            {
                FleetManager fm = KCDatabase.Instance.Fleet;
                foreach (var f in fm.Fleets.Values)
                {
                    int index = f.Members.IndexOf(this.MasterID);
                    if (index != -1)
                    {
                        return $"{f.FleetID}-{index + 1}";
                    }
                }
                return "";
            }

        }


        /// <summary>
        /// ケッコン済みかどうか
        /// </summary>
        public bool IsMarried => this.Level > 99;


        /// <summary>
        /// 次の改装まで必要な経験値
        /// </summary>
        public int ExpNextRemodel
        {
            get
            {
                ShipDataMaster master = this.MasterShip;
                if (master.RemodelAfterShipID <= 0)
                    return 0;
                return Math.Max(ExpTable.ShipExp[master.RemodelAfterLevel].Total - this.ExpTotal, 0);
            }
        }


        /// <summary>
        /// 艦名
        /// </summary>
        public string Name => this.MasterShip.Name;

        public string Name_JP => this.MasterShip.Name_JP;

        /// <summary>
        /// 艦名(レベルを含む)
        /// </summary>
        public string NameWithLevel => $"{this.MasterShip.Name} Lv. {this.Level}";


        /// <summary>
        /// HP/HPmax
        /// </summary>
        public double HPRate => this.HPMax > 0 ? (double)this.HPCurrent / this.HPMax : 0;



        /// <summary>
        /// 最大搭載燃料
        /// </summary>
        public int FuelMax => this.MasterShip.Fuel;

        /// <summary>
        /// 最大搭載弾薬
        /// </summary>
        public int AmmoMax => this.MasterShip.Ammo;


        /// <summary>
        /// 燃料残量割合
        /// </summary>
        public double FuelRate => (double)this.Fuel / Math.Max(this.FuelMax, 1);

        /// <summary>
        /// 弾薬残量割合
        /// </summary>
        public double AmmoRate => (double)this.Ammo / Math.Max(this.AmmoMax, 1);


        /// <summary>
        /// 補給で消費する燃料
        /// </summary>
        public int SupplyFuel => (this.FuelMax - this.Fuel) == 0 ? 0 : Math.Max((int)Math.Floor((this.FuelMax - this.Fuel) * (this.IsMarried ? 0.85 : 1)), 1);

        /// <summary>
        /// 補給で消費する弾薬
        /// </summary>
        public int SupplyAmmo => (this.AmmoMax - this.Ammo) == 0 ? 0 : Math.Max((int)Math.Floor((this.AmmoMax - this.Ammo) * (this.IsMarried ? 0.85 : 1)), 1);


        /// <summary>
        /// 搭載機残量割合
        /// </summary>
        public ReadOnlyCollection<double> AircraftRate
        {
            get
            {
                double[] airs = new double[this._aircraft.Length];
                var airmax = this.MasterShip.Aircraft;

                for (int i = 0; i < airs.Length; i++)
                {
                    airs[i] = (double)this._aircraft[i] / Math.Max(airmax[i], 1);
                }

                return Array.AsReadOnly(airs);
            }
        }

        /// <summary>
        /// 搭載機残量割合
        /// </summary>
        public double AircraftTotalRate => (double)this.AircraftTotal / Math.Max(this.MasterShip.AircraftTotal, 1);



        /// <summary>
        /// 補強装備スロットが使用可能か
        /// </summary>
        public bool IsExpansionSlotAvailable => this.ExpansionSlot != 0;



        #region ダメージ威力計算

        /// <summary>
        /// 航空戦威力
        /// 本来スロットごとのものであるが、ここでは最大火力を採用する
        /// </summary>
        public int AirBattlePower => this._airbattlePowers.Max();

        private int[] _airbattlePowers;
        /// <summary>
        /// 各スロットの航空戦威力
        /// </summary>
        public ReadOnlyCollection<int> AirBattlePowers => Array.AsReadOnly(this._airbattlePowers);

        /// <summary>
        /// 砲撃威力
        /// </summary>
        public int ShellingPower { get; private set; }

        //todo: ShellingPower に統合予定
        /// <summary>
        /// 空撃威力
        /// </summary>
        public int AircraftPower { get; private set; }

        /// <summary>
        /// 対潜威力
        /// </summary>
        public int AntiSubmarinePower { get; private set; }

        /// <summary>
        /// 雷撃威力
        /// </summary>
        public int TorpedoPower { get; private set; }

        /// <summary>
        /// 夜戦威力
        /// </summary>
        public int NightBattlePower { get; private set; }



        /// <summary>
        /// 装備改修補正(砲撃戦)
        /// </summary>
        private double GetDayBattleEquipmentLevelBonus()
        {

            double basepower = 0;
            foreach (var slot in this.AllSlotInstance)
            {
                if (slot == null)
                    continue;

                switch (slot.MasterEquipment.CategoryType)
                {
                    case EquipmentTypes.MainGunSmall:
                    case EquipmentTypes.MainGunMedium:
                    case EquipmentTypes.APShell:
                    case EquipmentTypes.AADirector:
                    case EquipmentTypes.Searchlight:
                    case EquipmentTypes.SearchlightLarge:
                    case EquipmentTypes.AAGun:
                    case EquipmentTypes.LandingCraft:
                    case EquipmentTypes.SpecialAmphibiousTank:
                        basepower += Math.Sqrt(slot.Level);
                        break;

                    case EquipmentTypes.MainGunLarge:
                    case EquipmentTypes.MainGunLarge2:
                        basepower += Math.Sqrt(slot.Level) * 1.5;
                        break;

                    case EquipmentTypes.SecondaryGun:
                        switch (slot.EquipmentID)
                        {
                            case 10:        // 12.7cm連装高角砲
                            case 66:        // 8cm高角砲
                            case 220:       // 8cm高角砲改+増設機銃
                            case 275:       // 10cm連装高角砲改+増設機銃
                                basepower += 0.2 * slot.Level;
                                break;

                            case 12:        // 15.5cm三連装副砲
                            case 234:       // 15.5cm三連装副砲改
                                basepower += 0.3 * slot.Level;
                                break;

                            default:
                                basepower += Math.Sqrt(slot.Level);
                                break;
                        }
                        break;

                    case EquipmentTypes.Sonar:
                    case EquipmentTypes.SonarLarge:
                        basepower += Math.Sqrt(slot.Level) * 0.75;
                        break;

                    case EquipmentTypes.DepthCharge:
                        if (slot.MasterEquipment.IsDepthCharge == false)
                            basepower += Math.Sqrt(slot.Level) * 0.75;
                        break;

                }
            }
            return basepower;
        }



        /// <summary>
        /// 装備改修補正(雷撃戦)
        /// </summary>
        private double GetTorpedoEquipmentLevelBonus()
        {
            double basepower = 0;
            foreach (var slot in this.AllSlotInstance)
            {
                if (slot == null)
                    continue;

                switch (slot.MasterEquipment.CategoryType)
                {
                    case EquipmentTypes.Torpedo:
                    case EquipmentTypes.AAGun:
                    case EquipmentTypes.SubmarineTorpedo:
                        basepower += Math.Sqrt(slot.Level) * 1.2;
                        break;
                }
            }
            return basepower;
        }

        /// <summary>
        /// 装備改修補正(対潜)
        /// </summary>
        private double GetAntiSubmarineEquipmentLevelBonus()
        {
            double basepower = 0;
            foreach (var slot in this.AllSlotInstance)
            {
                if (slot == null)
                    continue;

                switch (slot.MasterEquipment.CategoryType)
                {
                    case EquipmentTypes.DepthCharge:
                    case EquipmentTypes.Sonar:
                        basepower += Math.Sqrt(slot.Level);
                        break;
                }
            }
            return basepower;
        }

        /// <summary>
        /// 装備改修補正(夜戦)
        /// </summary>
        private double GetNightBattleEquipmentLevelBonus()
        {
            double basepower = 0;
            foreach (var slot in this.AllSlotInstance)
            {
                if (slot == null)
                    continue;

                switch (slot.MasterEquipment.CategoryType)
                {
                    case EquipmentTypes.MainGunSmall:
                    case EquipmentTypes.MainGunMedium:
                    case EquipmentTypes.MainGunLarge:
                    case EquipmentTypes.Torpedo:
                    case EquipmentTypes.APShell:
                    case EquipmentTypes.LandingCraft:
                    case EquipmentTypes.Searchlight:
                    case EquipmentTypes.SubmarineTorpedo:
                    case EquipmentTypes.AADirector:
                    case EquipmentTypes.MainGunLarge2:
                    case EquipmentTypes.SearchlightLarge:
                    case EquipmentTypes.SpecialAmphibiousTank:
                        basepower += Math.Sqrt(slot.Level);
                        break;

                    case EquipmentTypes.SecondaryGun:
                        switch (slot.EquipmentID)
                        {
                            case 10:        // 12.7cm連装高角砲
                            case 66:        // 8cm高角砲
                            case 220:       // 8cm高角砲改+増設機銃
                            case 275:       // 10cm連装高角砲改+増設機銃
                                basepower += 0.2 * slot.Level;
                                break;

                            case 12:        // 15.5cm三連装副砲
                            case 234:       // 15.5cm三連装副砲改
                                basepower += 0.3 * slot.Level;
                                break;

                            default:
                                basepower += Math.Sqrt(slot.Level);
                                break;
                        }
                        break;
                }
            }
            return basepower;
        }

        /// <summary>
        /// 耐久値による攻撃力補正
        /// </summary>
        private double GetHPDamageBonus()
        {
            if (this.HPRate <= 0.25)
                return 0.4;
            else if (this.HPRate <= 0.5)
                return 0.7;
            else
                return 1.0;
        }

        /// <summary>
        /// 耐久値による攻撃力補正(雷撃戦)
        /// </summary>
        /// <returns></returns>
        private double GetTorpedoHPDamageBonus()
        {
            if (this.HPRate <= 0.25)
                return 0.0;
            else if (this.HPRate <= 0.5)
                return 0.8;
            else
                return 1.0;
        }

        /// <summary>
        /// 交戦形態による威力補正
        /// </summary>
        private double GetEngagementFormDamageRate(int form)
        {
            switch (form)
            {
                case 1:     // 同航戦
                default:
                    return 1.0;
                case 2:     // 反航戦
                    return 0.8;
                case 3:     // T字有利
                    return 1.2;
                case 4:     // T字不利
                    return 0.6;
            }
        }

        /// <summary>
        /// 残り弾薬量による威力補正
        /// <returns></returns>
        private double GetAmmoDamageRate()
        {
            return Math.Min(Math.Floor(this.AmmoRate * 100) / 50.0, 1.0);
        }

        /// <summary>
        /// 連合艦隊編成における砲撃戦火力補正
        /// </summary>
        private double GetCombinedFleetShellingDamageBonus()
        {
            int fleet = this.Fleet;
            if (fleet == -1 || fleet > 2)
                return 0;

            switch (KCDatabase.Instance.Fleet.CombinedFlag)
            {
                case 1:     //機動部隊
                    if (fleet == 1)
                        return +2;
                    else
                        return +10;

                case 2:     //水上部隊
                    if (fleet == 1)
                        return +10;
                    else
                        return -5;

                case 3:     //輸送部隊
                    if (fleet == 1)
                        return -5;
                    else
                        return +10;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// 連合艦隊編成における雷撃戦火力補正
        /// </summary>
        private double GetCombinedFleetTorpedoDamageBonus()
        {
            int fleet = this.Fleet;
            if (fleet == -1 || fleet > 2)
                return 0;

            if (KCDatabase.Instance.Fleet.CombinedFlag == 0)
                return 0;

            return -5;
        }

        /// <summary>
        /// 軽巡軽量砲補正
        /// </summary>
        private double GetLightCruiserDamageBonus()
        {
            if (this.MasterShip.ShipType == ShipTypes.LightCruiser ||
                this.MasterShip.ShipType == ShipTypes.TorpedoCruiser ||
                this.MasterShip.ShipType == ShipTypes.TrainingCruiser)
            {
                int single = 0;
                int twin = 0;

                foreach (var slot in this.AllSlotMaster)
                {
                    if (slot == -1) continue;

                    switch (slot)
                    {
                        case 4:     // 14cm単装砲
                        case 11:    // 15.2cm単装砲
                            single++;
                            break;
                        case 65:    // 15.2cm連装砲
                        case 119:   // 14cm連装砲
                        case 139:   // 15.2cm連装砲改
                            twin++;
                            break;
                    }
                }

                return Math.Sqrt(twin) * 2.0 + Math.Sqrt(single);
            }

            return 0;
        }

        /// <summary>
        /// イタリア重巡砲補正
        /// </summary>
        /// <returns></returns>
        private double GetItalianDamageBonus()
        {
            switch (this.ShipID)
            {
                case 448:       // Zara
                case 358:       // 改
                case 496:       // due
                case 449:       // Pola
                case 361:       // 改
                    return Math.Sqrt(this.AllSlotMaster.Count(id => id == 162));     // √( 203mm/53 連装砲 装備数 )

                default:
                    return 0;
            }
        }

        private double CapDamage(double damage, int max)
        {
            if (damage < max)
                return damage;
            else
                return max + Math.Sqrt(damage - max);
        }


        /// <summary>
        /// 航空戦での威力を求めます。
        /// </summary>
        /// <param name="slotIndex">スロットのインデックス。 0 起点です。</param>
        private int CalculateAirBattlePower(int slotIndex)
        {
            double basepower = 0;
            var slots = this.AllSlotInstance;

            var eq = this.SlotInstance[slotIndex];

            if (eq == null || this._aircraft[slotIndex] == 0)
                return 0;

            switch (eq.MasterEquipment.CategoryType)
            {
                case EquipmentTypes.CarrierBasedBomber:
                case EquipmentTypes.SeaplaneBomber:
                case EquipmentTypes.JetBomber:              // 通常航空戦においては /√2 されるが、とりあえず考えない
                    basepower = eq.MasterEquipment.Bomber * Math.Sqrt(this._aircraft[slotIndex]) + 25;
                    break;
                case EquipmentTypes.CarrierBasedTorpedo:
                case EquipmentTypes.JetTorpedo:
                    // 150% 補正を引いたとする
                    basepower = (eq.MasterEquipment.Torpedo * Math.Sqrt(this._aircraft[slotIndex]) + 25) * 1.5;
                    break;
                default:
                    return 0;
            }

            //キャップ
            basepower = Math.Floor(this.CapDamage(basepower, 170));

            return (int)(basepower * this.GetAmmoDamageRate());
        }

        /// <summary>
        /// 砲撃戦での砲撃威力を求めます。
        /// </summary>
        /// <param name="engagementForm">交戦形態。既定値は 1 (同航戦) です。</param>
        private int CalculateShellingPower(int engagementForm = 1)
        {
            var attackKind = Calculator.GetDayAttackKind(this.AllSlotMaster.ToArray(), this.ShipID, -1);
            if (attackKind == DayAttackKind.AirAttack || attackKind == DayAttackKind.CutinAirAttack)
                return 0;

            double basepower = this.FirepowerTotal + this.GetDayBattleEquipmentLevelBonus() + this.GetCombinedFleetShellingDamageBonus() + 5;

            basepower *= this.GetHPDamageBonus() * this.GetEngagementFormDamageRate(engagementForm);

            basepower += this.GetLightCruiserDamageBonus() + this.GetItalianDamageBonus();

            // キャップ
            basepower = Math.Floor(this.CapDamage(basepower, 220));

            // 弾着観測射撃
            switch (attackKind)
            {
                case DayAttackKind.DoubleShelling:
                case DayAttackKind.CutinMainRadar:
                    basepower *= 1.2;
                    break;
                case DayAttackKind.CutinMainSub:
                    basepower *= 1.1;
                    break;
                case DayAttackKind.CutinMainAP:
                    basepower *= 1.3;
                    break;
                case DayAttackKind.CutinMainMain:
                    basepower *= 1.5;
                    break;
                case DayAttackKind.ZuiunMultiAngle:
                    basepower *= 1.35;
                    break;
                case DayAttackKind.SeaAirMultiAngle:
                    basepower *= 1.3;
                    break;
            }

            return (int)(basepower * this.GetAmmoDamageRate());
        }

        /// <summary>
        /// 砲撃戦での空撃威力を求めます。
        /// </summary>
        /// <param name="engagementForm">交戦形態。既定値は 1 (同航戦) です。</param>
        private int CalculateAircraftPower(int engagementForm = 1)
        {
            var attackKind = Calculator.GetDayAttackKind(this.AllSlotMaster.ToArray(), this.ShipID, -1);
            if (attackKind != DayAttackKind.AirAttack && attackKind != DayAttackKind.CutinAirAttack)
                return 0;

            double basepower = Math.Floor((this.FirepowerTotal + this.TorpedoTotal + Math.Floor(this.BomberTotal * 1.3) + this.GetDayBattleEquipmentLevelBonus() + this.GetCombinedFleetShellingDamageBonus()) * 1.5) + 55;

            basepower *= this.GetHPDamageBonus() * this.GetEngagementFormDamageRate(engagementForm);

            // キャップ
            basepower = Math.Floor(this.CapDamage(basepower, 220));


            // 空母カットイン
            if (attackKind == DayAttackKind.CutinAirAttack)
            {
                var kind = Calculator.GetDayAirAttackCutinKind(this.SlotInstanceMaster);
                switch (kind)
                {
                    case DayAirAttackCutinKind.FighterBomberAttacker:
                        basepower *= 1.25;
                        break;

                    case DayAirAttackCutinKind.BomberBomberAttacker:
                        basepower *= 1.20;
                        break;

                    case DayAirAttackCutinKind.BomberAttacker:
                        basepower *= 1.15;
                        break;
                }
            }

            return (int)(basepower * this.GetAmmoDamageRate());
        }

        public double CalculateAntiSubmarineSynergy()
        {
            int depthChargeCount = 0;
            int depthChargeProjectorCount = 0;
            int otherDepthChargeCount = 0;
            int sonarCount = 0;         // ソナーと大型ソナーの合算
            int largeSonarCount = 0;

            foreach (var slot in this.AllSlotInstanceMaster)
            {
                if (slot == null)
                    continue;

                switch (slot.CategoryType)
                {
                    case EquipmentTypes.DepthCharge:
                        if (slot.IsDepthCharge)
                            depthChargeCount++;
                        else if (slot.IsDepthChargeProjector)
                            depthChargeProjectorCount++;
                        else
                            otherDepthChargeCount++;
                        break;

                    case EquipmentTypes.Sonar:
                        sonarCount++;
                        break;
                    case EquipmentTypes.SonarLarge:
                        largeSonarCount++;
                        sonarCount++;
                        break;
                }
            }

            float newSynergy = 1.0f;
            float oldSynergy = 1.0f;

            if ((sonarCount > 0  || largeSonarCount > 0) && depthChargeCount > 0 )
                oldSynergy = 1.15f;

            if (sonarCount > 0 && depthChargeCount > 0 && depthChargeProjectorCount > 0)
            {
                newSynergy = 1.25f;
            }
            else if (depthChargeCount > 0 && depthChargeProjectorCount > 0)
            {
                newSynergy = 1.1f;
            }

            return newSynergy * oldSynergy;
        }

        /// <summary>
        /// 砲撃戦での対潜威力を求めます。
        /// </summary>
        /// <param name="engagementForm">交戦形態。既定値は 1 (同航戦) です。</param>
        private int CalculateAntiSubmarinePower(int engagementForm = 1)
        {
            if (!this.CanAttackSubmarine)
                return 0;

            double eqpower = 0;
            foreach (var slot in this.AllSlotInstance)
            {
                if (slot == null)
                    continue;

                switch (slot.MasterEquipment.CategoryType)
                {
                    case EquipmentTypes.CarrierBasedBomber:
                    case EquipmentTypes.CarrierBasedTorpedo:
                    case EquipmentTypes.SeaplaneBomber:
                    case EquipmentTypes.Sonar:
                    case EquipmentTypes.DepthCharge:
                    case EquipmentTypes.Autogyro:
                    case EquipmentTypes.ASPatrol:
                    case EquipmentTypes.SonarLarge:
                        eqpower += slot.MasterEquipment.ASW;
                        break;
                }
            }

            double basepower = Math.Sqrt(this.ASWBase) * 2 + eqpower * 1.5 + this.GetAntiSubmarineEquipmentLevelBonus();
            if (Calculator.GetDayAttackKind(this.AllSlotMaster.ToArray(), this.ShipID, 126, false) == DayAttackKind.AirAttack)
            {       //126=伊168; 対潜攻撃が空撃なら
                basepower += 8;
            }
            else
            {   //爆雷攻撃なら
                basepower += 13;
            }

            basepower *= this.GetHPDamageBonus() * this.GetEngagementFormDamageRate(engagementForm);
            //対潜シナジー
            basepower *= this.CalculateAntiSubmarineSynergy();
            //キャップ
            basepower = Math.Floor(this.CapDamage(basepower, 170));

            return (int)(basepower * this.GetAmmoDamageRate());
        }

        /// <summary>
        /// 雷撃戦での威力を求めます。
        /// </summary>
        /// <param name="engagementForm">交戦形態。既定値は 1 (同航戦) です。</param>
        private int CalculateTorpedoPower(int engagementForm = 1)
        {
            if (this.TorpedoBase == 0)
                return 0;       //雷撃不能艦は除外

            double basepower = this.TorpedoTotal + this.GetTorpedoEquipmentLevelBonus() + this.GetCombinedFleetTorpedoDamageBonus() + 5;

            basepower *= this.GetTorpedoHPDamageBonus() * this.GetEngagementFormDamageRate(engagementForm);

            //キャップ
            basepower = Math.Floor(this.CapDamage(basepower, 180));


            return (int)(basepower * this.GetAmmoDamageRate());
        }

        /// <summary>
        /// 夜戦での威力を求めます。
        /// </summary>
        private int CalculateNightBattlePower()
        {
            var kind = Calculator.GetNightAttackKind(this.AllSlotMaster.ToArray(), this.ShipID, -1);
            double basepower = 0;

            if (kind == NightAttackKind.CutinAirAttack)
            {
                var airs = this.SlotInstance.Zip(this.Aircraft, (eq, count) => new { eq, master = eq?.MasterEquipment, count }).Where(a => a.eq != null);

                basepower = this.FirepowerBase +
                    airs.Where(p => p.master.IsNightAircraft)
                        .Sum(p => p.master.Firepower + p.master.Torpedo + p.master.Bomber +
                            3 * p.count +
                            0.45 * (p.master.Firepower + p.master.Torpedo + p.master.Bomber + p.master.ASW) * Math.Sqrt(p.count) + Math.Sqrt(p.eq.Level)) +
                    airs.Where(p => p.master.IsSwordfish || p.master.EquipmentID == 154 || p.master.EquipmentID == 320)   // 零戦62型(爆戦/岩井隊)、彗星一二型(三一号光電管爆弾搭載機)
                        .Sum(p => p.master.Firepower + p.master.Torpedo + p.master.Bomber +
                            0.3 * (p.master.Firepower + p.master.Torpedo + p.master.Bomber + p.master.ASW) * Math.Sqrt(p.count) + Math.Sqrt(p.eq.Level));

            }
            else if (this.ShipID == 515 || this.ShipID == 393)
            {       // Ark Royal (改)
                basepower = this.FirepowerBase + this.SlotInstanceMaster.Where(eq => eq?.IsSwordfish ?? false).Sum(eq => eq.Firepower + eq.Torpedo);
            }
            else if (this.ShipID == 353 || this.ShipID == 432 || this.ShipID == 433)
            {       // Graf Zeppelin(改), Saratoga
                basepower = this.FirepowerBase + this.SlotInstanceMaster.Where(eq => eq?.IsSwordfish ?? false).Sum(eq => eq.Firepower + eq.Torpedo);
            }

            // 카가 개2호, 타이요형 추가..
            else
            {
                basepower = this.FirepowerTotal + this.TorpedoTotal + this.GetNightBattleEquipmentLevelBonus();
            }


            basepower *= this.GetHPDamageBonus();

            switch (kind)
            {
                case NightAttackKind.DoubleShelling:
                    basepower *= 1.2;
                    break;

                case NightAttackKind.CutinMainTorpedo:
                    basepower *= 1.3;
                    break;

                case NightAttackKind.CutinTorpedoTorpedo:
                    {
                        switch (Calculator.GetNightTorpedoCutinKind(this.AllSlotInstanceMaster, this.ShipID, -1))
                        {
                            case NightTorpedoCutinKind.LateModelTorpedoSubmarineEquipment:
                                basepower *= 1.75;
                                break;
                            case NightTorpedoCutinKind.LateModelTorpedo2:
                                basepower *= 1.6;
                                break;
                            default:
                                basepower *= 1.5;
                                break;
                        }
                    }
                    break;

                case NightAttackKind.CutinMainSub:
                    basepower *= 1.75;
                    break;

                case NightAttackKind.CutinMainMain:
                    basepower *= 2.0;
                    break;

                case NightAttackKind.CutinAirAttack:
                    {
                        int nightFighter = this.SlotInstanceMaster.Count(eq => eq?.IsNightFighter ?? false);
                        int nightAttacker = this.SlotInstanceMaster.Count(eq => eq?.IsNightAttacker ?? false);
                        int nightBomber = this.SlotInstanceMaster.Count(eq => eq?.EquipmentID == 320);     // 彗星一二型(三一号光電管爆弾搭載機)

                        if (nightFighter >= 2 && nightAttacker >= 1)
                            basepower *= 1.25;
                        else if (nightBomber >= 1 && nightFighter + nightAttacker >= 1)
                            basepower *= 1.2;
                        else if (nightFighter >= 1 && nightAttacker >= 1)
                            basepower *= 1.2;
                        else
                            basepower *= 1.18;
                    }
                    break;

                case NightAttackKind.CutinTorpedoRadar:
                    {
                        double baseModifier = 1.3;
                        int typeDmod2 = this.AllSlotInstanceMaster.Count(eq => eq?.EquipmentID == 267);  // 12.7cm連装砲D型改二
                        int typeDmod3 = this.AllSlotInstanceMaster.Count(eq => eq?.EquipmentID == 366);  // 12.7cm連装砲D型改三
                        var modifierTable = new double[] { 1, 1.25, 1.4 };

                        baseModifier *= modifierTable[Math.Min(typeDmod2 + typeDmod3, modifierTable.Length - 1)] * (1 + typeDmod3 * 0.05);

                        basepower *= baseModifier;
                        break;
                    }

                case NightAttackKind.CutinTorpedoPicket:
                    {
                        double baseModifier = 1.25;     // TODO: 処理の共通化
                        int typeDmod2 = this.AllSlotInstanceMaster.Count(eq => eq?.EquipmentID == 267);  // 12.7cm連装砲D型改二
                        int typeDmod3 = this.AllSlotInstanceMaster.Count(eq => eq?.EquipmentID == 366);  // 12.7cm連装砲D型改三
                        var modifierTable = new double[] { 1, 1.25, 1.4 };

                        baseModifier *= modifierTable[Math.Min(typeDmod2 + typeDmod3, modifierTable.Length - 1)] * (1 + typeDmod3 * 0.05);

                        basepower *= baseModifier;
                        break;
                    }
            }

            basepower += this.GetLightCruiserDamageBonus() + this.GetItalianDamageBonus();

            //キャップ
            basepower = Math.Floor(this.CapDamage(basepower, 360));


            return (int)(basepower * this.GetAmmoDamageRate());
        }


        /// <summary>
        /// 威力系の計算をまとめて行い、プロパティを更新します。
        /// </summary>
        private void CalculatePowers()
        {

            int form = Utility.Configuration.Config.Control.PowerEngagementForm;

            this._airbattlePowers = this.Slot.Select((_, i) => this.CalculateAirBattlePower(i)).ToArray();
            this.ShellingPower = this.CalculateShellingPower(form);
            this.AircraftPower = this.CalculateAircraftPower(form);
            this.AntiSubmarinePower = this.CalculateAntiSubmarinePower(form);
            this.TorpedoPower = this.CalculateTorpedoPower(form);
            this.NightBattlePower = this.CalculateNightBattlePower();

        }


        #endregion



        /// <summary>
        /// 対潜攻撃可能か
        /// </summary>
        public bool CanAttackSubmarine
        {
            get
            {
                switch (this.MasterShip.ShipType)
                {
                    case ShipTypes.Escort:
                    case ShipTypes.Destroyer:
                    case ShipTypes.LightCruiser:
                    case ShipTypes.TorpedoCruiser:
                    case ShipTypes.TrainingCruiser:
                    case ShipTypes.FleetOiler:
                        return this.ASWBase > 0;

                    case ShipTypes.AviationCruiser:
                    case ShipTypes.LightAircraftCarrier:
                    case ShipTypes.AviationBattleship:
                    case ShipTypes.SeaplaneTender:
                    case ShipTypes.AmphibiousAssaultShip:
                        return this.AllSlotInstanceMaster.Any(eq => eq != null && eq.IsAntiSubmarineAircraft);

                    case ShipTypes.AircraftCarrier:
                        return this.ShipID == 646 &&         // 加賀改二護
                            this.AllSlotInstanceMaster.Any(eq => eq != null && eq.IsAntiSubmarineAircraft);

                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// 開幕対潜攻撃可能か
        /// </summary>
        public bool CanOpeningASW
        {
            get
            {
                if (this.CanAttackSubmarine == false)
                    return false;

                switch (this.ShipID)
                {
                    case 141:       // 五十鈴改二
                    case 394:       // Jervis改
                    case 478:       // 龍田改二
                    case 681:       // Samuel B.Roberts改
                    case 562:       // Johnston
                    case 689:       // Johnston改
                    case 596:       // Fletcher
                    case 692:       // Fletcher改
                    case 628:       // Fletcher改 Mod.2
                    case 629:       // Fletcher Mk.II
                    case 893:       // Janus改
                    case 624:       // 夕張改二丁
                        return true;
                }

                var eqs = this.AllSlotInstance.Where(eq => eq != null);

                switch (this.ShipID)
                {
                    case 380:   // 大鷹改
                    case 529:   // 大鷹改二
                    case 381:   // 神鷹改
                    case 536:   // 神鷹改二
                    case 646:	// 加賀改二護
                        return true;

                    case 554:   // 日向改二
                        // カ号観測機, オ号観測機改, オ号観測機改二
                        if (eqs.Count(eq => eq.EquipmentID == 69 || eq.EquipmentID == 324 || eq.EquipmentID == 325) >= 2)
                            return true;
                        // S-51J, S-51J改
                        if (eqs.Any(eq => eq.EquipmentID == 326 || eq.EquipmentID == 327))
                            return true;

                        return false;
                }

                if (this.MasterShip.ShipType == ShipTypes.LightAircraftCarrier && this.ASWBase > 0)      // 護衛空母
                {
                    bool hasASWAircraft = eqs.Any(eq =>
                        (eq.MasterEquipment.CategoryType == EquipmentTypes.CarrierBasedTorpedo && eq.MasterEquipment.ASW >= 7) ||
                        eq.MasterEquipment.CategoryType == EquipmentTypes.ASPatrol ||
                        eq.MasterEquipment.CategoryType == EquipmentTypes.Autogyro);

                    if (hasASWAircraft && this.ASWTotal >= 65)
                        return true;

                    if (hasASWAircraft && this.ASWTotal >= 50 && eqs.Any(eq => eq.MasterEquipment.CategoryType == EquipmentTypes.SonarLarge))
                        return true;
                }

                bool hasSonar = eqs.Any(eq => eq.MasterEquipment.IsSonar);
                bool needSonar = !(
                    this.MasterShip.ShipType == ShipTypes.Escort &&
                    this.ASWTotal >= 75 &&
                    (this.ASWTotal - this.ASWBase) >= 4);

                if (needSonar && !hasSonar)
                    return false;

                if (this.MasterShip.ShipType == ShipTypes.Escort)
                    return this.ASWTotal >= 60;
                else
                    return this.ASWTotal >= 100;
            }
        }


        /// <summary>
        /// 夜戦攻撃可能か
        /// </summary>
        public bool CanAttackAtNight
        {
            get
            {
                var master = this.MasterShip;

                if (this.HPRate <= 0.25)
                    return false;

                if (master.FirepowerMin + master.TorpedoMin > 0)
                    return true;

                // Ark Royal(改)
                if (master.ShipID == 515 || master.ShipID == 393)
                {
                    if (this.AllSlotInstanceMaster.Any(eq => eq != null && eq.IsSwordfish))
                        return true;
                }

                if (master.IsAircraftCarrier)
                {
                    // 装甲空母ではなく、中破以上の被ダメージ
                    if (master.ShipType != ShipTypes.ArmoredAircraftCarrier && this.HPRate <= 0.5)
                        return false;

                    // Saratoga Mk.II/赤城改二戊/加賀改二戊 は不要
                    bool hasNightPersonnel = master.ShipID == 545 || master.ShipID == 599 || master.ShipID == 610 ||
                        this.AllSlotInstanceMaster.Any(eq => eq != null && eq.IsNightAviationPersonnel);

                    bool hasNightAircraft = this.AllSlotInstanceMaster.Any(eq => eq != null && eq.IsNightAircraft);

                    if (hasNightPersonnel && hasNightAircraft)
                        return true;
                }

                return false;
            }
        }


        /// <summary>
        /// 発動可能なダメコンのID -1=なし, 42=要員, 43=女神
        /// </summary>
        public int DamageControlID
        {
            get
            {
                if (this.ExpansionSlotMaster == 42 || this.ExpansionSlotMaster == 43)
                    return this.ExpansionSlotMaster;

                foreach (var eq in this.SlotMaster)
                {
                    if (eq == 42 || eq == 43)
                        return eq;
                }

                return -1;
            }
        }



        public int ID => this.MasterID;
        public override string ToString() => $"[{this.MasterID}] {this.NameWithLevel}";


        public override void LoadFromResponse(string apiname, dynamic data)
        {

            switch (apiname)
            {
                default:
                    base.LoadFromResponse(apiname, (object)data);

                    this.HPCurrent = (int)this.RawData.api_nowhp;
                    this.Fuel = (int)this.RawData.api_fuel;
                    this.Ammo = (int)this.RawData.api_bull;
                    this.Condition = (int)this.RawData.api_cond;
                    this.Slot = Array.AsReadOnly((int[])this.RawData.api_slot);
                    this.ExpansionSlot = (int)this.RawData.api_slot_ex;
                    this._aircraft = (int[])this.RawData.api_onslot;
                    this._modernized = (int[])this.RawData.api_kyouka;
                    break;

                case "api_req_hokyu/charge":
                    this.Fuel = (int)data.api_fuel;
                    this.Ammo = (int)data.api_bull;
                    this._aircraft = (int[])data.api_onslot;
                    break;
            }

            this.CalculatePowers();
        }


        public override void LoadFromRequest(string apiname, Dictionary<string, string> data)
        {
            base.LoadFromRequest(apiname, data);

            KCDatabase db = KCDatabase.Instance;

            switch (apiname)
            {
                case "api_req_kousyou/destroyship":
                    {
                        for (int i = 0; i < this.Slot.Count; i++)
                        {
                            if (this.Slot[i] == -1)
                                continue;

                            db.Equipments.Remove(this.Slot[i]);
                        }
                    }
                    break;

                case "api_req_kaisou/open_exslot":
                    this.ExpansionSlot = -1;
                    break;
            }
        }


        /// <summary>
        /// 入渠完了時の処理を行います。
        /// </summary>
        internal void Repair()
        {

            this.HPCurrent = this.HPMax;
            this.Condition = Math.Max(this.Condition, 40);

            this.RawData.api_ndock_time = 0;
            this.RawData.api_ndock_item[0] = 0;
            this.RawData.api_ndock_item[1] = 0;

        }


    }

}
