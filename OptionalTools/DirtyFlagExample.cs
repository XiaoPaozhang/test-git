using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 脏标记模式示例 - 性能优先方案
/// 适用于高频更新、性能敏感的场景
/// </summary>
public class PlayerDataWithDirtyFlag
{
    // ============ 字段 ============
    private int _score;
    private int _health;
    private int _maxHealth = 100;
    private string _playerName;
    private int _level;
    
    // 脏标记
    private bool _isDirty;
    
    // ============ 属性 ============
    
    public int Score
    {
        get => _score;
        set
        {
            if (_score != value)
            {
                _score = value;
                MarkDirty();
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
                _health = Mathf.Clamp(value, 0, _maxHealth);
                MarkDirty();
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
                _maxHealth = value;
                if (_health > _maxHealth)
                {
                    _health = _maxHealth;
                }
                MarkDirty();
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
                MarkDirty();
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
                _level = value;
                MarkDirty();
            }
        }
    }
    
    // ============ 脏标记管理 ============
    
    public bool IsDirty => _isDirty;
    
    private void MarkDirty()
    {
        _isDirty = true;
    }
    
    public void ClearDirty()
    {
        _isDirty = false;
    }
    
    // ============ 辅助属性 ============
    
    public float HealthPercentage => _maxHealth > 0 ? (float)_health / _maxHealth : 0;
    
    // ============ 构造函数 ============
    
    public PlayerDataWithDirtyFlag(string name = "Player")
    {
        _playerName = name;
        _health = _maxHealth;
        _level = 1;
        _score = 0;
        _isDirty = true; // 初始标记为脏，确保首次显示
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
}

/// <summary>
/// 使用脏标记的 UI 控制器
/// </summary>
public class PlayerUIDirtyFlag : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthBarFill;
    
    [Header("更新设置")]
    [SerializeField] private bool updateInUpdate = true;  // 在 Update 中更新
    [SerializeField] private bool updateInLateUpdate = false;  // 在 LateUpdate 中更新
    [SerializeField] private float updateInterval = 0f;  // 0 = 每帧检查
    
    private PlayerDataWithDirtyFlag _playerData;
    private float _lastUpdateTime;
    
    // ============ 数据绑定 ============
    
    public void BindData(PlayerDataWithDirtyFlag data)
    {
        _playerData = data;
        
        if (_playerData != null)
        {
            RefreshUI(); // 初始化显示
            _playerData.ClearDirty();
        }
    }
    
    // ============ Unity 生命周期 ============
    
    void Update()
    {
        if (updateInUpdate)
        {
            CheckAndUpdateUI();
        }
    }
    
    void LateUpdate()
    {
        if (updateInLateUpdate)
        {
            CheckAndUpdateUI();
        }
    }
    
    // ============ UI 更新逻辑 ============
    
    /// <summary>
    /// 检查并更新 UI（如果数据有变化）
    /// </summary>
    private void CheckAndUpdateUI()
    {
        if (_playerData == null) return;
        
        // 检查更新间隔
        if (updateInterval > 0f)
        {
            if (Time.time - _lastUpdateTime < updateInterval)
            {
                return;
            }
            _lastUpdateTime = Time.time;
        }
        
        // 只在数据脏时更新
        if (_playerData.IsDirty)
        {
            RefreshUI();
            _playerData.ClearDirty();
        }
    }
    
    /// <summary>
    /// 手动强制刷新 UI
    /// </summary>
    public void ForceRefreshUI()
    {
        RefreshUI();
    }
    
    /// <summary>
    /// 刷新所有 UI 元素
    /// </summary>
    private void RefreshUI()
    {
        if (_playerData == null) return;
        
        if (playerNameText != null)
        {
            playerNameText.text = _playerData.PlayerName;
        }
        
        if (levelText != null)
        {
            levelText.text = $"Lv.{_playerData.Level}";
        }
        
        if (scoreText != null)
        {
            scoreText.text = $"Score: {_playerData.Score:N0}";
        }
        
        if (healthText != null)
        {
            healthText.text = $"HP: {_playerData.Health}/{_playerData.MaxHealth}";
        }
        
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = _playerData.HealthPercentage;
            healthBarFill.color = GetHealthColor(_playerData.HealthPercentage);
        }
    }
    
    private Color GetHealthColor(float healthPercentage)
    {
        if (healthPercentage > 0.6f)
            return Color.green;
        else if (healthPercentage > 0.3f)
            return Color.yellow;
        else
            return Color.red;
    }
}

/// <summary>
/// 高性能场景示例：大量实体同时更新
/// </summary>
public class HighPerformanceExample : MonoBehaviour
{
    [SerializeField] private int entityCount = 100;
    
    private PlayerDataWithDirtyFlag[] _entities;
    private PlayerUIDirtyFlag[] _entityUIs;
    
    void Start()
    {
        // 创建大量实体
        _entities = new PlayerDataWithDirtyFlag[entityCount];
        _entityUIs = new PlayerUIDirtyFlag[entityCount];
        
        for (int i = 0; i < entityCount; i++)
        {
            _entities[i] = new PlayerDataWithDirtyFlag($"Player{i}");
            // _entityUIs[i] = ...绑定 UI
        }
    }
    
    void Update()
    {
        // 模拟：每帧随机更新一些实体的数据
        for (int i = 0; i < 10; i++)
        {
            int randomIndex = Random.Range(0, entityCount);
            _entities[randomIndex].AddScore(Random.Range(1, 10));
            _entities[randomIndex].TakeDamage(Random.Range(0, 5));
        }
        
        // UI 系统会在各自的 Update 中检查脏标记并更新
        // 只有发生变化的 UI 才会刷新，性能高效
    }
}