using System.Reflection;
using System.Text;
using Information.DbDocumentation.XmlDocExtractor;

namespace DocCreationHelper;

public class AsciiDocTableGenerator : ITableGenerator
{
    private readonly XmlDocExtractor _xmlDocExtractor;

    public AsciiDocTableGenerator(XmlDocExtractor xmlDocExtractor)
    {
        this._xmlDocExtractor = xmlDocExtractor ?? throw new ArgumentNullException(nameof(xmlDocExtractor));
    }

    public string GenerateTable(object parseObject)
    {
        var fullPathClass = parseObject.GetType();
        var outputBuilder = new StringBuilder();

        // Имя класса и его описание из XML-документации
        var className = fullPathClass.Name;
        var classDescription = _xmlDocExtractor.GetClassSummary(fullPathClass.FullName);

        // Выводим имя класса и его описание в формате AsciiDoc
        outputBuilder.AppendLine($"*Class*: {className}");
        outputBuilder.AppendLine(); // Пустая строка для разделения
        outputBuilder.AppendLine($"*Description*: {classDescription}");
        outputBuilder.AppendLine(); // Пустая строка для разделения

        // Заголовок таблицы AsciiDoc
        outputBuilder.AppendLine("[options=\"header\"]");
        outputBuilder.AppendLine("|===");

        // Заголовок таблицы свойств
        outputBuilder.AppendLine("| Field | Type | isNullable | Description");

        foreach (var prop in fullPathClass.GetProperties(BindingFlags.Public | BindingFlags.Instance |
                                                         BindingFlags.GetProperty))
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
            var propertyDescription = _xmlDocExtractor.GetPropertySummary(fullPath);

            // Добавляем строку с информацией о свойстве в таблицу
            outputBuilder.AppendLine(
                $"| {name} | {propertyTypeName} | {propertyIsNullable} | {propertyDescription}");
        }

        // Закрываем таблицу AsciiDoc
        outputBuilder.AppendLine("|===");

        return outputBuilder.ToString();
    }

    public string GenerateTableFromYaml(string yaml)
    {
        throw new NotImplementedException();
    }
}