namespace FunctionSkillset.Model;

public record VectorResponse(List<Data> data);

public record Data(IList<double> embedding);
