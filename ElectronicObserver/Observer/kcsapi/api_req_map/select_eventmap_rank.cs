﻿using ElectronicObserver.Data;
using System.Collections.Generic;

namespace ElectronicObserver.Observer.kcsapi.api_req_map
{

    public class select_eventmap_rank : APIBase
	{

		public override void OnRequestReceived(Dictionary<string, string> data)
		{

			var mapinfo = KCDatabase.Instance.MapInfo[int.Parse(data["api_maparea_id"]) * 10 + int.Parse(data["api_map_no"])];
			if (mapinfo != null)
				mapinfo.LoadFromRequest(this.APIName, data);

			base.OnRequestReceived(data);
		}


		public override bool IsRequestSupported => true;
		public override bool IsResponseSupported => false;


		public override string APIName => "api_req_map/select_eventmap_rank";
	}

}
