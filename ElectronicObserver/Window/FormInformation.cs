﻿using ElectronicObserver.Data;
using ElectronicObserver.Observer;
using ElectronicObserver.Resource;
using ElectronicObserver.Utility;
using ElectronicObserver.Utility.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace ElectronicObserver.Window
{

    public partial class FormInformation : DockContent
    {

        private int _ignorePort;
        private List<int> _inSortie;
        private int[] _prevResource;

        public FormInformation(FormMain parent)
        {
            this.InitializeComponent();

            this._ignorePort = 0;
            this._inSortie = null;
            this._prevResource = new int[4];

            this.ConfigurationChanged();

            this.Icon = ResourceManager.ImageToIcon(ResourceManager.Instance.Icons.Images[(int)ResourceManager.IconContent.FormInformation]);

            this.ApplyLockLayoutState();
        }


        private void FormInformation_Load(object sender, EventArgs e)
        {

            APIObserver o = APIObserver.Instance;

            o["api_port/port"].ResponseReceived += this.Updated;
            o["api_req_member/get_practice_enemyinfo"].ResponseReceived += this.Updated;
            o["api_get_member/picture_book"].ResponseReceived += this.Updated;
            o["api_get_member/mapinfo"].ResponseReceived += this.Updated;
            o["api_req_mission/result"].ResponseReceived += this.Updated;
            o["api_req_practice/battle_result"].ResponseReceived += this.Updated;
            o["api_req_sortie/battleresult"].ResponseReceived += this.Updated;
            o["api_req_combined_battle/battleresult"].ResponseReceived += this.Updated;
            o["api_req_hokyu/charge"].ResponseReceived += this.Updated;
            o["api_req_map/start"].ResponseReceived += this.Updated;
            o["api_req_map/next"].ResponseReceived += this.Updated;
            o["api_req_practice/battle"].ResponseReceived += this.Updated;
            o["api_get_member/sortie_conditions"].ResponseReceived += this.Updated;
            o["api_req_mission/start"].RequestReceived += this.Updated;

            Utility.Configuration.Instance.ConfigurationChanged += this.ConfigurationChanged;
            this.ApplyLockLayoutState();
        }

        void ConfigurationChanged()
        {

            this.Font = this.TextInformation.Font = Utility.Configuration.Config.UI.MainFont;
            this.TextInformation.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
            this.BackColor = Utility.ThemeManager.GetColor(Utility.ThemeColors.BackgroundColor);
            this.ForeColor = Utility.ThemeManager.GetColor(Utility.ThemeColors.MainFontColor);
            this.TextInformation.BackColor = Utility.ThemeManager.GetColor(Utility.ThemeColors.BackgroundColor);
            this.TextInformation.ForeColor = Utility.ThemeManager.GetColor(Utility.ThemeColors.MainFontColor);

            this.ApplyLockLayoutState();
        }

        void Updated(string apiname, dynamic data)
        {

            switch (apiname)
            {

                case "api_port/port":
                    if (this._ignorePort > 0)
                        this._ignorePort--;
                    else
                        this.TextInformation.Text = "";      //とりあえずクリア

                    if (this._inSortie != null)
                    {
                        this.TextInformation.Text = this.GetConsumptionResource(data);
                    }
                    this._inSortie = null;

                    this.RecordMaterials();

                    // '16 summer event
                    if (data.api_event_object() && data.api_event_object.api_m_flag2() && (int)data.api_event_object.api_m_flag2 > 0)
                    {
                        this.TextInformation.Text += "\r\n＊기믹해제＊\r\n";
                        Utility.Logger.Add(Utility.LogType.Gimick, "적세력의 약화를 확인했습니다!");
                    }
                    break;

                case "api_req_member/get_practice_enemyinfo":
                    this.TextInformation.Text = GetPracticeEnemyInfo(data);
                    this.RecordMaterials();
                    break;

                case "api_get_member/picture_book":
                    this.TextInformation.Text = GetAlbumInfo(data);
                    break;

                case "api_get_member/mapinfo":
                    this.TextInformation.Text = GetMapGauge(data);
                    break;

                case "api_req_mission/result":
                    this.TextInformation.Text = GetExpeditionResult(data);
                    this._ignorePort = 1;
                    break;

                case "api_req_practice/battle_result":
                case "api_req_sortie/battleresult":
                case "api_req_combined_battle/battleresult":
                    this.TextInformation.Text = GetBattleResult(data);
                    break;

                case "api_req_hokyu/charge":
                    this.TextInformation.Text = GetSupplyInformation(data);
                    break;

                case "api_req_mission/start":
                    if (Utility.Configuration.Config.Control.ShowExpeditionAlertDialog)
                        this.CheckExpeditionFleet(int.Parse(data["api_mission_id"]), int.Parse(data["api_deck_id"]));
                    break;

                case "api_get_member/sortie_conditions":
                    this.CheckSallyArea();
                    break;

                case "api_req_map/start":
                    this._inSortie = KCDatabase.Instance.Fleet.Fleets.Values.Where(f => f.IsInSortie || f.ExpeditionState == 1).Select(f => f.FleetID).ToList();

                    this.RecordMaterials();
                    break;

                case "api_req_map/next":
                    {
                        var str = CheckGimmickUpdated(data);
                        if (!string.IsNullOrWhiteSpace(str))
                            this.TextInformation.Text = str;

                        if (data.api_destruction_battle())
                        {
                            str = CheckGimmickUpdated(data.api_destruction_battle);
                            if (!string.IsNullOrWhiteSpace(str))
                                this.TextInformation.Text = str;
                        }
                    }
                    break;

                case "api_req_practice/battle":
                    this._inSortie = new List<int>() { KCDatabase.Instance.Battle.BattleDay.Initial.FriendFleetID };
                    break;

            }
        }

        private string GetPracticeEnemyInfo(dynamic data)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[연습정보]");
            sb.AppendLine("적제독명 : " + data.api_nickname);
            sb.AppendLine("적함대명 : " + data.api_deckname);

            {
                int ship1lv = (int)data.api_deck.api_ships[0].api_id != -1 ? (int)data.api_deck.api_ships[0].api_level : 1;
                int ship2lv = (int)data.api_deck.api_ships[1].api_id != -1 ? (int)data.api_deck.api_ships[1].api_level : 1;

                // 経験値テーブルが拡張されたとき用の対策
                ship1lv = Math.Min(ship1lv, ExpTable.ShipExp.Keys.Max());
                ship2lv = Math.Min(ship2lv, ExpTable.ShipExp.Keys.Max());

                double expbase = ExpTable.ShipExp[ship1lv].Total / 100.0 + ExpTable.ShipExp[ship2lv].Total / 300.0;
                if (expbase >= 500.0)
                    expbase = 500.0 + Math.Sqrt(expbase - 500.0);

                expbase = (int)expbase;

                sb.AppendFormat("획득경험치: {0} / S승리: {1}\r\n", expbase, (int)(expbase * 1.2));


                // 練巡ボーナス計算 - きたない
                var fleet = KCDatabase.Instance.Fleet[1];
                if (fleet.MembersInstance.Any(s => s != null && s.MasterShip.ShipType == ShipTypes.TrainingCruiser))
                {
                    var members = fleet.MembersInstance;
                    var subCT = members.Skip(1).Where(s => s != null && s.MasterShip.ShipType == ShipTypes.TrainingCruiser);

                    double bonus;

                    // 旗艦が練巡
                    if (members[0] != null && members[0].MasterShip.ShipType == ShipTypes.TrainingCruiser)
                    {

                        int level = members[0].Level;

                        if (subCT != null && subCT.Any())
                        {
                            // 旗艦+随伴
                            if (level < 10) bonus = 1.10;
                            else if (level < 30) bonus = 1.13;
                            else if (level < 60) bonus = 1.16;
                            else if (level < 100) bonus = 1.20;
                            else bonus = 1.25;

                        }
                        else
                        {
                            // 旗艦のみ
                            if (level < 10) bonus = 1.05;
                            else if (level < 30) bonus = 1.08;
                            else if (level < 60) bonus = 1.12;
                            else if (level < 100) bonus = 1.15;
                            else bonus = 1.20;
                        }

                    }
                    else
                    {

                        int level = subCT.Max(s => s.Level);

                        if (subCT.Count() > 1)
                        {
                            // 随伴複数	
                            if (level < 10) bonus = 1.04;
                            else if (level < 30) bonus = 1.06;
                            else if (level < 60) bonus = 1.08;
                            else if (level < 100) bonus = 1.12;
                            else bonus = 1.175;

                        }
                        else
                        {
                            // 随伴単艦
                            if (level < 10) bonus = 1.03;
                            else if (level < 30) bonus = 1.05;
                            else if (level < 60) bonus = 1.07;
                            else if (level < 100) bonus = 1.10;
                            else bonus = 1.15;
                        }
                    }

                    sb.AppendFormat("(연순보정: {0} / S승리: {1})\r\n", (int)(expbase * bonus), (int)((int)(expbase * 1.2) * bonus));


                }
            }

            return sb.ToString();
        }

        private string GetAlbumInfo(dynamic data)
        {

            StringBuilder sb = new StringBuilder();

            if (data != null && data.api_list() && data.api_list != null)
            {

                if (data.api_list[0].api_yomi())
                {
                    //艦娘図鑑
                    const int bound = 70;       // 図鑑1ページあたりの艦船数
                    int startIndex = (((int)data.api_list[0].api_index_no - 1) / bound) * bound + 1;
                    bool[] flags = Enumerable.Repeat<bool>(false, bound).ToArray();

                    sb.AppendLine("[중파이미지미회수]");

                    foreach (dynamic elem in data.api_list)
                    {

                        flags[(int)elem.api_index_no - startIndex] = true;

                        dynamic[] state = elem.api_state;
                        for (int i = 0; i < state.Length; i++)
                        {
                            if ((int)state[i][1] == 0)
                            {

                                var target = KCDatabase.Instance.MasterShips[(int)elem.api_table_id[i]];
                                if (target != null)     //季節の衣替え艦娘の場合存在しないことがある
                                    sb.AppendLine(target.Name);
                            }
                        }

                    }

                    sb.AppendLine("[미보유함]");
                    for (int i = 0; i < bound; i++)
                    {
                        if (!flags[i])
                        {
                            ShipDataMaster ship = KCDatabase.Instance.MasterShips.Values.FirstOrDefault(s => s.AlbumNo == startIndex + i);
                            if (ship != null)
                            {
                                sb.AppendLine(ship.Name);
                            }
                        }
                    }

                }
                else
                {
                    //装備図鑑
                    const int bound = 70;       // 図鑑1ページあたりの装備数
                    int startIndex = (((int)data.api_list[0].api_index_no - 1) / bound) * bound + 1;
                    bool[] flags = Enumerable.Repeat<bool>(false, bound).ToArray();

                    foreach (dynamic elem in data.api_list)
                    {

                        flags[(int)elem.api_index_no - startIndex] = true;
                    }

                    sb.AppendLine("[미보유장비]");
                    for (int i = 0; i < bound; i++)
                    {
                        if (flags[i] == false)
                        {
                            EquipmentDataMaster eq = KCDatabase.Instance.MasterEquipments.Values.FirstOrDefault(s => s.AlbumNo == startIndex + i);
                            if (eq != null)
                            {
                                sb.AppendLine(eq.Name);
                            }
                        }
                    }
                }
            }

            return sb.ToString();
        }

        private string GetMapGauge(dynamic data)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[해역게이지]");

            foreach (var map in KCDatabase.Instance.MapInfo.Values)
            {
                int gaugeType = -1;
                int current = 0;
                int max = 0;

                if (map.RequiredDefeatedCount != -1 && map.CurrentDefeatedCount < map.RequiredDefeatedCount)
                {
                    gaugeType = 1;
                    current = map.CurrentDefeatedCount;
                    max = map.RequiredDefeatedCount;
                }
                else if (map.MapHPMax > 0)
                {
                    gaugeType = map.GaugeType;
                    current = map.MapHPCurrent;
                    max = map.MapHPMax;
                }

                if (gaugeType > 0)
                {
                    sb.AppendLine(string.Format("{0}-{1}{2}: {3}{4} {5} / {6}{7}",
                        map.MapAreaID, map.MapInfoID,
                        map.EventDifficulty > 0 ? $" [{Constants.GetDifficulty(map.EventDifficulty)}]" : "",
                        map.CurrentGaugeIndex > 0 ? $"#{map.CurrentGaugeIndex} " : "",
                        gaugeType == 1 ? "격파" : gaugeType == 2 ? "HP" : "TP",
                        current, max,
                        gaugeType == 1 ? " 회" : ""));
                }
            }

            return sb.ToString();
        }

        private string GetExpeditionResult(dynamic data)
        {
            StringBuilder sb = new StringBuilder();

            int expeditionDestination = KCDatabase.Instance.Fleet[KCDatabase.Instance.Ships[(int)data.api_ship_id[1]].Fleet].ExpeditionDestination;

            sb.AppendLine("[원정 귀환]");
            sb.AppendLine(Utility.ExternalDataReader.Instance.GetTranslation
                (data.api_quest_name, Utility.TranslateType.ExpeditionTitle, expeditionDestination) + "\r\n");
            sb.AppendFormat("결과: {0}\r\n", Constants.GetExpeditionResult((int)data.api_clear_result));
            sb.AppendFormat("제독경험치: +{0}\r\n", (int)data.api_get_exp);
            sb.AppendFormat("함선경험치: +{0}\r\n", ((int[])data.api_get_ship_exp).Min());
            return sb.ToString();
        }

        private string GetBattleResult(dynamic data)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("[전투종료]");
            sb.AppendFormat("적함대명: {0}\r\n", Utility.ExternalDataReader.Instance.GetTranslation(data.api_enemy_info.api_deck_name, Utility.TranslateType.OperationSortie));
            sb.AppendFormat("승패판정: {0}\r\n", data.api_win_rank);
            sb.AppendFormat("제독경험치: +{0}\r\n", (int)data.api_get_exp);

            sb.Append(CheckGimmickUpdated(data));

            return sb.ToString();
        }

        private string CheckGimmickUpdated(dynamic data)
        {
            if ((data.api_m1() && data.api_m1 != 0) || (data.api_m2() && data.api_m2 != 0))
            {
                Utility.Logger.Add(Utility.LogType.Gimick, "해역의 변화를 확인하였습니다!");
                return "\r\n＊기믹 해제＊\r\n";
            }

            return "";
        }

        private string GetSupplyInformation(dynamic data)
        {

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("[보급완료]");
            sb.AppendFormat("보크사이트: {0} ( {1}기 )\r\n", (int)data.api_use_bou, (int)data.api_use_bou / 5);

            return sb.ToString();
        }

        private string GetConsumptionResource(dynamic data)
        {

            StringBuilder sb = new StringBuilder();
            var material = KCDatabase.Instance.Material;

            int fuel_diff = material.Fuel - this._prevResource[0],
                ammo_diff = material.Ammo - this._prevResource[1],
                steel_diff = material.Steel - this._prevResource[2],
                bauxite_diff = material.Bauxite - this._prevResource[3];

            var ships = KCDatabase.Instance.Fleet.Fleets.Values
                .Where(f => this._inSortie.Contains(f.FleetID))
                .SelectMany(f => f.MembersInstance)
                .Where(s => s != null);

            int fuel_supply = ships.Sum(s => s.SupplyFuel);
            int ammo = ships.Sum(s => s.SupplyAmmo);
            int bauxite = ships.Sum(s => s.Aircraft.Zip(s.MasterShip.Aircraft, (current, max) => new { Current = current, Max = max }).Sum(a => (a.Max - a.Current) * 5));

            int fuel_repair = ships.Sum(s => s.RepairFuel);
            int steel = ships.Sum(s => s.RepairSteel);

            sb.AppendLine("[함대귀환]");
            sb.AppendFormat("연료: {0:+0;-0} ( 자연회복 {1:+0;-0} - 보급 {2} - 입거 {3} )\r\n탄약: {4:+0;-0} ( 자연회복 {5:+0;-0} - 보급 {6} )\r\n강재: {7:+0;-0} ( 자연회복 {8:+0;-0} - 입거 {9} )\r\n보키: {10:+0;-0} ( 자연회복 {11:+0;-0} - 보급 {12} ( {13} 기 ) )",
                fuel_diff - fuel_supply - fuel_repair, fuel_diff, fuel_supply, fuel_repair,
                ammo_diff - ammo, ammo_diff, ammo,
                steel_diff - steel, steel_diff, steel,
                bauxite_diff - bauxite, bauxite_diff, bauxite, bauxite / 5);

            return sb.ToString();
        }


        private void CheckSallyArea()
        {
            if (KCDatabase.Instance.Ships.Values.First().SallyArea == -1)   // そもそも札情報がなければやる必要はない
                return;

            IEnumerable<IEnumerable<ShipData>> group;

            if (KCDatabase.Instance.Fleet.CombinedFlag != 0)
                group = new[] { KCDatabase.Instance.Fleet[1].MembersInstance.Concat(KCDatabase.Instance.Fleet[2].MembersInstance).Where(s => s != null) };
            else
                group = KCDatabase.Instance.Fleet.Fleets.Values
                    .Where(f => f?.ExpeditionState == 0)
                    .Select(f => f.MembersInstance.Where(s => s != null));


            group = group.Where(ss =>
                ss.All(s => s.RepairingDockID == -1) &&
                ss.Any(s => s.SallyArea == 0));

            if (group.Any())
            {
                var freeShips = group.SelectMany(f => f).Where(s => s.SallyArea == 0);

                this.TextInformation.Text = "[오출격경고]\r\n딱지없는 칸무스：\r\n" + string.Join("\r\n", freeShips.Select(s => s.NameWithLevel));

                if (Utility.Configuration.Config.Control.ShowSallyAreaAlertDialog)
                    MessageBox.Show("출격 딱지가 붙어있지않은 칸무스가 편성되어있습니다. \r\n주의해서 출격해주세요. \r\n\r\n（이 메시지는 설정→동작에서 비활성화할수있습니다.）", "오출격경고",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        public void ShowFitInfo(string ShipName, string EquipmentName, int[] data, float marry_modify, bool married)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[피트 정보]\r\n");
            sb.Append(string.Format("{0}\r\n", ShipName));
            sb.Append(string.Format("{0}\r\n\n", EquipmentName));

            if (data[0] == 0)
                sb.Append("주간 보정 : 무보정\r\n\n");
            else
            {
                if (married)
                {
                    if (marry_modify == 0)
                    {
                        sb.Append(string.Format("주간 보정 : {0}\r\n", (data[0])));
                        sb.Append("(결혼 보정 요검증)\r\n\n");
                    }
                    else
                    {
                        sb.Append(string.Format("주간 보정 : {0}\r\n", (data[0] * marry_modify)));
                        sb.Append(string.Format("(결혼 보정 {0} 배)\r\n\n", marry_modify));
                    }
                }
                else
                    sb.Append(string.Format("주간 보정 : {0}\r\n\n", data[0]));
            }

            if (data[1] == 0)
                sb.Append("야간 보정 : 무보정");
            else
            {
                if (married)
                {
                    if (marry_modify == 0)
                    {
                        sb.Append(string.Format("야간 보정 : {0}\r\n", (data[1])));
                        sb.Append("(결혼 보정 요검증)");
                    }
                    else
                    {
                        sb.Append(string.Format("야간 보정 : {0}\r\n", (data[1] * marry_modify)));
                        sb.Append(string.Format("(결혼 보정 {0} 배)", marry_modify));
                    }
                }
                else
                    sb.Append(string.Format("야간 보정 : {0}", data[1]));
            }

            this.TextInformation.Text = sb.ToString();
        }

        public void ShowFitInfo(int Shipid, int Equipmentid)
        {
            KCDatabase db = KCDatabase.Instance;

            StringBuilder sb = new StringBuilder();
            sb.Append("[피트정보]\r\n");
            sb.Append(string.Format("[{0}]\r\n", db.MasterShips[Shipid].Name));
            sb.Append(string.Format("[{0}]\r\n", db.MasterEquipments[Equipmentid].Name));
            sb.Append("정보 없음");

            this.TextInformation.Text = sb.ToString();
        }

        private void CheckExpeditionFleet(int missionID, int fleetID)
        {
            var fleet   = KCDatabase.Instance.Fleet[fleetID];
            var result  = MissionClearCondition.Check(missionID, fleet);
            var mission = KCDatabase.Instance.Mission[missionID];

            if (result.IsSuceeded == false)
            {
                MessageBox.Show(
                    $"#{fleet.FleetID} {fleet.Name} 함대는 {mission.DisplayID}:{mission.Name} 의 조건에 미달되는것같습니다. \r\n실패할 수 있습니다. .\r\n\r\n{string.Join("\r\n", result.FailureReason)}\r\n\r\n（이 경고는 설정->실행에서 비활성화 할 수 있습니다.)",
                    "원정 실패 경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (fleet.MembersInstance.Any(s => s?.FuelRate < 1 || s?.AmmoRate < 1))
            {
                MessageBox.Show(
                    $"#{fleet.FleetID} {fleet.Name} 함대는 {mission.DisplayID}:{mission.Name} 에 실패할 가능성이 있습니다.\r\n\r\n미보급\r\n\r\n (이 경고는 설정->실행에서 비활성화 할 수 있습니다.)",
                    "원정 실패 경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // 작업 체크포인트 - 원정체크
        public void CheckExpeditionCondition(int fleetID, int MissionId)
        {
            this.TextInformation.Text = 
                Utility.ExternalDataReader.Instance.CheckExpeditionCondition(fleetID, MissionId);
        }

        private void RecordMaterials()
        {
            var material = KCDatabase.Instance.Material;
            this._prevResource[0] = material.Fuel;
            this._prevResource[1] = material.Ammo;
            this._prevResource[2] = material.Steel;
            this._prevResource[3] = material.Bauxite;
        }

        protected override string GetPersistString()
        {
            return "Information";
        }
    }
}
