using System;
using System.Collections;
using System.Collections.Generic;

namespace CardFilePBX
{
	public class Cell<T>
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
	public class LinkedList<T> : IEnumerable<T>
	{
		public Cell<T> CurrentCell { get; set; }
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
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)this).GetEnumerator();
		}
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			Cell<T> current = CurrentCell.Next;
			while (current.Next != null)
			{
				yield return current.Value;
				current = current.Next;
			}
		}
	}
}
