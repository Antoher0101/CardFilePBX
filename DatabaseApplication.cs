using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;

namespace CardFilePBX
{
    class DatabaseApplication
    {
        public SqlConnection connection { get; set; }
        public DataView dataView { get; set; }
        private DataTable table = new DataTable();
        public ConnectionState State { get; set; }
        public DatabaseApplication()
        {
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["AbonentsDB"].ConnectionString);
        }
        public void Open()
        {
            try
            {
                connection.Open();
                State = ConnectionState.Open;
                Console.WriteLine("База данных успешно подключена.");
            }
            catch
            {
                Console.WriteLine("Не удалось подключиться к базе данных.");
            }
        }
        public void Close()
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
                State = ConnectionState.Closed;
            }
        }
        public void UpdateTable()
		{
            var adapter = new SqlDataAdapter("SELECT * FROM Abonents", connection);
            table.Clear();
            adapter.Fill(table);

            dataView = table.DefaultView;
        }
        public void AddAbonent(int id, string first_name, string last_name, string patronymic, string phone_number)
        {
            string query = $"INSERT INTO Abonents (first_name, last_name, patronymic, phone_number) VALUES (@first_name, @last_name, @patronymic, @phone_number)";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@first_name", SqlDbType.Text, 16).Value = first_name;
                command.Parameters.Add("@last_name", SqlDbType.Text, 16).Value = last_name;
                command.Parameters.Add("@patronymic", SqlDbType.Text, 16).Value = patronymic;
                command.Parameters.Add("@phone_number", SqlDbType.Text, 16).Value = phone_number;

                command.ExecuteNonQuery();
            }
        }

        public void RandQuery()
        {
            string query = $"INSERT T(F) VALUES(RAND()); GO 10000";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        // Полная очистка таблицы
        public void ClearTable()
		{
            SqlCommand command = new SqlCommand();
            // Очистка таблицы
            command.CommandText = "DELETE FROM Abonents ";
            command.Connection = connection;
            command.ExecuteNonQuery();
            // Обнуление id
            command.CommandText = "DBCC CHECKIDENT ('Abonents', RESEED, 0) ";
            command.ExecuteNonQuery();
        }
    }

    public class DbStateToBoolean : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (((ConnectionState)value) == ConnectionState.Open)
                return true;
            return false;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Boolean)value ? ConnectionState.Open : ConnectionState.Closed;
        }

    }
}