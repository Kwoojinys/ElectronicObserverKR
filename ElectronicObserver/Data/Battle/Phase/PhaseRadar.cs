﻿namespace ElectronicObserver.Data.Battle.Phase
{
    /// <summary>
    /// レーダー射撃フェーズの処理を行います。
    /// </summary>
    public class PhaseRadar : PhaseShelling
    {

        // 砲撃戦とフォーマットが同じなので流用

        public PhaseRadar(BattleData data, string title)
            : base(data, title, 1, "1")
        {
        }

        public override bool IsAvailable => this.RawData.api_hougeki1() && this.RawData.api_hougeki1 != null;

    }
}