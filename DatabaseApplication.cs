using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Text.RegularExpressions;


namespace CardFilePBX
{
	class DatabaseApplication : INotifyPropertyChanged
	{
		public SqlConnection Connection { get; set; }

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
			Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["AbonentsDB"].ConnectionString);
			DataTable = new DataTable();
		}
		public bool Open()
		{
			try
			{
				Connection.Open();
				State = ConnectionState.Open;
				return true;
			}
			catch
			{
				Console.WriteLine("Не удалось подключиться к базе данных.");
				State = ConnectionState.Broken;
				return false;
			}
		}
		public void Close()
		{
			if (Connection != null && Connection.State == ConnectionState.Open)
			{
				Connection.Close();
				State = ConnectionState.Closed;
			}
		}
		public void UpdateTable()
		{
			var adapter = new SqlDataAdapter("SELECT * FROM Abonents", Connection);
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
		}
		// Добавление записи в таблицу
		public void AddAbonent(string first_name, string last_name, string patronymic, string phone_number, string tariff)
		{
			string query = $"INSERT INTO Abonents (first_name, last_name, patronymic, phone_number, tariff) VALUES (@first_name, @last_name, @patronymic, @phone_number, @tariff)";
			using (SqlCommand command = new SqlCommand(query, Connection))
			{
				command.Parameters.Add("@first_name", SqlDbType.NVarChar, 40).Value = first_name;
				command.Parameters.Add("@last_name", SqlDbType.NVarChar, 40).Value = last_name;
				command.Parameters.Add("@patronymic", SqlDbType.NVarChar, 40).Value = patronymic;
				command.Parameters.Add("@phone_number", SqlDbType.Text, 16).Value = phone_number;
				command.Parameters.Add("@tariff", SqlDbType.Text, 16).Value = tariff;
				command.ExecuteNonQuery();
			}
		}
		// Удаление записи из таблицы
		public void DeleteAbonent(int id)
		{
			try
			{
				using (SqlCommand command = Connection.CreateCommand())
				{
					command.CommandText = "DELETE FROM Abonents WHERE Id = @id";
					command.Parameters.AddWithValue("@id", id);
					var deleted = command.ExecuteNonQuery();
					Console.WriteLine($"Удалено {deleted}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
		// Полная очистка таблицы
		public void ClearTable()
		{
			SqlCommand command = new SqlCommand();
			// Очистка таблицы
			command.CommandText = "DELETE FROM Abonents ";
			command.Connection = Connection;
			command.ExecuteNonQuery();
			// Обнуление id
			command.CommandText = "DBCC CHECKIDENT ('Abonents', RESEED, 0) ";
			command.ExecuteNonQuery();
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
}