﻿using ElectronicObserver.Data;

namespace ElectronicObserver.Observer.kcsapi.api_req_sortie
{

    public class battleresult : APIBase
	{

		public override void OnResponseReceived(dynamic data)
		{

			KCDatabase.Instance.Battle.LoadFromResponse(this.APIName, data);


			base.OnResponseReceived((object)data);
		}


		public override string APIName => "api_req_sortie/battleresult";
	}

}
