﻿using ElectronicObserver.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicObserver.Observer.kcsapi.api_req_kousyou
{
    public class createitem : APIBase
    {
        public override void OnRequestReceived(Dictionary<string, string> data)
        {

            KCDatabase.Instance.Development.LoadFromRequest(this.APIName, data);

            base.OnRequestReceived(data);
        }

        public override void OnResponseReceived(dynamic data)
        {
            var db = KCDatabase.Instance;
            var dev = db.Development;
            //装備の追加　データが不十分のため、自力で構築しなければならない

            dev.LoadFromResponse(this.APIName, data);

            if (Utility.Configuration.Config.Log.ShowSpoiler)
            {

                foreach (var result in dev.Results)
                {
                    if (result.IsSucceeded)
                    {
                        Utility.Logger.Add(Utility.LogType.Arsenal, string.Format("{0}「{1}」의 개발에 성공했습니다. ({2}/{3}/{4}/{5} 비서함: {6})",
                            result.MasterEquipment.CategoryTypeInstance.Name,
                            result.MasterEquipment.Name,
                            dev.Fuel, dev.Ammo, dev.Steel, dev.Bauxite,
                            db.Fleet[1].MembersInstance[0].NameWithLevel));
                    }
                    else
                    {
                        Utility.Logger.Add(Utility.LogType.Arsenal, string.Format("개발에 실패했습니다. ({0}/{1}/{2}/{3} 비서함: {4})",
                            dev.Fuel, dev.Ammo, dev.Steel, dev.Bauxite,
                            db.Fleet[1].MembersInstance[0].NameWithLevel));
                    }
                }
            }

            base.OnResponseReceived((object)data);
        }

        public override bool IsRequestSupported => true;
        public override bool IsResponseSupported => true;

        public override string APIName => "api_req_kousyou/createitem";
    }


}
