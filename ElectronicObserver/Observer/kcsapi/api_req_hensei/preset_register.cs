﻿using ElectronicObserver.Data;

namespace ElectronicObserver.Observer.kcsapi.api_req_hensei
{
    public class preset_register : APIBase
	{
		public override void OnResponseReceived(dynamic data)
		{
			KCDatabase.Instance.FleetPreset.LoadFromResponse(this.APIName, data);

			base.OnResponseReceived((object)data);
		}

		public override string APIName => "api_req_hensei/preset_register";
	}
}
