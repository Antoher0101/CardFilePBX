using System;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

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
	/// Отсутсвие/наличие базы данных
	/// Проверка номера телефона на дубликаты
	public partial class MainWindow : Window
	{
		//private DatabaseApplication db;
		private DataListApplication dl;
		public MainWindow()
		{
			InitializeComponent();

			dl = new DataListApplication(@"");
			this.DataContext = dl;
			// Подключение БД и инициализация таблицы
			dl.ReadDatabase();
			ConnectionDB(this, new RoutedEventArgs());
		}
		private void AddAbonent(object sender, RoutedEventArgs e)
		{
			bool valid = true;

			var firstName = NameAddBox.Text;
			var lastName = LastNameAddBox.Text;
			var patronymic = PatronymicAddBox.Text;
			var phoneNumber = PhoneNumberAddBox.Text;
			var tariff = TariffAddBox.SelectedIndex;

			// Восстановление стиля
			NameAddBox.Style = (Style)Application.Current.Resources["TextBox"];
			LastNameAddBox.Style = (Style)Application.Current.Resources["TextBox"]; ;
			PhoneNumberAddBox.Style = (Style)Application.Current.Resources["TextBox"];
			TariffAddBox.Style = (Style)Application.Current.Resources["ComboBox"];

			// Валидация ввода
			if (string.IsNullOrEmpty(firstName))
			{
				NameAddBox.Style = (Style)Application.Current.Resources["RedTextBox"];
				valid = false;
			}
			if (string.IsNullOrEmpty(lastName))
			{
				LastNameAddBox.Style = (Style)Application.Current.Resources["RedTextBox"];
				valid = false;
			}
			if (string.IsNullOrEmpty(patronymic))
			{
				PatronymicAddBox.Text = "-";
			}
			Regex ex = new Regex(@"^(\+\d)(\(\d{3}\)\d{3})(\-)(\d{2}\-\d{2})$");
			if (!ex.IsMatch(phoneNumber))
			{
				PhoneNumberAddBox.Style = (Style)Application.Current.Resources["RedTextBox"];
				valid = false;
			}
			if (tariff < 0)
			{
				TariffAddBox.Style = (Style)Application.Current.Resources["RedComboBox"];
				valid = false;
			}

			if (valid)
			{
				//db.AddAbonent(firstName, lastName, patronymic, phoneNumber, tariff.ToString());
				//db.UpdateTable();
				dl.AddAbonent(firstName, lastName, patronymic, phoneNumber, tariff.ToString());
				dl.UpdateTable();
				NameAddBox.Text = string.Empty;
				LastNameAddBox.Text = string.Empty;
				PatronymicAddBox.Text = string.Empty;
				PhoneNumberAddBox.Text = string.Empty;
				TariffAddBox.SelectedIndex = -1;
			}
		}

		private void QuestionMark_Click(object sender, RoutedEventArgs e)
		{
			var dialogResult = MessageBox.Show("Вы действительно хотите очистить базу данных от всех записей?", "Очистка базы данных", MessageBoxButton.YesNo);
			if (dialogResult == MessageBoxResult.Yes)
			{
				//db.ClearTable();
				dl.ClearTable();
			}
			//db.UpdateTable();
			dl.UpdateTable();
		}
		private void RefreshBtn_Click(object sender, RoutedEventArgs e)
		{
			//db.UpdateTable();
			dl.UpdateTable();
		}

		// Наименование и ширина столбцов в таблице
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
				case "Name":
					e.Column.Header = "Имя";
					break;
				case "LastName":
					e.Column.Header = "Фамилия";
					break;
				case "Patronymic":
					e.Column.Header = "Отчество";
					break;
				case "PhoneNumber":
					e.Column.Header = "Номер телефона";
					break;
				case "Tariff":
					e.Column.Header = "Тариф";
					e.Column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
					break;
			}
		}

		private async void LoadTest(object sender, RoutedEventArgs e)
		{
			StreamReader sr = new StreamReader(@"E:\C#Project\CardFilePBX\database\test2.csv");
			StreamReader srn = new StreamReader(@"C:\Users\MYAWUTB\Desktop\numbers.txt");
			StreamWriter sw = new StreamWriter(@"E:\C#Project\CardFilePBX\database\newdb2.csv");
			var id = 1;
			Random r = new Random();
			while (!sr.EndOfStream && !srn.EndOfStream)
			{
				string[] str = sr.ReadLine().Split(',');
				string strn = srn.ReadLine();
				
				var firstName = str[1];
				var lastName = str[2];
				var patronymic = str[3];
				var phoneNumber = strn;
				
				sw.WriteLine($"{id++},{firstName},{lastName},{patronymic},{phoneNumber},{r.Next(0,4).ToString()}");

				//await Task.Run(() =>
				//{
				//	db.AddAbonent(firstName, lastName, patronymic, phoneNumber, r.Next(0, 4).ToString());
				//});
			}
			sr.Close();
			srn.Close();
			sw.Close();
			dl.UpdateTable();
		}

		private void AbonentsDataGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			var data = AbonentsDataGrid.CurrentCell.Item as DataRowView;
			if (data != null)
			{
				var curr = new Abonent((int)data.Row[0], (string)data.Row[1], (string)data.Row[2], (string)data.Row[3],
					(string)data.Row[4], (string)data.Row[5]);
				//db.AbonentView = curr;
				dl.AbonentView = curr;
				AbonentCard.IsSelected = true;
			}
		}
		private void ConnectionDB(object sender, RoutedEventArgs args)
		{
			// Проверка доступности БД
			if (dl.CheckDB())
			{
				dl.State = ConnectionState.Open;
				dl.UpdateTable();
				if (sender != this)
				{
					MessageBox.Show("Успешное подключение к базе данных", "Подключение установлено", MessageBoxButton.OK);
				}
				return;
			}
			else dl.State = ConnectionState.Broken;
			MessageBox.Show("Невозможно подключиться к базе данных", "Подключение не установлено", MessageBoxButton.OK);
		}
		private void TextPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex(@"[0-9]");
			if (regex.Match(e.Text).Success)
			{
				e.Handled = true;
			}
		}
		private void PhoneNumberBox_GotFocus(object sender, RoutedEventArgs e)
		{
			((Xceed.Wpf.Toolkit.MaskedTextBox)sender).CaretIndex = 3;
		}

		private void EditButton_Click(object sender, RoutedEventArgs e)
		{
			dl.IsNoEditing = false;
		}
		private void ConfirmEditing(object sender, RoutedEventArgs e)
		{

			var dialogResult = MessageBox.Show("Вы действительно изменить данные?", "Изменение данных абонента", MessageBoxButton.YesNo);
			if (dialogResult == MessageBoxResult.Yes)
			{
				dl.EditAbonentData();
			}
			dl.IsNoEditing = true;
			dl.UpdateTable();
		}

		private void DeleteButton_Click(object sender, RoutedEventArgs e)
		{
			var dialogResult = MessageBox.Show("Вы действительно хотите удалить абонента из базы данных?", "Удаление из базы данных", MessageBoxButton.YesNo);
			if (dialogResult == MessageBoxResult.Yes)
			{
				dl.DeleteAbonent(dl.AbonentView.Id);
			}
			dl.UpdateTable();
		}

		private void SearchButton_Click(object sender, RoutedEventArgs e)
		{
			dl.UpdateTable();
			string query = "";
			if (SearchFirstNameBox.Text != "")
			{
				query = "first_name LIKE '%" + SearchFirstNameBox.Text.Trim() + "%' AND ";
			}
			if (SearchLastNameBox.Text != "")
			{
				query += "last_name LIKE '%" + SearchLastNameBox.Text.Trim() + "%' AND ";
			}
			if (SearchPatronymicBox.Text != "")
			{
				query += "patronymic LIKE '%" + SearchPatronymicBox.Text.Trim() + "%' AND ";
			}
			if (SearchPhoneNumberBox.Text != "")
			{
				query += "phone_number LIKE '%" + SearchPhoneNumberBox.Text.Trim() + "%'";
			}
			if (query.EndsWith("AND "))
			{
				query = query.Remove(query.Length - 4);
			}
			dl.SearchAbonent(query);
		}

		private void SetConnectionDB(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog1 = new OpenFileDialog();
			if (openFileDialog1.ShowDialog() != true)
				return;
			dl.connectionString = openFileDialog1.FileName;
			dl.UpdateTable();
		}
	}
}