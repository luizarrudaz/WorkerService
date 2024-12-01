public class CarInfoSelectorsConfig : ICarConfig
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Price { get; set; }
    public string Year { get; set; }
    public string Origin { get; set; }

    public CarInfoSelectorsConfig() { }
}
