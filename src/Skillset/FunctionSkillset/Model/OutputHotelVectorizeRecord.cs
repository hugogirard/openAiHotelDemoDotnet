namespace FunctionSkillset.Model;

public record OutputHotelVectorizeRecord(string RecordId, 
                                         HotelVectorizeData Data, 
                                         List<OutputRecordMessage> Errors, 
                                         List<OutputRecordMessage> Warnings);

public record HotelVectorizeData(IList<double> HotelNameVector, IList<double> DescriptionVector);

public record OutputRecordMessage(string Message);