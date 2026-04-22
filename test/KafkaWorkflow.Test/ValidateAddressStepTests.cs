using KafkaWorkflow.Consumer.Base.Workflow;
using KafkaWorkflow.Consumer.PeopleWorkflow;
using KafkaWorkflow.Consumer.PeopleWorkflow.Steps;
using KafkaWorkflow.WebApi.Db;
using KafkaWorkflow.WebApi.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace KafkaWorkflow.Test;

[TestFixture]
public class ValidateAddressStepTests
{
    private Mock<IMessageWorkflow<int, PersonState?>> _mockWorkflow = null!;
    private Mock<PeopleContext> _mockDbContext = null!;
    private ValidateAddressStep _step = null!;
    private PersonState _personState = null!;

    [SetUp]
    public void Setup()
    {
        _mockWorkflow = new Mock<IMessageWorkflow<int, PersonState?>>();
        _mockDbContext = new Mock<PeopleContext>();
        _personState = new PersonState(1);
        _mockWorkflow.Setup(w => w.State).Returns(_personState);

        _step = new ValidateAddressStep(_mockWorkflow.Object, _mockDbContext.Object);
    }

    [Test]
    public async Task ShouldExecute_WithAddresses_ReturnsTrue()
    {
        // Arrange
        var contactInfo = new ContactInfo { Id = 1 };
        var person = new Person 
        { 
            Id = 1, 
            FirstName = "John",
            LastName = "Doe",
            ContactInfos = [contactInfo]
        };
        var addresses = new List<Address>
        {
            new() { Id = 1, Street = "123 Main St", City = "Boston", State = "MA", ContactInfo = contactInfo, ContactInfoId = 1 },
            new() { Id = 2, Street = "456 Oak Ave", City = "Boston", State = "MA", ContactInfo = contactInfo, ContactInfoId = 1 }
        };

        SetupAddressQueries(person, addresses);

        // Act
        var result = await _step.ShouldExecute();

        // Assert
        Assert.That(result, Is.True);
        Assert.That(_personState.Addresses?.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task ShouldExecute_WithoutAddresses_ReturnsFalse()
    {
        // Arrange
        var contactInfo = new ContactInfo { Id = 1 };
        var person = new Person 
        { 
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            ContactInfos = [contactInfo]
        };
        SetupAddressQueries(person, new List<Address>());

        // Act
        var result = await _step.ShouldExecute();

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ShouldExecute_WithoutContactInfo_ReturnsFalse()
    {
        // Arrange
        var person = new Person 
        { 
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            ContactInfos = []
        };
        var mockPersons = CreateMockDbSet(new[] { person });
        _mockDbContext.Setup(c => c.Persons).Returns(mockPersons.Object);
        _mockDbContext.Setup(c => c.Addresses).Returns(CreateMockDbSet(new Address[] { }).Object);

        // Act
        var result = await _step.ShouldExecute();

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ShouldExecute_SetsAddressesInState()
    {
        // Arrange
        var contactInfo = new ContactInfo { Id = 1 };
        var person = new Person 
        { 
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            ContactInfos = [contactInfo]
        };
        var addresses = new List<Address>
        {
            new() { Id = 1, Street = "123 Main St", City = "Boston", State = "MA", ContactInfo = contactInfo, ContactInfoId = 1 }
        };

        SetupAddressQueries(person, addresses);

        // Act
        await _step.ShouldExecute();

        // Assert
        Assert.That(_personState.Addresses, Is.Not.Null);
        Assert.That(_personState.Addresses.Count(), Is.EqualTo(1));
        Assert.That(_personState.Addresses.First().Street, Is.EqualTo("123 Main St"));
    }

    [Test]
    public async Task ExecuteAsync_WithAddresses_WritesAddressInfo()
    {
        // Arrange
        var contactInfo = new ContactInfo { Id = 1 };
        var addresses = new List<Address>
        {
            new() { Id = 1, Street = "123 Main St", City = "Boston", State = "MA", ContactInfo = contactInfo, ContactInfoId = 1 }
        };
        _personState.Addresses = addresses;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        await _step.ExecuteAsync();

        // Assert
        var output = stringWriter.ToString();
        Assert.That(output, Does.Contain("Validating addresses"));
        Assert.That(output, Does.Contain("123 Main St"));
    }

    [Test]
    public async Task ExecuteAsync_WithoutAddresses_WritesNoAddressesFound()
    {
        // Arrange
        _personState.Addresses = new List<Address>();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        await _step.ExecuteAsync();

        // Assert
        var output = stringWriter.ToString();
        Assert.That(output, Does.Contain("No addresses found"));
    }

    [Test]
    public async Task ExecuteAsync_WithMultipleAddresses_PrintsEachAddress()
    {
        // Arrange
        var contactInfo = new ContactInfo { Id = 1 };
        var addresses = new List<Address>
        {
            new() { Id = 1, Street = "123 Main St", City = "Boston", State = "MA", ContactInfo = contactInfo, ContactInfoId = 1 },
            new() { Id = 2, Street = "456 Oak Ave", City = "Boston", State = "MA", ContactInfo = contactInfo, ContactInfoId = 1 }
        };
        _personState.Addresses = addresses;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Act
        await _step.ExecuteAsync();

        // Assert
        var output = stringWriter.ToString();
        Assert.That(output, Does.Contain("123 Main St"));
        Assert.That(output, Does.Contain("456 Oak Ave"));
    }

    private void SetupAddressQueries(Person person, List<Address> addresses)
    {
        var mockPersons = CreateMockDbSet(new[] { person });
        var mockAddresses = CreateMockDbSet(addresses);

        _mockDbContext.Setup(c => c.Persons).Returns(mockPersons.Object);
        _mockDbContext.Setup(c => c.Addresses).Returns(mockAddresses.Object);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> sourceList) where T : class
    {
        var queryable = sourceList.AsQueryable();
        var mock = new Mock<DbSet<T>>();

        mock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        return mock;
    }
}
