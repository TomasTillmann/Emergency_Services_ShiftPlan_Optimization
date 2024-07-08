using DistanceAPI;
using ESSP.DataModel;
using System.Collections.Immutable;
using Xunit;
using FluentAssertions;

namespace MoveGenerationTest;

public class RealDistanceCalculatorTests
{
    [Fact]
    public void GetTravelDurationSec_ValidInput_ReturnsDuration()
    {
        // Arrange
        var hospitals = ImmutableArray.Create(
            new Hospital { Location = new Coordinate { Latitude = 37.7749, Longitude = -122.4194 } }, // San Francisco, CA
            new Hospital { Location = new Coordinate { Latitude = 34.0522, Longitude = -118.2437 } }  // Los Angeles, CA
        );
        var _distanceCalculator = new RealDistanceCalculator(hospitals);
        Coordinate from = new Coordinate { Latitude = 37.7749, Longitude = -122.4194 }; // San Francisco, CA
        Coordinate to = new Coordinate { Latitude = 34.0522, Longitude = -118.2437 }; // Los Angeles, CA

        // Act
        int travelDuration = _distanceCalculator.GetTravelDurationSec(from, to);

        // Assert
        travelDuration.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetNewLocation_ValidInput_ReturnsIntermediateLocation()
    {
        // Arrange
        var hospitals = ImmutableArray.Create(
            new Hospital { Location = new Coordinate { Latitude = 37.7749, Longitude = -122.4194 } }, // San Francisco, CA
            new Hospital { Location = new Coordinate { Latitude = 34.0522, Longitude = -118.2437 } }  // Los Angeles, CA
        );
        var _distanceCalculator = new RealDistanceCalculator(hospitals);
        Coordinate from = new Coordinate { Latitude = 37.7749, Longitude = -122.4194 }; // San Francisco, CA
        Coordinate to = new Coordinate { Latitude = 34.0522, Longitude = -118.2437 }; // Los Angeles, CA
        int durationDrivingSec = 3.ToHours().ToMinutes().ToSeconds().Value + 20.ToMinutes().ToSeconds().Value;

        // Act
        Coordinate newLocation = _distanceCalculator.GetIntermediateLocation(from, to, durationDrivingSec);

        // Assert
        // expected latitude and longitude is calculated directly from google maps - location: Reef-Sunset Unified School District, California, USA
        newLocation.Latitude.Should().BeApproximately(35.90 , 1);
        newLocation.Longitude.Should().BeApproximately(-119.90, 1);
    }

    [Fact]
    public void GetNearestHospital_ValidInput_ReturnsNearestHospital()
    {
        // Arrange
        var hospitals = ImmutableArray.Create(
            new Hospital { Location = new Coordinate { Latitude = 37.7749, Longitude = -122.4194 } }, // San Francisco, CA
            new Hospital { Location = new Coordinate { Latitude = 34.0522, Longitude = -118.2437 } }, // Los Angeles, CA
            new Hospital { Location = new Coordinate { Latitude = 40.7128, Longitude = -74.0060 } }    // New York City, NY
            // Add more hospitals in different locations
        );
        var distanceCalculator = new RealDistanceCalculator(hospitals);
    
        // Test 1: Location exactly matches one hospital
        Coordinate location1 = new Coordinate { Latitude = 37.7749, Longitude = -122.4194 };
        Hospital expected1 = hospitals[0];

        // Test 2: Location equidistant between two hospitals
        Coordinate location2 = new Coordinate { Latitude = 37.5, Longitude = -122.5 }; // Between SF and LA
        Hospital expected2 = hospitals[0]; // Expect SF hospital due to order in ImmutableArray

        // Test 3: Location in a different region (e.g., NYC)
        Coordinate location3 = new Coordinate { Latitude = 40.7128, Longitude = -74.0060 };
        Hospital expected3 = hospitals[2];

        // Act
        Hospital nearestHospital1 = distanceCalculator.GetNearestHospital(location1);
        Hospital nearestHospital2 = distanceCalculator.GetNearestHospital(location2);
        Hospital nearestHospital3 = distanceCalculator.GetNearestHospital(location3);

        // Assert
        nearestHospital1.Should().Be(expected1);
        nearestHospital2.Should().Be(expected2);
        nearestHospital3.Should().Be(expected3);
    }
}
