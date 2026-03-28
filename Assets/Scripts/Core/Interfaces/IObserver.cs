namespace MariasGame.Core.Interfaces
{
    public interface IObserver<T>
    {
        void OnNotify(T eventData);
    }
}
