using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.ComponentModel;
using System.Globalization;

namespace CardFilePBX
{
	public class CardFileSettings : INotifyPropertyChanged
	{
		private LastFile _lastFile;
		private Tariffs _tariffs;
		private Date _date;
		public static CardFileSettings FromJson(string json) => JsonConvert.DeserializeObject<CardFileSettings>(json, CardFilePBX.Converter.Settings);

		[JsonProperty("lastFile")]
		public LastFile LastFile
		{
			get => _lastFile;
			set
			{
				_lastFile = value;
				OnPropertyChanged(nameof(LastFile));
			}
		}

		[JsonProperty("tariffs")]
		public Tariffs Tariffs
		{
			get => _tariffs;
			set
			{
				_tariffs = value;
				OnPropertyChanged(nameof(Tariffs));
			}
		}

		[JsonProperty("date")]
		public Date Date
		{
			get => _date;
			set
			{
				_date = value;
				OnPropertyChanged(nameof(Date));
			}
		}

		private CardFileSettings() { }

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
	public partial class Date
	{
		[JsonProperty("currentPeriod")]
		public string CurrentPeriod { get; set; }
	}

	public partial class LastFile
	{
		[JsonProperty("path")]
		public string Path { get; set; }
	}

	public partial class Tariffs
	{
		[JsonProperty("mega")]
		public Tariff Mega { get; set; }

		[JsonProperty("maximum")]
		public Tariff Maximum { get; set; }

		[JsonProperty("vip")]
		public Tariff Vip { get; set; }

		[JsonProperty("premium")]
		public Tariff Premium { get; set; }

		[JsonProperty("bonus")]
		public Tariff Bonus { get; set; }
	}

	public partial class Tariff
	{
		[JsonProperty("outgoing")]
		public long Outgoing { get; set; }

		[JsonProperty("incoming")]
		public long Incoming { get; set; }
	}
	public static class Serialize
	{
		public static string ToJson(this CardFileSettings self) => JsonConvert.SerializeObject(self, CardFilePBX.Converter.Settings);
	}

	internal static class Converter
	{
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			Converters =
			{
				new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
			},
			Formatting = Formatting.Indented,
		};
	}
}
