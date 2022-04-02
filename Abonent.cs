using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardFilePBX
{
	class Abonent
	{
		public int Id { get; }
		public string Name { get; set; }
		public string LastName { get; }
		public string Patronymic { get; }
		public string PhoneNumber { get; }
		public string Tariff { get; }
		public string Photo { get; }

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
