using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MariasGame.Core;
using MariasGame.Core.Events;
using MariasGame.Core.Interfaces;
using MariasGame.Game.Strategies;
using MariasGame.Managers;
using MariasGame.ScriptableObjects;

namespace MariasGame.Game
{
    /// <summary>
    /// Spouští AI tahy po PlayerTurnStarted eventu pro hráče 1 a 2.
    /// Vytváří AI strategii na základě GameModeConfig.AIDifficulty.
    /// </summary>
    public class AITurnController : MonoBehaviour, IObserver<GameEvent>
    {
        [SerializeField] private GameEventManager _gameEvents;
        [SerializeField] private GameBootstrapper _bootstrapper;
        [SerializeField] private GameModeConfig _gameModeConfig;

        [SerializeField] private float _aiThinkDelay = 0.8f;

        private IAIStrategy[] _aiStrategies;

        void Awake()
        {
            _aiStrategies = new IAIStrategy[3];
            _aiStrategies[1] = CreateStrategy(_gameModeConfig.AIDifficulty);
            _aiStrategies[2] = CreateStrategy(_gameModeConfig.AIDifficulty);
            _gameEvents.RegisterObserver(this);
        }

        void OnDestroy() => _gameEvents.UnregisterObserver(this);

        public void OnNotify(GameEvent e)
        {
            if (e.Type != GameEventType.PlayerTurnStarted) return;
            if (e.PlayerIndex == 0) return; // lidský hráč
            StartCoroutine(AITurn(e.PlayerIndex));
        }

        private IEnumerator AITurn(int playerIndex)
        {
            yield return new WaitForSeconds(_aiThinkDelay);

            var controller = _bootstrapper.GameController;
            var state = controller.State;
            var strategy = _aiStrategies[playerIndex];

            switch (state.Phase)
            {
                case GamePhase.Bidding:
                    controller.MakeBid(playerIndex, strategy.ChooseBid(state));
                    break;

                case GamePhase.DiscardingTalon:
                    var hand = state.PlayerHands[playerIndex];
                    var toDiscard = strategy.ChooseCardsToDiscard(hand.AsReadOnly(), state);
                    controller.DiscardToTalon(toDiscard);
                    break;

                case GamePhase.Declaring:
                    var declarerHand = state.PlayerHands[playerIndex];
                    controller.DeclareTrump(strategy.ChooseTrumpSuit(declarerHand.AsReadOnly()));
                    break;

                case GamePhase.Playing:
                    var legalPlays = controller.GetLegalPlays();
                    if (legalPlays.Count > 0)
                        controller.PlayCard(playerIndex, strategy.ChooseCardToPlay(legalPlays.AsReadOnly(), state));
                    break;
            }
        }

        private static IAIStrategy CreateStrategy(AIDifficulty difficulty) => difficulty switch
        {
            AIDifficulty.Medium => new MediumAIStrategy(),
            AIDifficulty.Hard   => new HardAIStrategy(),
            _                   => new EasyAIStrategy()
        };
    }
}
