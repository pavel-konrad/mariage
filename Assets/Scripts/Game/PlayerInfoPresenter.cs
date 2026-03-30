using UnityEngine;
using MariasGame.Core;
using MariasGame.Core.Events;
using MariasGame.Core.Interfaces;
using MariasGame.Managers;
using MariasGame.UI;

namespace MariasGame.Game
{
    /// <summary>
    /// Zobrazuje jméno a skóre každého hráče a zvýrazňuje aktivního hráče.
    /// Reaguje na ScoreEvent (aktualizace bodů) a GameEvent (tah, hláška, start hry).
    /// </summary>
    public class PlayerInfoPresenter : MonoBehaviour, IObserver<ScoreEvent>, IObserver<GameEvent>
    {
        [Header("Event Managers")]
        [SerializeField] private ScoreEventManager _scoreEvents;
        [SerializeField] private GameEventManager _gameEvents;

        [Header("Player Panels")]
        [SerializeField] private PlayerInfoPanel _panel0;
        [SerializeField] private PlayerInfoPanel _panel1;
        [SerializeField] private PlayerInfoPanel _panel2;

        [Header("Dependencies")]
        [SerializeField] private GameBootstrapper _bootstrapper;
        [SerializeField] private CharacterManager _characterManager;

        private PlayerInfoPanel[] _panels;

        void Awake()
        {
            _panels = new[] { _panel0, _panel1, _panel2 };
            _scoreEvents.RegisterObserver(this);
            _gameEvents.RegisterObserver(this);
        }

        void OnDestroy()
        {
            _scoreEvents.UnregisterObserver(this);
            _gameEvents.UnregisterObserver(this);
        }

        public void OnNotify(ScoreEvent e)
        {
            if (e.PlayerIndex < 0 || e.PlayerIndex >= 3) return;
            _panels[e.PlayerIndex]?.UpdateScore(e.NewScore);
        }

        public void OnNotify(GameEvent e)
        {
            switch (e.Type)
            {
                case GameEventType.GameStarted:
                    var names = _bootstrapper.GameController.State.PlayerNames;
                    var avatars = ResolvePlayerAvatars();
                    for (int i = 0; i < 3; i++)
                        _panels[i]?.SetPlayerInfo(names[i], avatars[i]);
                    break;

                case GameEventType.PlayerTurnStarted:
                    for (int i = 0; i < 3; i++)
                        _panels[i]?.SetHighlight(i == e.PlayerIndex);
                    break;

                case GameEventType.MarriageDeclared:
                    if (e.PlayerIndex >= 0 && e.PlayerIndex < 3)
                        _panels[e.PlayerIndex]?.ShowDeclaration(e.TrumpSuit);
                    break;
            }
        }

        private Sprite[] ResolvePlayerAvatars()
        {
            var sprites = new Sprite[3];
            if (!GameLaunchContextStore.HasContext) return sprites;

            var context = GameLaunchContextStore.Current;
            var service = _characterManager != null ? _characterManager.GetService() : null;
            if (service == null) return sprites;

            sprites[0] = service.GetById(context.PlayerCharacterId)?.CharacterSprite;
            sprites[1] = service.GetById(context.Enemy1CharacterId)?.CharacterSprite;
            sprites[2] = service.GetById(context.Enemy2CharacterId)?.CharacterSprite;
            return sprites;
        }
    }
}
