using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CardFilePBX
{
	/// <summary>
	/// Логика взаимодействия для AbonentInfo.xaml
	/// </summary>
	public partial class AbonentInfo : Window
	{
		private Abonent CurrentAbonent { get; set; }
		public Stack<Abonent> Prev { get; set; } = new Stack<Abonent>();
		public Stack<Abonent> Next { get; set; } = new Stack<Abonent>();
		public AbonentInfo()
		{
			this.DataContext = this;
			InitializeComponent();
		}
		public void AddInfoCard(Abonent abonent)
		{
			Prev.Push(CurrentAbonent);
			CurrentAbonent = abonent;
			UpdateAbonent();
		}
		public void SetAbonent(Abonent abonent)
		{
			CurrentAbonent = abonent;
			UpdateAbonent();
		}
		public void SetAbonent(List<Abonent> abonent)
		{
			for (int i = abonent.Count - 1; i > 0; i--)
			{
				Next.Push(abonent[i]);
			}
			CurrentAbonent = abonent.First();
			UpdateAbonent();
		}
		private void UpdateAbonent()
		{
			Title = $"{CurrentAbonent.Name} {CurrentAbonent.LastName}";
			ReportHeader.Text += " " + CurrentAbonent.CurrentPeriod.ToString("MM/yyyy");

			FullnameCell.Text = CurrentAbonent.Patronymic.Length > 1 ? $"{CurrentAbonent.Name} {CurrentAbonent.LastName} {CurrentAbonent.Patronymic}" : $"{CurrentAbonent.Name} {CurrentAbonent.LastName}";
			PhoneNumberCell.Text = CurrentAbonent.PhoneNumber;

			IncomingCallTime.Text = (CurrentAbonent.Incoming).ToString();
			OutgoingCallTime.Text = (CurrentAbonent.Outgoing).ToString();
			AllCallTime.Text = (CurrentAbonent.Incoming + CurrentAbonent.Outgoing).ToString();

			IncomingCost.Text = (CurrentAbonent.Incoming * CurrentAbonent.Tariff.Incoming).ToString();
			OutgoingCost.Text = (CurrentAbonent.Outgoing * CurrentAbonent.Tariff.Outgoing).ToString();
			AllCallCost.Text = (CurrentAbonent.Incoming * CurrentAbonent.Tariff.Incoming + CurrentAbonent.Outgoing * CurrentAbonent.Tariff.Outgoing).ToString();

			BackButton.IsEnabled = Prev.Count > 0;
			ForwardButton.IsEnabled = Next.Count > 0;
		}
		private void CloseAllButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void BackButton_Click(object sender, RoutedEventArgs e)
		{
			if (Prev.Count > 0)
			{
				Next.Push(CurrentAbonent);
				CurrentAbonent = Prev.Pop();
				UpdateAbonent();
			}
		}

		private void ForwardButton_Click(object sender, RoutedEventArgs e)
		{
			if (Next.Count > 0)
			{
				Prev.Push(CurrentAbonent);
				CurrentAbonent = Next.Pop();
				UpdateAbonent();
			}
		}
	}
}
