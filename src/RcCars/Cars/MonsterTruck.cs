using RcCars.Domain;

namespace RcCars.Cars;

public sealed class MonsterTruck : RcCar
{
    public MonsterTruck(CarPose startPose)
        : base(startPose)
    {
    }

    public override string Model => "Monster Truck";
}
