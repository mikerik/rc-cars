using RcCars.Domain;

namespace RcCars.Cars;

/// <summary>
/// Base class for all simulated radio controlled cars.
/// New car types (e.g. Off-road, Sport track) derive from this class.
/// </summary>
public abstract class RcCar
{
    protected RcCar(CarPose startPose)
    {
        Pose = startPose;
    }

    /// <summary>Human readable model name, e.g. "Monster Truck".</summary>
    public abstract string Model { get; }

    /// <summary>The current position and heading of the car.</summary>
    public CarPose Pose { get; private set; }

    /// <summary>Places the car at a new pose. Validating the move is the responsibility of the caller.</summary>
    public void MoveTo(CarPose pose) => Pose = pose;
}
