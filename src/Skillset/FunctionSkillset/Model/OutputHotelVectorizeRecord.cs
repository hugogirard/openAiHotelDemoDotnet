namespace FunctionSkillset.Model;

public record OutputHotelVectorizeRecord(string RecordId, 
                                         HotelVectorizeData Data, 
                                         List<OutputRecordMessage> Errors, 
                                         List<OutputRecordMessage> Warnings);

public record HotelVectorizeData(IList<int> HotelNameVector, IList<int> DescriptionVector);

public record OutputRecordMessage(string Message);