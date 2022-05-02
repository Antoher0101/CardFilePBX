using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardFilePBX
{
	class Cell<T>
	{
		public T Value { get; set; }
		public Cell<T> Next { get; set; }
		public Cell<T> Prev { get; set; }
		public Cell()
		{

		}
		public Cell(T Value)
		{
			this.Value = Value;
		}
	}
	class LinkedList<T> : IEnumerable<T>
	{
		public Cell<T> CurrentCell { get; set; }

		// Always empty cell
		public Cell<T> FirstCell { get; set; }
		public Cell<T> LastCell { get; set; }
		public int Count { get; private set; }
		public LinkedList()
		{
			FirstCell = new Cell<T>();
			LastCell = new Cell<T>();
			FirstCell.Next = new Cell<T>();
			CurrentCell = FirstCell;
		}
		public Cell<T> FindCell(T Value)
		{
			while (CurrentCell.Next != null && (object)CurrentCell.Next.Value != (object)Value)
			{
				CurrentCell = CurrentCell.Next;
			}
			if (CurrentCell.Next == null)
			{
				Console.WriteLine("Cell not found");
				return null;
			}
			return CurrentCell.Next;
		}
		public void Clear()
		{
			while (CurrentCell != LastCell.Next)
			{
				var next = CurrentCell.Next;
				CurrentCell.Next = null;
				CurrentCell.Prev = null;
				CurrentCell = next;
			}
			FirstCell.Next = LastCell;
			LastCell.Prev = FirstCell;
			CurrentCell = FirstCell;
			Count = 0;
		}
		public void AddFirst(T Value)
		{
			var newCell = new Cell<T>(Value);
			newCell.Next = FirstCell.Next;
			FirstCell.Next = newCell;

			newCell.Next.Prev = newCell;
			newCell.Prev = FirstCell;
			if (newCell.Next.Next == null)
			{
				LastCell.Prev = newCell;
			}
			CurrentCell = FirstCell;

			Count++;
		}
		public void AddBack(T Value)
		{
			var newCell = new Cell<T>(Value);
			if (LastCell.Prev == null)
			{
				AddFirst(Value);
				return;
			}
			newCell.Prev = LastCell.Prev;
			LastCell.Prev = newCell;

			newCell.Prev.Next = newCell;
			newCell.Next = LastCell;
			CurrentCell = FirstCell;

			Count++;
		}
		public void DeleteCell(T Value)
		{
			while (CurrentCell.Next != null && (object)CurrentCell.Next.Value != (object)Value)
			{
				CurrentCell = CurrentCell.Next;
			}
			if (CurrentCell.Next == null)
			{
				Console.WriteLine("Cell not found");
				return;
			}
			CurrentCell.Next.Next.Prev = CurrentCell;
			CurrentCell.Next = CurrentCell.Next.Next;

			CurrentCell = FirstCell;

			Count--;
		}
		public void Fill(DataTable table)
		{
			DataColumn column;
			DataRow row;
			string[] columnsName;

			Abonent template = new Abonent(0,"", "", "", "", "");

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
			}
			while (CurrentCell != LastCell.Next)
			{
				if (CurrentCell.Value is Abonent value)
				{
					row = table.NewRow();
					row[nameof(template.Id)] = value.Id;
					row[nameof(template.Name)] = value.Name;
					row[nameof(template.LastName)] = value.LastName;
					row[nameof(template.Patronymic)] = value.Patronymic;
					row[nameof(template.PhoneNumber)] = value.PhoneNumber;
					row[nameof(template.Tariff)] = value.Tariff;
					table.Rows.Add(row);
				}
				CurrentCell = CurrentCell.Next;
			}
			CurrentCell = FirstCell;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)this).GetEnumerator();
		}
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			Cell<T> current = CurrentCell;
			while (current != null)
			{
				yield return current.Value;
				current = current.Next;
			}
		}
	}
}
