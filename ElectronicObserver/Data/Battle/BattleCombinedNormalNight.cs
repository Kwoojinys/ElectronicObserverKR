﻿using ElectronicObserver.Data.Battle.Phase;
using System.Collections.Generic;

namespace ElectronicObserver.Data.Battle
{

    /// <summary>
    /// 連合艦隊 vs 通常艦隊 夜戦
    /// </summary>
    public class BattleCombinedNormalNight : BattleNight
	{

		public override void LoadFromResponse(string apiname, dynamic data)
		{
			base.LoadFromResponse(apiname, (object)data);

            this.NightInitial = new PhaseNightInitial(this, "야전개시", true);
            this.FriendlySupportInfo = new PhaseFriendlySupportInfo(this, "우군함대");
            this.FriendlyShelling = new PhaseFriendlyShelling(this, "우군함대원호");
            // 支援なし?
            this.NightBattle = new PhaseNightBattle(this, "야전", 0);

            foreach (var phase in this.GetPhases())
				phase.EmulateBattle(this._resultHPs, this._attackDamages);
		}


		public override string APIName => "api_req_combined_battle/midnight_battle";

		public override string BattleName => "연합함대 야전";


		public override IEnumerable<PhaseBase> GetPhases()
		{
			yield return this.Initial;
			yield return this.NightInitial;
            yield return this.FriendlySupportInfo;
            yield return this.FriendlyShelling;
            yield return this.NightBattle;
		}
	}

}
