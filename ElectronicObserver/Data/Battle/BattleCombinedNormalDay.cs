﻿using ElectronicObserver.Data.Battle.Phase;
using System.Collections.Generic;

namespace ElectronicObserver.Data.Battle
{

    /// <summary>
    /// 機動部隊 vs 通常艦隊 昼戦
    /// </summary>
    public class BattleCombinedNormalDay : BattleDay
	{

		public override void LoadFromResponse(string apiname, dynamic data)
		{
			base.LoadFromResponse(apiname, (object)data);

            this.JetBaseAirAttack = new PhaseJetBaseAirAttack(this, "기지항공대 분식 강습");
            this.JetAirBattle = new PhaseJetAirBattle(this, "분식 항공전");
            this.BaseAirAttack = new PhaseBaseAirAttack(this, "기지 항공대 공격");
            this.FriendlySupportInfo = new PhaseFriendlySupportInfo(this, "우군함대");
            this.FriendlyAirBattle = new PhaseFriendlyAirBattle(this, "우군지원항공공격");
            this.AirBattle = new PhaseAirBattle(this, "항공전");
            this.Support = new PhaseSupport(this, "지원 공격");
            this.OpeningASW = new PhaseOpeningASW(this, "선제대잠");
            this.OpeningTorpedo = new PhaseTorpedo(this, "선제뇌격", 0);
            this.Shelling1 = new PhaseShelling(this, "제1차포격전", 1, "1");
            this.Torpedo = new PhaseTorpedo(this, "뇌격전", 2);
            this.Shelling2 = new PhaseShelling(this, "제2차포격전", 3, "2");
            this.Shelling3 = new PhaseShelling(this, "제3차포격전", 4, "3");

            foreach (var phase in this.GetPhases())
				phase.EmulateBattle(this._resultHPs, this._attackDamages);

		}


		public override string APIName => "api_req_combined_battle/battle";

		public override string BattleName => "連合艦隊-機動部隊 昼戦";



		public override IEnumerable<PhaseBase> GetPhases()
		{
			yield return this.Initial;
			yield return this.Searching;
			yield return this.JetBaseAirAttack;
			yield return this.JetAirBattle;
			yield return this.BaseAirAttack;
            yield return this.FriendlySupportInfo;
            yield return this.FriendlyAirBattle;
            yield return this.AirBattle;
			yield return this.Support;
			yield return this.OpeningASW;
			yield return this.OpeningTorpedo;
			yield return this.Shelling1;
			yield return this.Torpedo;
			yield return this.Shelling2;
			yield return this.Shelling3;
		}

	}

}
