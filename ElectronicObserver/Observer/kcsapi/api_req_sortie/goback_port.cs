﻿using ElectronicObserver.Data;

namespace ElectronicObserver.Observer.kcsapi.api_req_sortie
{
    public class goback_port : APIBase
	{
		public override void OnResponseReceived(dynamic data)
		{
			KCDatabase.Instance.Fleet.LoadFromResponse(this.APIName, data);

			base.OnResponseReceived((object)data);
		}

		public override string APIName => "api_req_sortie/goback_port";
	}
}
