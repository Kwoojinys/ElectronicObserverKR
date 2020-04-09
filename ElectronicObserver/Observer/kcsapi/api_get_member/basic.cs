﻿using ElectronicObserver.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicObserver.Observer.kcsapi.api_get_member
{

	public class basic : APIBase
	{


		public override void OnResponseReceived(dynamic data)
		{

			KCDatabase.Instance.Admiral.LoadFromResponse(this.APIName, data);

			base.OnResponseReceived((object)data);
		}

		public override string APIName => "api_get_member/basic";
	}


}
