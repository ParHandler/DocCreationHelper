using Newtonsoft.Json;

namespace DocCreationHelper;

public static class DocCreation
{
    public static string WriteTestOutputHelperStructure(object parseObject, ITableGenerator tableGenerator)
    {
        return tableGenerator.GenerateTable(parseObject);
    }
    
    public static string WriteTestOutputHelperStructureFromYaml(string yaml, ITableGenerator tableGenerator)
    {
        return tableGenerator.GenerateTableFromYaml(yaml);
    }
    
    public static string WriteTestOutputHelperExample(object parseObject)
    {
        var objectMap = new Dictionary<string, object> { { parseObject.GetType().Name, parseObject } };
        var outputHelper = $@":prewrap!:
.payload
[%collapsible]
====
[source,json#, linenums, indent=0]
----
";
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
        outputHelper = outputHelper + $@"
----
====";
        return outputHelper;
    }
}