﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ElectronicObserver.Utility.Storage
{

    /// <summary>
    /// シリアル化可能な List を扱います。
    /// コンマ区切りでシリアル化するので、その形式で変換できないものは指定しないでくだちい
    /// </summary>
    /// <typeparam name="T">リストの型。ISerializableです。</typeparam>
    [DataContract(Name = "SerializableList")]
	public class SerializableList<T> where T : IConvertible
	{

		[IgnoreDataMember]
		public List<T> List { get; set; }

		public SerializableList()
		{
            this.List = null;
		}

		public SerializableList(List<T> list)
		{
            this.List = list;
		}

		public SerializableList(string serial)
		{
            this.SerializedList = serial;
		}


		[DataMember]
		public string SerializedList
		{
			get { return ListToString(this.List); }
			set { this.List = StringToList(value); }
		}


		public static implicit operator List<T>(SerializableList<T> value)
		{
			if (value == null) return null;
			return value.List;
		}

		public static implicit operator SerializableList<T>(List<T> value)
		{
			return new SerializableList<T>(value);
		}


		public static List<T> StringToList(string serial, bool suppressError = false)
		{
			if (serial == null)
				return null;
			if (serial == "")
				return new List<T>();

			try
			{

				return new List<T>(serial.Split(",".ToCharArray()).Select(s => (T)Convert.ChangeType(s, typeof(T))));

			}
			catch (Exception ex)
			{

				if (!suppressError)
                    ErrorReporter.SendErrorReport(ex, "SerializableList: StringToList 에 실패했습니다.");
			}

			return null;
		}

		public static string ListToString(List<T> value, bool suppressError = false)
		{
			if (value == null) return "";

			try
			{

				return string.Join(",", value.ToArray());

			}
			catch (Exception ex)
			{

				if (!suppressError)
                    ErrorReporter.SendErrorReport(ex, "SerializableList: ListToString 에 실패했습니다.");
			}

			return "";
		}

	}
}
