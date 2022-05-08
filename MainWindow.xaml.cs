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
	///	- обеспечивает начальное формирование картотеки в виде линейного списка;✅
	///	- производит вывод всей картотеки;✅
	///	- вводит номер телефона и время разговора;
	///	- выводит извещение на оплату телефонного разговора.
	///	Программа должна обеспечивать диалог с помощью меню и контроль ошибок при вводе.✅
	public partial class MainWindow : Window
	{
		//private DatabaseApplication db;
		private DataListApplication dl;
		private AbonentInfo InfoWindow;
		public MainWindow()
		{
			InitializeComponent();

			dl = new DataListApplication();
			this.DataContext = dl;
			// Подключение БД и инициализация таблицы
			dl.UpdateTable();
			ConnectionDB(this, new RoutedEventArgs());
		}

		private void AddAbonent(object sender, RoutedEventArgs e)
		{
			Random r = new Random();
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
				patronymic = "-";
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
				dl.AddAbonent(firstName, lastName, patronymic, phoneNumber, tariff.ToString(), DataListApplication.GetTotalCallTime(r), DataListApplication.GetTotalCallTime(r));
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
				dl.ClearTable();
			}
			dl.UpdateTable();
		}
		private void RefreshBtn_Click(object sender, RoutedEventArgs e)
		{
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
					break;
				default:
					e.Column.Visibility = Visibility.Collapsed;
					break;
			}
		}
		#region Debug
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

				await Task.Run(() =>
				{
					dl.AddAbonent(firstName, lastName, patronymic, phoneNumber, r.Next(0, 4).ToString(), DataListApplication.GetTotalCallTime(r), DataListApplication.GetTotalCallTime(r));
				});
			}
			sr.Close();
			srn.Close();
			sw.Close();
			dl.UpdateTable();
		} 
		#endregion

		private void AbonentViewChanged(object sender, EventArgs e)
		{
			var data = AbonentsDataGrid.CurrentCell.Item as DataRowView;
			if (data != null)
			{
				var curr = dl.SelectById((int)data.Row[0]);
				dl.AbonentView = curr;
				AbonentCard.IsSelected = true;
			}
		}
		private void ConnectionDB(object sender, RoutedEventArgs args)
		{
			// Проверка доступности БД
			if (dl.CheckDB())
			{
				string dbName = Path.GetFileNameWithoutExtension(dl.connectionString);
				dl.State = ConnectionState.Open;
				if (sender != this)
				{
					MessageBox.Show($"Успешное подключение к базе данных {dbName}.", "Подключение установлено", MessageBoxButton.OK);
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
			dl.AbonentView.Tariff = dl.TariffConverter(TariffChangeComboBox.SelectedIndex.ToString());
			var dialogResult = MessageBox.Show("Вы действительно изменить данные?", "Изменение данных абонента", MessageBoxButton.YesNo);
			if (dialogResult == MessageBoxResult.Yes)
			{
				dl.EditAbonentData();
			}
			dl.IsNoEditing = true;
			dl.AbonentView = null;
			dl.UpdateTable();
		}

		private void DeleteButton_Click(object sender, RoutedEventArgs e)
		{
			var dialogResult = MessageBox.Show("Вы действительно хотите удалить абонента из базы данных?", "Удаление из базы данных", MessageBoxButton.YesNo);
			if (dialogResult == MessageBoxResult.Yes)
			{
				dl.DeleteAbonent(dl.AbonentView.Id);
				dl.AbonentView = null;
			}
			dl.UpdateTable();
		}

		private void SearchButton_Click(object sender, RoutedEventArgs e)
		{
			dl.UpdateTable();
			string query = "";
			if (SearchFirstNameBox.Text != "")
			{
				query = "Name LIKE '%" + SearchFirstNameBox.Text.Trim() + "%' AND ";
			}
			if (SearchLastNameBox.Text != "")
			{
				query += "LastName LIKE '%" + SearchLastNameBox.Text.Trim() + "%' AND ";
			}
			if (SearchPatronymicBox.Text != "")
			{
				query += "Patronymic LIKE '%" + SearchPatronymicBox.Text.Trim() + "%' AND ";
			}
			if (SearchPhoneNumberBox.IsMaskFull)
			{
				query += "PhoneNumber LIKE '%" + SearchPhoneNumberBox.Text.Trim() + "%'";
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
			dl.SetConnectionString(openFileDialog1.FileName);
			dl.UpdateTable();
		}

		private void CreateDB(object sender, RoutedEventArgs e)
		{
			SaveFileDialog dialog = new SaveFileDialog()
			{
				DefaultExt = "*.db",
				AddExtension = true,
				Filter = "Data Base File(*.db)|*.db",
				InitialDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\database")),
			};
			if (dialog.ShowDialog() == true)
			{
				dl.CreateDbFile(dialog.FileName);
			}
		}

		private void GetAbonentInfo(object sender, RoutedEventArgs e)
		{
			if (InfoWindow is null)
			{
				InfoWindow = new AbonentInfo(dl.AbonentView);
				// memory leak
				InfoWindow.Closed += (o, args) => InfoWindow = null;
				InfoWindow.Owner = this;
				InfoWindow.Show();
			}
			else
			{
				InfoWindow.AddInfoCard(dl.AbonentView);
			}
		}
	}
}