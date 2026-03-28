using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MariasGame.Core.Events;
using MariasGame.Core.Interfaces;

namespace MariasGame.Managers
{
    public class ScoreEventManager : MonoBehaviour, ISubject<ScoreEvent>
    {
        private readonly List<IObserver<ScoreEvent>> _observers = new();

        public void RegisterObserver(IObserver<ScoreEvent> observer) => _observers.Add(observer);
        public void UnregisterObserver(IObserver<ScoreEvent> observer) => _observers.Remove(observer);

        public void NotifyObservers(ScoreEvent eventData)
        {
            foreach (var observer in _observers.ToArray())
                observer.OnNotify(eventData);
        }
    }
}
