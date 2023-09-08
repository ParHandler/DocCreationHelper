using Newtonsoft.Json;

namespace DocCreationHelper;

public static class DocCreation
{
    public static string WriteTestOutputHelperStructure(object parseObject, ITableGenerator tableGenerator)
    {
        return tableGenerator.GenerateTable(parseObject);
    }
    
    public static string WriteTestOutputHelperExample(object parseObject)
    {
        var objectMap = new Dictionary<string, object> { { parseObject.GetType().Name, parseObject } };
        var outputHelper = $@"**payload**";
        foreach (var kvp in objectMap)
        {
            outputHelper = outputHelper + $@"{JsonConvert.SerializeObject(
                kvp,
                new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    NullValueHandling = NullValueHandling.Ignore,
                })}";
        }

        return outputHelper;
    }
}