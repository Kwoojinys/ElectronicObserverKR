﻿using ElectronicObserver.Data;
using System.Collections.Generic;

namespace ElectronicObserver.Observer.kcsapi.api_req_practice
{
    public class battle : APIBase
	{

		public override bool IsRequestSupported => true;

		public override void OnRequestReceived(Dictionary<string, string> data)
		{

			KCDatabase.Instance.Fleet.LoadFromRequest(this.APIName, data);

			base.OnRequestReceived(data);
		}


		public override void OnResponseReceived(dynamic data)
		{

			KCDatabase.Instance.Battle.LoadFromResponse(this.APIName, data);


			base.OnResponseReceived((object)data);
		}

		public override string APIName => "api_req_practice/battle";
	}

}
