using System;
using UnityEngine;

/// <summary>
/// 玩家数据模型 - 进阶方案 B：精细化事件
/// 当不同字段需要触发不同行为时使用
/// </summary>
public class PlayerDataAdvanced
{
    // ============ 字段 ============
    private int _score;
    private int _health;
    private int _maxHealth = 100;
    private string _playerName;
    private int _level;
    private int _coins;
    
    // ============ 精细化事件 ============
    // 为需要独立控制的字段提供专门的事件
    
    /// <summary>
    /// 分数变化事件 - 传递新值
    /// </summary>
    public event Action<int> OnScoreChanged;
    
    /// <summary>
    /// 血量变化事件 - 传递新值、旧值和变化量
    /// </summary>
    public event Action<int, int, int> OnHealthChanged; // (newHealth, oldHealth, delta)
    
    /// <summary>
    /// 等级变化事件
    /// </summary>
    public event Action<int> OnLevelChanged;
    
    /// <summary>
    /// 金币变化事件 - 传递变化量
    /// </summary>
    public event Action<int, int> OnCoinsChanged; // (newAmount, delta)
    
    /// <summary>
    /// 玩家死亡事件
    /// </summary>
    public event Action OnPlayerDied;
    
    /// <summary>
    /// 低血量警告事件
    /// </summary>
    public event Action<bool> OnLowHealthWarning; // true=进入低血量, false=离开低血量
    
    /// <summary>
    /// 通用数据变化事件（兜底使用）
    /// </summary>
    public event Action<PlayerDataAdvanced> OnDataChanged;
    
    // ============ 属性 ============
    
    public int Score
    {
        get => _score;
        set
        {
            if (_score != value)
            {
                _score = value;
                OnScoreChanged?.Invoke(_score);
                OnDataChanged?.Invoke(this);
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
                int oldHealth = _health;
                int newHealth = Mathf.Clamp(value, 0, _maxHealth);
                int delta = newHealth - oldHealth;
                
                bool wasLowHealth = IsLowHealth;
                _health = newHealth;
                bool isNowLowHealth = IsLowHealth;
                
                // 触发血量变化事件
                OnHealthChanged?.Invoke(_health, oldHealth, delta);
                
                // 检查低血量状态变化
                if (wasLowHealth != isNowLowHealth)
                {
                    OnLowHealthWarning?.Invoke(isNowLowHealth);
                }
                
                // 检查死亡
                if (_health <= 0 && oldHealth > 0)
                {
                    OnPlayerDied?.Invoke();
                }
                
                OnDataChanged?.Invoke(this);
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
                if (_health > _maxHealth)
                {
                    Health = _maxHealth; // 通过属性修改，会触发相应事件
                }
                else
                {
                    OnDataChanged?.Invoke(this);
                }
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
                OnDataChanged?.Invoke(this);
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
                OnLevelChanged?.Invoke(_level);
                OnDataChanged?.Invoke(this);
            }
        }
    }
    
    public int Coins
    {
        get => _coins;
        set
        {
            if (_coins != value)
            {
                int delta = value - _coins;
                _coins = Mathf.Max(0, value);
                OnCoinsChanged?.Invoke(_coins, delta);
                OnDataChanged?.Invoke(this);
            }
        }
    }
    
    // ============ 计算属性 ============
    
    public float HealthPercentage => _maxHealth > 0 ? (float)_health / _maxHealth : 0;
    public bool IsAlive => _health > 0;
    public bool IsLowHealth => HealthPercentage < 0.3f;
    
    // ============ 构造函数 ============
    
    public PlayerDataAdvanced(string name = "Player")
    {
        _playerName = name;
        _health = _maxHealth;
        _level = 1;
        _score = 0;
        _coins = 0;
    }
    
    // ============ 业务方法 ============
    
    public void TakeDamage(int damage)
    {
        Health -= damage;
    }
    
    public void Heal(int amount)
    {
        Health += amount;
    }
    
    public void AddScore(int points)
    {
        Score += points;
    }
    
    public void AddCoins(int amount)
    {
        Coins += amount;
    }
    
    public void SpendCoins(int amount)
    {
        if (_coins >= amount)
        {
            Coins -= amount;
        }
        else
        {
            Debug.LogWarning($"Not enough coins! Have {_coins}, need {amount}");
        }
    }
    
    public void LevelUp()
    {
        Level++;
        MaxHealth += 10;
        Health = MaxHealth; // 升级回满血
    }
}