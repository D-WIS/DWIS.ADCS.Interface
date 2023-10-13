using System.Text.Json.Serialization;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Json;

namespace ADCS.Interface.Share;

public static class ConsoleExt
{
	public static JsonText GetJsonText(object obj)
	{
		var serializerOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			Converters = { new JsonStringEnumConverter() }
		};

		var json = JsonSerializer.Serialize(obj, serializerOptions);

		return new JsonText(json)
			.BracesColor(Color.Red)
			.BracketColor(Color.Green)
			.ColonColor(Color.Blue)
			.CommaColor(Color.Red)
			.StringColor(Color.Green)
			.NumberColor(Color.Blue)
			.BooleanColor(Color.Red)
			.NullColor(Color.Green);

	}
}