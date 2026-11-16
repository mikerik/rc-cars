using RcCars.Cars;
using RcCars.Domain;

namespace RcCars.Tests.Cars;

public class MonsterTruckTests
{
    [Fact]
    public void NewTruck_StartsAtGivenPose()
    {
        var start = new CarPose(new Position(2, 3), Heading.North);

        var truck = new MonsterTruck(start);

        Assert.Equal(start, truck.Pose);
        Assert.Equal("Monster Truck", truck.Model);
    }

    [Fact]
    public void MoveTo_UpdatesPose()
    {
        var truck = new MonsterTruck(new CarPose(new Position(0, 0), Heading.North));
        var next = new CarPose(new Position(0, 1), Heading.North);

        truck.MoveTo(next);

        Assert.Equal(next, truck.Pose);
    }
}
