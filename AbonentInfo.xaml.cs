using System.Collections.Generic;
using System.Windows;

namespace CardFilePBX
{
	/// <summary>
	/// Логика взаимодействия для AbonentInfo.xaml
	/// </summary>
	public partial class AbonentInfo : Window
	{
		public Abonent _Abonent { get; set; }
		private Stack<Abonent> Prev { get; set; } = new Stack<Abonent>();
		private Stack<Abonent> Next { get; set; } = new Stack<Abonent>();
		public AbonentInfo(Abonent abonent)
		{
			_Abonent = abonent;
			this.DataContext = this;
			InitializeComponent();
			UpdateAbonent();
		}
		public void AddInfoCard(Abonent abonent)
		{
			Prev.Push(_Abonent);
			_Abonent = abonent;
			UpdateAbonent();
		}

		private void UpdateAbonent()
		{
			Title = $"{_Abonent.Name} {_Abonent.LastName}";
			ReportHeader.Text += " " + _Abonent.CurrentPeriod.ToString("MM/yyyy");

			FullnameCell.Text = _Abonent.Patronymic.Length > 1 ? $"{_Abonent.Name} {_Abonent.LastName} {_Abonent.Patronymic}" : $"{_Abonent.Name} {_Abonent.LastName}";
			PhoneNumberCell.Text = _Abonent.PhoneNumber;

			IncomingCallTime.Text = (_Abonent.Incoming).ToString();
			OutgoingCallTime.Text = (_Abonent.Outgoing).ToString();
			AllCallTime.Text = (_Abonent.Incoming + _Abonent.Outgoing).ToString();

			IncomingCost.Text = (_Abonent.Incoming * _Abonent.Tariff.Incoming).ToString();
			OutgoingCost.Text = (_Abonent.Outgoing * _Abonent.Tariff.Outgoing).ToString();
			AllCallCost.Text = (_Abonent.Incoming * _Abonent.Tariff.Incoming + _Abonent.Outgoing * _Abonent.Tariff.Outgoing).ToString();
		}
		private void CloseAllButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void BackButton_Click(object sender, RoutedEventArgs e)
		{
			if (Prev.Count > 0)
			{
				Next.Push(_Abonent);
				_Abonent = Prev.Pop();
				UpdateAbonent();
			}
		}

		private void ForwardButton_Click(object sender, RoutedEventArgs e)
		{
			if (Next.Count > 0)
			{
				Prev.Push(_Abonent);
				_Abonent = Next.Pop();
				UpdateAbonent();
			}
		}
	}
}
