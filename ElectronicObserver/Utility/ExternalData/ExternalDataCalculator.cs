using ElectronicObserver.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElectronicObserver.Utility.Data;

namespace ElectronicObserver.Utility
{
    public partial class ExternalDataReader
    {
        public enum ExpeditionCheckParms
        {
            Fuel = 1,
            Ammo = 2,
            Steel = 3,
            Baux = 4,
            FleetMembers = 5,
            FlagShipType = 6,
            DrunNeed = 7,
            DrumBonus = 8,
            DrunKanmusu = 9,
            FleetPower = 10,
            FleetAA = 11,
            FleetASW = 12,
            FleetLOS = 13,
            FleetLevel = 14,
            FlagShipLevel = 15,
            OutValues,
        }

        class ConditionState
        {
            public int[]    Types;
            public int      ReqCount;
            public int      FleetCount;
        }

        public string CheckExpeditionCondition(int fleetID, int MissionId)
        {
            KCDatabase db = KCDatabase.Instance;
            MissionData mis = db.Mission[MissionId];
            var data = Instance.GetExpeditionData(MissionId.ToString());
            if (data == null)
            {
                return "[원정 정보 없음]";
            }

            FleetData fleet = db.Fleet[fleetID];
            var members     = db.Fleet[fleetID].MembersInstance.Where(s => s != null);
            if (fleet == null || fleet.Members.Count == 0)
            {
                return "[함대 정보 없음]";
            }

            List<ConditionState> shipConds          = new List<ConditionState>();
            List<ConditionState> expeditionConds    = new List<ConditionState>();

            Dictionary<ExpeditionCheckParms, int> expCheckParms = new Dictionary<ExpeditionCheckParms, int>();
            double resourceBonus = 1 + Calculator.GetExpeditionBonus(fleet);
            double fuelUnit = members.Sum(s => Math.Truncate(s.FuelMax * mis.Fuel * (s.IsMarried ? 0.85 : 1.00)));
            double ammoUnit = members.Sum(s => Math.Truncate(s.AmmoMax * mis.Ammo * (s.IsMarried ? 0.85 : 1.00)));

            int[] drumInfo = new int[] { 0, 0 }; // 0 = 인원 / 1 = 수량

            for (int parmIndex = 0; parmIndex < (int)ExpeditionCheckParms.OutValues; parmIndex++)
            {
                var checkType = (ExpeditionCheckParms)parmIndex;
                if (data[checkType.ToString()] != null)
                {
                    expCheckParms.Add(checkType, int.Parse(data[checkType.ToString()].ToString()));
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("[원정정보]\r\n");
            sb.Append("[" + mis.Name + "]\r\n");
            sb.Append("예상획득자원\r\n" + (int)(expCheckParms[ExpeditionCheckParms.Fuel] * resourceBonus - fuelUnit) + "/" +
                                           (int)(expCheckParms[ExpeditionCheckParms.Ammo] * resourceBonus - ammoUnit) + "/" +
                                           (int)(expCheckParms[ExpeditionCheckParms.Steel] * resourceBonus) + "/" +
                                           (int)(expCheckParms[ExpeditionCheckParms.Baux] * resourceBonus));
            sb.Append("\r\n보급소모 - 연료 : " + fuelUnit + " 탄약 : " + ammoUnit);
            List<int> shipConditions = new List<int>();

            if (data["FleetTypes"] != null)
            {
                string[] conds_types = data["FleetTypes"].ToString().Split('|');

                for (int i = 0; i < conds_types.Length; i++)
                {
                    ConditionState expeditionCond = new ConditionState();

                    string[] conditionData = conds_types[i].Split(',');
                    int requireCount = int.Parse(conditionData[1]);
                    int[] canReplaceType = conditionData[0].Split(':').Select(Int32.Parse).ToArray();

                    expeditionCond.ReqCount = requireCount;
                    expeditionCond.Types = canReplaceType;
                    shipConds.Add(expeditionCond);
                }
            }

            foreach (ShipData ship in members.Where(s => s != null)) // 각함선체크
            {
                bool isDrumEquipped = false;
                shipConditions.Add(ship.Condition);
                foreach (var eq in ship.AllSlotInstance.Where(eq => eq != null))
                {
                    if (eq.MasterEquipment.EquipmentID == 75)
                    {
                        isDrumEquipped = true;
                        drumInfo[1]++;
                    }
                }

                if (isDrumEquipped) drumInfo[0]++;

                foreach (ConditionState exCond in shipConds)
                {
                    if (exCond.Types.Contains((int)ship.MasterShip.ShipType)) // 함종에따른구별
                        exCond.FleetCount++;
                }
            }


            foreach (ConditionState ExCond in shipConds)
            {
                if (ExCond.FleetCount < ExCond.ReqCount)
                {
                    sb.Append("\r\n");
                    foreach (int type in ExCond.Types)
                    {
                        var ShipTypeName = KCDatabase.Instance.ShipTypes[type].Name;
                        sb.Append(ShipTypeName + "/");
                    }

                    sb.Remove((sb.Length - 1), 1);
                    sb.Append(" " + (ExCond.ReqCount - ExCond.FleetCount) + " 필요");
                }
            }

            
            ShipData flagship = db.Ships[fleet.Members[0]];
            if (expCheckParms.TryGetValue(ExpeditionCheckParms.FlagShipType, out int fType) == true
                && fType >= 1)
            {
                if (fType != (int) flagship.MasterShip.ShipType)
                    sb.Append("\r\n기함 : " + db.ShipTypes[fType]?.Name + " 지정 필요");
            }

            if (expCheckParms.TryGetValue(ExpeditionCheckParms.FleetLevel, out int fLevel) == true &&
                fLevel > members.Sum(s => s.Level))
            {
                sb.Append("\r\n함대레벨부족 : 추가 " + (fLevel - members.Sum(s => s.Level)) + " 필요");
            }

            if (expCheckParms.TryGetValue(ExpeditionCheckParms.FlagShipLevel, out int fShipLevel) == true &&
                fShipLevel > flagship.Level)
            {
                sb.Append("\r\n기함레벨부족 : 추가 " + (fShipLevel - flagship.Level) + " 필요");
            }

            if (expCheckParms.TryGetValue(ExpeditionCheckParms.FleetMembers, out int fCount) == true &&
                fCount > members.Count())
            {
                sb.Append("\r\n함대인원부족 : 추가 " + (fCount - members.Count()) + " 명 필요");
            }

            if (expCheckParms.TryGetValue(ExpeditionCheckParms.DrunKanmusu, out int dKanmusuCount) == true &&
                dKanmusuCount > drumInfo[0])
            {
                sb.Append("\r\n드럼통장착함선 " + (dKanmusuCount - drumInfo[0]) + "명 필요");
            }

            if (expCheckParms.TryGetValue(ExpeditionCheckParms.DrunNeed, out int dNeed) == true &&
                dNeed > drumInfo[1])
            {
                sb.Append("\r\n드럼통 " + (dNeed.ToInt() - drumInfo[1]) + "개 필요");
            }

            if (expCheckParms.TryGetValue(ExpeditionCheckParms.DrumBonus, out int dBonus) == true)
            {
                if (dBonus > drumInfo[1])
                {
                    sb.Append("(" + (dBonus - drumInfo[1]) + "개 추가시 대성공보너스)");
                }
            }

            if (expCheckParms.TryGetValue(ExpeditionCheckParms.FleetAA, out int fAA) == true &&
                fAA > members.Sum(s => s.AATotal))
            {
                sb.Append("\r\n함대대공부족 : 추가 " + (expCheckParms[ExpeditionCheckParms.FleetAA].ToInt() - members.Sum(s => s.AATotal)) + " 필요");
            }

            if (expCheckParms.TryGetValue(ExpeditionCheckParms.FleetASW, out int fASW) == true &&
                fASW > members.Sum(s => s.ASWTotal))
            {
                sb.Append("\r\n함대대잠부족 : 추가 " + (expCheckParms[ExpeditionCheckParms.FleetASW].ToInt() - members.Sum(s => s.ASWTotal)) + " 필요");
            }

            if (expCheckParms.TryGetValue(ExpeditionCheckParms.FleetLOS, out int fLOS) == true &&
                fLOS > members.Sum(s => s.LOSTotal))
            {
                sb.Append("\r\n함대색적부족 : 추가 " + (expCheckParms[ExpeditionCheckParms.FleetLOS].ToInt() - members.Sum(s => s.LOSTotal)) + " 필요");
            }

            if (expCheckParms.TryGetValue(ExpeditionCheckParms.FleetPower, out int fPower) == true &&
                fPower > members.Sum(s => s.FirepowerTotal))
            {
                sb.Append("\r\n함대화력부족 : 추가 " + (expCheckParms[ExpeditionCheckParms.FleetPower].ToInt() - members.Sum(s => s.FirepowerTotal)) + " 필요");
            }

            var greatChancePercent = 0f;
            int greatChanceCount = 12;
            int kiraCount = 0;

            for (int i = 0; i < shipConditions.Count; i++)
            {
                int tempCount = (int)Math.Ceiling((shipConditions[i] - 49) / 3.0);
                if (shipConditions[i] > 49)
                    kiraCount++;

                if (tempCount < greatChanceCount)
                    greatChanceCount = tempCount;
            }

            if (dBonus == 0)
            {
                if (greatChanceCount >= 1)
                {
                    greatChancePercent = (members.Count() / 6) * 100;
                    sb.Append("\r\n" + greatChanceCount + "회 대성공 가능. 확률 : " + greatChancePercent + "%");
                }
            }
            else
            {
                if (dBonus <= drumInfo[1] && greatChanceCount == 0)
                {
                    greatChancePercent = (40 + (kiraCount * 15)).Clamp(0, 100);
                    sb.Append("\r\n대성공 확률 : " + greatChancePercent + "%");
                }
                else if (greatChanceCount > 0)
                {
                    greatChancePercent = (members.Count() / 6) * 100;
                    sb.Append("\r\n" + greatChanceCount + "회 대성공 가능. 확률 : " + greatChancePercent + "%");
                }
            }
            
            return sb.ToString();
        }

        public string GenerateDeckBuilderCode()
        {
            StringBuilder sb = new StringBuilder();
            KCDatabase db = KCDatabase.Instance;

            // 手書き json の悲しみ

            sb.Append(@"{""version"":4,");

            foreach (var fleet in db.Fleet.Fleets.Values)
            {
                if (fleet == null || fleet.MembersInstance.All(m => m == null)) continue;

                sb.AppendFormat(@"""f{0}"":{{", fleet.FleetID);

                int shipcount = 1;
                foreach (var ship in fleet.MembersInstance)
                {
                    if (ship == null) break;

                    sb.AppendFormat(@"""s{0}"":{{""id"":{1},""lv"":{2},""luck"":{3},""items"":{{",
                        shipcount,
                        ship.ShipID,
                        ship.Level,
                        ship.LuckBase);

                    int eqcount = 1;
                    foreach (var eq in ship.AllSlotInstance.Where(eq => eq != null))
                    {
                        if (eq == null) break;

                        sb.AppendFormat(@"""i{0}"":{{""id"":{1},""rf"":{2},""mas"":{3}}},", eqcount >= 6 ? "x" : eqcount.ToString(), eq.EquipmentID, eq.Level, eq.AircraftLevel);

                        eqcount++;
                    }

                    if (eqcount > 1)
                        sb.Remove(sb.Length - 1, 1);        // remove ","
                    sb.Append(@"}},");

                    shipcount++;
                }

                if (shipcount > 0)
                    sb.Remove(sb.Length - 1, 1);        // remove ","
                sb.Append(@"},");

            }

            sb.Remove(sb.Length - 1, 1);        // remove ","
            sb.Append(@"}");

            return sb.ToString();
        }
    }
}
