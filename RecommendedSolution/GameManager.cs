using UnityEngine;

/// <summary>
/// 游戏管理器 - 演示如何使用数据绑定系统
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private PlayerUI miniMapUI; // 演示：一个数据源可以绑定多个 UI
    
    [Header("测试按钮")]
    [SerializeField] private bool enableTestButtons = true;
    
    // 数据模型
    private PlayerData _playerData;
    
    void Start()
    {
        InitializeGame();
    }
    
    /// <summary>
    /// 初始化游戏
    /// </summary>
    private void InitializeGame()
    {
        // 1. 创建数据模型
        _playerData = new PlayerData("Hero");
        _playerData.Level = 1;
        _playerData.Health = 100;
        _playerData.Score = 0;
        
        // 2. 绑定 UI（一个数据源可以绑定多个 UI）
        if (playerUI != null)
        {
            playerUI.BindData(_playerData);
        }
        
        if (miniMapUI != null)
        {
            miniMapUI.BindData(_playerData);
        }
        
        Debug.Log("Game initialized. Player data bound to UI.");
    }
    
    // ============ 游戏逻辑示例 ============
    
    /// <summary>
    /// 玩家击杀敌人
    /// </summary>
    public void OnEnemyKilled()
    {
        _playerData.AddScore(100);
        _playerData.AddExperience(25f);
        
        Debug.Log("Enemy killed! " + _playerData.ToString());
    }
    
    /// <summary>
    /// 玩家受到攻击
    /// </summary>
    public void OnPlayerAttacked(int damage)
    {
        _playerData.TakeDamage(damage);
        
        if (!_playerData.IsAlive)
        {
            OnPlayerDied();
        }
    }
    
    /// <summary>
    /// 玩家拾取治疗包
    /// </summary>
    public void OnHealthPickup(int healAmount)
    {
        _playerData.Heal(healAmount);
        Debug.Log($"Healed {healAmount} HP");
    }
    
    /// <summary>
    /// 玩家死亡
    /// </summary>
    private void OnPlayerDied()
    {
        Debug.Log("Player died!");
        // 显示游戏结束界面等
    }
    
    /// <summary>
    /// 批量更新示例：领取任务奖励
    /// </summary>
    public void OnQuestCompleted()
    {
        // 使用批量更新，避免触发多次 UI 刷新
        _playerData.BatchUpdate(() =>
        {
            _playerData.AddScore(500);
            _playerData.AddExperience(50f);
            _playerData.Heal(30);
        });
        
        Debug.Log("Quest completed! Rewards claimed.");
    }
    
    // ============ 测试用的 GUI ============
    
    void OnGUI()
    {
        if (!enableTestButtons) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 250, 500));
        GUILayout.Label("=== Test Controls ===");
        
        if (GUILayout.Button("Kill Enemy (+100 Score, +25 Exp)"))
        {
            OnEnemyKilled();
        }
        
        if (GUILayout.Button("Take Damage (-20 HP)"))
        {
            OnPlayerAttacked(20);
        }
        
        if (GUILayout.Button("Heal (+30 HP)"))
        {
            OnHealthPickup(30);
        }
        
        if (GUILayout.Button("Complete Quest (Batch Update)"))
        {
            OnQuestCompleted();
        }
        
        if (GUILayout.Button("Add Experience (+30)"))
        {
            _playerData.AddExperience(30f);
        }
        
        GUILayout.Space(20);
        GUILayout.Label("Current Data:");
        GUILayout.Label($"Name: {_playerData?.PlayerName}");
        GUILayout.Label($"Level: {_playerData?.Level}");
        GUILayout.Label($"HP: {_playerData?.Health}/{_playerData?.MaxHealth}");
        GUILayout.Label($"Score: {_playerData?.Score}");
        GUILayout.Label($"Exp: {_playerData?.Experience:F1}/100");
        
        GUILayout.EndArea();
    }
    
    void OnDestroy()
    {
        // 清理（虽然数据模型会在 GC 时自动清理，但这是好习惯）
        if (playerUI != null)
        {
            playerUI.UnbindData();
        }
        
        if (miniMapUI != null)
        {
            miniMapUI.UnbindData();
        }
    }
}