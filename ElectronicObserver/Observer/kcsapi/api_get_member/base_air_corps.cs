﻿using ElectronicObserver.Data;

namespace ElectronicObserver.Observer.kcsapi.api_get_member
{
    public class base_air_corps : APIBase
	{

		public override void OnResponseReceived(dynamic data)
		{

			var db = KCDatabase.Instance;

			db.BaseAirCorps.Clear();
			foreach (var elem in data)
			{

				int id = BaseAirCorpsData.GetID(elem);

				if (!db.BaseAirCorps.ContainsKey(id))
				{
					var a = new BaseAirCorpsData();
					a.LoadFromResponse(this.APIName, elem);
					db.BaseAirCorps.Add(a);

				}
				else
				{
					db.BaseAirCorps[id].LoadFromResponse(this.APIName, elem);
				}
			}

			base.OnResponseReceived((object)data);
		}

		public override string APIName => "api_get_member/base_air_corps";
	}

}
