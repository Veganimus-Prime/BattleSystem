﻿using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using Veganimus.BattleSystem;

[CreateAssetMenu(menuName = "Unit Moves/ Attack Move")]
public class UnitAttackMove : UnitMove
{
   public int damageAmount;
   public float attackSpeed;

    public UnityEvent<int, Transform> OnAttackMoveUsed;

    public  void RaiseAttackMoveUsedEvent(string unitName,Transform unit,int moveSlot)
    {
        _assignedUnit = unit;
        RaiseMoveQueuedEvent(this);
        PerformAttackMove(unitName, moveSlot);
    }
    
    private void PerformAttackMove(string unitName, int moveSlot)
    {
        if (moveName != "")
         _displayActionChannel.RaiseDisplayActionEvent($"{unitName} used {moveName}!");
        
        else
            return;
    }
    
    public void CreateNewAttackMove(string newMoveName)
    {
        UnitAttackMove newAttackMove = CreateInstance<UnitAttackMove>();
        newAttackMove.moveName = newMoveName;
        AssetDatabase.CreateAsset(CreateInstance<UnitAttackMove>(), $"Assets/Scripts/Scriptable Objects/Moves/Attack Moves/{newAttackMove.moveName}.asset");
    }
}
