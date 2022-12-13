using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UpgradeManager
{
    public enum UpgradeName
    {
        heartbeat,
        reinforce,
        warehouse,
        cooperation
    }

    // heal after each round
    public const string heartbeatStr = "heartbeat";
    const string heartbeatDesc = "polygons heal after each round";

    // increased max health
    public const string reinforceStr = "reinforce";
    const string reinforceDesc = "polygons gain increased max health";

    // increased inventory
    public const string warehouseStr = "warehouse";
    const string warehouseDesc = "increased inventory";

    // share damage with neighbours
    public const string cooperationStr = "cooperation";
    const string cooperationDesc = "polygons split damage with neighbours";

    static Dictionary<UpgradeName, Upgrade> upgradeStatus = new();
    
    // define order in which to apply upgrades
    static List<Upgrade> upgradeList = new();

    static UpgradeManager()
    {
        Add(UpgradeName.heartbeat, heartbeatStr, heartbeatDesc);
        Add(UpgradeName.reinforce, reinforceStr, reinforceDesc);
        Add(UpgradeName.warehouse, warehouseStr, warehouseDesc);
        Add(UpgradeName.cooperation, cooperationStr, cooperationDesc);
    }

    static void Add(UpgradeName name, string nameStr, string desc)
    {
        Upgrade upgrade = new(name, nameStr, desc);
        upgradeStatus.Add(name, upgrade);
        upgradeList.Add(upgrade);
    }

    public static List<Upgrade> GetUpgradeList()
    {
        return upgradeList;
    }

    public static bool IsActive(UpgradeName name)
    {
        if (upgradeStatus.ContainsKey(name))
        {
            return upgradeStatus[name].applied;
        }
        return false;
    }

    static List<Upgrade> GetInactive()
    {
        List<Upgrade> inactive = new();
        foreach (var item in upgradeStatus)
        {
            var upgrade = item.Value;
            if (!upgrade.applied)
            {
                inactive.Add(upgrade);
            }
        }
        return inactive;
    }

    public static List<Upgrade> TakeN(int n)
    {
        List<Upgrade> selected = new();
        var inactive = GetInactive();
        var limit = Mathf.Min(n, inactive.Count);
        while (selected.Count < limit)
        {
            selected.Add(Helpers.TakeRandom(inactive));
        }

        return selected;
    }
}

public class Upgrade
{
    public UpgradeManager.UpgradeName name { get; private set; }
    public string nameStr { get; private set; }
    public string desc { get; private set; }
    public bool applied { get; private set; } = false;
    public bool active { get; private set; } = false;

    public Upgrade(UpgradeManager.UpgradeName name, string nameStr, string desc)
    {
        this.name = name;
        this.nameStr = nameStr;
        this.desc = desc;
    }

    public void SetApplied()
    {
        applied = true;
    }
    public void SetActive()
    {
        active = true;
    }
}
