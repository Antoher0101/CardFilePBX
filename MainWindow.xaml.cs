using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CardFilePBX
{
	///	14. На междугородной телефонной станции картотека абонентов, содержащая сведения о телефонах и их владельцах, организована в виде линейного списка.
	/// Написать программу, которая:
	///	- обеспечивает начальное формирование картотеки в виде линейного списка;
	///	- производит вывод всей картотеки;
	///	- вводит номер телефона и время разговора;
	///	- выводит извещение на оплату телефонного разговора.
	///	Программа должна обеспечивать диалог с помощью меню и контроль ошибок при вводе.
	///	
	/// Фильтр по Имени/Фамилии/Отчеству
	public partial class MainWindow : Window
	{
		private DatabaseApplication db;
		public DataTable table { get; set; }
		public MainWindow()
		{
			InitializeComponent();

			db = new DatabaseApplication();
			this.DataContext = db;

			ConnectDB(this, new RoutedEventArgs());
		}

		private void AddAbonent(object sender, RoutedEventArgs e)
		{
			db.Open();
			var firstName = NameAddBox.Text;
			var lastName = LastNameAddBox.Text;
			var patronymic = PatronymicAddBox.Text;
			var phoneNumber = PhoneNumberAddBox.Text;
			var tariff = TariffAddBox.SelectedIndex;
			if (firstName != string.Empty && lastName != string.Empty && phoneNumber != string.Empty)
			{
				db.AddAbonent(firstName, lastName, patronymic, phoneNumber, tariff.ToString());
			}
			db.UpdateTable();
			db.Close();
			NameAddBox.Text = string.Empty;
			LastNameAddBox.Text = string.Empty;
			PatronymicAddBox.Text = string.Empty;
			PhoneNumberAddBox.Text = string.Empty;
			TariffAddBox.SelectedIndex = -1;
		}

		private void QuestionMark_Click(object sender, RoutedEventArgs e)
		{
			db.Open();
			var dialogResult = MessageBox.Show("Вы действительно хотите очистить базу данных от всех записей?", "Очистка базы данных", MessageBoxButton.YesNo);
			if (dialogResult == MessageBoxResult.Yes)
			{
				db.ClearTable();
			}
			db.UpdateTable();
			db.Close();
		}
		private void RefreshBtn_Click(object sender, RoutedEventArgs e)
		{
			db.Open();
			db.UpdateTable();
			db.Close();
		}

		// Наименование столбцов в таблице
		private void AbonentsDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
		{
			var col = e.Column.Header.ToString();
			e.Column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
			switch (col)
			{
				case "Id":
					e.Column.Header = "ИН";
					e.Column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);

					break;
				case "first_name":
					e.Column.Header = "Имя";
					break;
				case "last_name":
					e.Column.Header = "Фамилия";
					break;
				case "patronymic":
					e.Column.Header = "Отчество";
					break;
				case "phone_number":
					e.Column.Header = "Номер телефона";
					break;
				case "tariff":
					e.Column.Header = "Тариф";
					e.Column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
					break;
			}
		}

		private void LoadTest(object sender, RoutedEventArgs e)
		{
			db.Open();
			if (db.State == System.Data.ConnectionState.Open)
			{
				StreamReader sr = new StreamReader(@"E:\C#Project\CardFilePBX\database\test2.csv");
				while (!sr.EndOfStream)
				{
					string[] str = sr.ReadLine().Split(',');
					var firstName = str[1];
					var lastName = str[2];
					var patronymic = str[3];
					var phoneNumber = str[4];
					Random r = new Random();
					db.AddAbonent(firstName, lastName, patronymic, phoneNumber, r.Next(0,4).ToString());
				}
			}
			db.UpdateTable();
			db.Close();
		}

		private void AbonentsDataGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			var data = AbonentsDataGrid.CurrentCell.Item as DataRowView;
			if (data != null)
			{
				var curr = new Abonent((int)data.Row[0], (string)data.Row[1], (string)data.Row[2], (string)data.Row[3],
					(string)data.Row[4], (string)data.Row[5]);
				db.AbonentView = curr;
				AbonentCard.IsSelected = true;
			}
		}

		private void ConnectDB(object sender, RoutedEventArgs e)
		{
			if (sender == this || db.State == ConnectionState.Broken)
			{
				if (db.Open())
				{
					db.UpdateTable();
					db.Close();
				}
			}
			else
			{
				var dialogResult = MessageBox.Show("База данных уже подключена. Создать подключение заново?", "Подключение к базе данных", MessageBoxButton.YesNo);
				if (dialogResult == MessageBoxResult.Yes)
				{
					if (db.Open())
					{
						db.UpdateTable();
						db.Close();
					}
				}
			}
		}
		private void TextPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex(@"[0-9]");
			if (regex.Match(e.Text).Success)
			{
				e.Handled = true;
			}
		}
		private void PhoneNumberAddBox_GotFocus(object sender, RoutedEventArgs e)
		{
			((Xceed.Wpf.Toolkit.MaskedTextBox)sender).CaretIndex = 3;
		}

		private void EditButton_Click(object sender, RoutedEventArgs e)
		{
			if (db.IsNoEditing == false)
			{
				db.IsNoEditing = true;
			}
			else db.IsNoEditing = false;
		}
	}
}