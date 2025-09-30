using System;
using UnityEngine;

/// <summary>
/// 轻量级可观察属性类
/// 可选工具：只在需要时使用，避免过度包装
/// </summary>
[Serializable]
public class ReactiveProperty<T>
{
    [SerializeField] private T _value;
    
    /// <summary>
    /// 值变化时触发，传递新值
    /// </summary>
    public event Action<T> OnValueChanged;
    
    /// <summary>
    /// 值变化时触发，传递新值和旧值
    /// </summary>
    public event Action<T, T> OnValueChangedWithOld;
    
    /// <summary>
    /// 获取或设置值
    /// </summary>
    public T Value
    {
        get => _value;
        set
        {
            if (!Equals(_value, value))
            {
                T oldValue = _value;
                _value = value;
                
                OnValueChanged?.Invoke(_value);
                OnValueChangedWithOld?.Invoke(_value, oldValue);
            }
        }
    }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public ReactiveProperty(T initialValue = default)
    {
        _value = initialValue;
    }
    
    /// <summary>
    /// 静默设置值（不触发事件）
    /// </summary>
    public void SetValueWithoutNotify(T value)
    {
        _value = value;
    }
    
    /// <summary>
    /// 强制触发事件（即使值没变）
    /// </summary>
    public void ForceNotify()
    {
        OnValueChanged?.Invoke(_value);
        OnValueChangedWithOld?.Invoke(_value, _value);
    }
    
    /// <summary>
    /// 隐式转换为 T 类型
    /// </summary>
    public static implicit operator T(ReactiveProperty<T> property)
    {
        return property != null ? property.Value : default;
    }
    
    /// <summary>
    /// 重写 ToString
    /// </summary>
    public override string ToString()
    {
        return _value != null ? _value.ToString() : "null";
    }
    
    /// <summary>
    /// 重写 Equals
    /// </summary>
    public override bool Equals(object obj)
    {
        if (obj is ReactiveProperty<T> other)
        {
            return Equals(_value, other._value);
        }
        return false;
    }
    
    /// <summary>
    /// 重写 GetHashCode
    /// </summary>
    public override int GetHashCode()
    {
        return _value != null ? _value.GetHashCode() : 0;
    }
}

/// <summary>
/// 使用示例：只在需要精细控制的字段上使用
/// </summary>
public class PlayerDataWithReactive
{
    // 需要精细监听的字段使用 ReactiveProperty
    public ReactiveProperty<int> Score = new ReactiveProperty<int>(0);
    public ReactiveProperty<int> Health = new ReactiveProperty<int>(100);
    
    // 不需要监听的字段保持原始类型
    public string PlayerId;
    public DateTime CreatedTime;
    
    public PlayerDataWithReactive()
    {
        PlayerId = Guid.NewGuid().ToString();
        CreatedTime = DateTime.Now;
    }
}

/// <summary>
/// UI 使用示例
/// </summary>
public class UIExample : MonoBehaviour
{
    private PlayerDataWithReactive _playerData;
    
    void Start()
    {
        _playerData = new PlayerDataWithReactive();
        
        // 订阅分数变化
        _playerData.Score.OnValueChanged += (newScore) =>
        {
            Debug.Log($"Score changed to: {newScore}");
            // 更新 UI
        };
        
        // 订阅血量变化（带旧值）
        _playerData.Health.OnValueChangedWithOld += (newHealth, oldHealth) =>
        {
            int delta = newHealth - oldHealth;
            Debug.Log($"Health changed: {oldHealth} -> {newHealth} (delta: {delta})");
            // 更新 UI，播放受伤/治疗动画
        };
        
        // 修改值会自动触发事件
        _playerData.Score.Value = 100;
        _playerData.Health.Value = 80;
        
        // 使用隐式转换
        int currentScore = _playerData.Score; // 自动转换为 int
        Debug.Log($"Current score: {currentScore}");
    }
}