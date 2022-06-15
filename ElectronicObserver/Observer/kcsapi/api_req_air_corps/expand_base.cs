﻿using ElectronicObserver.Data;

namespace ElectronicObserver.Observer.kcsapi.api_req_air_corps
{

    public class expand_base : APIBase
	{

		public override void OnResponseReceived(dynamic data)
		{

			KCDatabase db = KCDatabase.Instance;

			foreach (var elem in data)
			{
				int id = BaseAirCorpsData.GetID(elem);

				if (db.BaseAirCorps[id] == null)
				{
					var inst = new BaseAirCorpsData();
					inst.LoadFromResponse(this.APIName, elem);
					db.BaseAirCorps.Add(inst);

				}
				else
				{
					db.BaseAirCorps[id].LoadFromResponse(this.APIName, elem);
				}
			}

			base.OnResponseReceived((object)data);
		}

		public override string APIName => "api_req_air_corps/expand_base";
	}

}
