﻿using ElectronicObserver.Data;
using System.Collections.Generic;

namespace ElectronicObserver.Observer.kcsapi.api_req_kaisou
{

    public class remodeling : APIBase
	{

		public override bool IsRequestSupported => true;
		public override bool IsResponseSupported => false;

		public override void OnRequestReceived(Dictionary<string, string> data)
		{

			int id = int.Parse(data["api_id"]);
			var ship = KCDatabase.Instance.Ships[id];
			Utility.Logger.Add(Utility.LogType.PowerUp, string.Format("{0} Lv. {1} 로 개장이 완료되었습니다.", ship.MasterShip.RemodelAfterShip.NameWithClass, ship.Level));

			KCDatabase.Instance.Fleet.LoadFromRequest(this.APIName, data);

			base.OnRequestReceived(data);
		}

		public override string APIName => "api_req_kaisou/remodeling";
	}

}
