public interface IDataFormatter
{
    List<CarInfoSelectorsConfig> Items { get; set; }
    bool Reprocess { get; set; }
    string Date { get; set; }

    void FormatData(string path);
}
