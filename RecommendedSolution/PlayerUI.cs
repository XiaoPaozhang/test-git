using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 玩家 UI 视图 - 推荐方案 A：统一数据绑定
/// UI 层只负责显示，通过事件响应数据变化
/// </summary>
public class PlayerUI : MonoBehaviour
{
    [Header("UI 组件引用")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image experienceBarFill;
    
    [Header("可选：动画和效果")]
    [SerializeField] private Animator scoreAnimator;
    [SerializeField] private ParticleSystem levelUpEffect;
    [SerializeField] private Image lowHealthWarning;
    
    // 数据源引用
    private PlayerData _playerData;
    private int _lastLevel; // 用于检测升级
    
    // ============ 数据绑定 ============
    
    /// <summary>
    /// 绑定数据源
    /// </summary>
    public void BindData(PlayerData data)
    {
        // 解绑旧数据
        UnbindData();
        
        _playerData = data;
        
        // 绑定新数据
        if (_playerData != null)
        {
            _playerData.OnDataChanged += OnPlayerDataChanged;
            _lastLevel = _playerData.Level;
            RefreshUI(); // 初始化显示
        }
    }
    
    /// <summary>
    /// 解绑数据源
    /// </summary>
    public void UnbindData()
    {
        if (_playerData != null)
        {
            _playerData.OnDataChanged -= OnPlayerDataChanged;
            _playerData = null;
        }
    }
    
    // ============ 事件响应 ============
    
    /// <summary>
    /// 数据变化时的回调 - 这是唯一的入口
    /// </summary>
    private void OnPlayerDataChanged(PlayerData data)
    {
        RefreshUI();
        
        // 可选：检测特定变化触发特效
        CheckForLevelUp();
        CheckForLowHealth();
    }
    
    // ============ UI 刷新 ============
    
    /// <summary>
    /// 统一的 UI 刷新方法
    /// 所有 UI 更新逻辑集中在这里，清晰易维护
    /// </summary>
    private void RefreshUI()
    {
        if (_playerData == null) return;
        
        // 更新文本
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
        
        // 更新血条
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = _playerData.HealthPercentage;
            
            // 根据血量改变颜色
            healthBarFill.color = GetHealthColor(_playerData.HealthPercentage);
        }
        
        // 更新经验条
        if (experienceBarFill != null)
        {
            experienceBarFill.fillAmount = _playerData.Experience / 100f;
        }
    }
    
    // ============ UI 特效和反馈 ============
    
    /// <summary>
    /// 检测升级并播放特效
    /// </summary>
    private void CheckForLevelUp()
    {
        if (_playerData.Level > _lastLevel)
        {
            OnLevelUp();
            _lastLevel = _playerData.Level;
        }
    }
    
    /// <summary>
    /// 升级时的 UI 反馈
    /// </summary>
    private void OnLevelUp()
    {
        Debug.Log($"Level Up! Now level {_playerData.Level}");
        
        // 播放升级特效
        if (levelUpEffect != null)
        {
            levelUpEffect.Play();
        }
        
        // 可以添加更多效果：音效、屏幕闪烁等
    }
    
    /// <summary>
    /// 检测低血量警告
    /// </summary>
    private void CheckForLowHealth()
    {
        if (lowHealthWarning != null)
        {
            // 血量低于 30% 时显示警告
            lowHealthWarning.enabled = _playerData.IsLowHealth;
        }
    }
    
    /// <summary>
    /// 根据血量百分比返回对应颜色
    /// </summary>
    private Color GetHealthColor(float healthPercentage)
    {
        if (healthPercentage > 0.6f)
            return Color.green;
        else if (healthPercentage > 0.3f)
            return Color.yellow;
        else
            return Color.red;
    }
    
    // ============ Unity 生命周期 ============
    
    private void OnDestroy()
    {
        // 重要：清理事件订阅，避免内存泄漏
        UnbindData();
    }
    
    // ============ 调试辅助 ============
    
#if UNITY_EDITOR
    [ContextMenu("Test: Take Damage")]
    private void TestTakeDamage()
    {
        _playerData?.TakeDamage(20);
    }
    
    [ContextMenu("Test: Add Score")]
    private void TestAddScore()
    {
        _playerData?.AddScore(100);
    }
    
    [ContextMenu("Test: Add Experience")]
    private void TestAddExperience()
    {
        _playerData?.AddExperience(30);
    }
#endif
}