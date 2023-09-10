namespace DocCreationHelper;

public interface ITableGenerator
{
    string GenerateTable(object parseObject);

    string GenerateTableFromYaml(string yaml);
}