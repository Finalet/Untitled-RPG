using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NavAgentBoss : NavAgentEnemy
{
    public GameObject healthBarPrefab;
    BossTitleUI instanciatedHealthBar;

    protected override void Update()
    {
        base.Update();
        if (agr && !isDead)
            ShowBossHealthBar();
        else   
            HideBossHealthBar();
    }
    
    protected virtual void ShowBossHealthBar() {
        if (!instanciatedHealthBar) {
            instanciatedHealthBar = Instantiate(healthBarPrefab, CanvasScript.instance.transform).GetComponent<BossTitleUI>();
            instanciatedHealthBar.Init(this);
        }
    }
    protected virtual void HideBossHealthBar() {
        if (instanciatedHealthBar) instanciatedHealthBar.Hide();
    }

    protected override void ShowHealthBar (){}



}
