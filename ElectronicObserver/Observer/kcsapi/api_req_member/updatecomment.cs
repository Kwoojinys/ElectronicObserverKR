using ElectronicObserver.Data;
using System.Collections.Generic;

namespace ElectronicObserver.Observer.kcsapi.api_req_member
{

    public class updatecomment : APIBase
	{

		public override bool IsRequestSupported => true;
		public override bool IsResponseSupported => false;

		public override void OnRequestReceived(Dictionary<string, string> data)
		{

			// 🎃
			if (data["api_cmt"].ToLower() == "jackolantern")
			{
				new Window.Dialog.DialogHalloween().Show();
			}

			KCDatabase.Instance.Admiral.LoadFromRequest(this.APIName, data);

			base.OnRequestReceived(data);
		}

		public override string APIName => "api_req_member/updatecomment";
	}

}
