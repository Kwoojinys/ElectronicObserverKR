﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ElectronicObserver.Resource;
using ElectronicObserver.Data;
using ElectronicObserver.Utility.Mathematics;
using ElectronicObserver.Utility.Data;

namespace ElectronicObserver.Window.Control
{

    /// <summary>
    /// 艦隊の状態を表示します。
    /// </summary>
    public partial class FleetState : UserControl
	{

		[System.Diagnostics.DebuggerDisplay("{State}")]
		private class StateLabel
		{
			public FleetStates State;
			public ImageLabel Label;
			public DateTime Timer;
			private bool _onmouse;

			private string _text;
			public string Text
			{
				get { return this._text; }
				set
				{
                    this._text = value;
                    this.UpdateText();
				}
			}
			private string _shortenedText;
			public string ShortenedText
			{
				get { return this._shortenedText; }
				set
				{
                    this._shortenedText = value;
                    this.UpdateText();
				}
			}
			private bool _autoShorten;
			public bool AutoShorten
			{
				get { return this._autoShorten; }
				set
				{
                    this._autoShorten = value;
                    this.UpdateText();
				}
			}

			private bool _enabled;
			public bool Enabled
			{
				get { return this._enabled; }
				set
				{
                    this._enabled = value;
                    this.Label.Visible = value;
				}
			}


			public StateLabel()
			{
                this.Label = GetDefaultLabel();
                this.Label.MouseEnter += this.Label_MouseEnter;
                this.Label.MouseLeave += this.Label_MouseLeave;
                this.Enabled = false;
			}

			public static ImageLabel GetDefaultLabel()
			{
				var label = new ImageLabel
				{
					Anchor = AnchorStyles.Left,
					ImageList = ResourceManager.Instance.Icons,
					Padding = new Padding(2, 2, 2, 2),
					Margin = new Padding(2, 0, 2, 0),
					AutoSize = true
				};
				return label;
			}

			public void SetInformation(FleetStates state, string text, string shortenedText, int imageIndex, Color backColor)
			{
                this.State = state;
                this.Text = text;
                this.ShortenedText = shortenedText;
                this.UpdateText();
                this.Label.ImageIndex = imageIndex;
                this.Label.BackColor = backColor;
			}

			public void SetInformation(FleetStates state, string text, string shortenedText, int imageIndex)
			{
                this.SetInformation(state, text, shortenedText, imageIndex, Color.Transparent);
			}

			public void UpdateText()
			{
                this.Label.Text = (!this.AutoShorten || this._onmouse) ? this.Text : this.ShortenedText;
			}


			void Label_MouseEnter(object sender, EventArgs e)
			{
                this._onmouse = true;
                this.UpdateText();
			}

			void Label_MouseLeave(object sender, EventArgs e)
			{
                this._onmouse = false;
                this.UpdateText();
			}

		}


		public override Font Font
		{
			get
			{
				return base.Font;
			}
			set
			{
				base.Font = value;
				foreach (var state in this.StateLabels)
					state.Label.Font = value;
			}
		}


		private List<StateLabel> StateLabels;


		public FleetState()
		{
            this.InitializeComponent();

            this.StateLabels = new List<StateLabel>();
		}


		private StateLabel AddStateLabel()
		{
            this.StateLabels.Add(new StateLabel());
			var ret = this.StateLabels.Last();
            this.LayoutBase.Controls.Add(ret.Label);
			return ret;
		}

		private StateLabel GetStateLabel(int index)
		{
			if (index >= this.StateLabels.Count)
			{
				for (int i = this.StateLabels.Count; i <= index; i++)
                    this.AddStateLabel();
			}
            this.StateLabels[index].Enabled = true;
			return this.StateLabels[index];
		}



