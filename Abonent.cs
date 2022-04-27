namespace CardFilePBX
{
	class Abonent
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string LastName { get; set; }
		public string Patronymic { get; set; }
		public string PhoneNumber { get; set; }
		public string Tariff { get; set; }
		public string Photo { get; set; }

		public Abonent(int id, string name, string lastName, string patronymic, string phoneNumber, string tariff, string PhotoUri = null)
		{
			Id = id;
			Name = name;
			LastName = lastName;
			Patronymic = patronymic;
			PhoneNumber = phoneNumber;
			Tariff = tariff;
			Photo = PhotoUri;
		}
	}

}
