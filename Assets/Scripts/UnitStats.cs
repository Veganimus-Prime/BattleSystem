﻿using UnityEngine;
using Veganimus.BattleSystem;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Unit Stats")]
public class UnitStats : ScriptableObject
{
    ///<summary>
    ///Assigned in Unit Class on Unit Prefab to determine Stats.
    ///</summary>
    public GameObject unitModelPrefab;
    [SerializeField] private string _unitName;
    public string UnitName { get { return _unitName; } }

    [SerializeField] private ElementType _unitType;
    public ElementType UnitType { get { return _unitType; } }

    [SerializeField] private int _unitHitPoints;
    public int UnitHitPoints { get { return _unitHitPoints; } }

    [SerializeField] private int _unitSpeed;
    public int UnitSpeed { get { return _unitSpeed; } }

    [SerializeField] private int _unitDefense;
    public int UnitDefense { get { return _unitDefense; } }

    [SerializeField] private int _unitAccuracyModifier;
    public int UnitAccuracyModifier { get { return _unitAccuracyModifier; } }

    [SerializeField] private List<UnitAttackMove> _unitAttackMoves = new List<UnitAttackMove>();
    public List<UnitAttackMove> UnitAttackMoves { get { return _unitAttackMoves; } }

    [SerializeField] private List<UnitDefenseMove> _unitDefenseMoves = new List<UnitDefenseMove>();
    public List<UnitDefenseMove> UnitDefenseMoves { get { return _unitDefenseMoves; } }

    public UnityEngine.Playables.PlayableDirector director;
    public List<GameObject> unitMoveCutscenes = new List<GameObject>();
    private Animator animator;


}