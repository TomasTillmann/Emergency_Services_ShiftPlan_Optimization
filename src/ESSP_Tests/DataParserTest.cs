using DataHandling;
using ESSP.DataModel;

namespace ESSP_Tests;

//TODO: write tests
public class DataParserTest : Tests
{
  [Test]
  public void ParsingWorldTest()
  {
    DataModelGenerator dataGenerator = new();
    DataParser parser = new();

    WorldOpt world = dataGenerator.GenerateExampleWorld();

    string json = parser.ParseToJson(world);
    WorldOpt parsedWorld = parser.ParseFromJson(json);

    AssertEqual(world, parsedWorld);

    throw new NotImplementedException();
  }

  private void AssertEqual(WorldOpt expected, WorldOpt actual)
  {
  }

  [Test]
  public void ParsingIncidentTest()
  {
    DataModelGenerator dataGenerator = new();
    IncidentOpt[] incidents = dataGenerator.GenerateExampleIncidents();

    throw new NotImplementedException();
  }
}
