using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CardFilePBX
{
	class DataListApplication : INotifyPropertyChanged
	{
		private LinkedList<Abonent> list;
		public string connectionString { get; set; }
		private StreamReader sr;
		public DataListApplication(string connectionString)
		{
			AbonentView = null;
			DataTable = new DataTable();
			this.connectionString = connectionString;
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
		public void Open()
		{
			sr = new StreamReader(connectionString);
		}
		public void Close()
		{
			sr.Close();
		}

		public async void EditAbonentData()
		{
		}

		public async void AddAbonent(string first_name, string last_name, string patronymic, string phone_number,
			string tariff)
		{
			string abonent = $"{list.Count + 1},{first_name},{last_name},{patronymic},{phone_number},{tariff}";
			StreamWriter sw = new StreamWriter(connectionString, true);
			await sw.WriteLineAsync(abonent); 
			sw.Close();

		}
		// Удаление записи из таблицы
		public async void DeleteAbonent(int id)
		{
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
		public void ReadDatabase()
		{
			list.Clear();
			try
			{
				Open();
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
						Abonent a = new Abonent(id, fname, lname, patronymic, pn, tf);
						list.AddBack(a);
					}
				}
				list.CurrentCell = list.FirstCell;
				Close();
			}
			catch (Exception e)
			{
				Console.WriteLine("Ошибка чтения базы данных: " + e);
				Close();
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
					string line = $"{list.CurrentCell.Value.Id},{list.CurrentCell.Value.Name},{list.CurrentCell.Value.LastName},{list.CurrentCell.Value.Patronymic},{list.CurrentCell.Value.PhoneNumber},{list.CurrentCell.Value.Tariff}";
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
		public bool CheckDB()
		{
			try
			{
				Open();
				Close();
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}
		}
		// Собитие для обновление привязок
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}