﻿using ElectronicObserver.Data;

namespace ElectronicObserver.Observer.kcsapi.api_req_combined_battle
{

    public class sp_midnight : APIBase
	{

		public override void OnResponseReceived(dynamic data)
		{

			KCDatabase.Instance.Battle.LoadFromResponse(this.APIName, data);


			base.OnResponseReceived((object)data);
		}


		public override string APIName => "api_req_combined_battle/sp_midnight";
	}

}
