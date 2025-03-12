namespace LinguisticsApp.DomainModel;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class EfCoreValueConverterAttribute : Attribute
{
    public EfCoreValueConverterAttribute(Type valueConverter)
    {
        ValueConverter = valueConverter;
    }

    public Type ValueConverter { get; }
}
