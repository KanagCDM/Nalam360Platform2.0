using MedWay.Domain.Primitives;

namespace MedWay.Domain.ValueObjects;

/// <summary>
/// Address value object
/// </summary>
public sealed class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }

    private Address(string street, string city, string state, string postalCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public static Result<Address> Create(
        string street,
        string city,
        string state,
        string postalCode,
        string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            return Result.Failure<Address>(Error.Validation(nameof(Street), "Street is required"));

        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<Address>(Error.Validation(nameof(City), "City is required"));

        if (string.IsNullOrWhiteSpace(state))
            return Result.Failure<Address>(Error.Validation(nameof(State), "State is required"));

        if (string.IsNullOrWhiteSpace(postalCode))
            return Result.Failure<Address>(Error.Validation(nameof(PostalCode), "Postal code is required"));

        if (string.IsNullOrWhiteSpace(country))
            return Result.Failure<Address>(Error.Validation(nameof(Country), "Country is required"));

        return new Address(street, city, state, postalCode, country);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString() =>
        $"{Street}, {City}, {State} {PostalCode}, {Country}";
}
