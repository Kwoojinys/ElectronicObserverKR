namespace ElectronicObserver.Data
{

    /// <summary>
    /// 装備種別
    /// </summary>
    public class EquipmentType : ResponseWrapper, IIdentifiable
	{

		/// <summary>
		/// 装備種別ID
		/// </summary>
		public int TypeID => (int)this.RawData.api_id;

		/// <summary>
		/// 名前
		/// </summary>
		public string Name 
		{ 
			get 
			{ 
				return Utility.ExternalDataReader.Instance.GetTranslation(this.RawData.api_name, Utility.TranslateType.EquipmentType); 
			} 
		}

    //show_flg


    /// <summary>
    /// 装備種別ID
    /// </summary>
    public EquipmentTypes Type => (EquipmentTypes)this.TypeID;


		public override string ToString() => $"[{this.TypeID}] {this.Name}";

		public int ID => this.TypeID;

	}

}
