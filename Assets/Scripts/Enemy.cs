using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy")]
public class Enemy : ScriptableObject
{
    public new string name;
    public int detectRange;
    public int enemySpeed;
    public int enemySize;
    public float knockbackForce;
    public bool isAggressive;
}
