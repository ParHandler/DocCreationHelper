using System.Reflection;
using System.Text;
using Information.DbDocumentation.XmlDocExtractor;

namespace DocCreationHelper;

public class AsciiDocTableGenerator : ITableGenerator
{
    private readonly XmlDocExtractor xmlDocExtractor;

    public AsciiDocTableGenerator(XmlDocExtractor xmlDocExtractor)
    {
        this.xmlDocExtractor = xmlDocExtractor ?? throw new ArgumentNullException(nameof(xmlDocExtractor));
    }

    public string GenerateTable(object parseObject)
    {
        var fullPathClass = parseObject.GetType();
        var outputBuilder = new StringBuilder();

        // Заголовок таблицы AsciiDoc
        outputBuilder.AppendLine("[options=\"header\"]");
        outputBuilder.AppendLine("|===");
        outputBuilder.AppendLine("| Field | Type | isNullable | Description |");

        foreach (var prop in fullPathClass.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty))
        {
            var fullPath = $"{fullPathClass.FullName}.{prop.Name}";
            var propertyTypeName = $"{prop.PropertyType.Name}";
            var propertyIsNullable = "";

            if (prop.PropertyType.IsGenericType &&
                prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyTypeName = $"{prop.PropertyType.GetGenericArguments()[0]}";
                propertyIsNullable = "Yes";
            }

            var name = prop.Name;

            // Используем xmlDocExtractor для получения описания свойства из XML-документации
            var propertyDescription = xmlDocExtractor.GetPropertySummary(fullPath);

            // Добавляем строку с информацией о свойстве в таблицу
            outputBuilder.AppendLine($"| {name} | {propertyTypeName} | {propertyIsNullable} | {propertyDescription} |");
        }

        // Закрываем таблицу AsciiDoc
        outputBuilder.AppendLine("|===");

        return outputBuilder.ToString();
    }
}