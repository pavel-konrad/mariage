using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MariasGame.Core.Events;
using MariasGame.Core.Interfaces;

namespace MariasGame.Managers
{
    public class CardEventManager : MonoBehaviour, ISubject<CardEvent>
    {
        private readonly List<IObserver<CardEvent>> _observers = new();

        public void RegisterObserver(IObserver<CardEvent> observer) => _observers.Add(observer);
        public void UnregisterObserver(IObserver<CardEvent> observer) => _observers.Remove(observer);

        public void NotifyObservers(CardEvent eventData)
        {
            foreach (var observer in _observers.ToArray())
                observer.OnNotify(eventData);
        }
    }
}
