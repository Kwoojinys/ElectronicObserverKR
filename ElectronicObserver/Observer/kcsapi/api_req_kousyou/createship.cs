﻿using ElectronicObserver.Data;
using System.Collections.Generic;

namespace ElectronicObserver.Observer.kcsapi.api_req_kousyou
{

    public class createship : APIBase
	{


		public override void OnRequestReceived(Dictionary<string, string> data)
		{

			//undone: このAPIが呼ばれた後 api_get_member/kdock が呼ばれ情報自体は更新されるので、建造ログのために使用？

			KCDatabase.Instance.Material.LoadFromRequest(this.APIName, data);

			base.OnRequestReceived(data);
		}

		public override bool IsRequestSupported => true;
		public override bool IsResponseSupported => false;


		public override string APIName => "api_req_kousyou/createship";
	}



}
