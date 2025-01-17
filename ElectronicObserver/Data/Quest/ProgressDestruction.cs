﻿using System.Runtime.Serialization;

namespace ElectronicObserver.Data.Quest
{

    /// <summary>
    /// 艦船解体任務の進捗を管理します。
    /// </summary>
    [DataContract(Name = "ProgressDestruction")]
	public class ProgressDestruction : ProgressData
	{

		public ProgressDestruction(QuestData quest, int maxCount)
			: base(quest, maxCount)
		{
		}

		public void Increment(int amount)
		{
			for (int i = 0; i < amount; i++)
                this.Increment();
		}

		public override string GetClearCondition()
		{
			return "해체 " + this.ProgressMax;
		}
	}
}
