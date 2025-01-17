﻿using ElectronicObserver.Resource;
using ElectronicObserver.Utility.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ElectronicObserver.Data
{

    /// <summary>
    /// 艦船グループのデータを管理します。
    /// </summary>
    [DataContract(Name = "ShipGroupManager")]
	public sealed class ShipGroupManager : DataStorage
	{

		public const string DefaultFilePath = @"Settings\ShipGroups.xml";


		/// <summary>
		/// 艦船グループリスト
		/// </summary>
		[IgnoreDataMember]
		public IDDictionary<ShipGroupData> ShipGroups { get; private set; }


		[DataMember]
		private IEnumerable<ShipGroupData> ShipGroupsSerializer
		{
			get { return this.ShipGroups.Values.OrderBy(g => g.ID); }
			set { this.ShipGroups = new IDDictionary<ShipGroupData>(value); }
		}

		public ShipGroupManager()
		{
            this.Initialize();
		}


		public override void Initialize()
		{
            this.ShipGroups = new IDDictionary<ShipGroupData>();
		}



		public ShipGroupData this[int index] => this.ShipGroups[index];



		public ShipGroupData Add()
		{

			int key = this.GetUniqueID();
			var group = new ShipGroupData(key);
            this.ShipGroups.Add(group);
			return group;

		}

		public int GetUniqueID()
		{
			return this.ShipGroups.Count > 0 ? this.ShipGroups.Keys.Max() + 1 : 1;
		}


		public ShipGroupManager Load()
		{

			ResourceManager.CopyFromArchive(DefaultFilePath.Replace("\\", "/"), DefaultFilePath, true, false);

			return (ShipGroupManager)this.Load(DefaultFilePath);
		}

		public void Save()
		{
            this.Save(DefaultFilePath);
		}

	}

}
