using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace CardFilePBX
{
	class DataListApplication : INotifyPropertyChanged
	{
		private CardFileSettings settings;
		private LinkedList<Abonent> list;
		public string connectionString { get; private set; }
		public int GUID { get; set; }
		private StreamReader sr;
		public DataListApplication(string connectionString = null)
		{
			settings = GetSettings();
			settings.PropertyChanged += JsonPropertyChanged;
			this.connectionString = connectionString ?? settings.LastFile.Path;
			AbonentView = null;
			DataTable = new DataTable();
			list = new LinkedList<Abonent>();
		}

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
		public bool Open()
		{
			try
			{
				sr = new StreamReader(connectionString);
				return true;
			}
			catch (Exception e)
			{
				Close();
				return false;
			}
		}
		public void Close()
		{
			if (sr != null)
			{
				sr.Close();
			}
		}
		private void JsonPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			StreamWriter jsonWriter = new StreamWriter("settings.json");
			string json = this.settings.ToJson();
			jsonWriter.Write(json);
			jsonWriter.Close();
		}
		private CardFileSettings GetSettings()
		{
			StreamReader jsonReader = new StreamReader("settings.json");
			string json = jsonReader.ReadToEnd();
			jsonReader.Close();
			return CardFileSettings.FromJson(json);
		}
		public void SetConnectionString(string connection)
		{
			this.connectionString = connection;
			settings.LastFile = new LastFile() { Path = connection };
		}
		public async void EditAbonentData()
		{
			if (isDuplicate(AbonentView.PhoneNumber))
			{
				MessageBox.Show("Данный номер телефона уже присутствует в базе данных.", "Дубликат номера телефона", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}
			try
			{
				if (Open())
				{
					List<string> table = new List<string>();
					while (!sr.EndOfStream)
					{
						var line = sr.ReadLine();
						if (line.Split(',')[0] == AbonentView.Id.ToString())
						{
							table.Add($"{AbonentView.Id},{NormalizeName(AbonentView.Name)},{NormalizeName(AbonentView.LastName)},{NormalizeName(AbonentView.Patronymic)},{AbonentView.PhoneNumber},{TariffToInt.Convert(AbonentView.Tariff)},{AbonentView.Outgoing},{AbonentView.Incoming}");
						}
						else table.Add(line);
					}
					Close();
					StreamWriter sw = new StreamWriter(connectionString, false);
					foreach (var abonent in table)
					{
						await sw.WriteLineAsync(abonent);
					}
					sw.Close();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Close();
			}
		}

		public async void AddAbonent(string first_name, string last_name, string patronymic, string phone_number,
			string tariff, int og, int ic)
		{
			if (isDuplicate(phone_number))
			{
				MessageBox.Show("Данный номер телефона уже присутствует в базе данных.", "Дубликат номера телефона", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}

			string abonent = $"{++GUID},{NormalizeName(first_name)},{NormalizeName(last_name)},{NormalizeName(patronymic)},{phone_number},{tariff},{og},{ic}";
			try
			{
				StreamWriter sw = new StreamWriter(connectionString, true);
				await sw.WriteLineAsync(abonent);
				sw.Close();

				// Обновление GUID
				FileStream fs = new FileStream(connectionString, FileMode.Open);
				fs.Position = 0;
				byte[] guid = new byte[10];
				Encoding.UTF8.GetBytes(GUID.ToString()).CopyTo(guid, 0);
				fs.Write(guid, 0, 10);
				fs.Close();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Ошибка записи файла", MessageBoxButton.OK, MessageBoxImage.Error);
			}

		}
		// Удаление записи из таблицы
		public async void DeleteAbonent(int id)
		{
			try
			{
				if (Open())
				{
					List<string> table = new List<string>();
					while (!sr.EndOfStream)
					{
						var line = sr.ReadLine();
						if (line.Split(',')[0] != AbonentView.Id.ToString())
						{
							table.Add(line);
						}
					}
					Close();
					StreamWriter sw = new StreamWriter(connectionString, false);
					foreach (var abonent in table)
					{
						await sw.WriteLineAsync(abonent);
					}
					sw.Close();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Close();
			}
		}
		// Полная очистка таблицы
		public async void ClearTable()
		{
			list.Clear();
			WriteDatabase();
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

		public async void UpdateTable()
		{
			try
			{
				ReadDatabase();
				DataTable.Clear();
				list.Fill(DataTable);

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
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
		// Имитация получения времени разговора абонента
		public static int GetTotalCallTime(Random rnd)
		{
			return rnd.Next(0, 1000);
		}
		private void ReadDatabase()
		{
			list.Clear();
			if (Open())
			{
				try
				{
					char[] r_guid = new char[12];
					sr.Read(r_guid, 0, 12);
					var t = r_guid.TakeWhile(c => { return (byte)c != 0 && c != 13; }).Select(Convert.ToByte).ToArray();
					GUID = Convert.ToInt32(Encoding.Default.GetString(t));

					while (!sr.EndOfStream)
					{
						string[] line = sr.ReadLine().Split(',');
						if (line[0] != string.Empty)
						{
							int id = int.Parse(line[0]);
							string fname = line[1];
							string lname = line[2];
							string patronymic = line[3];
							string pn = line[4];
							string tf = line[5];
							int og = int.Parse(line[6]);
							int ic = int.Parse(line[7]);
							Abonent a = new Abonent(id, fname, lname, patronymic, pn, tf, og, ic);
							list.AddBack(a);
						}
					}
					list.CurrentCell = list.FirstCell;
				}
				catch (Exception e)
				{
					MessageBox.Show("Файл базы данных поврежден.", "Ошибка чтения файла", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				finally { Close(); }

			}
		}
		public void WriteDatabase()
		{
			try
			{
				Close();
				StreamWriter sw = new StreamWriter(connectionString, false);
				while (list.CurrentCell != list.LastCell.Next && list.CurrentCell.Value != null)
				{
					string line = $"{list.CurrentCell.Value.Id},{list.CurrentCell.Value.Name},{list.CurrentCell.Value.LastName},{list.CurrentCell.Value.Patronymic},{list.CurrentCell.Value.PhoneNumber},{list.CurrentCell.Value.Tariff},{list.CurrentCell.Value.Outgoing},{list.CurrentCell.Value.Incoming}";
					sw.WriteLine(line);
				}
				list.CurrentCell = list.FirstCell;
				sw.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine("Ошибка записи базы данных: " + e);
			}
		}
		public void CreateDbFile(string filename)
		{
			try
			{
				GUID = 0;
				byte[] separator = { 13, 10 }; // \r\n bytes
				byte[] guid = new byte[12];
				Encoding.UTF8.GetBytes(GUID.ToString()).CopyTo(guid, 0);
				separator.CopyTo(guid, 10);
				File.WriteAllBytes(filename, guid);
				SetConnectionString(filename);
				UpdateTable();
			}
			catch (Exception e)
			{
				Console.WriteLine("Не удалось создать базу данных: " + e);
			}
		}
		public static string FindDbFiles()
		{
			var file = Directory.GetFiles("..\\..\\database\\", "*.db");
			return file.FirstOrDefault();
		}
		public static string NormalizeName(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return str;
			}
			var output = str.Trim();
			output = output.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture) + output.Substring(1);
			return output.Normalize();
		}
		public bool CheckDB()
		{
			try
			{
				return Open();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}
			finally { Close(); }
		}

		private bool isDuplicate(string num)
		{
			var dt = this.DataTable.Select("PhoneNumber LIKE '%" + num.Trim() + "%'");
			DataRow[] dr = null;
			if (AbonentView != null)
			{
				dr = DataTable.Select("Id = " + AbonentView.Id);
				return dt.Length > 0 && dt[0] != dr[0];
			}
			return dt.Length > 0;
		}
		// Собитие для обновление привязок
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public static class AbonentListExtension
	{
		public static void Fill(this LinkedList<Abonent> list, DataTable table)
		{
			DataColumn column;
			DataRow row;

			Abonent template = new Abonent(0, "", "", "", "", "", 0, 0);

			if (table.Columns.Count < 1)
			{
				// ИД
				column = new DataColumn();
				column.DataType = template.Id.GetType();
				column.ColumnName = nameof(template.Id);
				table.Columns.Add(column);
				// Имя
				column = new DataColumn();
				column.DataType = template.Name.GetType();
				column.ColumnName = nameof(template.Name);
				table.Columns.Add(column);
				// Фамилия
				column = new DataColumn();
				column.DataType = template.LastName.GetType();
				column.ColumnName = nameof(template.LastName);
				table.Columns.Add(column);
				// Отчество
				column = new DataColumn();
				column.DataType = template.Patronymic.GetType();
				column.ColumnName = nameof(template.Patronymic);
				table.Columns.Add(column);
				// Телефон
				column = new DataColumn();
				column.DataType = template.PhoneNumber.GetType();
				column.ColumnName = nameof(template.PhoneNumber);
				table.Columns.Add(column);
				// Тариф
				column = new DataColumn();
				column.DataType = template.Tariff.GetType();
				column.ColumnName = nameof(template.Tariff);
				table.Columns.Add(column);
				// Исходящие звонки
				column = new DataColumn();
				column.DataType = template.Outgoing.GetType();
				column.ColumnName = nameof(template.Outgoing);
				table.Columns.Add(column);
				// Входящие звонки
				column = new DataColumn();
				column.DataType = template.Incoming.GetType();
				column.ColumnName = nameof(template.Incoming);
				table.Columns.Add(column);
			}
			while (list.CurrentCell != list.LastCell.Next)
			{
				if (list.CurrentCell.Value is Abonent value)
				{
					row = table.NewRow();
					row[nameof(template.Id)] = value.Id;
					row[nameof(template.Name)] = value.Name;
					row[nameof(template.LastName)] = value.LastName;
					row[nameof(template.Patronymic)] = value.Patronymic;
					row[nameof(template.PhoneNumber)] = value.PhoneNumber;
					row[nameof(template.Tariff)] = value.Tariff;
					row[nameof(template.Outgoing)] = value.Outgoing;
					row[nameof(template.Incoming)] = value.Incoming;
					table.Rows.Add(row);
				}
				list.CurrentCell = list.CurrentCell.Next;
			}
			list.CurrentCell = list.FirstCell;
		}

	}
}