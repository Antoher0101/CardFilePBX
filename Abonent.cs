namespace CardFilePBX
{
	public class Abonent
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string LastName { get; set; }
		public string Patronymic { get; set; }
		public string PhoneNumber { get; set; }
		public string Tariff { get; set; }
		public int Outgoing { get; set; }
		public int Incoming { get; set; }
		public string Photo { get; set; }

		public Abonent(int id, string name, string lastName, string patronymic, string phoneNumber, string tariff, int outgoing, int incoming)
		{
			Id = id;
			Name = name;
			LastName = lastName;
			Patronymic = patronymic;
			PhoneNumber = phoneNumber;
			Tariff = tariff;
			Outgoing = outgoing;
			Incoming = incoming;
		}
	}

}
