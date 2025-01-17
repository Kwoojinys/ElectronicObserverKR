﻿using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace ElectronicObserver.Utility.Storage
{

    /// <summary>
    /// 汎用データ保存クラスの基底です。
    /// 使用時は DataContractAttribute を設定してください。
    /// </summary>
    [DataContract(Name = "DataStorage")]
	public abstract class DataStorage : IExtensibleDataObject
	{

		public ExtensionDataObject ExtensionData { get; set; }

		public abstract void Initialize();



		public DataStorage()
		{
		}

		[OnDeserializing]
		private void DefaultDeserializing(StreamingContext sc)
		{
            this.Initialize();
		}


		public void Save(string path)
		{

			try
			{

				var serializer = new DataContractSerializer(this.GetType());
				var xmlsetting = new XmlWriterSettings
				{
					Encoding = new System.Text.UTF8Encoding(false),
					Indent = true,
					IndentChars = "\t",
					NewLineHandling = NewLineHandling.Replace
				};

				using (XmlWriter xw = XmlWriter.Create(path, xmlsetting))
				{
					serializer.WriteObject(xw, this);
				}


			}
			catch (Exception ex)
			{

                ErrorReporter.SendErrorReport(ex, this.GetType().Name + " 의 쓰기에 실패했습니다.");
			}

		}

		public void Save(StringBuilder stringBuilder)
		{
			try
			{
				var serializer = new DataContractSerializer(this.GetType());
				using (XmlWriter xw = XmlWriter.Create(stringBuilder))
				{
					serializer.WriteObject(xw, this);
				}
			}
			catch (Exception ex)
			{
                ErrorReporter.SendErrorReport(ex, this.GetType().Name + " 의 쓰기에 실패했습니다.");
			}
		}

		public DataStorage Load(string path)
		{

			try
			{

				var serializer = new DataContractSerializer(this.GetType());

				using (XmlReader xr = XmlReader.Create(path))
				{
					return (DataStorage)serializer.ReadObject(xr);
				}


			}
			catch (FileNotFoundException)
			{

                Logger.Add(LogType.Error, string.Format("{0}: {1} 은 존재하지않습니다.", this.GetType().Name, path));

			}
			catch (DirectoryNotFoundException)
			{

                Logger.Add(LogType.Error, string.Format("{0}: {1} 은 존재하지않습니다.", this.GetType().Name, path));

			}
			catch (Exception ex)
			{

                ErrorReporter.SendErrorReport(ex, this.GetType().Name + " 의 로드에 실패했습니다.");

			}

			return null;
		}



		public void Save(Stream stream)
		{

			try
			{

				var serializer = new DataContractSerializer(this.GetType());
				var xmlsetting = new XmlWriterSettings
				{
					Encoding = new System.Text.UTF8Encoding(false),
					Indent = true,
					IndentChars = "\t",
					NewLineHandling = NewLineHandling.Replace
				};

				using (XmlWriter xw = XmlWriter.Create(stream, xmlsetting))
				{

					serializer.WriteObject(xw, this);
				}


			}
			catch (Exception ex)
			{

                ErrorReporter.SendErrorReport(ex, this.GetType().Name + " 의 쓰기에 실패했습니다.");
			}

		}


		public DataStorage Load(Stream stream)
		{

			try
			{

				var serializer = new DataContractSerializer(this.GetType());

				using (XmlReader xr = XmlReader.Create(stream))
				{
					return (DataStorage)serializer.ReadObject(xr);
				}

			}
			catch (FileNotFoundException)
			{

                Logger.Add(LogType.Error, this.GetType().Name + ": 파일은 존재하지않습니다.");

			}
			catch (DirectoryNotFoundException)
			{

                Logger.Add(LogType.Error, this.GetType().Name + ": 파일은 존재하지않습니다.");

			}
			catch (Exception ex)
			{

                ErrorReporter.SendErrorReport(ex, this.GetType().Name + " 의 로드에 실패했습니다.");

			}

			return null;
		}

		public DataStorage Load(TextReader reader)
		{
			try
			{
				var serializer = new DataContractSerializer(this.GetType());

				using (XmlReader xr = XmlReader.Create(reader))
				{
					return (DataStorage)serializer.ReadObject(xr);
				}
			}
			catch (DirectoryNotFoundException)
			{

                Logger.Add(LogType.Error, this.GetType().Name + ": 파일은 존재하지 않습니다.");

			}
			catch (Exception ex)
			{

                ErrorReporter.SendErrorReport(ex, this.GetType().Name + " 의 로드에 실패했습니다.。");

			}

			return null;
		}

	}
}
