﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Veganimus.BattleSystem
{
    ///<summary>
    ///@author
    ///Aaron Grincewicz
    ///@info: Represents either the User as "Player" or an AI.
    ///</summary>
    public class Character : MonoBehaviour
    {
        public Quaternion activeUnitSpotRotation;
        public CharacterStats ThisCharacterStats;
        [SerializeField] private CharacterType _thisCharacterType;
        public CharacterType ThisCharacterType { get { return _thisCharacterType; } }
        [SerializeField] private string _characterName;
        public string CharacterName { get { return _characterName; } }
        [Range(-1, 1)][SerializeField] private int _aiAgression;
        public int AIAgression { get { return _aiAgression; } }
        [SerializeField] private List<UnitStats> _party = new List<UnitStats>();
        public List<UnitStats> Party { get { return _party; } }
        public List<MoveEffect> effects = new List<MoveEffect>();
        private BattleInventory _inventory;
        public BattleInventory ThisInventory { get { return _inventory; } }
        public Unit activeUnit;
        private int _activeUnitSlotNumber;
        public GameObject activeUnitPrefab;
        public bool isDefeated;
        [SerializeField] private Transform _activeUnitSpot;
        public Transform ActiveUnitSpot { get { return _activeUnitSpot; } }
        [Header("Broadcasting on")]
        [SerializeField] private TurnCompleteChannel _turnCompleteChannel;
        public TurnCompleteChannel TurnCompleteChannel { get { return _turnCompleteChannel; } }
        [SerializeField] private DisplayActionChannel _displayActionChannel;
        [SerializeField] private UnitMoveNameUpdate _itemNameUpdateChannel;
        [SerializeField] private SwapUnitChannel _swapUnitChannel;
        [Space]
        [Header("Listening to:")]
        [SerializeField] private CharacterTurnChannel characterTurnChannel;
        [Space]
        [SerializeField] private int _turnCount;
        public int TurnCount { get { return _turnCount; } }
        [SerializeField] private bool _isTurnComplete;
        public bool IsTurnComplete { get => _isTurnComplete; set => _isTurnComplete = value; }
        [SerializeField] private UnitNameUpdate _unitNameUpdateChannel;
        private WaitForSeconds _turnDelay;

        private void OnEnable() => characterTurnChannel.OnCharacterTurn.AddListener(InitiateCharacterTurn);

        private void OnDisable() => characterTurnChannel.OnCharacterTurn.RemoveListener(InitiateCharacterTurn);

        private void Start()
        {
            _turnDelay = new WaitForSeconds(5.0f);
            _characterName = ThisCharacterStats.CharacterName;
            _inventory = GetComponent<BattleInventory>();
            activeUnitPrefab = Instantiate(_party[0].UnitModelPrefab ,_activeUnitSpot);
            activeUnit.unitStats = _party[0];
            _activeUnitSlotNumber = _party.IndexOf(activeUnit.unitStats);
           // Debug.Log($"Active Unit Slot:{_activeUnitSlotNumber}");
            activeUnitPrefab.transform.position = new Vector3(activeUnit.transform.position.x, 15, activeUnit.transform.position.z);
            activeUnitPrefab.transform.rotation = ActiveUnitSpot.rotation;
            UpdateCharacterNames();
            UpdatePartyUnitNames();
        }

        private void InitiateCharacterTurn(CharacterType characterType)
        {
            if (characterType == ThisCharacterType)
            {
                _turnCount++;
                IsTurnComplete = false;
                TurnCompleteChannel.RaiseTurnCompleteEvent(characterType, IsTurnComplete);
                DeActivateEffects(false);
                if (ThisCharacterType != CharacterType.Player && IsTurnComplete == false)
                 StartCoroutine(TurnDelayRoutine());
            }
        }
        private void DeActivateEffects(bool isSwappingUnit)
        {
            var efffect = GetComponentsInChildren<MoveEffect>();
            foreach(var obj in activeUnit.GetComponentsInChildren<MoveEffect>())
                effects.Add(obj);
            
            for (int i = effects.Count - 1; i > 0; i--)
            {
                if (TurnCount - effects[i].activatedOnTurn > effects[i].turnsActive || isSwappingUnit)
                {
                    if (isSwappingUnit)
                        effects.Clear();
                    else
                    {
                        if (effects[i] != null)
                        {
                            effects[i].gameObject.SetActive(false);
                            effects.Remove(effects[i]);
                        }
                        activeUnit.ResetDefense();
                    }
                }
            }
        }
        private void UpdateCharacterNames() => BattleUIManager.Instance.UpdateCharacterNames(ThisCharacterType, CharacterName);

        public void UpdateItemNames()
        {
            UpdateItemUseUI();
            for (int i = _inventory.battleInventory.Count - 1; i >= 0; i--)
            {
                var item = _inventory.battleInventory[i];
                var type = item.ItemType;
                _itemNameUpdateChannel.RaiseMoveNameUpdateEvent(item.ItemName, i);
                switch (type)
                {
                    case ItemType.Health:
                    case ItemType.Equipment:
                        BattleUIManager.Instance.DisplayItemEffects(type, item.StatAffected, item.EffectAmount, i);
                        break;
                }
            }
        }
        private void UpdateItemUseUI()
        {
            for (int i = _inventory.battleInventory.Count - 1; i >= 0; i--)
            {
                var item = _inventory.battleInventory[i];
                uint usesLeft = item.ItemUses;
                if (usesLeft >= 0)
                    BattleUIManager.Instance.DisplayCurrentMoveUsesLeft("item", usesLeft, i);
                else
                    return;
            }
        }
        ///<summary>
        ///Uses a dice roll system to determine what Action an AI character will take.
        ///</summary>
        public void DetermineAction()
        {
            var inv = ThisInventory.battleInventory;
            var dieRoll = Random.Range(1, 6);
            var attackToUse = Random.Range(0, activeUnit.AttackMoveSet.Count);
            var defenseToUse = Random.Range(0, activeUnit.DefenseMoveSet.Count);

            if (dieRoll + AIAgression >= 3)
                activeUnit.UseAttackMoveSlot(attackToUse);

            else if (dieRoll + AIAgression < 3 && dieRoll + AIAgression > 1)
                activeUnit.UseDefenseMoveSlot(defenseToUse);

            else if (dieRoll + AIAgression <= 1)
            {
                Item itemToUse = inv[Random.Range(0, inv.Count)];
               
                if (activeUnit.CurrentUnitHP < activeUnit.unitStats.UnitHitPoints)
                    itemToUse = inv.Find(i => i.ItemType.Equals(ItemType.Health));

                else if (activeUnit.RunTimeUnitInfo.speed < activeUnit.TargetUnit.TargetStats.speed)
                    itemToUse = inv.Find(i => i.StatAffected.Equals(StatAffected.Speed));

                else
                    itemToUse = inv.Find(i => i.ItemType.Equals(ItemType.Equipment));

                int itemSlot = inv.IndexOf(itemToUse);
                if (itemToUse == null)
                    DetermineAction();
                
                else
                     UseItemSlot(itemSlot);
            }
        }
        public void UseItemSlot(int slotNumber)
        {
            var itemName = _inventory.battleInventory[slotNumber].ItemName;
            uint usesLeft = _inventory.battleInventory[slotNumber].ItemUses;

            if (usesLeft > 0 && itemName != "")
            {
                _inventory.UseItem(slotNumber);
                BattleUIManager.Instance.DisplayCurrentMoveUsesLeft("item", usesLeft, slotNumber);
                BattleUIManager.Instance.ActivateButtons(false);
                _displayActionChannel.RaiseDisplayActionEvent($"{_characterName} used {_inventory.battleInventory[slotNumber].ItemName}!");
                IsTurnComplete = true;
                TurnCompleteChannel.RaiseTurnCompleteEvent(ThisCharacterType, IsTurnComplete);
            }
            else if (usesLeft <= 0|| itemName == "")
            {
                if (ThisCharacterType != CharacterType.Player)
                    DetermineAction();
                else
                    return;
            }
        }
        private void UpdatePartyUnitNames()
        {
            for (int u = _party.Count - 1; u >= 0; u--)
            {
                var unit = _party[u];
                BattleUIManager.Instance.DisplayPartyUnitNames(unit.UnitName, u);
            }
        }
        public void SwapUnit(int slotNumber)
        {
            var unitName = _party[slotNumber].UnitName;
            if (slotNumber != _activeUnitSlotNumber && unitName != "")
            {
                DeActivateEffects(true);
                Destroy(activeUnitPrefab);
                activeUnitPrefab = Instantiate(_party[slotNumber].UnitModelPrefab, _activeUnitSpot);
                activeUnit.unitStats = _party[slotNumber];
                _activeUnitSlotNumber = _party.IndexOf(activeUnit.unitStats);
                _swapUnitChannel.RaiseUnitSwapEvent();
                activeUnitPrefab.transform.position = new Vector3(activeUnit.transform.position.x, 15, activeUnit.transform.position.z);
                activeUnitPrefab.transform.rotation = _activeUnitSpot.rotation;
                BattleUIManager.Instance.ActivateButtons(false);
                _displayActionChannel.RaiseDisplayActionEvent($"{_characterName} swapped in {unitName}!");
                IsTurnComplete = true;
                TurnCompleteChannel.RaiseTurnCompleteEvent(ThisCharacterType, IsTurnComplete);
            }
            else
            {
                if (ThisCharacterType != CharacterType.Player)
                    DetermineAction();
                else
                    return;
            }
        }
        private IEnumerator TurnDelayRoutine()
        {
            yield return _turnDelay;
            DetermineAction();
        }
    }
}