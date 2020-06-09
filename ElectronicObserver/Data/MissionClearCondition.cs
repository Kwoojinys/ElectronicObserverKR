﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicObserver.Data
{
    /// <summary>
    /// 遠征可否判定を行います。
    /// </summary>
    public static class MissionClearCondition
    {

        /// <summary>
        /// 遠征に成功する編成かどうかを判定します。
        /// </summary>
        /// <param name="missionID">遠征ID。</param>
        /// <param name="fleet">対象となる艦隊。達成条件を確認したい場合は null を指定します。</param>
        public static MissionClearConditionResult Check(int missionID, FleetData fleet)
        {
            var result = new MissionClearConditionResult(fleet);

            switch (missionID)
            {
                case 1:     // 練習航海
                    return result
                        .CheckFlagshipLevel(1)
                        .CheckShipCount(2);
                case 2:     // 長距離練習航海
                    return result
                        .CheckFlagshipLevel(2)
                        .CheckShipCount(4);
                case 3:     // 警備任務
                    return result
                        .CheckFlagshipLevel(3)
                        .CheckShipCount(3);
                case 4:     // 対潜警戒任務
                    return result
                        .CheckFlagshipLevel(3)
                        .CheckEscortFleet();
                case 5:     // 海上護衛任務
                    return result
                        .CheckFlagshipLevel(3)
                        .CheckShipCount(4)
                        .CheckEscortFleet();
                case 6:     // 防空射撃演習
                    return result
                        .CheckFlagshipLevel(4)
                        .CheckShipCount(4);
                case 7:     // 観艦式予行
                    return result
                        .CheckFlagshipLevel(5)
                        .CheckShipCount(6);
                case 8:     // 観艦式
                    return result
                        .CheckFlagshipLevel(6)
                        .CheckShipCount(6);
                case 100:   // 兵站強化任務
                    return result
                        .CheckFlagshipLevel(5)
                        .CheckLevelSum(10)
                        .CheckShipCount(4)
                        .CheckSmallShipCount(3);
                case 101:   // 海峡警備行動
                    return result
                        .CheckFlagshipLevel(20)
                        .CheckSmallShipCount(4)
                        .CheckFirepower(50)
                        .CheckAA(70)
                        .CheckASW(180);
                case 102:   // 長時間対潜警戒
                    return result
                        .CheckFlagshipLevel(35)
                        .CheckLevelSum(185)
                        .CheckShipCount(5)
                        .CheckEscortFleetDD3()
                        .CheckAA(59)
                        .CheckASW(280)
                        .CheckLOS(60);
                case 103:   // 南西方面連絡線哨戒
                    return result
                        .CheckFlagshipLevel(40)
                        .CheckLevelSum(200)
                        .CheckShipCount(5)
                        .CheckEscortFleet()
                        .CheckFirepower(300)
                        .CheckAA(200)
                        .CheckASW(200)
                        .CheckLOS(120);
                case 104:   // 小笠原沖哨戒線(unchecked)
                    return result
                        .CheckFlagshipLevel(50)
                        .CheckLevelSum(260)
                        .CheckShipCount(5)
                        .CheckEscortFleet()     // 適当
                        .CheckFirepower(280)
                        .CheckAA(274)
                        .CheckASW(240)
                        .CheckLOS(150)
                        .SuppressWarnings();
                case 105:   // 小笠原沖戦闘哨戒(unchecked)
                    return result
                        .CheckFlagshipLevel(55)
                        .CheckLevelSum(357)
                        .CheckShipCount(6)
                        .CheckEscortFleetDD3()  // 適当
                        .CheckFirepower(330)
                        .CheckAA(314)
                        .CheckASW(270)
                        .CheckLOS(180)
                        .SuppressWarnings();

                case 9:     // タンカー護衛任務
                    return result
                        .CheckFlagshipLevel(3)
                        .CheckShipCount(4)
                        .CheckEscortFleet();
                case 10:    // 強行偵察任務
                    return result
                        .CheckFlagshipLevel(3)
                        .CheckShipCount(3)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 2);
                case 11:    // ボーキサイト輸送任務
                    return result
                        .CheckFlagshipLevel(6)
                        .CheckShipCount(4)
                        .CheckSmallShipCount(2);
                case 12:    // 資源輸送任務
                    return result
                        .CheckFlagshipLevel(4)
                        .CheckShipCount(4)
                        .CheckSmallShipCount(2);
                case 13:    // 鼠輸送作戦
                    return result
                        .CheckFlagshipLevel(5)
                        .CheckShipCount(6)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                        .CheckShipCountByType(ShipTypes.Destroyer, 4);
                case 14:    // 包囲陸戦隊撤収作戦
                    return result
                        .CheckFlagshipLevel(6)
                        .CheckShipCount(6)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                        .CheckShipCountByType(ShipTypes.Destroyer, 3);
                case 15:    // 囮機動部隊支援作戦
                    return result
                        .CheckFlagshipLevel(8)
                        .CheckShipCount(6)
                        .CheckAircraftCarrierCount(2)
                        .CheckShipCountByType(ShipTypes.Destroyer, 2);
                case 16:    // 艦隊決戦援護作戦
                    return result
                        .CheckFlagshipLevel(10)
                        .CheckShipCount(6)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                        .CheckShipCountByType(ShipTypes.Destroyer, 2);
                case 110:   // 南西方面航空偵察作戦
                    return result
                        .CheckFlagshipLevel(40)
                        .CheckLevelSum(150)
                        .CheckShipCount(6)
                        .CheckShipCountByType(ShipTypes.SeaplaneTender, 1)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                        .CheckSmallShipCount(2)
                        .CheckAA(200)
                        .CheckASW(200)
                        .CheckLOS(140);
                case 111:   // 敵泊地強襲反撃作戦
                    return result
                        .CheckFlagshipLevel(45)
                        .CheckLevelSum(220)
                        .CheckShipCount(6)
                        .CheckShipCountByType(ShipTypes.HeavyCruiser, 1)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                        .CheckShipCountByType(ShipTypes.Destroyer, 3)
                        .CheckFirepower(360)
                        .CheckAA(160)
                        .CheckASW(160)
                        .CheckLOS(140);
                case 112:   // 南西諸島離島哨戒作戦
                    return result
                        .CheckFlagshipLevel(50)
                        .CheckLevelSum(250)
                        .CheckShipCountByType(ShipTypes.SeaplaneTender, 1)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                        .CheckSmallShipCount(4)
                        .CheckFirepower(400)
                        .CheckAA(220)
                        .CheckASW(220)
                        .CheckLOS(190);
                case 113:   // 南西諸島離島防衛作戦
                    return result
                        .CheckFlagshipLevel(55)
                        .CheckLevelSum(300)
                        .CheckShipCountByType(ShipTypes.HeavyCruiser, 2)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                        .CheckShipCountByType(ShipTypes.Destroyer, 2)
                        .CheckSubmarineCount(1)
                        .CheckFirepower(500)
                        .CheckASW(280);
                case 114:   // 南西諸島捜索撃滅戦(unchecked)
                    return result
                        .CheckFlagshipLevel(60)
                        .CheckLevelSum(346)
                        .CheckShipCount(6)
                        .CheckShipCountByType(ShipTypes.SeaplaneTender, 1)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                        .CheckShipCountByType(ShipTypes.Destroyer, 2)
                        .CheckFirepower(510)
                        .CheckAA(400)
                        .CheckASW(285)
                        .CheckLOS(385)
                        .SuppressWarnings();

                case 17:    // 敵地偵察作戦
                    return result
                        .CheckFlagshipLevel(20)
                        .CheckShipCount(6)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                        .CheckShipCountByType(ShipTypes.Destroyer, 3);
                case 18:    // 航空機輸送作戦
                    return result
                        .CheckFlagshipLevel(15)
                        .CheckShipCount(6)
                        .CheckAircraftCarrierCount(3)
                        .CheckShipCountByType(ShipTypes.Destroyer, 2);
                case 19:    // 北号作戦
                    return result
                        .CheckFlagshipLevel(20)
                        .CheckShipCount(6)
                        .CheckShipCountByType(ShipTypes.AviationBattleship, 2)
                        .CheckShipCountByType(ShipTypes.Destroyer, 2);
                case 20:    // 潜水艦哨戒任務
                    return result
                        .CheckFlagshipLevel(1)
                        .CheckSubmarineCount(1)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1);
                case 21:    // 北方鼠輸送作戦
                    return result
                        .CheckFlagshipLevel(15)
                        .CheckLevelSum(30)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                        .CheckShipCountByType(ShipTypes.Destroyer, 4)
                        .CheckEquippedShipCount(EquipmentTypes.TransportContainer, 3);
                case 22:    // 艦隊演習
                    return result
                        .CheckFlagshipLevel(30)
                        .CheckLevelSum(45)
                        .CheckShipCount(6)
                        .CheckShipCountByType(ShipTypes.HeavyCruiser, 1)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                        .CheckShipCountByType(ShipTypes.Destroyer, 2);
                case 23:    // 航空戦艦運用演習
                    return result
                        .CheckFlagshipLevel(50)
                        .CheckLevelSum(200)
                        .CheckShipCount(6)
                        .CheckShipCountByType(ShipTypes.AviationBattleship, 2)
                        .CheckShipCountByType(ShipTypes.Destroyer, 2);
                case 24:    // 北方航路海上護衛
                    return result
                        .CheckFlagshipLevel(50)
                        .CheckLevelSum(200)
                        .CheckShipCount(6)
                        .CheckFlagshipType(ShipTypes.LightCruiser)
                        .CheckSmallShipCount(4);

                case 25:    // 通商破壊作戦
                    return result
                        .CheckFlagshipLevel(25)
                        .CheckShipCountByType(ShipTypes.HeavyCruiser, 2)
                        .CheckShipCountByType(ShipTypes.Destroyer, 2);
                case 26:    // 敵母港空襲作戦
                    return result
                        .CheckFlagshipLevel(30)
                        .CheckAircraftCarrierCount(1)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                        .CheckShipCountByType(ShipTypes.Destroyer, 2);
                case 27:    // 潜水艦通商破壊作戦
                    return result
                        .CheckFlagshipLevel(1)
                        .CheckSubmarineCount(2);
                case 28:    // 潜水艦通商破壊作戦
                    return result
                        .CheckFlagshipLevel(30)
                        .CheckSubmarineCount(3);
                case 29:    // 潜水艦派遣演習
                    return result
                        .CheckFlagshipLevel(50)
                        .CheckSubmarineCount(3);
                case 30:    // 潜水艦派遣作戦
                    return result
                        .CheckFlagshipLevel(55)
                        .CheckSubmarineCount(4);
                case 31:    // 海外艦との接触
                    return result
                        .CheckFlagshipLevel(60)
                        .CheckLevelSum(200)
                        .CheckSubmarineCount(4);
                case 32:    // 遠洋練習航海
                    return result
                        .CheckFlagshipLevel(5)
                        .CheckFlagshipType(ShipTypes.TrainingCruiser)
                        .CheckShipCountByType(ShipTypes.Destroyer, 2);
                case 131:   // 西方海域偵察作戦
                    return result
                        .CheckFlagshipLevel(50)
                        .CheckLevelSum(200)
                        .CheckShipCount(5)
                        .CheckFlagshipType(ShipTypes.SeaplaneTender)
                        .CheckShipCountByType(ShipTypes.Destroyer, 3)
                        .CheckAA(240)
                        .CheckASW(240)
                        .CheckLOS(300);
                case 132:   // 西方潜水艦作戦
                    return result
                        .CheckFlagshipLevel(55)
                        .CheckLevelSum(270)
                        .CheckShipCount(5)
                        .CheckFlagshipType(ShipTypes.SubmarineTender)
                        .CheckSubmarineCount(3)
                        .CheckFirepower(60)
                        .CheckAA(80)
                        .CheckASW(50);

                case 33:    // 前衛支援任務
                    return result
                        .CheckShipCountByType(ShipTypes.Destroyer, 2);
                case 34:    // 艦隊決戦支援任務
                    return result
                        .CheckShipCountByType(ShipTypes.Destroyer, 2);
                case 35:    // MO作戦
                    return result
                        .CheckFlagshipLevel(40)
                        .CheckShipCount(6)
                        .CheckAircraftCarrierCount(2)
                        .CheckShipCountByType(ShipTypes.HeavyCruiser, 1)
                        .CheckShipCountByType(ShipTypes.Destroyer, 1);
                case 36:    // 水上機基地建設
                    return result
                        .CheckFlagshipLevel(30)
                        .CheckShipCount(6)
                        .CheckShipCountByType(ShipTypes.SeaplaneTender, 2)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                        .CheckShipCountByType(ShipTypes.Destroyer, 1);
                case 37:    // 東京急行
                    return result
                        .CheckFlagshipLevel(50)
                        .CheckLevelSum(200)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                        .CheckShipCountByType(ShipTypes.Destroyer, 5)
                        .CheckEquippedShipCount(EquipmentTypes.TransportContainer, 3)
                        .CheckEquipmentCount(EquipmentTypes.TransportContainer, 4);
                case 38:    // 東京急行(弐)
                    return result
                        .CheckFlagshipLevel(65)
                        .CheckLevelSum(240)
                        .CheckShipCount(6)
                        .CheckShipCountByType(ShipTypes.Destroyer, 5)
                        .CheckEquippedShipCount(EquipmentTypes.TransportContainer, 4)
                        .CheckEquipmentCount(EquipmentTypes.TransportContainer, 8);
                case 39:    // 遠洋潜水艦作戦
                    return result
                        .CheckFlagshipLevel(3)
                        .CheckLevelSum(180)
                        .CheckShipCountByType(ShipTypes.SubmarineTender, 1)
                        .CheckSubmarineCount(4);
                case 40:    // 水上機前線輸送
                    return result
                        .CheckFlagshipLevel(25)
                        .CheckLevelSum(150)
                        .CheckShipCount(6)
                        .CheckFlagshipType(ShipTypes.LightCruiser)
                        .CheckShipCountByType(ShipTypes.SeaplaneTender, 2)
                        .CheckShipCountByType(ShipTypes.Destroyer, 2);
                case 141:    // ラバウル方面艦隊進出(unchecked)
                    return result
                        .CheckFlagshipLevel(55)
                        .CheckLevelSum(299)
                        .CheckShipCount(6)
                        .CheckFlagshipType(ShipTypes.HeavyCruiser)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                        .CheckShipCountByType(ShipTypes.Destroyer, 3)
                        .CheckFirepower(450)
                        .CheckAA(350)
                        .CheckASW(330)
                        .CheckLOS(250)
                        .SuppressWarnings();

                case 41:    // ブルネイ泊地沖哨戒
                    return result
                        .CheckFlagshipLevel(30)
                        .CheckLevelSum(100)
                        .CheckSmallShipCount(3)
                        .CheckFirepower(60)
                        .CheckAA(80)
                        .CheckASW(210);
                case 42:    // ミ船団護衛(一号船団)
                    return result
                        .CheckFlagshipLevel(45)
                        .CheckLevelSum(200)
                        .CheckShipCount(4)
                        .CheckEscortFleet();
                case 43:    // ミ船団護衛(二号船団)
                    return result
                        .CheckFlagshipLevel(55)
                        .CheckLevelSum(300)
                        .CheckShipCount(6)
                        .OrCondition(
                            r => r
                                .CheckFlagshipEscortAircraftCarrier()
                                .CheckSmallShipCount(2),
                            r => r
                                .CheckFlagshipType(ShipTypes.LightAircraftCarrier)
                                .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                                .CheckShipCountByType(ShipTypes.Destroyer, 4)
                        )
                        .CheckFirepower(500)
                        .CheckAA(280)
                        .CheckASW(280)
                        .CheckLOS(179);
                case 44:    // 航空装備輸送任務
                    return result
                        .CheckFlagshipLevel(35)
                        .CheckLevelSum(210)
                        .CheckShipCount(6)
                        .CheckAircraftCarrierCount(1)
                        .CheckShipCountByType(ShipTypes.SeaplaneTender, 1)
                        .CheckShipCountByType(ShipTypes.LightCruiser, 1)
                        .CheckSmallShipCount(2)
                        .CheckEquippedShipCount(EquipmentTypes.TransportContainer, 3)
                        .CheckEquipmentCount(EquipmentTypes.TransportContainer, 6)
                        .CheckAA(200)
                        .CheckASW(200)
                        .CheckLOS(150);
                case 45:    // ボーキサイト船団護衛
                    return result
                        .CheckFlagshipLevel(50)
                        .CheckLevelSum(240)
                        .CheckFlagshipType(ShipTypes.LightAircraftCarrier)
                        .CheckSmallShipCount(4)
                        .CheckAA(240)
                        .CheckASW(300)
                        .CheckLOS(180);

                default:
                    {
                        // イベント海域での支援遠征への対応
                        var mission = KCDatabase.Instance.Mission[missionID];

                        if (mission != null && (mission.Name == "전위지원임무(1)" || mission.Name == "함대결전지원임무(2)"))
                        {
                            return result
                                .CheckShipCountByType(ShipTypes.Destroyer, 2);
                        }

                        return result
                            .AddMessage($"미대응(ID:{missionID})");
                    }
            }
        }

        /// <summary>
        /// 遠征可否判定の結果を保持します。
        /// </summary>
        public class MissionClearConditionResult
        {

            /// <summary>
            /// 遠征が成功するかどうか
            /// </summary>
            public bool IsSuceeded { get; private set; }

            private List<string> _failureReason;

            /// <summary>
            /// 遠征が失敗した理由 / 未対応遠征の場合のメッセージ
            /// </summary>
            public ReadOnlyCollection<string> FailureReason => this._failureReason.AsReadOnly();

            // nullable!
            private FleetData _targetFleet;
            private IEnumerable<ShipData> members => (this._targetFleet?.MembersInstance ?? Enumerable.Empty<ShipData>()).Where(s => s != null);


            public MissionClearConditionResult(FleetData targetFleet)
            {
                this._targetFleet = targetFleet;
                this.IsSuceeded = true;
                this._failureReason = new List<string>();
            }


            private void Assert(bool condition, Func<string> failedMessage)
            {
                if (!condition)
                {
                    this.IsSuceeded = false;
                    this._failureReason.Add(failedMessage());
                }
            }

            private string CurrentValue(int value) => this._targetFleet != null ? (value.ToString() + "/") : "";

            public MissionClearConditionResult AddMessage(string message)
            {
                this._failureReason.Add(message);
                return this;
            }
            public MissionClearConditionResult SuppressWarnings()
            {
                this._failureReason.Add("(미확정)");
                this.IsSuceeded = true;
                return this;
            }
            public MissionClearConditionResult OrCondition(params Action<MissionClearConditionResult>[] conditions)
            {
                var conds = new MissionClearConditionResult[conditions.Length];
                for (int i = 0; i < conditions.Length; i++)
                {
                    conds[i] = new MissionClearConditionResult(this._targetFleet);
                    conditions[i](conds[i]);
                }

                this.Assert(conds.Any(c => c.IsSuceeded), () => "(" + string.Join(") or (", conds.Select(c => string.Join(", ", c.FailureReason))) + ")");
                return this;
            }

            public MissionClearConditionResult Fail(string reason)
            {
                this.Assert(false, () => reason);
                return this;
            }

            public MissionClearConditionResult CheckFlagshipLevel(int leastLevel)
            {
                int actualLevel = this.members.FirstOrDefault()?.Level ?? 0;
                this.Assert(actualLevel >= leastLevel,
                    () => $"기함Lv{this.CurrentValue(actualLevel)}{leastLevel}");
                return this;
            }

            public MissionClearConditionResult CheckLevelSum(int leastSum)
            {
                int actualSum = this.members.Sum(s => s.Level);
                this.Assert(actualSum >= leastSum,
                    () => $"Lv합계{this.CurrentValue(actualSum)}{leastSum}");
                return this;
            }

            public MissionClearConditionResult CheckShipCount(int leastCount)
            {
                int actualCount = this.members.Count();
                this.Assert(
                    actualCount >= leastCount,
                    () => $"함선수{this.CurrentValue(actualCount)}{leastCount}");
                return this;
            }


            public MissionClearConditionResult CheckShipCount(Func<ShipData, bool> predicate, int leastCount, string whatis)
            {
                int actualCount = this.members.Count(predicate);
                this.Assert(
                    actualCount >= leastCount,
                    () => $"{whatis}{this.CurrentValue(actualCount)}{leastCount}");
                return this;
            }

            public MissionClearConditionResult CheckShipCountByType(ShipTypes shipType, int leastCount) =>
                this.CheckShipCount(s => s.MasterShip.ShipType == shipType, leastCount, KCDatabase.Instance.ShipTypes[(int)shipType].Name);

            public MissionClearConditionResult CheckSmallShipCount(int leastCount) =>
                this.CheckShipCount(s => s.MasterShip.ShipType == ShipTypes.Destroyer || s.MasterShip.ShipType == ShipTypes.Escort, leastCount, "(구축+해방)");

            public MissionClearConditionResult CheckAircraftCarrierCount(int leastCount) =>
                this.CheckShipCount(s => s.MasterShip.IsAircraftCarrier || s.MasterShip.ShipType == ShipTypes.SeaplaneTender, leastCount, "항모계");

            public MissionClearConditionResult CheckSubmarineCount(int leastCount) =>
               this.CheckShipCount(s => s.MasterShip.IsSubmarine, leastCount, "잠수함계");

            public MissionClearConditionResult CheckEscortFleet()
            {
                int lightCruiser = this.members.Count(s => s.MasterShip.ShipType == ShipTypes.LightCruiser);
                int destroyer = this.members.Count(s => s.MasterShip.ShipType == ShipTypes.Destroyer);
                int trainingCruiser = this.members.Count(s => s.MasterShip.ShipType == ShipTypes.TrainingCruiser);
                int escort = this.members.Count(s => s.MasterShip.ShipType == ShipTypes.Escort);
                int escortAircraftCarrier = this.members.Count(s => s.MasterShip.ShipType == ShipTypes.LightAircraftCarrier && s.ASWBase > 0);

                this.Assert(
                    (lightCruiser >= 1 && (destroyer + escort) >= 2) ||
                    (escortAircraftCarrier >= 1 && (destroyer >= 2 || escort >= 2)) ||
                    (destroyer >= 1 && escort >= 3) ||
                    (trainingCruiser >= 1 && escort >= 2),
                    //() => "[軽巡+(駆逐+海防)2 or 護衛空母)+(駆逐2 or 海防2) or 駆逐+海防3 or 練巡+海防2]"       // 厳密だけど長いので
                    () => "호위대(경순1구축2등)"
                    );
                return this;
            }

            public MissionClearConditionResult CheckEscortFleetDD3()
            {
                int lightCruiser = this.members.Count(s => s.MasterShip.ShipType == ShipTypes.LightCruiser);
                int destroyer = this.members.Count(s => s.MasterShip.ShipType == ShipTypes.Destroyer);
                int trainingCruiser = this.members.Count(s => s.MasterShip.ShipType == ShipTypes.TrainingCruiser);
                int escort = this.members.Count(s => s.MasterShip.ShipType == ShipTypes.Escort);
                int escortAircraftCarrier = this.members.Count(s => s.MasterShip.ShipType == ShipTypes.LightAircraftCarrier && s.ASWBase > 0);

                this.Assert(
                    (lightCruiser >= 1 && (destroyer + escort) >= 3) ||
                    (lightCruiser >= 1 && escort >= 2) ||
                    (escortAircraftCarrier >= 1 && (destroyer >= 2 || escort >= 2)) ||
                    (destroyer >= 1 && escort >= 3) ||
                    (trainingCruiser >= 1 && escort >= 2),
                    //() => "[軽巡+(駆逐+海防)3 or 軽巡+海防2 or 護衛空母+(駆逐2 or 海防2) or 駆逐+海防3 or 練巡+海防2]"       // 厳密だけど長いので
                    () => "호위대(경순1구축3등)"
                    );
                return this;
            }

            public MissionClearConditionResult CheckFlagshipType(ShipTypes shipType)
            {
                this.Assert(
                   this.members.FirstOrDefault()?.MasterShip?.ShipType == shipType,
                    () => $"기함:{KCDatabase.Instance.ShipTypes[(int)shipType].Name}");
                return this;
            }

            public MissionClearConditionResult CheckFlagshipEscortAircraftCarrier()
            {
                Assert(
                   members.FirstOrDefault()?.MasterShip.IsEscortAircraftCarrier ?? false,
                    () => "旗艦:護衛空母");
                return this;
            }

            public MissionClearConditionResult CheckParameter(Func<ShipData, int> selector, int leastSum, string parameterName)
            {
                int actualSum = this.members.Sum(s => selector(s));
                this.Assert(
                    actualSum >= leastSum,
                    () => $"{parameterName}{this.CurrentValue(actualSum)}{leastSum}");
                return this;
            }

            public MissionClearConditionResult CheckFirepower(int leastSum) =>
               this.CheckParameter(s => s.FirepowerTotal, leastSum, "화력");

            public MissionClearConditionResult CheckAA(int leastSum) =>
                this.CheckParameter(s => s.AATotal, leastSum, "대공");

            public MissionClearConditionResult CheckLOS(int leastSum) =>
               this.CheckParameter(s => s.LOSTotal, leastSum, "색적");


            public MissionClearConditionResult CheckASW(int leastSum) =>
                this.CheckParameter(s => s.ASWTotal - s.AllSlotInstance.Sum(eq =>
                {
                    if (eq == null) return 0;
                    switch (eq.MasterEquipment.CategoryType)
                    {
                        case EquipmentTypes.SeaplaneRecon:
                        case EquipmentTypes.SeaplaneBomber:
                        case EquipmentTypes.FlyingBoat:
                            return eq.MasterEquipment.ASW;
                        default:
                            return 0;
                    }
                }), leastSum, "대잠");


            public MissionClearConditionResult CheckEquipmentCount(Func<EquipmentData, bool> predicate, int leastCount, string whatis)
            {
                int actualCount = this.members.Sum(s => s.AllSlotInstance.Count(eq => eq != null && predicate(eq)));
                this.Assert(actualCount >= leastCount,
                    () => $"{whatis}:장비수{this.CurrentValue(actualCount)}{leastCount}");
                return this;
            }

            public MissionClearConditionResult CheckEquipmentCount(EquipmentTypes equipmentType, int leastCount) =>
                this.CheckEquipmentCount(eq => eq.MasterEquipment.CategoryType == equipmentType, leastCount, KCDatabase.Instance.EquipmentTypes[(int)equipmentType].Name);


            public MissionClearConditionResult CheckEquippedShipCount(Func<EquipmentData, bool> predicate, int leastCount, string whatis)
            {
                int actualCount = this.members.Count(s => s.AllSlotInstance.Any(eq => eq != null && predicate(eq)));
                this.Assert(actualCount >= leastCount,
                    () => $"{whatis}:장비함선수{this.CurrentValue(actualCount)}{leastCount}");
                return this;
            }

            public MissionClearConditionResult CheckEquippedShipCount(EquipmentTypes equipmentType, int leastCount) =>
                this.CheckEquippedShipCount(eq => eq.MasterEquipment.CategoryType == equipmentType, leastCount, KCDatabase.Instance.EquipmentTypes[(int)equipmentType].Name);



            public override string ToString()
            {
                return (this.IsSuceeded ? "성공" : "실패") + (this.FailureReason.Count == 0 ? "" : (" - " + string.Join(", ", this.FailureReason)));
            }
        }

    }
}