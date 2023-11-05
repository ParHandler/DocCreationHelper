using System.Reflection;
using System.Text;
using Information.DbDocumentation.XmlDocExtractor;

namespace DocCreationHelper;

public class MarkdownTableGenerator : ITableGenerator
{
    private readonly XmlDocExtractor _xmlDocExtractor;

    public MarkdownTableGenerator(XmlDocExtractor xmlDocExtractor)
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

        // Выводим имя класса и его описание в формате Markdown
        outputBuilder.AppendLine($"Type: {className}");
        outputBuilder.AppendLine($"Description: {classDescription}");
        outputBuilder.AppendLine(); // Пустая строка для разделения

        // Заголовок таблицы Markdown
        outputBuilder.AppendLine("| **Field** | **Type** | **isNullable** | **Description** |");
        //outputBuilder.AppendLine("| --- | --- | --- | --- |");

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


            // Split the propertyDescription into multiple lines with a maximum line length
            const int maxLineLength = 30; // Adjust the line length as needed
            var descriptionLines = SplitDescription(propertyDescription, maxLineLength);

            // Combine the description lines with line breaks
            var formattedDescription = string.Join($"\\n", descriptionLines);

            // Добавляем строку с информацией о свойстве в таблицу
            outputBuilder.AppendLine(
                $"| {name} | {propertyTypeName} | {propertyIsNullable} | {formattedDescription} |");
        }

        return outputBuilder.ToString();
    }

    public string GenerateTableFromYaml(string yaml)
    {
        throw new NotImplementedException();
    }

    // Helper method to split a description string into lines
    private IEnumerable<string> SplitDescription(string description, int maxLineLength)
    {
        var lines = new List<string>();
        var currentLine = new StringBuilder();

        foreach (var word in description.Split(' '))
        {
            if (currentLine.Length + word.Length + 1 <= maxLineLength)
            {
                if (currentLine.Length > 0)
                    currentLine.Append(' ');
                currentLine.Append(word);
            }
            else
            {
                lines.Add(currentLine.ToString());
                currentLine = new StringBuilder(word);
            }
        }

        if (currentLine.Length > 0)
            lines.Add(currentLine.ToString());

        return lines;
    }
}