		public void UpdateFleetState(FleetData fleet, ToolTip tooltip)
		{

			KCDatabase db = KCDatabase.Instance;

			int index = 0;


			bool emphasizesSubFleetInPort = Utility.Configuration.Config.FormFleet.EmphasizesSubFleetInPort &&
				(db.Fleet.CombinedFlag > 0 ? fleet.FleetID >= 3 : fleet.FleetID >= 2);
			var displayMode = (FleetStateDisplayModes)Utility.Configuration.Config.FormFleet.FleetStateDisplayMode;

			Color colorDanger = Color.LightCoral;
			Color colorInPort = Color.Transparent;

			//所属艦なし
			if (fleet == null || fleet.Members.All(id => id == -1))
			{
				var state = this.GetStateLabel(index);

				state.SetInformation(FleetStates.NoShip, "소속함없음", "", (int)ResourceManager.IconContent.FleetNoShip);
				tooltip.SetToolTip(state.Label, null);

				emphasizesSubFleetInPort = false;
				index++;

			}
			else
			{

				if (fleet.IsInSortie)
				{

					//大破出撃中
					if (fleet.MembersWithoutEscaped.Any(s => s != null && s.HPRate <= 0.25))
					{
						var state = this.GetStateLabel(index);

						state.SetInformation(FleetStates.SortieDamaged, "！！대파진격중！！", "！！대파진격중！！", (int)ResourceManager.IconContent.FleetSortieDamaged, colorDanger);
						tooltip.SetToolTip(state.Label, null);

						index++;

					}
					else
					{   //出撃中
						var state = this.GetStateLabel(index);

						state.SetInformation(FleetStates.Sortie, "출격중", "", (int)ResourceManager.IconContent.FleetSortie);
						tooltip.SetToolTip(state.Label, null);

						index++;
					}

					emphasizesSubFleetInPort = false;
				}

				//遠征中
				if (fleet.ExpeditionState != 0)
				{
					var state = this.GetStateLabel(index);

					state.Timer = fleet.ExpeditionTime;
					state.SetInformation(FleetStates.Expedition,
						"원정중 " + DateTimeHelper.ToTimeRemainString(state.Timer),
						DateTimeHelper.ToTimeRemainString(state.Timer),
						(int)ResourceManager.IconContent.FleetExpedition);

					var dest = db.Mission[fleet.ExpeditionDestination];
					tooltip.SetToolTip(state.Label,
						string.Format("{0} : {1}\r\n완료 시간 : {2}",
						dest.ID, dest.Name, DateTimeHelper.TimeToCSVString(state.Timer)));

					emphasizesSubFleetInPort = false;
					index++;
				}

				//大破艦あり
				if (!fleet.IsInSortie && fleet.MembersWithoutEscaped.Any(s => s != null && s.HPRate <= 0.25 && s.RepairingDockID == -1))
				{
					var state = this.GetStateLabel(index);

					state.SetInformation(FleetStates.Damaged, "대파함있음！", "대파함있음！", (int)ResourceManager.IconContent.FleetDamaged, colorDanger);
					tooltip.SetToolTip(state.Label, null);

					emphasizesSubFleetInPort = false;
					index++;
				}

				//泊地修理中
				if (fleet.CanAnchorageRepair)
				{
					var state = this.GetStateLabel(index);

					state.Timer = db.Fleet.AnchorageRepairingTimer;
					state.SetInformation(FleetStates.AnchorageRepairing,
						"아카시수리중 " + DateTimeHelper.ToTimeElapsedString(state.Timer),
						DateTimeHelper.ToTimeElapsedString(state.Timer),
						(int)ResourceManager.IconContent.FleetAnchorageRepairing);


					StringBuilder sb = new StringBuilder();
					sb.AppendFormat("시작 시간 : {0}\r\n수리 시간 :\r\n",
						DateTimeHelper.TimeToCSVString(db.Fleet.AnchorageRepairingTimer));

					for (int i = 0; i < fleet.Members.Count; i++)
					{
						var ship = fleet.MembersInstance[i];
						if (ship != null && ship.HPRate < 1.0)
						{
							var totaltime = DateTimeHelper.FromAPITimeSpan(ship.RepairTime);
							var unittime = Calculator.CalculateDockingUnitTime(ship);
							sb.AppendFormat("#{0} : {1} @ {2} x -{3} HP\r\n",
								i + 1,
								DateTimeHelper.ToTimeRemainString(totaltime),
								DateTimeHelper.ToTimeRemainString(unittime),
								ship.HPMax - ship.HPCurrent
								);
						}
						else
						{
							sb.Append("#").Append(i + 1).Append(" : ----\r\n");
						}
					}

					tooltip.SetToolTip(state.Label, sb.ToString());

					emphasizesSubFleetInPort = false;
					index++;
				}

				//入渠中
				{
					long ntime = db.Docks.Values.Where(d => d.State == 1 && fleet.Members.Contains(d.ShipID)).Select(d => d.CompletionTime.Ticks).DefaultIfEmpty().Max();

					if (ntime > 0)
					{   //入渠中
						var state = this.GetStateLabel(index);

						state.Timer = new DateTime(ntime);
						state.SetInformation(FleetStates.Docking,
							 "수리중 " + DateTimeHelper.ToTimeRemainString(state.Timer),
							 DateTimeHelper.ToTimeRemainString(state.Timer),
							 (int)ResourceManager.IconContent.FleetDocking);

						tooltip.SetToolTip(state.Label, "완료 시간 : " + DateTimeHelper.TimeToCSVString(state.Timer));

						emphasizesSubFleetInPort = false;
						index++;
					}

				}

				//未補給
				{
					var members = fleet.MembersInstance.Where(s => s != null);

					int fuel = members.Sum(ship => ship.SupplyFuel);
					int ammo = members.Sum(ship => ship.SupplyAmmo);
					int aircraft = members.SelectMany(s => s.MasterShip.Aircraft.Zip(s.Aircraft, (max, now) => max - now)).Sum();
					int bauxite = aircraft * 5;

					if (fuel > 0 || ammo > 0 || bauxite > 0)
					{
						var state = this.GetStateLabel(index);

						state.SetInformation(FleetStates.NotReplenished, "미보급", "", (int)ResourceManager.IconContent.FleetNotReplenished, colorInPort);
						tooltip.SetToolTip(state.Label, string.Format("연료 : {0}\r\n탄약 : {1}\r\n보키 : {2} ({3}기)", fuel, ammo, bauxite, aircraft));

						index++;
					}
				}

				//疲労
				{
					int cond = fleet.MembersInstance.Min(s => s == null ? 100 : s.Condition);

					if (cond < Utility.Configuration.Config.Control.ConditionBorder && fleet.ConditionTime != null && fleet.ExpeditionState == 0)
					{
						var state = this.GetStateLabel(index);

						int iconIndex;
						if (cond < 20)
							iconIndex = (int)ResourceManager.IconContent.ConditionVeryTired;
						else if (cond < 30)
							iconIndex = (int)ResourceManager.IconContent.ConditionTired;
						else
							iconIndex = (int)ResourceManager.IconContent.ConditionLittleTired;

						state.Timer = (DateTime)fleet.ConditionTime;
						state.SetInformation(FleetStates.Tired,
							"피로 " + DateTimeHelper.ToTimeRemainString(state.Timer),
							DateTimeHelper.ToTimeRemainString(state.Timer),
							iconIndex,
							colorInPort);

						tooltip.SetToolTip(state.Label, string.Format("회복 시간: {0}\r\n(오차범위: {1})",
							DateTimeHelper.TimeToCSVString(state.Timer), DateTimeHelper.ToTimeRemainString(TimeSpan.FromSeconds(db.Fleet.ConditionBorderAccuracy))));

						index++;

					}
					else if (cond >= 50)
					{       //戦意高揚
						var state = this.GetStateLabel(index);

						state.SetInformation(FleetStates.Sparkled, "피로도 좋음！", "", (int)ResourceManager.IconContent.ConditionSparkle, colorInPort);
						tooltip.SetToolTip(state.Label, string.Format("최소cond: {0}\r\n, {1} 회 원정가능", cond, Math.Ceiling((cond - 49) / 3.0)));

						index++;
					}

				}

				//出撃可能！
				if (index == 0)
				{
					var state = this.GetStateLabel(index);

					state.SetInformation(FleetStates.Ready, "출격가능！", "", (int)ResourceManager.IconContent.FleetReady, colorInPort);
					tooltip.SetToolTip(state.Label, null);

					index++;
				}

			}


			if (emphasizesSubFleetInPort)
			{
				for (int i = 0; i < index; i++)
				{
					if (this.StateLabels[i].Label.BackColor == Color.Transparent)
                        this.StateLabels[i].Label.BackColor = Utility.ThemeManager.GetColor(Utility.ThemeColors.GreenHighlight);
				}
			}


			for (int i = displayMode == FleetStateDisplayModes.Single ? 1 : index; i < this.StateLabels.Count; i++)
                this.StateLabels[i].Enabled = false;


			switch (displayMode)
			{

				case FleetStateDisplayModes.AllCollapsed:
					for (int i = 0; i < index; i++)
                        this.StateLabels[i].AutoShorten = true;
					break;

				case FleetStateDisplayModes.MultiCollapsed:
					if (index == 1)
					{
                        this.StateLabels[0].AutoShorten = false;
					}
					else
					{
						for (int i = 0; i < index; i++)
                            this.StateLabels[i].AutoShorten = true;
					}
					break;

				case FleetStateDisplayModes.Single:
				case FleetStateDisplayModes.AllExpanded:
					for (int i = 0; i < index; i++)
                        this.StateLabels[i].AutoShorten = false;
					break;
			}

		}


