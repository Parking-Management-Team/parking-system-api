using PBMS.Domain.Entities;

namespace PBMS.UnitTests;

public class VehicleTypeTests
{
    [Fact]
    public void VehicleType_HasPhysicalModelAttributes()
    {
        var vehicleType = new VehicleType
        {
            Id = 1,
            Name = VehicleType.MotorcycleTypeName,
            Description = "Motorcycle parking type",
            VehicleTypeStatus = VehicleType.StatusActive
        };

        Assert.Equal(1, vehicleType.Id);
        Assert.Equal(VehicleType.MotorcycleTypeName, vehicleType.Name);
        Assert.Equal("Motorcycle parking type", vehicleType.Description);
        Assert.Equal(VehicleType.StatusActive, vehicleType.VehicleTypeStatus);
    }
}
