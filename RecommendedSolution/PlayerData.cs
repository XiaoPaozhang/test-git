using System;
using UnityEngine;

/// <summary>
/// 玩家数据模型 - 推荐方案 A：统一事件驱动
/// 数据层保持简洁，只负责数据管理和通知
/// </summary>
public class PlayerData
{
    // ============ 字段保持原生类型，简洁清晰 ============
    private int _score;
    private int _health;
    private int _maxHealth = 100;
    private string _playerName;
    private int _level;
    private float _experience;
    
    // ============ 统一的数据变更事件 ============
    /// <summary>
    /// 任何数据变化时触发，UI 可以统一响应
    /// </summary>
    public event Action<PlayerData> OnDataChanged;
    
    // ============ 属性：只负责数据验证和触发通知 ============
    
    public int Score
    {
        get => _score;
        set
        {
            if (_score != value)
            {
                _score = value;
                NotifyDataChanged();
            }
        }
    }
    
    public int Health
    {
        get => _health;
        set
        {
            if (_health != value)
            {
                // 数据验证逻辑放在这里是合理的
                _health = Mathf.Clamp(value, 0, _maxHealth);
                NotifyDataChanged();
            }
        }
    }
    
    public int MaxHealth
    {
        get => _maxHealth;
        set
        {
            if (_maxHealth != value)
            {
                _maxHealth = Mathf.Max(1, value);
                // 确保当前血量不超过最大值
                if (_health > _maxHealth)
                {
                    _health = _maxHealth;
                }
                NotifyDataChanged();
            }
        }
    }
    
    public string PlayerName
    {
        get => _playerName;
        set
        {
            if (_playerName != value)
            {
                _playerName = value;
                NotifyDataChanged();
            }
        }
    }
    
    public int Level
    {
        get => _level;
        set
        {
            if (_level != value)
            {
                _level = Mathf.Max(1, value);
                NotifyDataChanged();
            }
        }
    }
    
    public float Experience
    {
        get => _experience;
        set
        {
            if (!Mathf.Approximately(_experience, value))
            {
                _experience = Mathf.Max(0, value);
                NotifyDataChanged();
            }
        }
    }
    
    // ============ 计算属性（不需要触发事件） ============
    
    public float HealthPercentage => _maxHealth > 0 ? (float)_health / _maxHealth : 0;
    
    public bool IsAlive => _health > 0;
    
    public bool IsLowHealth => HealthPercentage < 0.3f;
    
    // ============ 构造函数 ============
    
    public PlayerData(string name = "Player")
    {
        _playerName = name;
        _health = _maxHealth;
        _level = 1;
        _score = 0;
        _experience = 0;
    }
    
    // ============ 业务方法 ============
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(int damage)
    {
        Health -= damage; // 通过属性修改，自动触发事件
    }
    
    /// <summary>
    /// 治疗
    /// </summary>
    public void Heal(int amount)
    {
        Health += amount;
    }
    
    /// <summary>
    /// 增加分数
    /// </summary>
    public void AddScore(int points)
    {
        Score += points;
    }
    
    /// <summary>
    /// 增加经验值
    /// </summary>
    public void AddExperience(float exp)
    {
        Experience += exp;
        
        // 检查是否升级（经验满 100 升级）
        if (_experience >= 100f)
        {
            LevelUp();
        }
    }
    
    /// <summary>
    /// 升级
    /// </summary>
    private void LevelUp()
    {
        Level++;
        Experience = 0;
        MaxHealth += 10; // 升级增加最大血量
        Health = MaxHealth; // 恢复满血
    }
    
    /// <summary>
    /// 批量更新数据时使用，避免多次触发事件
    /// </summary>
    public void BatchUpdate(Action updateAction)
    {
        if (updateAction == null) return;
        
        // 临时移除所有监听者
        var tempEvent = OnDataChanged;
        OnDataChanged = null;
        
        // 执行批量更新
        updateAction.Invoke();
        
        // 恢复监听者并触发一次通知
        OnDataChanged = tempEvent;
        NotifyDataChanged();
    }
    
    // ============ 私有方法 ============
    
    /// <summary>
    /// 统一的通知方法
    /// </summary>
    private void NotifyDataChanged()
    {
        OnDataChanged?.Invoke(this);
    }
    
    // ============ 调试辅助 ============
    
    public override string ToString()
    {
        return $"PlayerData[Name={_playerName}, Level={_level}, HP={_health}/{_maxHealth}, Score={_score}, Exp={_experience:F1}]";
    }
}