using Persistence.Entities;

namespace UnitTests.Common;

public static class TestData
{
    public static readonly Client ClientTestData = new()
    {
        Id = "c13f9e3e-9447-43b0-ad0c-0f32a2d8887e",
        Email = "test@example.com",
        FirstName = "TestFirstName",
        LastName = "TestLastName",
        PhoneNumber = "+6390000001"
    };
}