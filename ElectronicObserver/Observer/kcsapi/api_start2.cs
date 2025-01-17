﻿using ElectronicObserver.Data;
using System;
using System.Collections.Generic;

namespace ElectronicObserver.Observer.kcsapi
{
    [Obsolete]
	public class api_start2_old : APIBase
	{


		public override void OnResponseReceived(dynamic data)
		{

			KCDatabase db = KCDatabase.Instance;


			//特別置換処理
			data.api_mst_stype[7].api_name = "巡洋戦艦";


			//api_mst_ship
			foreach (var elem in data.api_mst_ship)
			{

				int id = (int)elem.api_id;
				if (db.MasterShips[id] == null)
				{
					var ship = new ShipDataMaster();
					ship.LoadFromResponse(this.APIName, elem);
					db.MasterShips.Add(ship);
				}
				else
				{
					db.MasterShips[id].LoadFromResponse(this.APIName, elem);
				}
			}

			//改装関連のデータ設定
			foreach (var ship in db.MasterShips)
			{
				int remodelID = ship.Value.RemodelAfterShipID;
				if (remodelID != 0)
				{
					db.MasterShips[remodelID].RemodelBeforeShipID = ship.Key;
				}
			}


			//api_mst_slotitem_equiptype
			foreach (var elem in data.api_mst_slotitem_equiptype)
			{

				int id = (int)elem.api_id;
				if (db.EquipmentTypes[id] == null)
				{
					var eqt = new EquipmentType();
					eqt.LoadFromResponse(this.APIName, elem);
					db.EquipmentTypes.Add(eqt);
				}
				else
				{
					db.EquipmentTypes[id].LoadFromResponse(this.APIName, elem);
				}
			}


			//api_mst_stype
			foreach (var elem in data.api_mst_stype)
			{

				int id = (int)elem.api_id;
				if (db.ShipTypes[id] == null)
				{
					var spt = new ShipType();
					spt.LoadFromResponse(this.APIName, elem);
					db.ShipTypes.Add(spt);
				}
				else
				{
					db.ShipTypes[id].LoadFromResponse(this.APIName, elem);
				}
			}


			//api_mst_slotitem
			foreach (var elem in data.api_mst_slotitem)
			{

				int id = (int)elem.api_id;
				if (db.MasterEquipments[id] == null)
				{
					var eq = new EquipmentDataMaster();
					eq.LoadFromResponse(this.APIName, elem);
					db.MasterEquipments.Add(eq);
				}
				else
				{
					db.MasterEquipments[id].LoadFromResponse(this.APIName, elem);
				}
			}

			//api_mst_slotitemgraph
			/*
			foreach ( var elem in data.api_mst_slotitemgraph ) {

				int id = (int)elem.api_id;
				EquipmentDataMaster eq = db.MasterEquipments[id];
				if ( eq != null ) {
					eq.ResourceVersion = elem.api_version;
				}
			}
			*/

			//api_mst_useitem
			foreach (var elem in data.api_mst_useitem)
			{

				int id = (int)elem.api_id;
				if (db.MasterUseItems[id] == null)
				{
					var item = new UseItemMaster();
					item.LoadFromResponse(this.APIName, elem);
					db.MasterUseItems.Add(item);
				}
				else
				{
					db.MasterUseItems[id].LoadFromResponse(this.APIName, elem);
				}
			}

			//api_mst_maparea
			foreach (var elem in data.api_mst_maparea)
			{
				int id = (int)elem.api_id;
				if (db.MapArea[id] == null)
				{
					var item = new MapAreaData();
					item.LoadFromResponse(this.APIName, elem);
					db.MapArea.Add(item);
				}
				else
				{
					db.MapArea[id].LoadFromResponse(this.APIName, elem);
				}
			}

			//api_mst_mapinfo
			foreach (var elem in data.api_mst_mapinfo)
			{

				int id = (int)elem.api_id;
				if (db.MapInfo[id] == null)
				{
					var item = new MapInfoData();
					item.LoadFromResponse(this.APIName, elem);
					db.MapInfo.Add(item);
				}
				else
				{
					db.MapInfo[id].LoadFromResponse(this.APIName, elem);
				}
			}

			//api_mst_mission
			foreach (var elem in data.api_mst_mission)
			{
				int id = (int)elem.api_id;
				if (db.Mission[id] == null)
				{
					var item = new MissionData();
					item.LoadFromResponse(this.APIName, elem);
					db.Mission.Add(item);
				}
				else
				{
					db.Mission[id].LoadFromResponse(this.APIName, elem);
				}
			}


			//api_mst_shipupgrade
			Dictionary<int, int> upgradeLevels = new Dictionary<int, int>();
			foreach (var elem in data.api_mst_shipupgrade)
			{

				int idbefore = (int)elem.api_current_ship_id;
				int idafter = (int)elem.api_id;
				var shipbefore = db.MasterShips[idbefore];
				var shipafter = db.MasterShips[idafter];
				int level = (int)elem.api_upgrade_level;

				if (upgradeLevels.ContainsKey(idafter))
				{
					if (level < upgradeLevels[idafter])
					{
						shipafter.RemodelBeforeShipID = idbefore;
						upgradeLevels[idafter] = level;
					}
				}
				else
				{
					shipafter.RemodelBeforeShipID = idbefore;
					upgradeLevels.Add(idafter, level);
				}

				if (shipbefore != null)
				{
					shipbefore.NeedBlueprint = (int)elem.api_drawing_count;
					shipbefore.NeedCatapult = (int)elem.api_catapult_count;
                    shipbefore.NeedActionReport = (int)elem.api_report_count;
				}
			}


			Utility.Logger.Add(Utility.LogType.System, "제독이 진수부에 착임했습니다. 지금 부터 함대의 지휘를 맡습니다.");

			base.OnResponseReceived((object)data);
		}

		public override string APIName => "api_start2";
	}


}
