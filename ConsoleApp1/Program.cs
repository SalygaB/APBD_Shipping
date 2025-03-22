namespace ConsoleApp1;

public interface IHazardNotifier
{
    void NotifyHazard(string message);
}

public abstract class Container
{
    private static int _idCounter = 1;
    public string SerialNumber { get; }
    public double MaxPayload { get; }
    public double CurrentLoad { get; protected set; }
    public double BaseWeight { get; set; }
    public double TotalWeight { get; set; }

    protected Container(string type, double maxPayload)
    {
        SerialNumber = $"KON-{type}-{_idCounter++}";
        MaxPayload = maxPayload;
        CurrentLoad = 0;
    }

    public virtual void Load(double weight)
    {
        if (CurrentLoad + weight > MaxPayload)
            throw new Exception("OverfillException: Exceeded max payload!");
        
        CurrentLoad += weight;
    }

    public virtual void Unload()
    {
        CurrentLoad = 0;
    }

    public override string ToString()
    {
        BaseWeight = MaxPayload / 10;
        TotalWeight = BaseWeight + CurrentLoad;
        return $"Container {SerialNumber}, Load: {CurrentLoad}/{MaxPayload}, BaseWeight: {BaseWeight}, TotalWeight: {TotalWeight}";
    }
}

public class LiquidContainer : Container, IHazardNotifier
{
    private bool _isHazardous;

    public LiquidContainer(double maxPayload, bool isHazardous) : base("L", maxPayload)
    {
        _isHazardous = isHazardous;
    }

    public override void Load(double weight)
    {
        double limit = _isHazardous ? MaxPayload * 0.5 : MaxPayload * 0.9;
        if (CurrentLoad + weight > limit)
            NotifyHazard("Attempted overfill in hazardous conditions.");
        else
            base.Load(weight);
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"[HAZARD] {message} - {SerialNumber}");
    }
}

public class GasContainer : Container, IHazardNotifier
{
    public double Pressure { get; }

    public GasContainer(double maxPayload, double pressure) : base("G", maxPayload)
    {
        Pressure = pressure;
    }

    public override void Unload()
    {
        CurrentLoad *= 0.05; // Leaves 5%
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"[HAZARD] {message} - {SerialNumber}");
    }
}

public class RefrigeratedContainer : Container
{
    public string ProductType { get; }
    public double Temperature { get; }

    public RefrigeratedContainer(double maxPayload, string productType, double temperature) : base("C", maxPayload)
    {
        ProductType = productType;
        Temperature = temperature;
    }
}

public class ContainerShip
{
    public string Name { get; }
    public int MaxContainers { get; }
    public double MaxWeight { get; }
    public List<Container> Containers { get; }
    public double SumWeight { get; set; }

    public ContainerShip(string name, int maxContainers, double maxWeight)
    {
        Name = name;
        MaxContainers = maxContainers;
        MaxWeight = maxWeight;
        Containers = new List<Container>();
    }

    public void LoadContainer(Container container)
    {
        if (Containers.Count >= MaxContainers || GetTotalWeight() + container.TotalWeight > MaxWeight)
            throw new Exception("Ship cannot take more containers or weight exceeded!");
        
        Containers.Add(container);
        SumWeight += container.TotalWeight;
    }

    public void RemoveContainer(Container container)
    {
        Containers.Remove(container);
        SumWeight -= container.TotalWeight;
    }

    private double GetTotalWeight()
    {
        double weight = 0;
        foreach (var container in Containers)
            weight += container.TotalWeight;
        return weight;
    }

    public void PrintShipInfo()
    {
        Console.WriteLine($"Ship: {Name}, Max Containers: {MaxContainers}, Max Weight: {MaxWeight} Tons,");
        foreach (var container in Containers)
            Console.WriteLine(container);
        Console.WriteLine($"SHIP: {Name}: Current Payload: {GetTotalWeight()} Tons, Capacity: {GetTotalWeight() / MaxWeight * 100:F2}%");
    }
}

class Program
{
    static void Main()
    {
        var liq1 = new LiquidContainer(200, true);
        var liq2 = new LiquidContainer(200, true);
        var liq3 = new LiquidContainer(200, true);
        var liq4 = new LiquidContainer(200, true);
        var liq5 = new LiquidContainer(200, true);
        
        var ship1 = new ContainerShip("Bismark", 5, 3000);
        
        liq1.Load(100);
        liq2.Load(100);
        liq3.Load(100);
        liq4.Load(100);
        liq5.Load(100);
        
        liq1.Unload();
        liq2.Unload();
        liq3.Unload();
        
        ship1.LoadContainer(liq1);
        ship1.LoadContainer(liq2);
        ship1.LoadContainer(liq3);
        ship1.PrintShipInfo();
        
        Console.WriteLine("\n");
        
        var ship2 = new ContainerShip("Voyager", 10, 500);
        var gasContainer = new GasContainer(100, 5);
        var liquidContainer = new LiquidContainer(200, true);
        var fridgeContainer = new RefrigeratedContainer(150, "Bananas", -5);
        
        ship2.LoadContainer(gasContainer);
        ship2.LoadContainer(liquidContainer);
        ship2.LoadContainer(fridgeContainer);
        
        liquidContainer.Load(90);
        gasContainer.Load(80);
        fridgeContainer.Load(140);
        
        ship2.PrintShipInfo();
    }
}