using System;

namespace ElectronicObserver.Data
{

    public class RelocationData : IIdentifiable
	{

		/// <summary>
		/// 装備ID
		/// </summary>
		public int EquipmentID { get; set; }

		/// <summary>
		/// 配置転換を開始した時間
		/// </summary>
		public DateTime RelocatedTime { get; set; }


		/// <summary>
		/// 装備のインスタンス
		/// </summary>
		public EquipmentData EquipmentInstance => KCDatabase.Instance.Equipments[this.EquipmentID];


		public RelocationData(int equipmentID, DateTime relocatedTime)
		{
            this.EquipmentID = equipmentID;
            this.RelocatedTime = relocatedTime;
		}

		public int ID => this.EquipmentID;
		public override string ToString() => $"[{this.EquipmentID}] {this.EquipmentInstance.NameWithLevel} @ {this.RelocatedTime}";
	}

}
