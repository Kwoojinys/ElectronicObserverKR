﻿using ElectronicObserver.Data;
using System.Collections.Generic;
using System.Linq;

namespace ElectronicObserver.Observer.kcsapi.api_req_kousyou
{

    public class destroyship : APIBase
	{


		public override void OnRequestReceived(Dictionary<string, string> data)
		{

			KCDatabase db = KCDatabase.Instance;


			//todo: ここに処理を書くのはみょんな感があるので、可能なら移動する

			var shipIDs = data["api_ship_id"].Split(",".ToCharArray()).Select(s => int.Parse(s));
			bool discardEquipment = int.Parse(data["api_slot_dest_flag"]) != 0;

			var ships = shipIDs.Select(id => db.Ships[id]);


			db.Fleet.LoadFromRequest(this.APIName, data);


			foreach (var ship in ships)
				Utility.Logger.Add(Utility.LogType.Arsenal, ship.NameWithLevel + " 를 해체했습니다.");


			if (discardEquipment)
			{
				foreach (var ship in ships)
				{
					foreach (var eqid in ship.AllSlot.Where(id => id != -1))
					{
						db.Equipments.Remove(eqid);
					}
				}
			}

			foreach (int id in shipIDs)
				db.Ships.Remove(id);


			base.OnRequestReceived(data);
		}

		public override void OnResponseReceived(dynamic data)
		{
			KCDatabase.Instance.Material.LoadFromResponse(this.APIName, data.api_material);

			base.OnResponseReceived((object)data);
		}


		public override bool IsRequestSupported => true;
		public override bool IsResponseSupported => true;

		public override string APIName => "api_req_kousyou/destroyship";
	}


}
