﻿using ElectronicObserver.Data;
using System;
using System.Collections.Generic;

namespace ElectronicObserver.Observer.kcsapi.api_req_kaisou
{

    public class open_exslot : APIBase
	{

		public override void OnRequestReceived(Dictionary<string, string> data)
		{

			var ship = KCDatabase.Instance.Ships[Convert.ToInt32(data["api_id"])];
			if (ship != null)
			{
				ship.LoadFromRequest(this.APIName, data);

				Utility.Logger.Add(Utility.LogType.PowerUp, $"{ship.NameWithLevel} 의 보강 장비 증설 개수가 완료되었습니다.");
			}

			base.OnRequestReceived(data);
		}


		public override bool IsRequestSupported => true;
		public override bool IsResponseSupported => false;

		public override string APIName => "api_req_kaisou/open_exslot";
	}

}
