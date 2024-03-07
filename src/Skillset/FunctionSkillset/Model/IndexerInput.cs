namespace FunctionSkillset.Model;

public record IndexerInput(IEnumerable<InputDocument> Values);

public record InputDocument(string RecordId,string Data);

public record Hotel(string HotelName, string Description);