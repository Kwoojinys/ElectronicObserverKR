using ElectronicObserver.Data.Battle.Detail;
using System.Collections.Generic;
using System.Linq;

namespace ElectronicObserver.Data.Battle.Phase
{
	/// <summary>
	/// 昼戦における友軍艦隊航空攻撃フェーズの処理を行います。
	/// </summary>
	public class PhaseFriendlyAirBattle : PhaseAirBattleBase
	{
		public PhaseFriendlyAirBattle(BattleData battle, string title)
			: base(battle, title)
		{
			if (!this.IsAvailable)
				return;

            this.AirBattleData = this.RawData.api_friendly_kouku;
            this.StageFlag = this.AirBattleData.api_stage_flag;

            this.LaunchedShipIndexFriend = this.GetLaunchedShipIndex(0);
            this.LaunchedShipIndexEnemy = this.GetLaunchedShipIndex(1);

            this.TorpedoFlags = this.ConcatStage3Array<int>("api_frai_flag", "api_erai_flag");
            this.BomberFlags = this.ConcatStage3Array<int>("api_fbak_flag", "api_ebak_flag");
            this.Criticals = this.ConcatStage3Array<int>("api_fcl_flag", "api_ecl_flag");
            this.Damages = this.ConcatStage3Array<double>("api_fdam", "api_edam");
		}

		public override bool IsAvailable => this.RawData.api_friendly_kouku();

		public override void EmulateBattle(int[] hps, int[] damages)
		{
			if (!this.IsAvailable)
				return;

			var friendHps = this.Battle.FriendlySupportInfo.FriendlyInitialHPs.ToArray();

			for (int i = 0; i < this.TorpedoFlags.Length; i++)
			{
				int attackType = (this.TorpedoFlags[i] > 0 ? 1 : 0) | (this.BomberFlags[i] > 0 ? 2 : 0);
				if (attackType > 0)
				{
					bool isEnemy = new BattleIndex(i, false, this.Battle.IsEnemyCombined).IsEnemy;


                    // 航空戦は miss/hit=0, critical=1 のため +1 する(通常は miss=0, hit=1, critical=2) 
                    this.BattleDetails.Add(new BattleFriendlyAirDetail(
                        this.Battle,
						new BattleIndex(i, this.Battle.IsFriendCombined, this.Battle.IsEnemyCombined),
                        this.Damages[i],
                        this.Criticals[i] + 1,
						attackType,
						isEnemy ? hps[i] : friendHps[i]));

					if (isEnemy)
                        this.AddDamage(hps, i, (int)this.Damages[i]);
				}
			}
		}

		protected override IEnumerable<BattleDetail> SearchBattleDetails(int index)
		{
			return this.BattleDetails.Where(d => d.DefenderIndex.IsEnemy && d.DefenderIndex == index);
		}

		public override string AACutInShipName => this.Battle.FriendlySupportInfo.FriendlyMembersInstance[this.AACutInIndex].Name + " Lv. " + this.Battle.FriendlySupportInfo.FriendlyLevels[this.AACutInIndex];
	}
}
