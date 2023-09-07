using System.Reflection;
using Information.DbDocumentation.XmlDocExtractor;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace MycAuthServer.DocCreationHelper;

public static class DocCreation
{
    public static string WriteTestOutputHelperStructure(object parseObject)
    {
        var xmlDocExtractor = new XmlDocExtractor(new[] { $"myc-auth-server.xml" });

        var fullPathClass = parseObject.GetType();
        var outputHelper =
            $"Type: {fullPathClass.Name}{Environment.NewLine}Description: {xmlDocExtractor.GetClassSummary(fullPathClass.FullName)}{Environment.NewLine}" +
            Environment.NewLine + $"| Field | Type | isNullable | Description |" + Environment.NewLine +
            "| --- | --- | --- | --- |" + Environment.NewLine;
        foreach (var prop in fullPathClass
                     .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty))
        {
            var fullPath = $"{fullPathClass.FullName}.{prop.Name}";
            var propertyTypeName = $"{prop.PropertyType.Name}";
            var propertyIsNullable = $"";

            if (prop.PropertyType.IsGenericType &&
                prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // If it is NULLABLE, then get the underlying type. eg if "Nullable<int>" then this will return just "int"
                propertyTypeName = $"{prop.PropertyType.GetGenericArguments()[0]}";
                propertyIsNullable = $"Yes";
            }

            var name = prop.Name;
            outputHelper = outputHelper +
                           $"| {name} | {propertyTypeName} | {propertyIsNullable} | {xmlDocExtractor.GetPropertySummary(fullPath)} |" +
                           Environment.NewLine;
        }

        return outputHelper;
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