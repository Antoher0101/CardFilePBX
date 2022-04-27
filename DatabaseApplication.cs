using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;


namespace CardFilePBX
{
	class DatabaseApplication : INotifyPropertyChanged
	{
		private string connectionString { get; set; } = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='|DataDirectory|\database\AbonentsDB.mdf';Integrated Security=True";

		#region Notifiable properties

		private DataTable _dataTable;
		public DataTable DataTable
		{
			get => _dataTable;
			set
			{
				_dataTable = value;
				NotifyPropertyChanged(nameof(DataTable));
			}
		}
		private Abonent _abonentView;
		public Abonent AbonentView
		{
			get => _abonentView;
			set
			{
				_abonentView = value;
				NotifyPropertyChanged(nameof(AbonentView));
			}
		}

		private bool _isNoEditing = true;

		public bool IsNoEditing
		{
			get => _isNoEditing;
			set
			{
				_isNoEditing = value;
				NotifyPropertyChanged(nameof(IsNoEditing));
			}
		}
		private ConnectionState _state = ConnectionState.Broken;

		public ConnectionState State
		{
			get => _state;
			set
			{
				_state = value;
				NotifyPropertyChanged(nameof(State));
			}
		}
		#endregion


		public DatabaseApplication()
		{
			AbonentView = null;
			DataTable = new DataTable();
		}
		public bool CheckDB()
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					connection.Close();
				}
				return true;
			}
			catch
			{
				return false;
			}
		}
		public async void UpdateTable()
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					await connection.OpenAsync();
					var adapter = new SqlDataAdapter("SELECT * FROM Abonents", connection);
					DataTable.Clear();
					adapter.Fill(DataTable);

					foreach (DataRowView row in DataTable.DefaultView)
					{
						switch (row[5])
						{
							case "0":
								row[5] = "МегаТариф";
								break;
							case "1":
								row[5] = "Максимум";
								break;
							case "2":
								row[5] = "VIP";
								break;
							case "3":
								row[5] = "Премиум";
								break;
							default:
								row[5] = "Бонус";
								break;
						}
					}
					connection.Close();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
		// Добавление записи в таблицу
		public async void AddAbonent(string first_name, string last_name, string patronymic, string phone_number, string tariff)
		{
			string query = $"INSERT INTO Abonents (Name, LastName, Patronymic, PhoneNumber, Tariff) VALUES (@first_name, @last_name, @patronymic, @phone_number, @tariff)";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				await connection.OpenAsync();
				using (SqlCommand command = new SqlCommand(query, connection))
				{
					command.Parameters.Add("@first_name", SqlDbType.NVarChar, 40).Value = first_name;
					command.Parameters.Add("@last_name", SqlDbType.NVarChar, 40).Value = last_name;
					command.Parameters.Add("@patronymic", SqlDbType.NVarChar, 40).Value = patronymic;
					command.Parameters.Add("@phone_number", SqlDbType.Text, 16).Value = phone_number;
					command.Parameters.Add("@tariff", SqlDbType.Text, 16).Value = tariff;
					await command.ExecuteNonQueryAsync();
				}
				connection.Close();
			}
		}
		public void SearchAbonent(string query)
		{
			if (query.Equals(""))
			{
				return;
			}

			var filteredRows = DataTable.Select(query);
			if (filteredRows.Length == 0)
			{
				DataTable dtTemp = DataTable.Clone();
				dtTemp.Clear();
				DataTable = dtTemp;
			}
			else
			{
				DataTable = filteredRows.CopyToDataTable();
			}
		}
		public async void EditAbonentData()
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					await connection.OpenAsync();
					using (SqlCommand command = connection.CreateCommand())
					{
						command.CommandText = "UPDATE Abonents SET Name = @first_name, LastName = @last_name, Patronymic = @patronymic, PhoneNumber = @phone_number, Tariff = @tariff WHERE Id = @id";
						command.Parameters.Add("@id", SqlDbType.Int, 4).Value = AbonentView.Id;
						command.Parameters.Add("@first_name", SqlDbType.NVarChar, 40).Value = AbonentView.Name;
						command.Parameters.Add("@last_name", SqlDbType.NVarChar, 40).Value = AbonentView.LastName;
						command.Parameters.Add("@patronymic", SqlDbType.NVarChar, 40).Value = AbonentView.Patronymic;
						command.Parameters.Add("@phone_number", SqlDbType.Text, 16).Value = AbonentView.PhoneNumber;
						command.Parameters.Add("@tariff", SqlDbType.Text, 16).Value = AbonentView.Tariff;
						var deleted = command.ExecuteNonQueryAsync();
						Console.WriteLine($"Изменено {deleted.Result}");
					}
					connection.Close();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
		// Удаление записи из таблицы
		public async void DeleteAbonent(int id)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					await connection.OpenAsync();
					using (SqlCommand command = connection.CreateCommand())
					{
						command.CommandText = "DELETE FROM Abonents WHERE Id = @id";
						command.Parameters.AddWithValue("@id", id);
						var deleted = command.ExecuteNonQueryAsync();
						Console.WriteLine($"Удалено {deleted.Result}");
					}
					connection.Close();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
		// Полная очистка таблицы
		public async void ClearTable()
		{
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				await connection.OpenAsync();
				SqlCommand command = new SqlCommand();
				// Очистка таблицы
				command.CommandText = "DELETE FROM Abonents ";
				command.Connection = connection;
				command.ExecuteNonQuery();
				// Обнуление id
				command.CommandText = "DBCC CHECKIDENT ('Abonents', RESEED, 0) ";
				await command.ExecuteNonQueryAsync();
				connection.Close();
			}
		}
		// Собитие для обновление привязок
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
	public class DbStateToBoolean : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (((ConnectionState)value) != ConnectionState.Broken)
				return true;
			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (Boolean)value ? ConnectionState.Closed : ConnectionState.Broken;
		}

	}
	public class TariffConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var val = value as string;
			if (val == "МегаТариф") return 0;
			if (val == "Максимум") return 1;
			if (val == "VIP") return 2;
			if (val == "Премиум") return 3;
			return 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.ToString();
		}

	}
	public class BooleanToVisibility : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((bool)value)
			{
				if (parameter != null)
				{
					return (string)parameter == "1" ? Visibility.Collapsed : Visibility.Visible;
				}
				return Visibility.Visible;
			}
			if (parameter != null)
			{
				return (string)parameter == "1" ? Visibility.Visible : Visibility.Collapsed;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((Visibility)value == Visibility.Visible)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

	}
}