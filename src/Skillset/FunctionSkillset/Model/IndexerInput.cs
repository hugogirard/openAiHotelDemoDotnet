namespace FunctionSkillset.Model;

public record IndexerInput(IEnumerable<InputDocument> Values);

public record InputDocument(string RecordId,Hotel Data);

public record Hotel(string HotelName, string Description);