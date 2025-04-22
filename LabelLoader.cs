using System.IO;
using System.Text.Json;

// SON 읽는 유틸리티 함수 만들기
public static class LabelLoader
{
    public static LabelConfig LoadLabelConfig(string path)
{
    if (!File.Exists(path))
        throw new FileNotFoundException("Label config file not found", path);

    var json = File.ReadAllText(path);
    var config = JsonSerializer.Deserialize<LabelConfig>(json);

    return config ?? throw new InvalidDataException("Failed to parse labels.json");
}

}