using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 玩家 UI - 进阶方案：精细化事件订阅
/// 根据需要订阅特定字段的变化事件，实现更精细的控制
/// </summary>
public class PlayerUIAdvanced : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Image healthBarFill;
    
    [Header("特效组件")]
    [SerializeField] private Animator scoreAnimator;
    [SerializeField] private Animator healthAnimator;
    [SerializeField] private ParticleSystem levelUpEffect;
    [SerializeField] private GameObject lowHealthWarningPanel;
    [SerializeField] private AudioSource audioSource;
    
    [Header("音效")]
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private AudioClip levelUpSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip healSound;
    
    private PlayerDataAdvanced _playerData;
    
    // ============ 数据绑定 ============
    
    public void BindData(PlayerDataAdvanced data)
    {
        UnbindData();
        
        _playerData = data;
        
        if (_playerData != null)
        {
            // 精细化订阅：只订阅需要的事件
            _playerData.OnScoreChanged += OnScoreChanged;
            _playerData.OnHealthChanged += OnHealthChanged;
            _playerData.OnLevelChanged += OnLevelChanged;
            _playerData.OnCoinsChanged += OnCoinsChanged;
            _playerData.OnPlayerDied += OnPlayerDied;
            _playerData.OnLowHealthWarning += OnLowHealthWarning;
            
            // 初始化显示
            InitializeUI();
        }
    }
    
    public void UnbindData()
    {
        if (_playerData != null)
        {
            _playerData.OnScoreChanged -= OnScoreChanged;
            _playerData.OnHealthChanged -= OnHealthChanged;
            _playerData.OnLevelChanged -= OnLevelChanged;
            _playerData.OnCoinsChanged -= OnCoinsChanged;
            _playerData.OnPlayerDied -= OnPlayerDied;
            _playerData.OnLowHealthWarning -= OnLowHealthWarning;
            _playerData = null;
        }
    }
    
    // ============ 初始化 ============
    
    private void InitializeUI()
    {
        if (_playerData == null) return;
        
        UpdatePlayerName();
        UpdateLevel(_playerData.Level);
        UpdateScore(_playerData.Score);
        UpdateHealth(_playerData.Health, _playerData.MaxHealth);
        UpdateCoins(_playerData.Coins);
    }
    
    // ============ 精细化事件响应 ============
    
    /// <summary>
    /// 分数变化 - 播放动画和音效
    /// </summary>
    private void OnScoreChanged(int newScore)
    {
        UpdateScore(newScore);
        
        // 播放分数增加动画
        if (scoreAnimator != null)
        {
            scoreAnimator.SetTrigger("ScoreUp");
        }
    }
    
    /// <summary>
    /// 血量变化 - 根据增减播放不同效果
    /// </summary>
    private void OnHealthChanged(int newHealth, int oldHealth, int delta)
    {
        UpdateHealth(newHealth, _playerData.MaxHealth);
        
        if (delta < 0)
        {
            // 受到伤害
            OnDamageTaken(-delta);
        }
        else if (delta > 0)
        {
            // 恢复血量
            OnHealed(delta);
        }
    }
    
    /// <summary>
    /// 等级变化 - 播放升级特效
    /// </summary>
    private void OnLevelChanged(int newLevel)
    {
        UpdateLevel(newLevel);
        
        // 播放升级特效
        if (levelUpEffect != null)
        {
            levelUpEffect.Play();
        }
        
        // 播放升级音效
        PlaySound(levelUpSound);
        
        // 可以添加屏幕闪光等效果
        StartCoroutine(LevelUpFlash());
    }
    
    /// <summary>
    /// 金币变化 - 显示浮动文字
    /// </summary>
    private void OnCoinsChanged(int newAmount, int delta)
    {
        UpdateCoins(newAmount);
        
        if (delta > 0)
        {
            // 获得金币
            PlaySound(coinSound);
            ShowFloatingText($"+{delta} 金币", Color.yellow);
        }
        else if (delta < 0)
        {
            // 花费金币
            ShowFloatingText($"{delta} 金币", Color.red);
        }
    }
    
    /// <summary>
    /// 玩家死亡
    /// </summary>
    private void OnPlayerDied()
    {
        Debug.Log("Player died! Show game over screen");
        
        // 播放死亡动画
        if (healthAnimator != null)
        {
            healthAnimator.SetTrigger("Died");
        }
        
        // 显示游戏结束界面
        // ShowGameOverScreen();
    }
    
    /// <summary>
    /// 低血量警告
    /// </summary>
    private void OnLowHealthWarning(bool isLowHealth)
    {
        if (lowHealthWarningPanel != null)
        {
            lowHealthWarningPanel.SetActive(isLowHealth);
        }
        
        if (isLowHealth)
        {
            Debug.Log("Warning: Low Health!");
            StartCoroutine(LowHealthPulse());
        }
    }
    
    // ============ UI 更新方法 ============
    
    private void UpdatePlayerName()
    {
        if (playerNameText != null && _playerData != null)
        {
            playerNameText.text = _playerData.PlayerName;
        }
    }
    
    private void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Lv.{level}";
        }
    }
    
    private void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score:N0}";
        }
    }
    
    private void UpdateHealth(int health, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {health}/{maxHealth}";
        }
        
        if (healthBarFill != null)
        {
            float targetFill = maxHealth > 0 ? (float)health / maxHealth : 0;
            
            // 平滑过渡血条
            StartCoroutine(AnimateHealthBar(targetFill));
            
            // 根据血量改变颜色
            healthBarFill.color = GetHealthColor(targetFill);
        }
    }
    
    private void UpdateCoins(int coins)
    {
        if (coinsText != null)
        {
            coinsText.text = $"💰 {coins}";
        }
    }
    
    // ============ 视觉效果 ============
    
    private void OnDamageTaken(int damage)
    {
        // 播放受伤动画
        if (healthAnimator != null)
        {
            healthAnimator.SetTrigger("Damage");
        }
        
        // 播放受伤音效
        PlaySound(damageSound);
        
        // 显示伤害数字
        ShowFloatingText($"-{damage}", Color.red);
        
        // 屏幕震动
        // CameraShake();
    }
    
    private void OnHealed(int healAmount)
    {
        // 播放治疗音效
        PlaySound(healSound);
        
        // 显示治疗数字
        ShowFloatingText($"+{healAmount}", Color.green);
    }
    
    private Color GetHealthColor(float healthPercentage)
    {
        if (healthPercentage > 0.6f)
            return new Color(0.2f, 1f, 0.2f); // 绿色
        else if (healthPercentage > 0.3f)
            return new Color(1f, 0.9f, 0.2f); // 黄色
        else
            return new Color(1f, 0.2f, 0.2f); // 红色
    }
    
    // ============ 协程动画 ============
    
    private IEnumerator AnimateHealthBar(float targetFill)
    {
        if (healthBarFill == null) yield break;
        
        float startFill = healthBarFill.fillAmount;
        float elapsed = 0f;
        float duration = 0.3f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            healthBarFill.fillAmount = Mathf.Lerp(startFill, targetFill, t);
            yield return null;
        }
        
        healthBarFill.fillAmount = targetFill;
    }
    
    private IEnumerator LevelUpFlash()
    {
        // 屏幕闪光效果
        // 这里可以添加 CanvasGroup 或 Image 的闪光动画
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Level up flash animation");
    }
    
    private IEnumerator LowHealthPulse()
    {
        // 低血量时血条脉动效果
        while (_playerData != null && _playerData.IsLowHealth && _playerData.IsAlive)
        {
            if (healthBarFill != null)
            {
                float pulse = (Mathf.Sin(Time.time * 5f) + 1f) * 0.5f;
                healthBarFill.color = Color.Lerp(Color.red, Color.white, pulse * 0.3f);
            }
            yield return null;
        }
    }
    
    // ============ 辅助方法 ============
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    private void ShowFloatingText(string text, Color color)
    {
        // 这里可以实例化浮动文字预制体
        Debug.Log($"[FloatingText] {text}");
        
        // 示例实现：
        // var floatingText = Instantiate(floatingTextPrefab, transform);
        // floatingText.SetText(text, color);
    }
    
    // ============ Unity 生命周期 ============
    
    private void OnDestroy()
    {
        UnbindData();
    }
}