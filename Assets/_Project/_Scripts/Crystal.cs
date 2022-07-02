using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour, ICrystal
{
    public IMiner CurrentMiner => miner;
    private IMiner miner;
    [SerializeField] private int money;
    [SerializeField] private Transform minerDEBUG;
    private void Update()
    {
        TimerUtils.AddTimer(5f, GiveMoney);
    }
    private void GiveMoney()
    {
        if (miner != null && miner.IsAlive())
        {
            miner.GainIncome(money);
        }
    }

    public void Assign(IMiner miner)
    {
        this.miner = miner;
        miner.OnDestroyed += Miner_OnDestroyed;
        this.minerDEBUG = miner.transform;
    }

    private void Miner_OnDestroyed()
    {
        miner = null;
    }

    public void Damage(float damage, IDamagable owner) { }
}


public interface IMiner : IBuilding
{
    void GainIncome(int money);
    event System.Action OnDestroyed;
}
public interface ICrystal
{
    IMiner CurrentMiner { get; }
    void Assign(IMiner miner);
}