﻿using ElectronicObserver.Data;

namespace ElectronicObserver.Observer.kcsapi.api_get_member
{

    public class require_info : APIBase
	{

		public override void OnResponseReceived(dynamic data)
		{

			KCDatabase db = KCDatabase.Instance;

			// Admiral - 各所でバグるので封印
			//db.Admiral.LoadFromResponse( APIName, data.api_basic );


			// Equipments
			db.Equipments.Clear();
			foreach (var elem in data.api_slot_item)
			{

				var eq = new EquipmentData();
				eq.LoadFromResponse(this.APIName, elem);
				db.Equipments.Add(eq);

			}


			// Arsenal
			foreach (var ars in data.api_kdock)
			{

				int id = (int)ars.api_id;

				if (!db.Arsenals.ContainsKey(id))
				{
					var a = new ArsenalData();
					a.LoadFromResponse(this.APIName, ars);
					db.Arsenals.Add(a);

				}
				else
				{
					db.Arsenals[id].LoadFromResponse(this.APIName, ars);
				}
			}


			// UseItem
			db.UseItems.Clear();
			if (data.api_useitem())
			{
				foreach (var elem in data.api_useitem)
				{

					var item = new UseItem();
					item.LoadFromResponse(this.APIName, elem);
					db.UseItems.Add(item);

				}
			}

			base.OnResponseReceived((object)data);
		}

		public override string APIName => "api_get_member/require_info";
	}

}
