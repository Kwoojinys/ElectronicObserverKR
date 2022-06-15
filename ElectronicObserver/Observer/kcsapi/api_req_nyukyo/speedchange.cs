using ElectronicObserver.Data;
using System.Collections.Generic;

namespace ElectronicObserver.Observer.kcsapi.api_req_nyukyo
{

    public class speedchange : APIBase
	{


		public override void OnRequestReceived(Dictionary<string, string> data)
		{

			KCDatabase db = KCDatabase.Instance;

			db.Docks[int.Parse(data["api_ndock_id"])].LoadFromResponse(this.APIName, data);
			db.Material.InstantRepair--;


			db.Fleet.LoadFromRequest(this.APIName, data);

			base.OnRequestReceived(data);
		}


		public override bool IsRequestSupported => true;
		public override bool IsResponseSupported => false;

		public override string APIName => "api_req_nyukyo/speedchange";
	}


}
