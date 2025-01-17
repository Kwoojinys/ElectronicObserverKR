﻿using System.Collections.Generic;

namespace ElectronicObserver.Observer.kcsapi.api_req_kaisou
{

    public class slotset : APIBase
    {
        public override bool IsRequestSupported => true;
        public override bool IsResponseSupported => false;

        public override void OnRequestReceived(Dictionary<string, string> data)
        {
            Utility.ExternalDataReader.Instance.GetFit(data);

            base.OnRequestReceived(data);
        }

        public override string APIName => "api_req_kaisou/slotset";
    }

}