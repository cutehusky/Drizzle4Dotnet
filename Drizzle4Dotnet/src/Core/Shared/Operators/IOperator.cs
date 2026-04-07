namespace Drizzle4Dotnet.Core.Shared.Operators;

public interface IOperator: ISql
{
}


public interface IOperator<T>: ISql<T>
{
}