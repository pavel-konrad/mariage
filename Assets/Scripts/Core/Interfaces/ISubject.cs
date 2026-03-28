namespace MariasGame.Core.Interfaces
{
    public interface ISubject<T>
    {
        void RegisterObserver(IObserver<T> observer);
        void UnregisterObserver(IObserver<T> observer);
        void NotifyObservers(T eventData);
    }
}
