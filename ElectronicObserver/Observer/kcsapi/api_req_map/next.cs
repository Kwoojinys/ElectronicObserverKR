using ElectronicObserver.Data;
using System.Collections.Generic;
using System.Linq;

namespace ElectronicObserver.Observer.kcsapi.api_req_map
{

    public class next : APIBase
	{

		public override void OnResponseReceived(dynamic data)
		{

			KCDatabase.Instance.Battle.LoadFromResponse(this.APIName, data);

			base.OnResponseReceived((object)data);

			switch (KCDatabase.Instance.Battle.Compass.EventID)
            {
				case 3:
                    {
                        EmulateWhirlpool();
                        break;
                    }
				case 2:
				case 6:
                    {
						this.GetMaterialInfo(KCDatabase.Instance.Battle.Compass);
						break;
                    }
            }
		}

        private string GetMaterialInfo(CompassData compass)
        {
            var strs = new LinkedList<string>();

            foreach (var item in compass.GetItems)
            {
                string itemName;

                if (item.ItemID == 4)
                {
                    itemName = Constants.GetMaterialName(item.Metadata);

                }
                else
                {
                    var itemMaster = KCDatabase.Instance.MasterUseItems[item.Metadata];
                    if (itemMaster != null)
                        itemName = itemMaster.Name;
                    else
                        itemName = "알수없는아이템";
                }

                Utility.Logger.Add(Utility.LogType.GetItem,
                    $"{compass.MapAreaID}-{compass.MapInfoID}-{compass.Destination_Name} 에서 {itemName} {item.Amount}개를 획득했습니다.");
                strs.AddLast(itemName + " x " + item.Amount);
            }

            if (!strs.Any())
            {
                return "(없음)";

            }
            else
            {
                return string.Join(", ", strs);
            }


        }

        /// <summary>
        /// 渦潮による燃料・弾薬の減少をエミュレートします。
        /// </summary>
        public static void EmulateWhirlpool()
		{

			int itemID = KCDatabase.Instance.Battle.Compass.WhirlpoolItemID;
			int materialmax = KCDatabase.Instance.Fleet.Fleets.Values
				.Where(f => f != null && f.IsInSortie)
				.SelectMany(f => f.MembersWithoutEscaped)
				.Max(s =>
				{
					if (s == null) return 0;
					switch (itemID)
					{
						case 1:
							return s.Fuel;
						case 2:
							return s.Ammo;
						default:
							return 0;
					}
				});

			double rate = (double)KCDatabase.Instance.Battle.Compass.WhirlpoolItemAmount / materialmax;

			foreach (var ship in KCDatabase.Instance.Fleet.Fleets.Values
				.Where(f => f != null && f.IsInSortie)
				.SelectMany(f => f.MembersWithoutEscaped))
			{

				if (ship == null) continue;

				switch (itemID)
				{
					case 1:
						ship.Fuel -= (int)(ship.Fuel * rate);
						break;
					case 2:
						ship.Ammo -= (int)(ship.Ammo * rate);
						break;
				}
			}

		}


		public override string APIName => "api_req_map/next";
	}

}
