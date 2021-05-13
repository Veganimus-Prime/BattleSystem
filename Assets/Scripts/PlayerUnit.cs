using UnityEngine;

namespace Veganimus.BattleSystem
{
    public class PlayerUnit : Unit
    { 
        [Header("Broadcasting on")]
        //[SerializeField] private UnitNameUpdate _unitNameUpdateChannel;
        //[SerializeField] private UnitHitPointUpdate _unitHPUpdateChannel;
        [SerializeField] private TurnCompleteChannel _playerTurnCompleteChannel;
        [Space]
        [Header("Listening to:")]
        [SerializeField] private PlayerTurnChannel _playerTurnChannel;
        [Space]
        private bool _isPlayerTurnComplete;

        private void OnEnable() => _playerTurnChannel.OnPlayerTurn.AddListener(InitiatePlayerTurn);

        private void OnDisable() => _playerTurnChannel.OnPlayerTurn.RemoveListener(InitiatePlayerTurn);

        //private void Start()
        //{
        //    _unitNameUpdateChannel.RaiseUnitNameUpdateEvent(CharacterType.Player, _unitName);
        //    _animator = GetComponent<Animator>();
        //}

        //public void Damage(int amount)
        //{
        //    var damage = amount -= _unitDefense;
        //    if (damage <= 0)
        //        damage = 0;

        //    _currentUnitHP -= damage;
        //    if (_currentUnitHP <= 0)
        //    {
        //        _currentUnitHP = 0;
        //        _animator.SetInteger("hitPoints", 0);
        //        _endBattleChannel.RaiseBattleStateChangeEvent(BattleState.Lose);
        //    }
        //    _unitHPUpdateChannel.RaiseUnitHPUpdateEvent(CharacterType.Player, _unitHitPoints, _currentUnitHP);
        //   StartCoroutine(StatUpdateDelayRoutine($"{_unitName} took {damage} damage!"));
        //}
        //public void Heal(int amount)
        //{
        //    _currentUnitHP += amount;
        //    _unitHPUpdateChannel.RaiseUnitHPUpdateEvent(CharacterType.Player, _unitHitPoints, _currentUnitHP);
        //    StartCoroutine(StatUpdateDelayRoutine($"{_unitName} healed {amount} HP!"));
        //}
        //public void AdjustDefense(int amount)
        //{
        //    _unitDefense += amount;
        //    StartCoroutine(StatUpdateDelayRoutine(($"{_unitName} raised Defense by {amount}.")));
        //}

        private void InitiatePlayerTurn()
        {
            _isPlayerTurnComplete = false;
            _playerTurnCompleteChannel.RaiseTurnCompleteEvent(_isPlayerTurnComplete);
        }
       
        public void UseAttackMoveSlot(int slotNumber)
        {
            int usesLeft = _attackMoveUses[slotNumber];
            if (usesLeft > 0)
            {
                _attackMoveUses[slotNumber]--;
                BattleUIManager.Instance.DisplayCurrentMoveUsesLeft("attack", _attackMoveUses[slotNumber], slotNumber);
                BattleUIManager.Instance.ActivateButtons(false);
                bool didMoveHit = _unitAttacksMoveSet[slotNumber].RollForMoveAccuracy(_accuracyModifier);
                if (didMoveHit == true)
                {
                    int damageAmount = _unitAttacksMoveSet[slotNumber].damageAmount;
                    _targetUnit.targetIDamageable.Damage(damageAmount);
                    _unitAttacksMoveSet[slotNumber].RaiseAttackMoveUsedEvent(_unitName, this.transform, slotNumber);
                }
                else if (didMoveHit == false)
                {
                    _displayAttackActionChannel.RaiseDisplayActionEvent($"{_unitName} used {_unitAttacksMoveSet[slotNumber].moveName}!");
                    StartCoroutine(StatUpdateDelayRoutine($"{_unitName} Missed!"));
                }
                _isPlayerTurnComplete = true;
                _playerTurnCompleteChannel.RaiseTurnCompleteEvent(_isPlayerTurnComplete);
            }
            else if (usesLeft <= 0)
                return;
        }

        public void UseDefenseMoveSlot(int slotNumber)
        {
            int usesLeft = _defenseMoveUses[slotNumber];
            if (usesLeft >0)
            {
                _defenseMoveUses[slotNumber]--;
                BattleUIManager.Instance.DisplayCurrentMoveUsesLeft("defense", _defenseMoveUses[slotNumber], slotNumber);
                BattleUIManager.Instance.ActivateButtons(false);
                _unitDefensesMoveSet[slotNumber].RaiseDefenseMoveUsedEvent(_unitName);
                AdjustDefense(_unitDefensesMoveSet[slotNumber].defenseBuff);
                _isPlayerTurnComplete = true;
                _playerTurnCompleteChannel.RaiseTurnCompleteEvent(_isPlayerTurnComplete);
            }
            else if (usesLeft <=0)
                return;
        }
    }
}
