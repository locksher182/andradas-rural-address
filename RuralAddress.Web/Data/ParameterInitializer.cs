using RuralAddress.Core.Entities;
using RuralAddress.Infrastructure.Data;

namespace RuralAddress.Web.Data;

public static class ParameterInitializer
{
    public static async Task SeedData(AppDbContext context)
    {
        if (!context.SystemParameters.Any())
        {
            var vehicleTypes = new List<SystemParameter>
            {
                new SystemParameter { Group = "VehicleType", Name = "Carro" },
                new SystemParameter { Group = "VehicleType", Name = "Moto" },
                new SystemParameter { Group = "VehicleType", Name = "Caminhao" },
                new SystemParameter { Group = "VehicleType", Name = "Trator" },
                new SystemParameter { Group = "VehicleType", Name = "Outro" }
            };

            var cropTypes = new List<SystemParameter>
            {
                new SystemParameter { Group = "CropType", Name = "Soja" },
                new SystemParameter { Group = "CropType", Name = "Milho" },
                new SystemParameter { Group = "CropType", Name = "Trigo" },
                new SystemParameter { Group = "CropType", Name = "Algodão" },
                new SystemParameter { Group = "CropType", Name = "Café" },
                new SystemParameter { Group = "CropType", Name = "Outro" }
            };

            context.SystemParameters.AddRange(vehicleTypes);
            context.SystemParameters.AddRange(cropTypes);
            await context.SaveChangesAsync();
        }
    }
}
