﻿using ElectronicObserver.Data;

namespace ElectronicObserver.Observer.kcsapi.api_req_kaisou
{

    public class marriage : APIBase
	{

		public override void OnResponseReceived(dynamic data)
		{

			Utility.Logger.Add(Utility.LogType.PowerUp, string.Format("{0} 와 결혼 했습니다. 축하드립니다!", KCDatabase.Instance.Ships[(int)data.api_id].Name));

            var db = KCDatabase.Instance;
            int id = (int)data.api_id;
            var ship = db.Ships[id];

            if (ship != null)
                ship.LoadFromResponse(this.APIName, data);
            else
            {
                var a = new ShipData();
                a.LoadFromResponse(this.APIName, data);
                db.Ships.Add(a);
            }

            base.OnResponseReceived((object)data);
        }

		public override string APIName => "api_req_kaisou/marriage";
	}

}