		public void RefreshFleetState()
		{

			foreach (var state in this.StateLabels)
			{

				if (!state.Enabled)
					continue;

				switch (state.State)
				{

					case FleetStates.Damaged:
						if (Utility.Configuration.Config.FormFleet.BlinkAtDamaged)
							state.Label.BackColor = DateTime.Now.Second % 2 == 0 ? Color.LightCoral : Color.Transparent;
						break;

					case FleetStates.SortieDamaged:
						state.Label.BackColor = DateTime.Now.Second % 2 == 0 ? Color.LightCoral : Color.Transparent;
						break;

					case FleetStates.Docking:
						state.ShortenedText = DateTimeHelper.ToTimeRemainString(state.Timer);
						state.Text = "입거중 " + state.ShortenedText;
						state.UpdateText();
						if (Utility.Configuration.Config.FormFleet.BlinkAtCompletion && (state.Timer - DateTime.Now).TotalMilliseconds <= Utility.Configuration.Config.NotifierRepair.AccelInterval)
							state.Label.BackColor = DateTime.Now.Second % 2 == 0 ? Utility.ThemeManager.GetColor(Utility.ThemeColors.GreenHighlight) : Color.Transparent;
						break;

					case FleetStates.Expedition:
						state.ShortenedText = DateTimeHelper.ToTimeRemainString(state.Timer);
						state.Text = "원정중 " + state.ShortenedText;
						state.UpdateText();
						if (Utility.Configuration.Config.FormFleet.BlinkAtCompletion && (state.Timer - DateTime.Now).TotalMilliseconds <= Utility.Configuration.Config.NotifierExpedition.AccelInterval)
							state.Label.BackColor = DateTime.Now.Second % 2 == 0 ? Utility.ThemeManager.GetColor(Utility.ThemeColors.GreenHighlight) : Color.Transparent;
						break;

					case FleetStates.Tired:
						state.ShortenedText = DateTimeHelper.ToTimeRemainString(state.Timer);
						state.Text = "피로도 " + state.ShortenedText;
						state.UpdateText();
						if (Utility.Configuration.Config.FormFleet.BlinkAtCompletion && (state.Timer - DateTime.Now).TotalMilliseconds <= 0)
							state.Label.BackColor = DateTime.Now.Second % 2 == 0 ? Utility.ThemeManager.GetColor(Utility.ThemeColors.GreenHighlight) : Color.Transparent;
						break;

					case FleetStates.AnchorageRepairing:
						state.ShortenedText = DateTimeHelper.ToTimeElapsedString(KCDatabase.Instance.Fleet.AnchorageRepairingTimer);
						state.Text = "아카시수리중 " + state.ShortenedText;
						state.UpdateText();
						break;

				}

			}

		}

		public int GetIconIndex()
		{
			var first = this.StateLabels.Where(s => s.Enabled).OrderBy(s => s.State).FirstOrDefault();
			return first == null ? (int)ResourceManager.IconContent.FormFleet : first.Label.ImageIndex;
		}

	}

	/// <summary>
	/// 艦隊の状態を表します。
	/// </summary>
	public enum FleetStates
	{
		NoShip,
		SortieDamaged,
		Sortie,
		Expedition,
		Damaged,
		AnchorageRepairing,
		Docking,
		NotReplenished,
		Tired,
		Sparkled,
		Ready,
	}

	/// <summary>
	/// 状態の表示モードを指定します。
	/// </summary>
	public enum FleetStateDisplayModes
	{
		/// <summary> 1つだけ表示 </summary>
		Single,

		/// <summary> 複数表示(すべて短縮表示) </summary>
		AllCollapsed,

		/// <summary> 複数表示(1つの時は通常表示、複数の時は短縮表示) </summary>
		MultiCollapsed,

		/// <summary> 複数表示(すべて通常表示) </summary>
		AllExpanded,
	}
}
