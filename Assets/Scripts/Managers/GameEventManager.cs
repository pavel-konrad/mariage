using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MariasGame.Core.Events;
using MariasGame.Core.Interfaces;

namespace MariasGame.Managers
{
    public class GameEventManager : MonoBehaviour, ISubject<GameEvent>
    {
        private readonly List<IObserver<GameEvent>> _observers = new();

        public void RegisterObserver(IObserver<GameEvent> observer) => _observers.Add(observer);
        public void UnregisterObserver(IObserver<GameEvent> observer) => _observers.Remove(observer);

        public void NotifyObservers(GameEvent eventData)
        {
            foreach (var observer in _observers.ToArray())
                observer.OnNotify(eventData);
        }
    }
}
