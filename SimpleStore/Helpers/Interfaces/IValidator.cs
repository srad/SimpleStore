namespace SimpleStore.Helpers.Interfaces;

public interface IValidator<in T>
{
    bool IsValid(T t);
}