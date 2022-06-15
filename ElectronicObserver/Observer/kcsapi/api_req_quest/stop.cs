﻿using ElectronicObserver.Data;
using System.Collections.Generic;

namespace ElectronicObserver.Observer.kcsapi.api_req_quest
{
    public class stop : APIBase
	{

		public override bool IsRequestSupported => true;

		public override void OnRequestReceived(Dictionary<string, string> data)
		{

			KCDatabase.Instance.Quest.LoadFromRequest(this.APIName, data);

			base.OnRequestReceived(data);
		}


		public override string APIName => "api_req_quest/stop";
	}

}
